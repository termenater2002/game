using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class RigidTransformModel
    {
        /// <summary>
        /// Defines how big a position change should be to update the transform and trigger change events
        /// Default value takes care of avoiding rounding up errors
        /// </summary>
        public float PositionEpsilon { get; set; } = 1e-5f;

        /// <summary>
        /// Defines how big a rotation change should be to update the transform and trigger change events
        /// Default value takes care of avoiding rounding up errors
        /// </summary>
        public float RotationEpsilon { get; set; } = 1e-6f;

        [SerializeField]
        RigidTransformData m_Data;

        public delegate void PositionChanged(RigidTransformModel model, Vector3 newPosition);
        public event PositionChanged OnPositionChanged;

        public delegate void RotationChanged(RigidTransformModel model, Quaternion newRotation);
        public event RotationChanged OnRotationChanged;

        public delegate void Changed(RigidTransformModel model);
        public event Changed OnChanged;

        public RigidTransformData Transform
        {
            get => m_Data;
            set
            {
                var posChanged = !m_Data.Position.NearlyEquals(value.Position, PositionEpsilon);
                var rotChanged = !m_Data.Rotation.NearlyEquals(value.Rotation, RotationEpsilon);

                if (!posChanged && !rotChanged)
                    return;

                m_Data.Position = value.Position;
                m_Data.Rotation = value.Rotation;

                if (posChanged)
                    OnPositionChanged?.Invoke(this, m_Data.Position);
                if (rotChanged)
                    OnRotationChanged?.Invoke(this, m_Data.Rotation);

                OnChanged?.Invoke(this);
            }
        }

        public Vector3 Position
        {
            get => m_Data.Position;
            set
            {
                if (m_Data.Position.NearlyEquals(value, PositionEpsilon))
                    return;

                m_Data.Position = value;
                OnPositionChanged?.Invoke(this, m_Data.Position);
                OnChanged?.Invoke(this);
            }
        }

        public Quaternion Rotation
        {
            get => m_Data.Rotation;
            set
            {
                // Prevent setting an invalid rotation
                if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z) || float.IsNaN(value.w) ||
                    float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z) || float.IsInfinity(value.w))
                {
                    Debug.LogError($"Invalid quaternion value: ({value.x}, {value.y}, {value.z}, {value.w})");
                    return;
                }
                
                if (m_Data.Rotation.NearlyEquals(value, RotationEpsilon))
                    return;

                m_Data.Rotation = value;
                OnRotationChanged?.Invoke(this, m_Data.Rotation);
                OnChanged?.Invoke(this);
            }
        }

        public bool IsValid => true;

        public RigidTransformModel()
        {
            m_Data.Position = Vector3.zero;
            m_Data.Rotation = Quaternion.identity;
        }

        public RigidTransformModel(Vector3 position, Quaternion rotation)
        {
            m_Data.Position = position;
            m_Data.Rotation = rotation;
        }

        public RigidTransformModel(RigidTransformModel other)
        {
            other.CopyTo(this);
        }

        public void Set(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void Translate(Vector3 offset)
        {
            Position += offset;
        }

        public void Rotate(Vector3 pivot, Quaternion offset)
        {
            Position = pivot + offset * (Position - pivot);
            Rotation = offset * Rotation;
        }

        public void CopyTo(RigidTransformModel other)
        {
            other.Transform = Transform;
        }
    }
}
