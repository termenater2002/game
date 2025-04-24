using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Components
{
    [ExecuteAlways]
    class PhysicsSolverComponent : MonoBehaviour
    {
        [SerializeField]
        bool m_AutoSolve = true;

        [SerializeField]
        public bool UseFixedUpdate = false;

        [SerializeField]
        List<PhysicsSolver.RagdollSettings> m_Ragdolls = new();

        [SerializeField]
        PhysicsSolver.SolverSettings m_SolverSettings = PhysicsSolver.SolverSettings.Default;

        [SerializeField]
        ActiveRagdoll.SimulationSettings m_SimulationSettings = ActiveRagdoll.SimulationSettings.Default;

        [SerializeField]
        ActiveRagdoll.RigidBodySettings m_RigidBodySettings = ActiveRagdoll.RigidBodySettings.Default;

        public List<PhysicsSolver.RagdollSettings> Ragdolls => m_Ragdolls;

        public bool AutoSolve
        {
            get => m_AutoSolve;
            set => m_AutoSolve = value;
        }

        PhysicsSolver m_Solver;
        bool m_IsInitialized;

        public int RagdollCount => m_Ragdolls.Count;

        public ActiveRagdoll AddRagdoll(PhysicsSolver.RagdollSettings settings)
        {
            m_Ragdolls.Add(settings);
            var activeRagdoll = m_Solver.AddRagdoll(settings);
            return activeRagdoll;
        }

        public void RemoveRagdoll(PhysicsSolver.RagdollSettings settings)
        {
            m_Ragdolls.Remove(settings);
            m_Solver.RemoveRagdoll(settings);
        }

        public void RemoveRagdoll(int idx)
        {
            Assert.IsTrue(idx >= 0 || idx < m_Ragdolls.Count, "Invalid ragdoll index");
            m_Ragdolls.RemoveAt(idx);
        }

        public PhysicsSolver.RagdollSettings GetRagdoll(int idx)
        {
            Assert.IsTrue(idx >= 0 || idx < m_Ragdolls.Count, "Invalid ragdoll index");
            return m_Ragdolls[idx];
        }

        public void RemoveAllRagdolls()
        {
            foreach (var settings in m_Ragdolls)
            {
                m_Solver.RemoveRagdoll(settings);
            }
            m_Ragdolls.Clear();
        }

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            Terminate();
        }

        public void Initialize()
        {
            if (m_IsInitialized)
                return;

            m_IsInitialized = true;

            m_Solver = new PhysicsSolver();
            m_Solver.SimulationConfig = m_SimulationSettings;
            m_Solver.RigidBodyConfig = m_RigidBodySettings;
            m_Solver.SolverConfig = m_SolverSettings;
            m_Solver.Initialize(m_Ragdolls);
        }

        public void Terminate()
        {
            if (!m_IsInitialized)
                return;

            m_IsInitialized = false;

            // TODO: We probably need to Reset / Dispose of m_Solver here?
        }

        void LateUpdate()
        {
            if (UseFixedUpdate || !m_AutoSolve)
                return;

            Solve();
        }

        public void Solve(bool forceSimulation = false)
        {
            // Hack: make sure settings are up to date so that we can tweak them
            // TODO: make an inspector instead
            m_Solver.SolverConfig = m_SolverSettings;
            m_Solver.SimulationConfig = m_SimulationSettings;

            m_Solver.Solve(forceSimulation);
        }

        public void ResetAllVelocities()
        {
            m_Solver.ResetAllVelocities();
        }

        public void SnapToTargets()
        {
            m_Solver.SnapToTargets();
        }

        void FixedUpdate()
        {
            if (!UseFixedUpdate || !m_AutoSolve)
                return;

            // Hack: make sure settings are up to date so that we can tweak them
            // TODO: make an inspector instead
            m_Solver.SolverConfig = m_SolverSettings;
            m_Solver.SimulationConfig = m_SimulationSettings;

            m_Solver.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}
