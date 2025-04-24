using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct CompletionSolverSettings
    {
        [SerializeField]
        CompletionConfiguration m_Config;

        [SerializeField]
        List<Transform> m_Joints;

        public List<Transform> Joints
        {
            get { return m_Joints ??= new List<Transform>(); }
            set => m_Joints = value;
        }

        public CompletionConfiguration Config
        {
            get => m_Config;
            set => m_Config = value;
        }

        public bool IsValid => m_Config != null; // && m_Joints.Count > 0;

        public void SetDefaultValues()
        {
            m_Joints = new List<Transform>();
        }
    }
}
