using System;
using System.Collections.Generic;
using Unity.DeepPose.Cloud;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;
using ProcessState = Unity.Muse.Animate.IProcessingRequest.ProcessState;

namespace Unity.Muse.Animate
{
    class TimelineBakerCloud : TimelineBakerBase
    {
        const float k_PostProcessPhysicsProgressRatio = 0.9f;

        public enum PhysicsMode
        {
            PostProcess,
            None
        }

        public PhysicsMode Physics
        {
            get => m_PhysicsMode;
            set => m_PhysicsMode = value;
        }

        public override ProcessState State => m_State;

        public override float WaitDelay => 2f;
        public override DateTime WaitStartTime { get; protected set; }

        public override float Progress => m_ProcessInfo.Progress;

        public bool IsRunning => State == ProcessState.InProgress;

        Dictionary<EntityID, EntityBakingContext> m_EntityContexts;
        Dictionary<EntityID, CloudMotionCompletionComponent> m_MotionComponents;
        ProcessState m_State;
        WebAPI m_MotionCompletionAPI;

        enum SubStep
        {
            Unknown,
            SendRequest,
            WaitResponse,
            PhysicsPostProcess
        }

        struct ProcessInfo
        {
            public float Progress;
            public SubStep CurrentStep;
        }

        ProcessInfo m_ProcessInfo;
        PhysicsMode m_PhysicsMode;

        TimelineModel m_SourceTimeline;
        BakedTimelineModel m_DestinationBakedTimeline;
        BakedTimelineMappingModel m_DestinationMapping;
        PhysicsSolverComponent m_PhysicsSolverComponent;

        MotionCompletionAPI.Request m_MotionCompletionRequest;
        HashSet<Guid> m_PendingRequests;

        List<BakedFrameModel> m_PhysicsFrames;
        PhysicsRequest m_PhysicsRequest;

        public TimelineBakerCloud(PhysicsSolverComponent physicsSolverComponent)
        {
            m_State = ProcessState.Unknown;
            m_ProcessInfo.Progress = 0f;
            m_PhysicsMode = PhysicsMode.PostProcess;

            m_MotionCompletionAPI = new WebAPI(WebUtils.BackendUrl, MotionCompletionAPI.ApiName);

            m_PhysicsFrames = new List<BakedFrameModel>();
            m_PendingRequests = new HashSet<Guid>();
            m_MotionCompletionRequest = new MotionCompletionAPI.Request();
            m_MotionComponents = new Dictionary<EntityID, CloudMotionCompletionComponent>();
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
            WaitStartTime = DateTime.Now;
        }

        public override void Start()
        {
            Assert.IsNotNull(m_SourceTimeline, "No source timeline. Did you Initialize first?");
            Assert.IsNotNull(m_DestinationBakedTimeline, "No destination baked timeline. Did you Initialize first?");

            Assert.IsTrue(m_State != ProcessState.InProgress, "Baking already running.");

            if (m_SourceTimeline.KeyCount == 0)
            {
                m_DestinationBakedTimeline.Clear();
                m_State = ProcessState.Done;
                m_ProcessInfo.Progress = 1f;
                return;
            }

            // Make sure timeline and baked timeline have the same entities
            BakingUtils.SyncTimelineAndBakedTimelineEntities(m_SourceTimeline, m_DestinationBakedTimeline);

            // Compute baked timeline duration
            var animationLength = m_SourceTimeline.ComputeAnimationLength();

            // Limit number of frames
            if (animationLength > ApplicationConstants.MaxCloudInferenceFrames)
            {
                TriggerFailEvent("Too many frames");
                Stop();
                return;
            }

            // Set baked timeline duration
            m_DestinationBakedTimeline.FramesCount = animationLength;

            // Get all keyed actors
            using var keyedEntityIDs = TempHashSet<EntityID>.Allocate();
            m_SourceTimeline.GetAllEntities(keyedEntityIDs.Set);
            m_SourceTimeline.UpdateBakingMapping(m_DestinationMapping);
            m_SourceTimeline.TransferKeysToBakedTimeline(keyedEntityIDs.Set, m_DestinationMapping, m_DestinationBakedTimeline);

            // If a single key, stop here and do nothing
            if (m_SourceTimeline.KeyCount == 1)
            {
                Stop();
                return;
            }

            SetupPhysicsEntities(m_EntityContexts);
            GetMotionComponents(m_EntityContexts);
            SetupRagdolls(m_EntityContexts, m_PhysicsSolverComponent);

            if (m_PhysicsMode == PhysicsMode.PostProcess)
                InitializePhysicsPostProcess();

            if (WaitDelay > 0f)
            {
                WaitStartTime = DateTime.Now;
                m_State = ProcessState.InWaitDelay;
            }
            else
            {
                m_State = ProcessState.InProgress;
            }

            m_ProcessInfo.Progress = 0f;
            m_ProcessInfo.CurrentStep = SubStep.Unknown;
            m_CanSkipToNextFrame = false;
        }

