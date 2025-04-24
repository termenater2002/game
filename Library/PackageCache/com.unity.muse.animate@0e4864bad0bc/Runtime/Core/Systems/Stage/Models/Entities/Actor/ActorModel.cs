using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Model of an Actor present in a StageModel.
    /// </summary>
    [Serializable]
    class ActorModel: ICopyable<ActorModel>, ISerializationCallbackReceiver
    {
        [SerializeField]
        ActorData m_Data;

        public ActorID ID => m_Data.ID;
        public EntityID EntityID => m_Data.ID.EntityID;
        public string PrefabID => m_Data.PrefabID;

        public bool IsValid => m_Data.ID.IsValid;

        public Vector3 SpawnPosition => m_Data.SpawnPosition;
        public Quaternion SpawnRotation => m_Data.SpawnRotation;
        
        public ActorModel(ActorModel source)
        {
            source.CopyTo(this);
        }
        
        public ActorModel(ActorID id, string prefabId, Vector3 position, Quaternion rotation)
        {
            m_Data.ID = id;
            m_Data.PrefabID = prefabId;
            m_Data.SpawnPosition = position;
            m_Data.SpawnRotation = rotation;
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            // Note: We can't check IsValid here, since OnAfterDeserialize can happen before 
            // m_Data.ID is properly deserialized
            // Assert.IsTrue(IsValid, "Invalid data");
        }

        public void CopyTo(ActorModel target)
        {
            target.m_Data.ID = m_Data.ID;
            target.m_Data.PrefabID = m_Data.PrefabID;
            target.m_Data.SpawnPosition = m_Data.SpawnPosition;
            target.m_Data.SpawnRotation = m_Data.SpawnRotation;
        }

        public ActorModel Clone()
        {
            return new ActorModel(this);
        }
    }
}
