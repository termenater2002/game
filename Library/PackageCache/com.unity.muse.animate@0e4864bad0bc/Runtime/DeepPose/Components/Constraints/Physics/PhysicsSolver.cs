using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    class PhysicsSolver
    {
        public static Action<float> OnSimulate;
        
        [Serializable]
        public struct SolverSettings
        {
            public float FixedDeltaTime;
            public float DeltaTimeFactor;
            public int IterationCount;
            public bool CumulativeForces;

            public static SolverSettings Default = new SolverSettings(1e-4f, 20, false);

            public SolverSettings(float fixedDeltaTime, int iterationCount, bool cumulativeForces = false,
                float deltaTimeFactor = 1f)

            {
                FixedDeltaTime = fixedDeltaTime;
                DeltaTimeFactor = deltaTimeFactor;
                IterationCount = iterationCount;
                CumulativeForces = cumulativeForces;
            }
        }

        [Serializable]
        public struct RagdollSettings
        {
            public Transform PhysicsRoot;
            public Transform TargetRoot;
        }

        struct PhysicsState
        {
            public SimulationMode SimulationMode;
            public float FixedDeltaTime;

            public void Capture()
            {
                SimulationMode = Physics.simulationMode;
                FixedDeltaTime = Time.fixedDeltaTime;
            }

            public void Restore()
            {
                Physics.simulationMode = SimulationMode;
                Time.fixedDeltaTime = FixedDeltaTime;
            }
        }

        SolverSettings m_SolverSettings = SolverSettings.Default;
        ActiveRagdoll.SimulationSettings m_SimulationSettings = ActiveRagdoll.SimulationSettings.Default;
        ActiveRagdoll.RigidBodySettings m_RigidBodySettings = ActiveRagdoll.RigidBodySettings.Default;

        List<ActiveRagdoll> m_Ragdolls = new();
        List<ActiveRagdoll> m_RagdollsToRemove = new();
        PhysicsState m_PhysicsState;

        bool m_DebugLogs = false;

        public SolverSettings SolverConfig
        {
            get => m_SolverSettings;
            set => m_SolverSettings = value;
        }

        public ActiveRagdoll.SimulationSettings SimulationConfig
        {
            get => m_SimulationSettings;
            set
            {
                m_SimulationSettings = value;
                foreach (var ragdoll in m_Ragdolls)
                {
                    ragdoll.SimulationConfig = m_SimulationSettings;
                }
            }
        }

        public ActiveRagdoll.RigidBodySettings RigidBodyConfig
        {
            get => m_RigidBodySettings;
            set => m_RigidBodySettings = value;
        }

        public PhysicsSolver()
        { }

        public void Initialize(IList<RagdollSettings> ragdollSettings)
        {
            if (m_DebugLogs)
                Debug.Log("Physics -> Initialize()");

            m_Ragdolls.Clear();

            foreach (var settings in ragdollSettings)
            {
                AddRagdoll(settings);
            }
        }

        public ActiveRagdoll AddRagdoll(RagdollSettings settings)
        {
            var ragdoll = new ActiveRagdoll();
            ragdoll.SimulationConfig = m_SimulationSettings;
            ragdoll.RigidBodyConfig = m_RigidBodySettings;
            ragdoll.Initialize(settings.PhysicsRoot, settings.TargetRoot);

            m_Ragdolls.Add(ragdoll);

            return ragdoll;
        }

        public void RemoveRagdoll(RagdollSettings settings)
        {
            // TODO: Find a better way to compress this deletion
            foreach (var activeRagdoll in m_Ragdolls)
            {
                if (activeRagdoll.PhysicsRoot == settings.PhysicsRoot)
                {
                    m_RagdollsToRemove.Add(activeRagdoll);
                }
            }

            foreach (var activeRagdoll in m_RagdollsToRemove)
            {
                m_Ragdolls.Remove(activeRagdoll);
            }

            m_RagdollsToRemove.Clear();
        }

        public bool Solve(bool forceSimulation = false)
        {
            var anyForceApplied = false;

            // Setup physics simulation
            m_PhysicsState.Capture();
            Physics.simulationMode = SimulationMode.Script;

            // IMPORTANT NOTE
            // This makes sure physics joints are aligned with the ragdoll transforms
            // Otherwise this would break the simulation
            Physics.SyncTransforms();

            FreezeAllRigidBodies(false);

            var fixedDeltaTime = m_SolverSettings.FixedDeltaTime;
            for (var it = 0; it < m_SolverSettings.IterationCount; it++)
            {
                if (it == 0 || !m_SolverSettings.CumulativeForces)
                {
                    ResetAllVelocities();
                }

                Time.fixedDeltaTime = fixedDeltaTime;

                foreach (var ragdoll in m_Ragdolls)
                {
                    if (ragdoll.ApplyForces(Time.fixedDeltaTime))
                        anyForceApplied = true;
                }

                if (!anyForceApplied && !forceSimulation)
                    break;

                Physics.Simulate(Time.fixedDeltaTime);
                OnSimulate?.Invoke(Time.fixedDeltaTime);
                fixedDeltaTime *= m_SolverSettings.DeltaTimeFactor;
            }

            ResetAllVelocities();
            FreezeAllRigidBodies(true);

            // Restore physics simulation settings
            m_PhysicsState.Restore();

            return anyForceApplied;
        }

        public void ResetAllVelocities()
        {
            foreach (var ragdoll in m_Ragdolls)
            {
                if (!ragdoll.IsValid)
                    continue;

                ragdoll.ResetVelocities();
            }
        }

        public void SnapToTargets()
        {
            foreach (var ragdoll in m_Ragdolls)
            {
                if (!ragdoll.IsValid)
                    continue;

                ragdoll.SnapToTarget();
            }
        }

        public bool FixedUpdate(float deltaTime)
        {
            var anyForceApplied = false;

            FreezeAllRigidBodies(false);

            if (!m_SolverSettings.CumulativeForces)
            {
                ResetAllVelocities();
            }

            foreach (var ragdoll in m_Ragdolls)
            {
                if (ragdoll.ApplyForces(deltaTime))
                    anyForceApplied = true;
            }

            return anyForceApplied;
        }

        void FreezeAllRigidBodies(bool freeze = true)
        {
            foreach (var ragdoll in m_Ragdolls)
            {
                if (!ragdoll.IsValid)
                    continue;

                ragdoll.FreezeRigidBodies(freeze);
            }
        }
    }
}
