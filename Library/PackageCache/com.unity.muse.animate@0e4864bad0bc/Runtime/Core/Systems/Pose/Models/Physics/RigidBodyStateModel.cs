using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class RigidBodyStateModel
    {
        const float k_Epsilon = 1e-5f;

        [SerializeField]
        RigidBodyStateData m_Data;

        public delegate void Changed(RigidBodyStateModel model);
        public event Changed OnChanged;

        public bool IsValid => true;

        public Vector3 Velocity
        {
            get => m_Data.Velocity;
            set
            {
                if (value.NearlyEquals(m_Data.Velocity, k_Epsilon))
                    return;

                m_Data.Velocity = value;
                OnChanged?.Invoke(this);
            }
        }

        public Vector3 AngularVelocity
        {
            get => m_Data.AngularVelocity;
            set
            {
                if (value.NearlyEquals(m_Data.AngularVelocity, k_Epsilon))
                    return;

                m_Data.AngularVelocity = value;
                OnChanged?.Invoke(this);
            }
        }

        public RigidBodyStateModel()
        {
            m_Data.Velocity = Vector3.zero;
            m_Data.AngularVelocity = Vector3.zero;
        }

        [JsonConstructor]
        public RigidBodyStateModel(RigidBodyStateData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void CopyTo(RigidBodyStateModel other)
        {
            other.Velocity = Velocity;
            other.AngularVelocity = AngularVelocity;
        }

        public void Capture(Rigidbody source)
        {
#if UNITY_6000_0_OR_NEWER            
            Velocity = source.linearVelocity;
#else
            Velocity = source.velocity;
#endif
            AngularVelocity = source.angularVelocity;
        }

        public void Apply(Rigidbody target)
        {
#if UNITY_6000_0_OR_NEWER
            target.linearVelocity = Velocity;
#else
            target.velocity = Velocity;
#endif
            target.angularVelocity = AngularVelocity;
        }
    }
}
