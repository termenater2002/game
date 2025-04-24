using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a posing effector
    /// </summary>
    [Serializable]
    class DeepPoseEffectorModel
    {
        const float k_Epsilon = 1e-5f;
        const float k_RotationEpsilon = 1e-5f;

        public enum Property
        {
            Position,
            Rotation,
            PositionWeight,
            PositionTolerance,
            RotationWeight,
            LookAtWeight
        }

        [SerializeField]
        DeepPoseEffectorData m_Data;

        public delegate void PropertyChanged(DeepPoseEffectorModel model, Property property);
        public event PropertyChanged OnPropertyChanged;

        public ArmatureEffectorIndex Index => m_Data.Index;

        public Vector3 Position
        {
            get => m_Data.Position;
            set
            {
                if (m_Data.Position.NearlyEquals(value, k_Epsilon))
                    return;

                m_Data.Position = value;
                OnPropertyChanged?.Invoke(this, Property.Position);
            }
        }

        public Quaternion Rotation
        {
            get => m_Data.Rotation;
            set
            {
                if (HandlesLookAt && !ApplicationConstants.EnableGeneralizedLookAt)
                    return;

                if (m_Data.Rotation.NearlyEquals(value, k_RotationEpsilon))
                    return;

                m_Data.Rotation = value.normalized;
                OnPropertyChanged?.Invoke(this, Property.Rotation);
            }
        }

        public float PositionWeight
        {
            get => m_Data.PositionWeight;
            set
            {
                if (m_Data.PositionWeight.NearlyEquals(value, k_Epsilon, true))
                    return;

                m_Data.PositionWeight = value;
                OnPropertyChanged?.Invoke(this, Property.PositionWeight);
            }
        }

        public float PositionTolerance
        {
            get => m_Data.PositionTolerance;
            set
            {
                if (m_Data.PositionTolerance.NearlyEquals(value, k_Epsilon, true))
                    return;

                m_Data.PositionTolerance = value;
                OnPropertyChanged?.Invoke(this, Property.PositionTolerance);
            }
        }

        public float RotationWeight
        {
            get => m_Data.RotationWeight;
            set
            {
                if (m_Data.RotationWeight.NearlyEquals(value, k_Epsilon, true))
                    return;

                m_Data.RotationWeight = value;
                OnPropertyChanged?.Invoke(this, Property.RotationWeight);
            }
        }

        public float LookAtWeight
        {
            get => m_Data.LookAtWeight;
            set
            {
                if (m_Data.LookAtWeight.NearlyEquals(value, k_Epsilon, true))
                    return;

                m_Data.LookAtWeight = value;
                OnPropertyChanged?.Invoke(this, Property.LookAtWeight);
            }
        }

        public bool IsActive => PositionEnabled || RotationEnabled || LookAtEnabled;

        public bool PositionEnabled
        {
            get => HandlesPosition && PositionWeight > 0f;
            set => PositionWeight = (value ? 1f : 0f);
        }

        public bool RotationEnabled
        {
            get => HandlesRotation && RotationWeight > 0f;
            set => RotationWeight = (value ? 1f : 0f);
        }

        public bool LookAtEnabled
        {
            get => HandlesLookAt && LookAtWeight > 0f;
            set => LookAtWeight = (value ? 1f : 0f);
        }

        public bool HandlesPosition => m_Data.Index.EffectorIndex.HasPosition;

        public bool HandlesRotation => m_Data.Index.EffectorIndex.HasRotation;

        public bool HandlesLookAt => m_Data.Index.EffectorIndex.HasLookAt;
        public int ArmatureJointIndex => m_Data.Index.JointIndex;

        public DeepPoseEffectorModel(ArmatureEffectorIndex index)
        {
            if (index.JointIndex < 0)
                AssertUtils.Fail($"Invalid joint index: {index.JointIndex.ToString()}");

            m_Data.Index = index;
            m_Data.Position = Vector3.zero;
            m_Data.Rotation = Quaternion.identity;
            m_Data.PositionWeight = 0f;
            m_Data.RotationWeight = 0f;
            m_Data.LookAtWeight = 0f;
            m_Data.PositionTolerance = 0f;
        }

        [JsonConstructor]
        public DeepPoseEffectorModel(DeepPoseEffectorData m_Data)
        {
            this.m_Data = m_Data;
        }

        public DeepPoseEffectorModel(DeepPoseEffectorModel copiedModel)
        {
            copiedModel.CopyTo(this);
        }

        public void CopyTo(DeepPoseEffectorModel other)
        {
            other.m_Data.Index = m_Data.Index;
            
            other.Position = m_Data.Position;
            other.Rotation = m_Data.Rotation;
            other.PositionWeight = m_Data.PositionWeight;
            other.PositionTolerance = m_Data.PositionTolerance;
            other.RotationWeight = m_Data.RotationWeight;
            other.LookAtWeight = m_Data.LookAtWeight;
        }

        public void Lerp(DeepPoseEffectorModel from, in DeepPoseEffectorModel to, float t)
        {
            Position = Vector3.Lerp(from.Position, to.Position, t);
            Rotation = Quaternion.Slerp(from.Rotation, to.Rotation, t);
            PositionWeight = Mathf.Lerp(from.PositionWeight, to.PositionWeight, t);
            PositionTolerance = Mathf.Lerp(from.PositionTolerance, to.PositionTolerance, t);
            RotationWeight = Mathf.Lerp(from.RotationWeight, to.RotationWeight, t);
            LookAtWeight = Mathf.Lerp(from.LookAtWeight, to.LookAtWeight, t);
        }

        public void Translate(Vector3 offset)
        {
            if (HandlesPosition || HandlesLookAt)
                Position += offset;
        }

        public void Rotate(Vector3 pivot, Quaternion offset)
        {
            if (HandlesPosition || HandlesLookAt)
            {
                Position = pivot + offset * (Position - pivot);
            }
            else if (HandlesRotation)
            {
                Rotation = offset * Rotation;
            }
        }

        /// <summary>
        /// Checks if both models share compatible data
        /// </summary>
        /// <param name="other">The model to check compatibility with</param>
        /// <returns>true if both models are compatible, false otherwise</returns>
        public bool IsCompatibleWith(DeepPoseEffectorModel other)
        {
            return other.Index == Index
                && other.ArmatureJointIndex == ArmatureJointIndex
                && other.HandlesPosition == HandlesPosition
                && other.HandlesRotation == HandlesRotation
                && other.HandlesLookAt == HandlesLookAt;
        }
    }
}
