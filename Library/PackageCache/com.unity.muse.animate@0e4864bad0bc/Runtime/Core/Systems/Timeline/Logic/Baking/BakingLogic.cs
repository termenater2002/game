using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class BakingLogic
    {
        internal struct BakingEventData
        {
            public enum BakingEventType
            {
                Started,
                Canceled,
                Completed,
                Progressed,
                Failed
            }

            public BakingEventType Type;
            public string Message;
            public BakingLogic Logic;
            public TimelineModel TimelineModel;
            public BakedTimelineModel BakedTimelineModel;
            public BakedTimelineMappingModel BakedTimelineMappingModel;
            public float Progress;
        }

        const float k_BakingThrottle = 0.1f;
        public bool NeedToUpdate => IsRunning || m_RebakeAnimation;
        public bool IsRunning => m_Runner.IsRunning;
        public bool IsWaitingDelay => m_Runner.IsWaitingDelay;
        public float BakingProgress => IsRunning ? m_Runner.Progress : 1f;

        public event Action<BakingEventData> OnBakingStarted;
        public event Action<BakingEventData> OnBakingCanceled;
        public event Action<BakingEventData> OnBakingCompleted;
        public event Action<BakingEventData> OnBakingProgressed;
        public event Action<BakingEventData> OnBakingFailed;

        TimelineModel m_TimelineModel;
        BakedTimelineModel m_BakedTimelineModel;
        BakedTimelineMappingModel m_BakedTimelineMappingModel;

        RequestRunner m_Runner;
        TimelineBakerBase m_TimelineBaker;
        Dictionary<EntityID, EntityBakingContext> m_EntityBakingContexts;
        bool m_RebakeAnimation;

        public BakingLogic(TimelineModel timeline, BakedTimelineModel bakedTimeline, BakedTimelineMappingModel mappingModel,
            TimelineBakerBase timelineBaker)
        {
            m_TimelineModel = timeline;
            m_BakedTimelineModel = bakedTimeline;
            m_BakedTimelineMappingModel = mappingModel;

            m_EntityBakingContexts = new Dictionary<EntityID, EntityBakingContext>();

            m_TimelineBaker = timelineBaker;
            m_TimelineBaker.OnBakingFailed += OnTimelineBakerFailed;

            m_Runner = new(ApplicationConstants.BakingTimeBudget);
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referencePhysicsArmature, ArmatureMappingComponent referenceMotionArmature,
            PhysicsEntityType physicsEntityType)
        {
            if (m_EntityBakingContexts.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is already registered: {entityID}");

            if (physicsEntityType == PhysicsEntityType.Active)
                Assert.IsNotNull(referenceMotionArmature, "Active entities must have a motion armature");

            var motionArmature = referenceMotionArmature != null ? referenceMotionArmature.Clone() : null;

            if (motionArmature != null)
            {
                motionArmature.name = "Motion (" + GetType().Name + ")";
            }

            var physicsArmature = referencePhysicsArmature != null ? referencePhysicsArmature.Clone(ApplicationLayers.LayerBaking) : null;

            if (physicsArmature != null)
            {
                physicsArmature.name = "Physics (" + GetType().Name + ")";

                // Note: Disable physics armature root to prevent fighting between multiple solvers
                physicsArmature.RootTransform.gameObject.SetActive(false);
            }

            var bakingContext = new EntityBakingContext
            {
                Type = physicsEntityType,
                MotionArmature = motionArmature,
                PhysicsArmatures = physicsArmature
            };

            m_EntityBakingContexts[entityID] = bakingContext;
        }

        public void RemoveEntity(EntityID entityID)
        {
            if (!m_EntityBakingContexts.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is not registered: {entityID}");

            var bakingContext = m_EntityBakingContexts[entityID];

            if (bakingContext.MotionArmature != null)
                GameObjectUtils.Destroy(bakingContext.MotionArmature.gameObject);

            if (bakingContext.PhysicsArmatures != null)
                GameObjectUtils.Destroy(bakingContext.PhysicsArmatures.gameObject);

            m_EntityBakingContexts.Remove(entityID);
        }

        public void Update(float delta, bool throttle)
        {
            if (m_RebakeAnimation)
                Start();

            if (m_Runner.IsRunning)
            {
                Step(delta, throttle);
            }
        }

        public void BakeFully(float delta)
        {
            while (IsRunning)
            {
                Step(delta);
            }
        }

        public void Cancel(string message = "")
        {
            if (m_Runner.IsRunning)
            {
                m_Runner.Stop();
            }

            m_RebakeAnimation = false;
            OnBakingCanceled?.Invoke(GetEventData(BakingEventData.BakingEventType.Canceled, message));
        }

        internal BakingEventData GetEventData(BakingEventData.BakingEventType type, string message = "")
        {
            return new BakingEventData
            {
                Type = type,
                Message = message,
                Logic = this,
                Progress = BakingProgress,
                TimelineModel = m_TimelineModel,
                BakedTimelineModel = m_BakedTimelineModel,
                BakedTimelineMappingModel = m_BakedTimelineMappingModel
            };
        }

        void Start()
        {
            if (m_Runner.IsRunning)
            {
                Cancel();
            }

            m_RebakeAnimation = false;

            m_TimelineBaker.Initialize(m_TimelineModel, m_BakedTimelineModel, m_BakedTimelineMappingModel, m_EntityBakingContexts);
            m_Runner.Initialize(m_TimelineBaker);
            m_Runner.Start();

            OnBakingStarted?.Invoke(GetEventData(BakingEventData.BakingEventType.Started));
            OnBakingProgressed?.Invoke(GetEventData(BakingEventData.BakingEventType.Progressed));

            if (!m_Runner.IsRunning)
            {
                OnBakingCompleted?.Invoke(GetEventData(BakingEventData.BakingEventType.Completed));
            }
        }

        public void QueueBaking(bool forceUpdate)
        {
            m_RebakeAnimation = true;

            if (forceUpdate)
            {
                // Note: A bake step has to be performed here, to update the Context.BakedTimelineMapping
                Update(0f, false);
            }
        }

        void Step(float delta, bool throttle = false)
        {
            m_Runner.TimeBudget = throttle ? ApplicationConstants.BakingTimeBudgetThrottled : ApplicationConstants.BakingTimeBudget;
            m_Runner.Step();

            OnBakingProgressed?.Invoke(GetEventData(BakingEventData.BakingEventType.Progressed));

            if (!m_Runner.IsRunning)
                OnBakingCompleted?.Invoke(GetEventData(BakingEventData.BakingEventType.Completed));
        }

        void OnTimelineBakerFailed(TimelineBakerBase baker, string error)
        {
            OnBakingFailed?.Invoke(GetEventData(BakingEventData.BakingEventType.Failed, error));
        }
    }
}