        public override void Stop()
        {
            m_PendingRequests.Clear();

            m_PhysicsRequest?.Stop();
            m_PhysicsRequest = null;

            m_State = ProcessState.Failed;
            m_ProcessInfo.Progress = 1f;
        }

        public override void Step()
        {
            if (m_State == ProcessState.InWaitDelay)
            {
                m_ProcessInfo.Progress = GetWaitProgress();
                var timeElapsed = (float)DateTime.Now.Subtract(WaitStartTime).TotalSeconds;

                // Check if done waiting
                if (timeElapsed >= WaitDelay)
                {
                    m_State = ProcessState.InProgress;
                    m_ProcessInfo.Progress = 0f;
                }
                return;
            }

            if (m_State != ProcessState.InProgress)
                return;
            
            switch (m_ProcessInfo.CurrentStep)
            {
                case SubStep.Unknown:
                    m_ProcessInfo.CurrentStep = SubStep.SendRequest;
                    break;

                case SubStep.SendRequest:
                    StepSendRequest();
                    break;

                case SubStep.WaitResponse:
                    StepWaitResponse();
                    break;

                case SubStep.PhysicsPostProcess:
                    StepPhysicsPostProcess();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        void StepSendRequest()
        {
            foreach (var pair in m_EntityContexts)
            {
                var entityID = pair.Key;

                var motionComponent = m_MotionComponents[entityID];
                if (motionComponent == null)
                {
                    Debug.LogError("Missing Motion Completion Component.");
                    continue;
                }

                var characterToMotionMapping = motionComponent.CharacterToMotionArmatureMapping;
                Assert.IsNotNull(characterToMotionMapping, "Missing character to motion mapping");

                // Initialize request
                m_MotionCompletionRequest.CharacterID = motionComponent.CloudCharacterID;
                m_MotionCompletionRequest.Keys.Clear();

                // FIll request
                for (var keyIndex = 0; keyIndex < m_SourceTimeline.KeyCount; keyIndex++)
                {
                    var timelineKey = m_SourceTimeline.GetKey(keyIndex);
                    if (!timelineKey.Key.TryGetKey(entityID, out var entityKey))
                    {
                        Debug.LogError($"Cannot find key for entity: {entityID}");
                        continue;
                    }

                    if (!m_DestinationMapping.TryGetBakedKeyIndex(keyIndex, out var bakedFrameIndex))
                    {
                        Debug.LogError($"Cannot find baked frame index for key {keyIndex}");
                        continue;
                    }

                    // TODO: pool to avoid mallocs ?
                    var requestKey = new MotionCompletionAPI.Key(1, entityKey.NumJoints);
                    requestKey.Index = bakedFrameIndex;

                    // Set key type
                    requestKey.Type = timelineKey.Key.Type switch
                    {
                        KeyData.KeyType.Empty => MotionCompletionAPI.Key.KeyType.Empty,
                        KeyData.KeyType.FullPose => MotionCompletionAPI.Key.KeyType.FullPose,
                        KeyData.KeyType.Loop => MotionCompletionAPI.Key.KeyType.Loop,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    switch (timelineKey.Key.Type)
                    {
                        case KeyData.KeyType.Empty:
                        {
                            break;
                        }

                        case KeyData.KeyType.FullPose:
                        {
                            var rootJointPosition = entityKey.LocalPose.GetPosition(0);
                            requestKey.Positions[0] = rootJointPosition;
                            entityKey.LocalPose.CopyTo(requestKey.Rotations, characterToMotionMapping);
                            break;
                        }

                        case KeyData.KeyType.Loop:
                        {
                            var loopKey = timelineKey.Key.Loop;
                            if (loopKey.TryGetOffset(entityID, out var loopOffset))
                            {
                                requestKey.Loop.NumLoopbacks = loopKey.NumBakingLoopbacks;
                                requestKey.Loop.TargetFrame = loopKey.StartFrame;
                                requestKey.Loop.Translation = loopOffset.Position;
                                requestKey.Loop.Rotation = loopOffset.Rotation;
                            }
                            else
                            {
                                Debug.LogError($"Cannot find loop for entity: {entityID}");
                            }
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Add key to request
                    m_MotionCompletionRequest.Keys.Add(requestKey);
                }

                // Send request
                var requestId = Guid.NewGuid();
                m_PendingRequests.Add(requestId);
                m_MotionCompletionAPI.SendJobRequestWithAuthHeaders<MotionCompletionAPI.Request, MotionCompletionAPI.Response>(
                    m_MotionCompletionRequest,
                    response =>
                    {
                        OnRequestSuccess(requestId, entityID, response);
                        response?.Dispose();
                    },
                    error => OnRequestFail(requestId, error)
                    );

                Common.Model.SendAnalytics(new MotionCompletionAnalytic());
            }

            m_ProcessInfo.CurrentStep = SubStep.WaitResponse;
            m_CanSkipToNextFrame = true;
        }

        void OnRequestSuccess(Guid requestId, EntityID entityID, MotionCompletionAPI.Response response)
        {
            if (!m_PendingRequests.Contains(requestId))
                return;

            if (response == null || response.Frames == null)
            {
                OnRequestFail(requestId, "Invalid response");
                return;
            }

            // Check frames count
            if (response.Frames.Count != m_DestinationBakedTimeline.FramesCount)
            {
                OnRequestFail(requestId, $"Expected {m_DestinationBakedTimeline.FramesCount} frames but got {response.Frames.Count}");
                return;
            }

            if (!m_EntityContexts.TryGetValue(entityID, out var entityContext))
            {
                OnRequestFail(requestId, $"Cannot get entity context");
                return;
            }

            var motionComponent = m_MotionComponents[entityID];
            if (motionComponent == null)
            {
                Debug.LogError("Missing Motion Completion Component.");
                response.Dispose();
                return;
            }

            var characterToMotionMapping = motionComponent.CharacterToMotionArmatureMapping;
            Assert.IsNotNull(characterToMotionMapping, "Missing character to motion mapping");

            for (var frameIdx = 0; frameIdx < m_DestinationBakedTimeline.FramesCount; frameIdx++)
            {
                var armature = entityContext.MotionArmature;
                var responseFrame = response.Frames[frameIdx];
                var timelineFrame = m_DestinationBakedTimeline.GetFrame(frameIdx);

                if (!timelineFrame.TryGetModel(entityID, out var entityPose))
                {
                    OnRequestFail(requestId, $"Could not get frame {frameIdx} for entity {entityID}");
                    return;
                }

                if (responseFrame.Positions.Length != 1)
                {
                    OnRequestFail(requestId, $"Expected {1} positions but got {responseFrame.Positions.Length} for frame {frameIdx}");
                    return;
                }

                if (responseFrame.Rotations.Length != entityPose.NumJoints)
                {
                    OnRequestFail(requestId, $"Expected {entityPose.NumJoints} rotations but got {responseFrame.Rotations.Length} for frame {frameIdx}");
                    return;
                }

                // Capture initial pose to get local position of all joints
                entityPose.LocalPose.Capture(armature.ArmatureMappingData);

                // Read frame data
                var rootJointPosition = responseFrame.Positions[0];
                entityPose.LocalPose.SetPosition(0, rootJointPosition);
                entityPose.LocalPose.CopyFrom(responseFrame.Rotations, characterToMotionMapping, inverseMapping: true);

                // Apply locally and update global pose
                entityPose.LocalPose.ApplyTo(armature.ArmatureMappingData);
                entityPose.GlobalPose.Capture(armature.ArmatureMappingData);
            }

            m_PendingRequests.Remove(requestId);
        }

        void OnRequestFail(Guid requestId, string error)
        {
            if (!m_PendingRequests.Contains(requestId))
                return;

            m_PendingRequests.Remove(requestId);
            DevLogger.LogError(error);
            Stop();
            TriggerFailEvent(error);
        }
        
        void OnRequestFail(Guid requestId, Exception error)
        {
            OnRequestFail(requestId, error.Message);
            
            if (error is ApiVersionMismatchException)
            {
                // There's a problem with the package version, we need
                // to propagate this outside the application.
                DevLogger.LogError("API version mismatch. Please update the package.");
                Application.Instance.PublishMessage(new VersionErrorMessage());
            }
        }
        
        void StepWaitResponse()
        {
            // Wait until all requests have been processed
            if (m_PendingRequests.Count != 0)
                return;

            if (m_PhysicsMode == PhysicsMode.PostProcess)
            {
                m_ProcessInfo.CurrentStep = SubStep.PhysicsPostProcess;
                m_ProcessInfo.Progress = k_PostProcessPhysicsProgressRatio;
                m_CanSkipToNextFrame = false;
            }
            else
            {
                m_State = ProcessState.Done;
                m_ProcessInfo.Progress = 1f;
            }
        }

        void StepPhysicsPostProcess()
        {
            m_PhysicsRequest.Step();

            if (m_PhysicsRequest.State is ProcessState.InProgress or ProcessState.InWaitDelay)
            {
                m_ProcessInfo.Progress = k_PostProcessPhysicsProgressRatio + (1f - k_PostProcessPhysicsProgressRatio) * m_PhysicsRequest.Progress;
            }
            else
            {
                m_State = ProcessState.Done;
                m_ProcessInfo.Progress = 1f;
            }
        }

        void GetMotionComponents(Dictionary<EntityID, EntityBakingContext> entityContexts)
        {
            m_MotionComponents.Clear();
            foreach (var pair in entityContexts)
            {
                var motionArmature = pair.Value.MotionArmature;
                var motionComponent = motionArmature == null ? null : motionArmature.GetComponent<CloudMotionCompletionComponent>();
                m_MotionComponents[pair.Key] = motionComponent;
            }
        }

        void InitializePhysicsPostProcess()
        {
            m_PhysicsFrames.Clear();
            m_DestinationBakedTimeline.GetFrames(0, m_DestinationBakedTimeline.FramesCount - 1, m_PhysicsFrames);
            m_PhysicsRequest = new PhysicsRequest(m_EntityContexts, m_PhysicsFrames[0], m_PhysicsFrames, m_PhysicsSolverComponent);
            m_PhysicsRequest.Start();
        }
    }
}
