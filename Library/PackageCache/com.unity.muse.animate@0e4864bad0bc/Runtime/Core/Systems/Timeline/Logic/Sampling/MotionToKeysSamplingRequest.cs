using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class MotionToKeysSamplingRequest : IProcessingRequest
    {
        BakedTimelineModel m_Source;
        TimelineModel m_Target;
        readonly List<int> m_KeysIndices = new();
        PoseAuthoringLogic m_PoseAuthoringLogic;
        List<TimelineModel.SequenceKey> m_SampledKeys;
        StageModel m_Stage;
        int m_KeyIndex;
        bool m_PreviouslyUsingPhysics;
        Phase m_Phase;
        float m_Sensitivity;
        bool m_UseMotionCompletion;

        Dictionary<EntityID, EntitySamplingContext> m_EntityContexts;

        Task m_IndexSamplingTask;
        CancellationTokenSource m_CancellationTokenSource;

        public event Action<MotionToKeysSamplingRequest, string> OnBakingFailed;

        public IProcessingRequest.ProcessState State
        {
            get
            {
                return m_Phase switch
                {
                    Phase.SampleKeyIndices => IProcessingRequest.ProcessState.InProgress,
                    Phase.CreateKeys => IProcessingRequest.ProcessState.InProgress,
                    Phase.SetKeysDuration => IProcessingRequest.ProcessState.InProgress,
                    Phase.Complete => IProcessingRequest.ProcessState.Done,
                    Phase.Failed => IProcessingRequest.ProcessState.Failed,
                    Phase.Unknown => IProcessingRequest.ProcessState.Unknown,
                    _ => IProcessingRequest.ProcessState.Unknown
                };
            }
        }

        public float Progress
        {
            get
            {
                switch (m_Phase)
                {
                    case Phase.SampleKeyIndices:
                        return 0;
                    case Phase.CreateKeys:
                        return m_KeyIndex / (m_KeysIndices.Count / 2f);
                    case Phase.SetKeysDuration:
                        return 0.5f + m_KeyIndex / (m_KeysIndices.Count / 2f);
                    case Phase.Complete:
                        return 1f;
                    default:
                        return 0;
                }
            }
        }

        public float WaitDelay => 0f;
        public DateTime WaitStartTime => DateTime.Now;

        enum Phase
        {
            Unknown,
            SampleKeyIndices,
            CreateKeys,
            SetKeysDuration,
            Complete,
            Failed
        }

        public void Initialize(StageModel stage,
            PoseAuthoringLogic poseAuthoringLogic,
            BakedTimelineModel source,
            TimelineModel target,
            Dictionary<EntityID, EntitySamplingContext> entityData,
            float sensitivity,
            bool useMotionCompletion)
        {
            m_Stage = stage;
            m_Source = source;
            m_Target = target;
            m_PoseAuthoringLogic = poseAuthoringLogic;
            m_SampledKeys = new List<TimelineModel.SequenceKey>();
            m_UseMotionCompletion = useMotionCompletion;
            m_Sensitivity = sensitivity;
            m_Phase = Phase.Unknown;
            m_EntityContexts = entityData;
        }

        public void Start()
        {
            m_Target.Clear();
            m_SampledKeys.Clear();

            m_KeyIndex = 0;
            m_Phase = Phase.SampleKeyIndices;

            // TODO: Figure out why using physics produces poor results
            m_PreviouslyUsingPhysics = m_PoseAuthoringLogic.UsePhysics;
            m_PoseAuthoringLogic.UsePhysics = false;
        }

        public void Stop()
        {
            if (m_IndexSamplingTask is { IsCompleted: false })
            {
                m_CancellationTokenSource.Cancel();
            }

            // Resume the UsePhysics to the value it was
            // before it was used by this processing request
            m_PoseAuthoringLogic.UsePhysics = m_PreviouslyUsingPhysics;

            // TODO: Find out if/why this is really needed
            m_PoseAuthoringLogic.SolvePhysicsFully();

            m_CancellationTokenSource.Dispose();
            m_CancellationTokenSource = null;
            
            m_Phase = Phase.Failed;
        }

        void ResumePosingPhysics()
        {
            // Resume the UsePhysics to the value it was
            // before it was used by this processing request
            m_PoseAuthoringLogic.UsePhysics = m_PreviouslyUsingPhysics;

            // TODO: Find out if/why this is really needed
            m_PoseAuthoringLogic.SolvePhysicsFully();
        }

        public void Step()
        {
            if (m_Phase == Phase.SampleKeyIndices)
            {
                StepSampleIndices();
            }
            else if (m_Phase == Phase.CreateKeys)
            {
                StepCreateKey();
            }
            else
            {
                StepSetKeyDuration();
            }
        }

        public bool CanSkipToNextFrame => true;

        void StepSampleIndices()
        {
            switch (m_IndexSamplingTask)
            {
                case { IsCompleted: false }:
                    return;
                case { IsFaulted: true }:
                    m_Phase = Phase.Failed;
                    m_IndexSamplingTask = null;
                    OnBakingFailed?.Invoke(this, "Failed to sample key indices");
                    return;
                case { IsCompleted: true }:
                    m_Phase = Phase.CreateKeys;
                    m_IndexSamplingTask = null;
                    return;
            }

            StartSampleIndices();
        }

        void StartSampleIndicesWithMotionCompletion()
        {
            using var entities = TempHashSet<EntityID>.Allocate();
            m_Source.GetAllEntities(entities.Set);
            
            m_KeysIndices.Clear();

            if (entities.Count == 0)
            {
                m_IndexSamplingTask = Task.CompletedTask;
                return;
            }

            using var tasks = TempList<Task>.Allocate();

            var indicesOut = new ConcurrentBag<int>();
            foreach (var (entityID, entityData) in m_EntityContexts)
            {
                var jointMask = entityData.JointMask;
                m_CancellationTokenSource?.Dispose();
                m_CancellationTokenSource = new CancellationTokenSource();

                var motionCompletionInterpolator = new MotionCompletionCloudInterpolator(m_Source.FramesCount,
                    entityData.MotionComponent.CharacterToMotionArmatureMapping);

                tasks.Add(m_Source.ExtractKeyIndicesFromBakedTimelineAsync(entityID,
                    motionCompletionInterpolator.InterpolateAsync,
                    (pose1, pose2) => PoseMetrics.GetPoseErrorMetric(pose1, pose2, jointMask, 15, 75, 200),
                    1f - m_Sensitivity,
                    indicesOut,
                    m_CancellationTokenSource.Token));
            }

            // We MUST NOT await m_IndexSamplingTask because we're on the main thread.
            // See comment in MotionCompletionCloudInterpolator.InterpolateAsync
            m_IndexSamplingTask = Task.WhenAll(tasks.List).ContinueWith(_ =>
            {
                m_KeysIndices.AddRange(indicesOut);
                m_KeysIndices.Sort();
            });
        }

        void SampleIndicesWithLerp()
        {
            using var entities = TempHashSet<EntityID>.Allocate();
            m_Source.GetAllEntities(entities.Set);
            m_KeysIndices.Clear();

            if (entities.Count == 0)
                return;

            void InterpolationFunc(int index, ArmatureStaticPoseModel startPose,
                int startIndex, ArmatureStaticPoseModel endPose, int endIndex, 
                ArmatureStaticPoseModel interpolatedOut)
            {
                interpolatedOut.Interpolate(startPose,
                    endPose, Mathf.InverseLerp(startIndex, endIndex, index));
            }

            foreach (var (entityID, entityData) in m_EntityContexts)
            {
                m_Source.ExtractKeyIndicesFromBakedTimeline(entityID,
                    InterpolationFunc,
                    (pose1, pose2) => PoseMetrics.GetPoseErrorMetric(pose1, pose2, entityData.JointMask, 15, 75, 200),
                    1f - m_Sensitivity,
                    m_KeysIndices);
            }
            m_KeysIndices.Sort();
            m_IndexSamplingTask = Task.CompletedTask;
        }

        void StartSampleIndices()
        {
            if (m_UseMotionCompletion)
            {
                StartSampleIndicesWithMotionCompletion();
            }
            else
            {
                SampleIndicesWithLerp();
            }
        }

        void StepCreateKey()
        {
            // Create a key
            var key = m_Target.AddKey(false);
            m_PoseAuthoringLogic.PoseFromBakedTimeline(m_Stage, m_Source, m_KeysIndices[m_KeyIndex]);
            m_PoseAuthoringLogic.ApplyPosingStateToKey(key.Key);
            m_SampledKeys.Add(key);

            m_KeyIndex++;

            if (m_KeyIndex >= m_KeysIndices.Count)
            {
                StepCreateKeyCompleted();
            }
        }

        void StepCreateKeyCompleted()
        {
            m_Phase = Phase.SetKeysDuration;
            m_KeyIndex = 0;

            ResumePosingPhysics();
        }

        void StepSetKeyDuration()
        {
            if (m_SampledKeys[m_KeyIndex].OutTransition?.Transition is { } outTransition)
            {
                outTransition.Duration = GetKeyDuration(m_KeyIndex);
            }

            m_KeyIndex++;

            if (m_KeyIndex >= m_KeysIndices.Count)
            {
                m_Phase = Phase.Complete;
            }
        }

        public int GetKeyDuration(int index)
        {
            if (index + 1 >= m_KeysIndices.Count || index < 0)
            {
                return 0;
            }

            return m_KeysIndices[index + 1] - m_KeysIndices[index];
        }
    }
}
