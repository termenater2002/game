using System;
using System.Collections.Generic;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    abstract class TimelineBakerBase : IProcessingRequest
    {
        protected bool m_CanSkipToNextFrame;
        public event Action<TimelineBakerBase, string> OnBakingFailed;
        public abstract void Initialize(TimelineModel sourceTimeline,
            BakedTimelineModel destinationBakedTimeline,
            BakedTimelineMappingModel destinationMapping,
            Dictionary<EntityID, EntityBakingContext> entityBakingContexts);
        
        public abstract IProcessingRequest.ProcessState State { get; }
        public abstract float Progress { get; }
        public abstract float WaitDelay { get; }
        public abstract DateTime WaitStartTime { get; protected set; }

        public abstract void Start();
        public abstract void Stop();
        public abstract void Step();

        bool IProcessingRequest.CanSkipToNextFrame => m_CanSkipToNextFrame;

        protected float GetWaitProgress()
        {
            if (WaitDelay <= 0f)
                return 1f;
            
            var timeElapsed = (float)DateTime.Now.Subtract(WaitStartTime).TotalSeconds;

            if (timeElapsed >= WaitDelay)
                return 0f;
            
            return 1f - Mathf.Min(1f, timeElapsed / WaitDelay);
        }

        protected void TriggerFailEvent(string error)
        {
            OnBakingFailed?.Invoke(this, error);
        }

        protected static void SetupPhysicsEntities(Dictionary<EntityID, EntityBakingContext> entityContexts)
        {
            foreach (var pair in entityContexts)
            {
                switch (pair.Value.Type)
                {
                    case PhysicsEntityType.Kinematic:
                    {
                        // TODO: use some physics armature instead, and move this in baking request initialization
                        var physicsGameObject = pair.Value.PhysicsArmatures.gameObject;
                        physicsGameObject.InitializeRigidBodies(true, true);
                        break;
                    }

                    case PhysicsEntityType.Dynamic:
                    case PhysicsEntityType.Active:
                    {
                        // TODO: use some physics armature instead, and move this in baking request initialization
                        var physicsGameObject = pair.Value.PhysicsArmatures.gameObject;
                        physicsGameObject.InitializeRigidBodies(false, true);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected static void SetupRagdolls(Dictionary<EntityID, EntityBakingContext> entityContexts, PhysicsSolverComponent physicsSolverComponent)
        {
            physicsSolverComponent.RemoveAllRagdolls();
            foreach (var pair in entityContexts)
            {
                switch (pair.Value.Type)
                {
                    case PhysicsEntityType.Kinematic:
                        break;

                    case PhysicsEntityType.Dynamic:
                        break;

                    case PhysicsEntityType.Active:
                    {
                        Assert.IsNotNull(pair.Value.MotionArmature, "No motion armature provided");
                        Assert.IsNotNull(pair.Value.PhysicsArmatures, "No physics armature provided");

                        var ragdollSettings = new PhysicsSolver.RagdollSettings
                        {
                            TargetRoot = pair.Value.MotionArmature.RootTransform,
                            PhysicsRoot = pair.Value.PhysicsArmatures.RootTransform
                        };
                        physicsSolverComponent.AddRagdoll(ragdollSettings);

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
