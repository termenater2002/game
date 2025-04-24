using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    [Serializable]
    class PropModel: ICopyable<PropModel>, ISerializationCallbackReceiver
    {
        const float k_Epsilon = 1e-5f;

        [SerializeField]
        PropData m_Data;

        public PropID ID => m_Data.ID;
        public EntityID EntityID => m_Data.ID.EntityID;

        public string PrefabID => m_Data.PrefabID;

        public bool IsValid => !string.IsNullOrEmpty(m_Data.PrefabID) && m_Data.ID.IsValid;

        public Vector3 SpawnPosition => m_Data.SpawnPosition;
        public Quaternion SpawnRotation => m_Data.SpawnRotation;

        public PropModel(PropID id, string prefabId, Vector3 position, Quaternion rotation)
        {
            m_Data.ID = id;
            m_Data.PrefabID = prefabId;
            m_Data.SpawnPosition = position;
            m_Data.SpawnRotation = rotation;
        }
        
        public PropModel(PropModel source)
        {
            source.CopyTo(this);
        }

        public void CopyTo(PropModel target)
        {
            target.m_Data.ID = m_Data.ID;
            target.m_Data.PrefabID = m_Data.PrefabID;
            target.m_Data.SpawnPosition = m_Data.SpawnPosition;
            target.m_Data.SpawnRotation = m_Data.SpawnRotation;
        }

        public PropModel Clone()
        {
            return new PropModel(this);
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            DevLogger.LogInfo($"PropModel -> OnAfterDeserialize()");
            // Note: We can't check IsValid here, since OnAfterDeserialize can happen before 
            // m_Data.ID is properly deserialized
            // Assert.IsTrue(IsValid, "Invalid data");
        }
    }
}
