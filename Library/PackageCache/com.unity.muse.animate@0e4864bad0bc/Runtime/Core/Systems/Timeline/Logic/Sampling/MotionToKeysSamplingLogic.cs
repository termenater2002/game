
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class MotionToKeysSamplingLogic
    {
        public bool NeedToUpdate => IsRunning || m_Restart;
        public bool IsRunning => m_Runner.IsRunning;
        
        public bool IsWaitingDelay => m_Runner.IsWaitingDelay;
        public float SamplingProgress => IsRunning ? m_Runner.Progress : 1f;

        public delegate void Started(MotionToKeysSamplingLogic logic);
        public event Started OnStarted;

        public delegate void Canceled(MotionToKeysSamplingLogic logic);
        public event Canceled OnCanceled;

        public delegate void Completed(MotionToKeysSamplingLogic logic);
        public event Completed OnCompleted;

        public delegate void Progressed(MotionToKeysSamplingLogic logic, float overallProgress);
        public event Progressed OnProgressed;

        public delegate void Failed(MotionToKeysSamplingLogic logic, string error);
        public event Failed OnFailed;
        
        readonly MotionToKeysSamplingRequest m_SamplingRequest;
        
        bool m_Restart;
        RequestRunner m_Runner;
        RequestRunner m_SamplingRunner;
        List<int> m_KeyIndices;
        float m_Sensitivity = 0.5f;
        bool m_UseMotionCompletion;
        
        readonly BakedTimelineModel m_Source;
        readonly TimelineModel m_Target;
        readonly PoseAuthoringLogic m_PoseAuthoringLogic;
        readonly StageModel m_Stage;

        readonly Dictionary<EntityID, EntitySamplingContext> m_EntityContexts = new();

        public MotionToKeysSamplingLogic(StageModel stage, PoseAuthoringLogic poseAuthoringLogic, BakedTimelineModel source, TimelineModel target)
        {
            m_Source = source;
            m_Target = target;
            m_PoseAuthoringLogic = poseAuthoringLogic;
            m_Stage = stage;
            m_SamplingRequest = new MotionToKeysSamplingRequest();
            m_SamplingRequest.OnBakingFailed += OnRequestFailed;
            m_Runner = new RequestRunner(ApplicationConstants.BakingTimeBudget);
        }

        public void AddEntity(EntityID entityID, JointMask jointMask, ArmatureMappingComponent referenceMotionArmature)
        {
            if (!referenceMotionArmature.TryGetComponent<CloudMotionCompletionComponent>(out var motionComponent))
            {
                Debug.LogError("Missing CloudMotionCompletionComponent component");
                return;
            }
            m_EntityContexts.Add(entityID, new EntitySamplingContext
            {
                JointMask = jointMask,
                MotionComponent = motionComponent
            });
        }
        
        public void RemoveEntity(EntityID entityID)
        {
            m_EntityContexts.Remove(entityID);
        }
        
        public void Update(float delta, bool throttle)
        {
            if (m_Restart)
                Restart();

            if (m_Runner.IsRunning)
            {
                StepBaking(delta, throttle);
            }
        }

        public void ProcessFully(float delta)
        {
            while (IsRunning)
            {
                StepBaking(delta);
            }
        }

        public void Cancel()
        {
            if (!m_Runner.IsRunning)
                return;

            m_Runner.Stop();
            m_Restart = false;
            OnCanceled?.Invoke(this);
        }

        public void Restart()
        {
            Cancel();

            m_SamplingRequest.Initialize(m_Stage, m_PoseAuthoringLogic, m_Source, m_Target, m_EntityContexts, m_Sensitivity, m_UseMotionCompletion);
            m_Runner.Initialize(m_SamplingRequest);
            m_Runner.Start();

            m_Restart = false;

            OnProgressed?.Invoke(this, SamplingProgress);

            if (m_Runner.IsRunning)
            {
                OnStarted?.Invoke(this);
            }
            else
            {
                OnCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Queue Motion-to-Keys baking.
        /// </summary>
        /// <param name="sensitivity">A value of 0-1, where 0 yields the smallest number of keys, and 1 yields the most.</param>
        /// <param name="useMotionCompletionSampling">Use motion completion to assist in the key sampling. Can improve
        /// the results at the cost of additional processing time.</param>
        public void QueueBaking(float sensitivity, bool useMotionCompletionSampling)
        {
            m_Sensitivity = sensitivity;
            m_UseMotionCompletion = useMotionCompletionSampling;
            m_Restart = true;
        }

        void StepBaking(float delta, bool throttle = false)
        {
            m_Runner.TimeBudget = throttle ? ApplicationConstants.BakingTimeBudgetThrottled : ApplicationConstants.BakingTimeBudget;
            m_Runner.Step();
            OnProgressed?.Invoke(this, SamplingProgress);

            if (!m_Runner.IsRunning)
                OnCompleted?.Invoke(this);
        }

        void OnRequestFailed(MotionToKeysSamplingRequest request, string error)
        {
            OnFailed?.Invoke(this, error);
        }
        
        // [Section] Debugging
        
        void Log(string msg)
        {
            if (!ApplicationConstants.DebugMotionToKeysSampling)
                return;

            Debug.Log(GetType().Name + " -> " + msg);
        }
        
        
    }
}
