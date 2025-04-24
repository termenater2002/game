using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct AutoregressiveSolverSettings
    {
        [SerializeField]
        AutoregressiveConfiguration m_Config;

        [SerializeField]
        List<Transform> m_Joints;
        public bool InputFkPositions;

        public List<Transform> Joints
        {
            get { return m_Joints ??= new List<Transform>(); }
            set => m_Joints = value;
        }

        public AutoregressiveConfiguration Config
        {
            get => m_Config;
            set => m_Config = value;
        }

        public bool IsValid => m_Config != null;

        public void SetDefaultValues()
        {
            m_Joints = new List<Transform>();
        }
    }
}
