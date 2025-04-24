using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Abstract effector that represents a generic transform
    /// </summary>
    [Serializable]
    class GenericEffectorModel
    {
        const float k_Epsilon = 1e-5f;
        const float k_RotationEpsilon = 1e-5f;

        public enum Property
        {
            Position,
            Rotation
        }

        [SerializeField]
        GenericEffectorData m_Data;

        public delegate void PropertyChanged(GenericEffectorModel model, Property property);
        public event PropertyChanged OnPropertyChanged;

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
                if (m_Data.Rotation.NearlyEquals(value, k_RotationEpsilon))
                    return;

                m_Data.Rotation = value;
                OnPropertyChanged?.Invoke(this, Property.Rotation);
            }
        }

        public GenericEffectorModel()
        {
            m_Data.Position = Vector3.zero;
            m_Data.Rotation = Quaternion.identity;
        }

        [JsonConstructor]
        public GenericEffectorModel(GenericEffectorData m_Data)
        {
            this.m_Data = m_Data;
        }

        public GenericEffectorModel(GenericEffectorModel copiedModel)
        {
            copiedModel.CopyTo(this);
        }

        public void CopyTo(GenericEffectorModel other)
        {
            other.Position = Position;
            other.Rotation = Rotation;
        }
    }
}
