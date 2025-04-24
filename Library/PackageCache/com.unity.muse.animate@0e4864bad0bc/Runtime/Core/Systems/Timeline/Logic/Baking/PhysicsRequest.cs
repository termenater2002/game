using System;
using System.Collections.Generic;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class PhysicsRequest : IProcessingRequest
    {
        public IProcessingRequest.ProcessState State => m_State;
        public float Progress => m_Progress;
        public float WaitDelay => 0f;
        public DateTime WaitStartTime => DateTime.Now;
        
        Dictionary<EntityID, EntityBakingContext> m_EntityContexts;
        PhysicsSolverComponent m_PhysicsSolverComponent;
        List<BakedFrameModel> m_FramesToBake;
        BakedFrameModel m_InitialFrame;

        IProcessingRequest.ProcessState m_State;
        float m_Progress;
        int m_InternalStep;

        public PhysicsRequest(Dictionary<EntityID, EntityBakingContext> entityContexts, BakedFrameModel initialFrame,
            List<BakedFrameModel> framesToBake, PhysicsSolverComponent physicsSolverComponent)
        {
            m_State = IProcessingRequest.ProcessState.Unknown;
            m_Progress = 0f;
            m_EntityContexts = entityContexts;
            m_PhysicsSolverComponent = physicsSolverComponent;
            m_FramesToBake = framesToBake;
            m_InitialFrame = initialFrame;
        }

        public void Start()
        {
            Assert.IsTrue( m_State == IProcessingRequest.ProcessState.Unknown, "Request already started");

            if (m_FramesToBake.Count == 0)
            {
                m_State = IProcessingRequest.ProcessState.Done;
                m_Progress = 1f;
                return;
            }

            m_PhysicsSolverComponent.RemoveAllRagdolls();
            
            foreach (var pair in m_EntityContexts)
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
                        
                        m_PhysicsSolverComponent.AddRagdoll(ragdollSettings);

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            EnableAllRagdolls();
            
            m_State = IProcessingRequest.ProcessState.InProgress;
            m_InternalStep = 0;
        }

        public void Stop()
        {
            if (State != IProcessingRequest.ProcessState.InProgress)
                return;
            
            RemoveAllRagdolls();
            m_State = IProcessingRequest.ProcessState.Failed;
        }
        
        void EnableAllRagdolls()
        {
            foreach (var ragDoll in m_PhysicsSolverComponent.Ragdolls)
            {
                ragDoll.PhysicsRoot.gameObject.SetActive(true);
            }
        }
        
        void RemoveAllRagdolls()
        {
            foreach (var ragDoll in m_PhysicsSolverComponent.Ragdolls)
            {
                ragDoll.PhysicsRoot.gameObject.SetActive(false);
            }
            
            m_PhysicsSolverComponent.RemoveAllRagdolls();
        }

        public void Step()
        {
            // We need last valid frame to initialize physics state
            var prevFrame = m_InternalStep > 0 ? m_FramesToBake[m_InternalStep - 1] : m_InitialFrame;

            var frameToBake = m_FramesToBake[m_InternalStep];

            // Set initial state for all entities
            foreach (var pair in m_EntityContexts)
            {
                if (!prevFrame.TryGetModel(pair.Key, out var initialPose))
                {
                    Debug.LogWarning($"Could not find previous pose for entity: {pair.Key}");
                    continue;
                }

                // Apply last valid frame to physics armature
                initialPose.ApplyTo(pair.Value.PhysicsArmatures.ArmatureMappingData);

                switch (pair.Value.Type)
                {
                    case PhysicsEntityType.Kinematic:
                    {
                        if (!frameToBake.TryGetModel(pair.Key, out var targetPose))
                            throw new ArgumentOutOfRangeException($"Could not find pose for entity: {pair.Key}");

                        // Apply target pose to physics armature
                        targetPose.ApplyTo(pair.Value.PhysicsArmatures.ArmatureMappingData);
                        break;
                    }

                    case PhysicsEntityType.Dynamic:
                        break;

                    case PhysicsEntityType.Active:
                    {
                        if (!frameToBake.TryGetModel(pair.Key, out var targetPose))
                            throw new ArgumentOutOfRangeException($"Could not find pose for entity: {pair.Key}");

                        // Apply target pose to target armature
                        targetPose.ApplyTo(pair.Value.MotionArmature.ArmatureMappingData);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Solve all entities at once
            m_PhysicsSolverComponent.Solve();

            // Capture all entities
            foreach (var pair in m_EntityContexts)
            {
                if (!frameToBake.TryGetModel(pair.Key, out var pose))
                {
                    Debug.LogWarning($"Could not find pose for entity: {pair.Key}");
                    continue;
                }

                // Capture solved physics pose
                pose.Capture(pair.Value.PhysicsArmatures.ArmatureMappingData);
            }

            m_Progress += 1f / m_FramesToBake.Count;

            m_InternalStep++;
            if (m_InternalStep >= m_FramesToBake.Count)
            {
                RemoveAllRagdolls();
                m_State = IProcessingRequest.ProcessState.Done;
                m_Progress = 1f;
            }
        }

        public bool CanSkipToNextFrame => false;
    }
}
