using System;
using Unity.DeepPose.Core;
using UnityEngine;

namespace Unity.DeepPose.Components
{
    [Serializable]
    class MultiCcdComponent : MonoBehaviour
    {
        public ref MultiCcdData data => ref m_data;

        [SerializeField]
        MultiCcdData m_data;

        MultiCcdSolver m_Solver;

        [SerializeField]
        public bool IsSolverAlwaysActive = true;

        void Reset()
        {
            m_data = new MultiCcdData();
            m_data.SetDefaultValues();
        }

        void OnEnable()
        {
            m_Solver = new MultiCcdSolver();
            m_Solver.Initialize(in m_data);
        }

        void OnDisable()
        {
            m_Solver = null;
        }

        void LateUpdate()
        {
            if (!IsSolverAlwaysActive)
                return;

            if (m_Solver == null)
                return;

            m_Solver.Solve(in m_data);
        }

        public void Solve()
        {
            m_Solver.Solve(in m_data);
        }
    }
}
