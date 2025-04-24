using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct MultiCcdData
    {
        [SerializeField, Range(0f, 1f)]
        float m_Weight;

        [SerializeField]
        List<DynamicCcdJoint> m_Joints;

        [SerializeField]
        List<DynamicCcdEffector> m_Positions;

        [SerializeField, Range(1, 50)]
        int m_MaxIterations;

        [SerializeField, Range(0f, 0.1f)]
        float m_Tolerance;

        public float Weight
        {
            get => m_Weight;
            set => m_Weight = value;
        }

        public List<DynamicCcdJoint> Joints
        {
            get { return m_Joints ??= new List<DynamicCcdJoint>(); }
            set => m_Joints = value;
        }

        public List<DynamicCcdEffector> Positions
        {
            get { return m_Positions ??= new List<DynamicCcdEffector>(); }
            set => m_Positions = value;
        }

        public int MaxIterations => m_MaxIterations;
        public float Tolerance => m_Tolerance;

        public MultiCcdData(float weight, int maxIterations, float tolerance)
        {
            m_Weight = weight;
            m_MaxIterations = maxIterations;
            m_Tolerance = tolerance;

            m_Joints = new List<DynamicCcdJoint>();
            m_Positions = new List<DynamicCcdEffector>();
        }

        public void SetDefaultValues()
        {
            m_Weight = 1f;
            m_MaxIterations = 10;
            m_Tolerance = 0f;

            m_Joints = new List<DynamicCcdJoint>();
            m_Positions = new List<DynamicCcdEffector>();
        }
    }
}
