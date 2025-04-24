using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class ArmaturePhysicsPoseModel
    {
        [SerializeField]
        ArmaturePhysicsPoseData m_Data;

        public bool IsValid
        {
            get
            {
                if (m_Data.RigidBodyStates == null)
                    return false;

                for (var i = 0; i < m_Data.RigidBodyStates.Length; i++)
                {
                    var state = m_Data.RigidBodyStates[i];
                    if (state == null || !state.IsValid)
                        return false;
                }

                return true;
            }
        }

        public int NumBodies => m_Data.RigidBodyStates.Length;

        public delegate void Changed(ArmaturePhysicsPoseModel model);
        public event Changed OnChanged;

        public ArmaturePhysicsPoseModel(int numBodies)
        {
            m_Data = new ArmaturePhysicsPoseData(numBodies);
            RegisterEvents();
        }

        public ArmaturePhysicsPoseModel(ArmaturePhysicsPoseModel other)
        {
            m_Data = new ArmaturePhysicsPoseData(other.NumBodies);
            other.CopyTo(this);
            RegisterEvents();
        }

        [JsonConstructor]
        public ArmaturePhysicsPoseModel(ArmaturePhysicsPoseData m_Data)
        {
            this.m_Data = m_Data;
        }

        public void CopyTo(ArmaturePhysicsPoseModel other)
        {
            Assert.AreEqual(NumBodies, other.NumBodies);
            m_Data.RigidBodyStates.CopyTo(other.m_Data.RigidBodyStates, 0);
        }

        public void Capture(in RagdollData source)
        {
            Assert.IsTrue(IsValid);
            Assert.AreEqual(NumBodies, source.NumBodies);

            for (var i = 0; i < m_Data.RigidBodyStates.Length; i++)
            {
                m_Data.RigidBodyStates[i].Capture(source.RigidBodies[i]);
            }
        }

        public void Apply(in RagdollData target)
        {
            Assert.IsTrue(IsValid);
            Assert.AreEqual(NumBodies, target.NumBodies);

            for (var i = 0; i < m_Data.RigidBodyStates.Length; i++)
            {
                m_Data.RigidBodyStates[i].Apply(target.RigidBodies[i]);
            }
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        void RegisterEvents()
        {
            if (m_Data.RigidBodyStates == null)
                return;
            
            for (var i = 0; i < m_Data.RigidBodyStates.Length; i++)
            {
                if (m_Data.RigidBodyStates[i] is {} state)
                    state.OnChanged += OnRigidBodyStateChanged;
            }
        }

        void OnRigidBodyStateChanged(RigidBodyStateModel model)
        {
            OnChanged?.Invoke(this);
        }
    }
}
