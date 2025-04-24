using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// The <see cref="NeuralConstraint"/> data.
    /// </summary>
    [Serializable]
    struct DeepPoseSolverData
    {
        [SerializeField]
        DeepPoseConfiguration m_Config;

        [SerializeField]
        List<Transform> m_Joints;

        [SerializeField]
        FixedArray32<Effector> m_Positions;

        [SerializeField]
        FixedArray32<Effector> m_Rotations;

        [SerializeField]
        FixedArray32<Effector> m_LookAts;

        [SerializeField]
        Vector3 m_ReferencePoint;

        [SerializeField]
        float m_Scaling;

        /// <summary>The list of joint Transforms, ordered by ID.</summary>
        public List<Transform> Joints
        {
            get { return m_Joints ??= new List<Transform>(); }
            set => m_Joints = value;
        }

        /// <summary>The position effectors.</summary>
        public FixedArray32<Effector> Positions
        {
            get => m_Positions;
            set => m_Positions = value;
        }

        /// <summary>The rotation effectors.</summary>
        public FixedArray32<Effector> Rotations
        {
            get => m_Rotations;
            set => m_Rotations = value;
        }

        /// <summary>The lookAt effectors.</summary>
        public FixedArray32<Effector> LookAts
        {
            get => m_LookAts;
            set => m_LookAts = value;
        }

        /// <summary>The global scaling factor from Unity space to Solver space</summary>
        public float Scaling
        {
            get => m_Scaling;
            set => m_Scaling = value;
        }

        /// <summary>The reference position to map from Unity space to Solver space</summary>
        public Vector3 ReferencePoint
        {
            get => m_ReferencePoint;
            set => m_ReferencePoint = value;
        }

        /// <summary>The configuration of the deep pose solver.</summary>
        public DeepPoseConfiguration Config
        {
            get => m_Config;
            set => m_Config = value;
        }

        public void RemoveAllPositionalEffectors()
        {
            m_Positions.Clear();
        }

        public void RemoveAllRotationalEffectors()
        {
            m_Rotations.Clear();
        }

        public void RemoveAllLookAtEffectors()
        {
            m_LookAts.Clear();
        }

        public void AddPositionalEffector(Transform transform, int id, float weight, float tolerance)
        {
            m_Positions.Add(new Effector(transform, id, weight, tolerance));
        }

        public void AddRotationalEffector(Transform transform, int id, float weight, float tolerance)
        {
            m_Rotations.Add(new Effector(transform, id, weight, tolerance));
        }

        public void AddLookAtEffector(Transform transform, int id, float weight, float tolerance)
        {
            m_LookAts.Add(new Effector(transform, id, weight, tolerance));
        }

        public void SetPositionalEffectorWeight(int idx, float weight)
        {
            var effector = m_Positions[idx];
            effector.weight = weight;
            m_Positions[idx] = effector;
        }

        public void SetRotationalEffectorWeight(int idx, float weight)
        {
            var effector = m_Rotations[idx];
            effector.weight = weight;
            m_Rotations[idx] = effector;
        }

        public void SetLookAtEffectorWeight(int idx, float weight)
        {
            var effector = m_LookAts[idx];
            effector.weight = weight;
            m_LookAts[idx] = effector;
        }

        public int EnabledPositionEffectorsCount()
        {
            int count = 0;

            for (int i = 0; i < m_Positions.Count; i++)
            {
                if (m_Positions[i].weight > 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
