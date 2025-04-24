using System;
using System.Collections.Generic;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class TimelineBakerAutoRegressive : TimelineBakerBase
    {
        const int k_MaxPastContext = 10;
        const int k_MaxFramesAhead = 60;
        const float k_PostProcessPhysicsProgressRatio = 0.9f;

        public enum PhysicsMode
        {
            AutoRegressive,
            PostProcess,
            None
        }

        public PhysicsMode Physics
        {
            get => m_PhysicsMode;
            set => m_PhysicsMode = value;
        }
        
        enum SubStep
        {
            Unknown,
            RemoveOldContext,
            RemoveAllFutureContext,
            CaptureFutureFrames,
            CapturePreviousFrame,
            Interpolation,
            InitializeSolver,
            RunSolver,
            RunPhysics,
            CaptureOutputFrame,
            GoToNextFrame,
            PhysicsPostProcess
        }

        struct ProcessInfo
        {
            public float Progress;
            public SubStep CurrentStep;
            public int CurrentFrameIdx;

            public int LoopbackStartFrame;
            public int LoopbackEndFrame;
            public int NumLoopbackLeft;
            public int LastBakedFrame;
        }
        
        public bool IsRunning => State == IProcessingRequest.ProcessState.InProgress;
        public override IProcessingRequest.ProcessState State => m_State;
        public override float Progress => m_ProcessInfo.Progress;
        public override float WaitDelay => 0f;
        public override DateTime WaitStartTime { get; protected set; }
        
        ProcessInfo m_ProcessInfo;
        PhysicsMode m_PhysicsMode;

        TimelineModel m_SourceTimeline;
        BakedTimelineModel m_DestinationBakedTimeline;
        BakedTimelineMappingModel m_DestinationMapping;
        PhysicsSolverComponent m_PhysicsSolverComponent;

        Dictionary<EntityID, EntityBakingContext> m_EntityContexts;
        Dictionary<EntityID, MotionAutoRegressiveComponent> m_MotionAutoRegressiveComponents;
        IProcessingRequest.ProcessState m_State;

        List<BakedFrameModel> m_PhysicsFrames = new();
        PhysicsRequest m_PhysicsRequest = null;

        float m_Tolerance = 0.0f;
        
        public TimelineBakerAutoRegressive(PhysicsSolverComponent physicsSolverComponent)
        {
            m_State = IProcessingRequest.ProcessState.Unknown;
            m_ProcessInfo.Progress = 0f;

            m_PhysicsMode = PhysicsMode.PostProcess;

            m_MotionAutoRegressiveComponents = new Dictionary<EntityID, MotionAutoRegressiveComponent>();
            m_PhysicsSolverComponent = physicsSolverComponent;
        }

        public override void Initialize(TimelineModel sourceTimeline,
            BakedTimelineModel destinationBakedTimeline,
            BakedTimelineMappingModel destinationMapping,
            Dictionary<EntityID, EntityBakingContext> entityBakingContexts)
        {
            Assert.IsFalse(IsRunning, "Request is already running. Stop it before initializing.");
            m_SourceTimeline = sourceTimeline;
            m_DestinationBakedTimeline = destinationBakedTimeline;
            m_DestinationMapping = destinationMapping;
            m_EntityContexts = entityBakingContexts;
        }

        public override void Start()
        {
            Assert.IsNotNull(m_SourceTimeline, "No source timeline. Did you Initialize first?");
            Assert.IsNotNull(m_DestinationBakedTimeline, "No destination baked timeline. Did you Initialize first?");

            Assert.IsTrue(m_State != IProcessingRequest.ProcessState.InProgress, "Baking already running.");

            if (m_SourceTimeline.KeyCount == 0)
            {
                m_DestinationBakedTimeline.Clear();
                m_State = IProcessingRequest.ProcessState.Done;
                m_ProcessInfo.Progress = 1f;
                return;
            }

            // Make sure timeline and baked timeline have the same entities
            BakingUtils.SyncTimelineAndBakedTimelineEntities(m_SourceTimeline, m_DestinationBakedTimeline);

            // Set baked timeline duration
            var animationLength = m_SourceTimeline.ComputeAnimationLength();
            m_DestinationBakedTimeline.FramesCount = animationLength;

            // Get all keyed actors
            using var keyedEntityIDs = TempHashSet<EntityID>.Allocate();
            m_SourceTimeline.GetAllEntities(keyedEntityIDs.Set);

            m_SourceTimeline.UpdateBakingMapping(m_DestinationMapping);
            m_SourceTimeline.TransferKeysToBakedTimeline(keyedEntityIDs.Set, m_DestinationMapping, m_DestinationBakedTimeline);

            SetupPhysicsEntities(m_EntityContexts);
            GetMotionComponents(m_EntityContexts);
            SetupRagdolls(m_EntityContexts, m_PhysicsSolverComponent);
            InitializeMotionComponents();

            // Setup the physics request
            m_PhysicsFrames.Clear();
            if (m_PhysicsMode == PhysicsMode.PostProcess)
            {
                m_DestinationBakedTimeline.GetFrames(0, m_DestinationBakedTimeline.FramesCount - 1, m_PhysicsFrames);
                m_PhysicsRequest = new PhysicsRequest(m_EntityContexts, m_PhysicsFrames[0], m_PhysicsFrames, m_PhysicsSolverComponent);
                m_PhysicsRequest.Start();
            }
            else
            {
                m_PhysicsRequest = null;
            }

            m_State = IProcessingRequest.ProcessState.InProgress;

            m_ProcessInfo.Progress = 0f;
            m_ProcessInfo.CurrentFrameIdx = 0;
            m_ProcessInfo.CurrentStep = SubStep.Unknown;
            m_ProcessInfo.LastBakedFrame = -1;
            m_ProcessInfo.NumLoopbackLeft = 0;
            m_ProcessInfo.LoopbackStartFrame = -1;
            m_ProcessInfo.LoopbackEndFrame = -1;
        }

        public override void Stop()
        {
            StopAllSolvers();
            m_PhysicsRequest?.Stop();

            m_State = IProcessingRequest.ProcessState.Failed;
            m_ProcessInfo.Progress = 0f;
        }

        public override void Step()
        {
            Assert.AreEqual(IProcessingRequest.ProcessState.InProgress, m_State);

            // Apply motion synthesis
            if (m_ProcessInfo.CurrentFrameIdx > 0)
            {
                switch (m_ProcessInfo.CurrentStep)
                {
                    case SubStep.Unknown:
                        m_ProcessInfo.CurrentStep = SubStep.RemoveOldContext;
                        break;

                    case SubStep.RemoveOldContext:
                        StepRemoveOldContext();
                        break;

                    case SubStep.RemoveAllFutureContext:
                        StepRemoveAllFutureContext();
                        break;

                    case SubStep.CapturePreviousFrame:
                        StepCapturePreviousFrames();
                        break;

                    case SubStep.CaptureFutureFrames:
                        StepCaptureFutureFrames();
                        break;

                    case SubStep.Interpolation:
                        StepInterpolation();
                        break;

                    case SubStep.InitializeSolver:
                        StepInitializeSolvers();
                        break;

                    case SubStep.RunSolver:
                        StepRunSolvers();
                        break;

                    case SubStep.RunPhysics:
                        StepRunPhysics();
                        break;

                    case SubStep.CaptureOutputFrame:
                        StepCaptureOutput();
                        break;

                    case SubStep.GoToNextFrame:
                        StepGoToNextFrame();
                        break;

                    case SubStep.PhysicsPostProcess:
                        StepPhysicsPostProcess();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                StepGoToNextFrame();
            }
        }

        void StepPhysicsPostProcess()
        {
            m_PhysicsRequest.Step();

            if (m_PhysicsRequest.State == IProcessingRequest.ProcessState.InProgress)
            {
                m_ProcessInfo.Progress = k_PostProcessPhysicsProgressRatio + (1f - k_PostProcessPhysicsProgressRatio) * m_PhysicsRequest.Progress;
            }
            else
            {
                m_State = IProcessingRequest.ProcessState.Done;
                m_ProcessInfo.Progress = 1f;
            }
        }

        void StepGoToNextFrame()
        {
            // Store the last (ie most ahead) frame that was baked
            m_ProcessInfo.LastBakedFrame = Mathf.Max(m_ProcessInfo.LastBakedFrame, m_ProcessInfo.CurrentFrameIdx);

            // Process next frame
            m_ProcessInfo.CurrentFrameIdx++;

            // Baking end
            if (m_ProcessInfo.CurrentFrameIdx >= m_DestinationBakedTimeline.FramesCount)
            {
                // Do physics post-process
                if (m_PhysicsMode == PhysicsMode.PostProcess)
                {
                    m_ProcessInfo.CurrentStep = SubStep.PhysicsPostProcess;
                    m_ProcessInfo.Progress = k_PostProcessPhysicsProgressRatio;
                }

                // Or finish baking if physics was done in auto-regressive fashion
                else
                {
                    m_State = IProcessingRequest.ProcessState.Done;
                    m_ProcessInfo.Progress = 1f;
                }
            }

            // Bake next frame
            else
            {
                // Check for new loopbacks
                // New loopbacks occur if we are not already in a loopback
                // and if the frames going to be baked was not already included in a previous loopback
                // and if we are at the end of a loop
                if (m_ProcessInfo.NumLoopbackLeft == 0 && m_ProcessInfo.CurrentFrameIdx > m_ProcessInfo.LoopbackEndFrame
                    &&m_DestinationMapping.TryGetKey(m_SourceTimeline, m_ProcessInfo.CurrentFrameIdx, out var currentKey)
                    && currentKey.Key.Type == KeyData.KeyType.Loop
                    && currentKey.Key.Loop.StartFrame < m_ProcessInfo.CurrentFrameIdx - 1) // loop of at least 1 middle frame
                {
                    // Setup loopback
                    m_ProcessInfo.LoopbackStartFrame = currentKey.Key.Loop.StartFrame;
                    m_ProcessInfo.LoopbackEndFrame = m_ProcessInfo.CurrentFrameIdx;
                    m_ProcessInfo.NumLoopbackLeft = currentKey.Key.Loop.NumBakingLoopbacks;
                }

                // Check if we should loop back
                // Loopback excludes both the first and the last frame of the loop to preserve them
                if (m_ProcessInfo.NumLoopbackLeft > 0 && m_ProcessInfo.CurrentFrameIdx == m_ProcessInfo.LoopbackEndFrame)
                {
                    // Go to loopback frame
                    m_ProcessInfo.CurrentFrameIdx = m_ProcessInfo.LoopbackStartFrame + 1;

                    // Decrease number of loopbacks left
                    m_ProcessInfo.NumLoopbackLeft--;
                }

                // Bake next frame
                m_ProcessInfo.CurrentStep = SubStep.RemoveOldContext;

                // Update progress
                m_ProcessInfo.Progress = (k_PostProcessPhysicsProgressRatio * m_ProcessInfo.CurrentFrameIdx) / m_DestinationBakedTimeline.FramesCount;
            }
        }

        void StepRemoveOldContext()
        {
            // Add motion synthesis inputs
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                if (motionComponent == null)
                    continue;

                // Remove frames too far away and capture new input
                motionComponent.RemoveAllInputsBefore(m_ProcessInfo.CurrentFrameIdx);
            }

            m_ProcessInfo.CurrentStep = SubStep.RemoveAllFutureContext;
        }

        void StepRemoveAllFutureContext()
        {
            // Add motion synthesis inputs
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                if (motionComponent == null)
                    continue;

                // Remove frames too far away and capture new input
                motionComponent.RemoveAllInputsAfter(m_ProcessInfo.CurrentFrameIdx);
            }

            m_ProcessInfo.CurrentStep = SubStep.CapturePreviousFrame;
        }

        void StepCapturePreviousFrames()
        {
            var isLoopingBack = m_ProcessInfo.CurrentFrameIdx > m_ProcessInfo.LoopbackStartFrame && m_ProcessInfo.CurrentFrameIdx < m_ProcessInfo.LoopbackEndFrame;
            var firstPastFrame = m_ProcessInfo.CurrentFrameIdx - k_MaxPastContext;
            var referenceFrameIdx = m_ProcessInfo.CurrentFrameIdx - 1;

            for (var pastFrameIdx = firstPastFrame; pastFrameIdx < m_ProcessInfo.CurrentFrameIdx; pastFrameIdx++)
            {
                var isReferenceFrame = pastFrameIdx == referenceFrameIdx;

                var targetPastFrameIdx = pastFrameIdx;
                var usingFutureFrames = false;

                BakedFrameModel loopStartFrameModel = null;
                TimelineModel.SequenceKey loopKey = null;
                if (isLoopingBack)
                {
                    usingFutureFrames = pastFrameIdx < m_ProcessInfo.LoopbackStartFrame;
                    if (usingFutureFrames)
                    {
                        targetPastFrameIdx = m_ProcessInfo.LoopbackEndFrame + pastFrameIdx - m_ProcessInfo.LoopbackStartFrame;

                        // retrieve loop data
                        Assert.IsTrue(m_DestinationMapping.TryGetKey(m_SourceTimeline, m_ProcessInfo.LoopbackEndFrame, out loopKey));
                        Assert.AreEqual(KeyData.KeyType.Loop, loopKey.Key.Type);
                        loopStartFrameModel = m_DestinationBakedTimeline.GetFrame(m_ProcessInfo.LoopbackStartFrame);
                    }
                }

                if (targetPastFrameIdx < 0)
                    continue;

                var pastFrame = m_DestinationBakedTimeline.GetFrame(targetPastFrameIdx);

                foreach (var pair in m_EntityContexts)
                {
                    var entityID = pair.Key;

                    var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                    if (motionComponent == null)
                        continue;

                    var motionArmature = pair.Value.MotionArmature;

                    if (!pastFrame.TryGetModel(entityID, out var prevPose))
                    {
                        Debug.LogWarning($"Could not find pose for entity: {entityID} at frame: {targetPastFrameIdx}");
                        continue;
                    }

                    var translationOffset = Vector3.zero;
                    var rotationOffset = Quaternion.identity;

                    // Apply inverse loop transform if using future loop frames
                    if (usingFutureFrames)
                    {
                        if (!loopKey.Key.Loop.TryGetOffset(entityID, out var loopOffset))
                            continue;

                        // Get the loop start pose to compute offsets
                        if (!loopStartFrameModel.TryGetModel(entityID, out var loopStartPose))
                        {
                            Debug.LogWarning($"Could not find loop start pose for entity: {entityID} at frame: {m_ProcessInfo.LoopbackStartFrame}");
                            continue;
                        }

                        // Key Translation / Rotation are expressed as offsets of the root joint wrt to its original position
                        // Ie, rotation does not impact the root position (ie root position is not rotated)
                        // But applying a frame to a skeleton applies world space transforms, ie the root position will be rotated
                        // So we must convert the translation to compensate for this rotation
                        var sourceRootPosition = loopStartPose.LocalPose.GetPosition(0);

                        // We first compute the forward transform (ie transform to loop toward the future)
                        translationOffset = sourceRootPosition + loopOffset.Position - loopOffset.Rotation * sourceRootPosition;
                        rotationOffset = loopOffset.Rotation;

                        // As we are looking in the past, we need the inverse transform
                        rotationOffset = Quaternion.Inverse(rotationOffset);
                        translationOffset = -(rotationOffset * translationOffset);
                    }

                    // Apply last valid frame to motion armature
                    prevPose.ApplyTo(motionArmature.ArmatureMappingData, translationOffset, rotationOffset);

                    // Remove frames too far away and capture new input
                    motionComponent.CaptureInputPose(pastFrameIdx, isReferenceFrame);
                }
            }

            m_ProcessInfo.CurrentStep = SubStep.CaptureFutureFrames;
        }

        void StepCaptureFutureFrames()
        {
            var isLooping = false;
            var loopStartFrame = -1;
            var loopKeyFrame = -1;
            var maxLoopEndFrame = -1;
            BakedFrameModel loopStartFrameModel = null;

            using var loopTranslations = TempDictionary<EntityID, Vector3>.Allocate();
            using var loopRotations = TempDictionary<EntityID, Quaternion>.Allocate();

            foreach (var pair in m_EntityContexts)
            {
                loopTranslations.Dictionary[pair.Key] = Vector3.zero;
                loopRotations.Dictionary[pair.Key] = Quaternion.identity;
            }

            for (var k = 0; k < k_MaxFramesAhead; k++)
            {
                var futureFrameIdx = m_ProcessInfo.CurrentFrameIdx + k;

                // Stop looping if max looping frame reached
                if (isLooping && maxLoopEndFrame != -1 && futureFrameIdx > maxLoopEndFrame)
                    isLooping = false;

                // Check if there is a key
                var isAtKey = m_DestinationMapping.TryGetKey(m_SourceTimeline, futureFrameIdx, out var futureKey);

                // Handle looping keys
                if (isAtKey && futureKey.Key.Type == KeyData.KeyType.Loop)
                {
                    isLooping = true;
                    loopStartFrame = futureKey.Key.Loop.StartFrame;
                    loopKeyFrame = futureFrameIdx;
                    loopStartFrameModel = m_DestinationBakedTimeline.GetFrame(loopStartFrame);

                    foreach (var pair in m_EntityContexts)
                    {
                        if (!futureKey.Key.Loop.TryGetOffset(pair.Key, out var loopOffset))
                            continue;

                        loopTranslations.Dictionary[pair.Key] = loopOffset.Position;
                        loopRotations.Dictionary[pair.Key] = loopOffset.Rotation;
                    }

                    if (futureKey.NextKey != null)
                    {
                        // If future key, only allow to loop past frame during the first half of the transition
                        maxLoopEndFrame = m_ProcessInfo.CurrentFrameIdx + futureKey.OutTransition.Transition.Duration / 2;
                    }
                    else
                    {
                        // If no future key, loop as much as we can
                        maxLoopEndFrame = -1;
                    }
                }

                // If we are not in a loop, and not at a full pose key, then there is no pose data to handle
                if (!isLooping && (!isAtKey || futureKey.Key.Type != KeyData.KeyType.FullPose))
                    continue;

                // If we are in a loop, source frame index is in the past
                var sourceFrameIdx = isLooping ? loopStartFrame + futureFrameIdx - loopKeyFrame : futureFrameIdx;

                // Loops should not go further that current predicted frame
                if (isLooping && sourceFrameIdx >= m_ProcessInfo.CurrentFrameIdx)
                    continue;

                // Retrieve source frame
                var futureFrame = m_DestinationBakedTimeline.GetFrame(sourceFrameIdx);

                foreach (var pair in m_EntityContexts)
                {
                    var entityID = pair.Key;
                    var motionArmature = pair.Value.MotionArmature;
                    var motionComponent = m_MotionAutoRegressiveComponents[entityID];

                    if (motionComponent == null)
                        continue;

                    if (!futureFrame.TryGetModel(entityID, out var futurePose))
                    {
                        Debug.LogWarning($"Could not find target pose for entity: {entityID} at frame: {futureFrameIdx}");
                        continue;
                    }

                    // Compute source frame offsets
                    var positionOffset = isLooping ? loopTranslations.Dictionary[entityID] : Vector3.zero;
                    var rotationOffset = isLooping ? loopRotations.Dictionary[entityID] : Quaternion.identity;

                    var convertedPositionOffset = Vector3.zero;
                    if (isLooping)
                    {
                        // Get the loop start pose to compute offsets
                        if (!loopStartFrameModel.TryGetModel(entityID, out var loopStartPose))
                        {
                            Debug.LogWarning($"Could not find loop start pose for entity: {entityID} at frame: {loopStartFrame}");
                            continue;
                        }

                        // Key Translation / Rotation are expressed as offsets of the root joint wrt to its original position
                        // Ie, rotation does not impact the root position (ie root position is not rotated)
                        // But applying a frame to a skeleton applies world space transforms, ie the root position will be rotated
                        // So we must convert the translation to compensate for this rotation
                        var sourceRootPosition = loopStartPose.LocalPose.GetPosition(0);
                        convertedPositionOffset = sourceRootPosition + positionOffset - rotationOffset * sourceRootPosition;
                    }

                    // Apply last valid frame to motion armature
                    futurePose.ApplyTo(motionArmature.ArmatureMappingData, convertedPositionOffset, rotationOffset);

                    // Capture target frame
                    motionComponent.CaptureInputPose(futureFrameIdx, false);
                }
            }

            m_ProcessInfo.CurrentStep = SubStep.Interpolation;
        }

        void StepInterpolation()
        {
            var currentFrame = m_DestinationBakedTimeline.GetFrame(m_ProcessInfo.CurrentFrameIdx);
            FindInterpolationFrames(m_ProcessInfo.CurrentFrameIdx, out var prevFrame, out var nextFrame, out var interpolationT);

            // Perform linear interpolation
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                ArmatureMappingComponent targetArmature = null;
                switch (pair.Value.Type)
                {
                    case PhysicsEntityType.Kinematic:
                        targetArmature = pair.Value.PhysicsArmatures;
                        break;

                    case PhysicsEntityType.Dynamic:
                        // No interpolation for dynamically simulated entities
                        break;

                    case PhysicsEntityType.Active:
                    {
                        targetArmature = pair.Value.MotionArmature;
                        if (targetArmature == null)
                            break;

                        var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                        if (motionComponent != null)
                            targetArmature = null; // Skip interpolation if using motion synthesis

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Skip interpolation if not applicable
                if (targetArmature == null)
                    continue;

                if (!currentFrame.TryGetModel(entityID, out var currentPose))
                {
                    Debug.LogWarning($"Could not find pose for entity: {entityID} at frame: {m_ProcessInfo.CurrentFrameIdx}");
                    continue;
                }

                if (!prevFrame.TryGetModel(entityID, out var prevPose))
                {
                    Debug.LogWarning($"Could not find previous pose for entity: {entityID}");
                    continue;
                }

                if (!nextFrame.TryGetModel(entityID, out var nextPose))
                {
                    Debug.LogWarning($"Could not find next pose for entity: {entityID}");
                    continue;
                }

                // interpolate local poses
                currentPose.LocalPose.Interpolate(prevPose.LocalPose, nextPose.LocalPose, interpolationT);

                // Synchronize local & global poses
                currentPose.LocalPose.ApplyTo(targetArmature.ArmatureMappingData);
                currentPose.Capture(targetArmature.ArmatureMappingData);
            }

            m_ProcessInfo.CurrentStep = SubStep.InitializeSolver;
        }

        void StepInitializeSolvers()
        {
            // Add motion synthesis inputs
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                if (motionComponent == null)
                    continue;

                motionComponent.SetInputTolerance(m_Tolerance);

                // Run motion synthesis
                motionComponent.StartAsyncSolve(m_ProcessInfo.CurrentFrameIdx, 0);
            }

            m_ProcessInfo.CurrentStep = SubStep.RunSolver;
        }

        bool StepRunSolvers()
        {
            var allDone = true;

            // Add motion synthesis inputs
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                var motionComponent = m_MotionAutoRegressiveComponents[entityID];
                if (motionComponent == null)
                    continue;

                if (!motionComponent.IsRunningAsyncSolve)
                    continue;

                // Step solver
                if (motionComponent.StepAsyncSolve())
                {
                    // If done, apply result
                    motionComponent.Apply(0);
                }
                else
                {
                    allDone = false;
                }
            }

            // All solvers are done
            if (allDone)
                m_ProcessInfo.CurrentStep = SubStep.RunPhysics;

            return allDone;
        }

        void StepRunPhysics()
        {
            if (m_PhysicsMode == PhysicsMode.AutoRegressive)
            {
                var prevFrameIdx = m_ProcessInfo.CurrentFrameIdx - 1;
                var prevFrame = m_DestinationBakedTimeline.GetFrame(prevFrameIdx);

                // Restore the previous last know physics state of each entity
                foreach (var pair in m_EntityContexts)
                {
                    var entityID = pair.Key;
                    var physicsArmature = pair.Value.PhysicsArmatures;

                    if (!prevFrame.TryGetModel(pair.Key, out var prevPose))
                    {
                        Debug.LogWarning($"Could not find pose for entity: {entityID} at frame: {prevFrameIdx}");
                        continue;
                    }

                    switch (pair.Value.Type)
                    {
                        case PhysicsEntityType.Kinematic:
                            // Kinematic entities are not solved by physics, keep current pose
                            // TODO: translate & rotate kinematic entities inside the physics solve?
                            break;

                        case PhysicsEntityType.Dynamic:
                        case PhysicsEntityType.Active:
                            // Apply last valid frame to physics armature
                            prevPose.ApplyTo(physicsArmature.ArmatureMappingData);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Solve all entities together at once
                m_PhysicsSolverComponent.Solve();
            }

            m_ProcessInfo.CurrentStep = SubStep.CaptureOutputFrame;
        }

        void StepCaptureOutput()
        {
            var currentFrame = m_DestinationBakedTimeline.GetFrame(m_ProcessInfo.CurrentFrameIdx);

            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                ArmatureMappingComponent sourceArmature;
                switch (pair.Value.Type)
                {
                    case PhysicsEntityType.Kinematic:
                        sourceArmature = pair.Value.PhysicsArmatures;
                        break;

                    case PhysicsEntityType.Dynamic:
                        sourceArmature = pair.Value.PhysicsArmatures;
                        break;

                    case PhysicsEntityType.Active:
                        sourceArmature = m_PhysicsMode == PhysicsMode.AutoRegressive ? pair.Value.PhysicsArmatures : pair.Value.MotionArmature;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!currentFrame.TryGetModel(entityID, out var currentPose))
                {
                    Debug.LogWarning($"Could not find pose for entity: {entityID}");
                    continue;
                }

                // Capture solved physics pose in baked data
                currentPose.Capture(sourceArmature.ArmatureMappingData);
            }

            m_ProcessInfo.CurrentStep = SubStep.GoToNextFrame;
        }

        void FindInterpolationFrames(int frameIndex, out BakedFrameModel prevFrame, out BakedFrameModel nextFrame, out float interpolationT)
        {
            var foundPrevKey = m_DestinationMapping.TryGetFirstKeyBefore(frameIndex, out var prevFrameIndex, out _);
            if (!foundPrevKey)
                throw new Exception($"Could not find previous input index for frame: {frameIndex}");

            prevFrame = m_DestinationBakedTimeline.GetFrame(prevFrameIndex);

            // Get first key that contains a full pose
            var foundNextKey = m_DestinationMapping.TryGetFirstKeyAfter(frameIndex, out var nextFrameIndex, out var nextKeyTimelineIndex);
            while (foundNextKey && m_SourceTimeline.GetKey(nextKeyTimelineIndex).Key.Type != KeyData.KeyType.FullPose)
            {
                foundNextKey = m_DestinationMapping.TryGetFirstKeyAfter(nextFrameIndex + 1, out nextFrameIndex, out nextKeyTimelineIndex);
            }

            if (!foundNextKey)
            {
                // When extrapolating, there is no next frame, we keep the previous frame instead
                nextFrame = prevFrame;
                interpolationT = 0f;
            }
            else
            {
                nextFrame = m_DestinationBakedTimeline.GetFrame(nextFrameIndex);
                interpolationT = nextFrameIndex == prevFrameIndex ? 0f : (frameIndex - prevFrameIndex) / (float)(nextFrameIndex - prevFrameIndex);
            }
        }

        void StopAllSolvers()
        {
            foreach (var pair in m_MotionAutoRegressiveComponents)
            {
                var motionComponent = pair.Value;
                if (motionComponent == null)
                    continue;

                if (!motionComponent.IsRunningAsyncSolve)
                    continue;

                motionComponent.StopAsyncSolve();
            }
        }

        void GetMotionComponents(Dictionary<EntityID, EntityBakingContext> entityContexts)
        {
            m_MotionAutoRegressiveComponents.Clear();
            foreach (var pair in entityContexts)
            {
                var motionArmature = pair.Value.MotionArmature;
                var motionComponent = motionArmature == null || !ApplicationConstants.MotionSynthesisEnabled
                    ? null
                    : motionArmature.GetComponent<MotionAutoRegressiveComponent>();

                m_MotionAutoRegressiveComponents[pair.Key] = motionComponent;
            }
        }

        void InitializeMotionComponents()
        {
            foreach (var pair in m_MotionAutoRegressiveComponents)
            {
                var motionComponent = pair.Value;
                if (motionComponent == null)
                    continue;

                motionComponent.ClearInputFrames();
            }
        }
    }
}
