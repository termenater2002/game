using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Interface which provides unique/sequential EntityIDs
    /// </summary>
    interface IEntityIDProvider
    {
        public int GetNextEntityId();
    }
    
    [Serializable]
    struct EntityID : IEquatable<EntityID>, ISerializationCallbackReceiver
    {
        public bool IsValid => m_Id != -1;

        [NonSerialized]
        int m_Id;

        [SerializeField]
        int m_SerializedId;

        public static EntityID Generate(IEntityIDProvider provider)
        {
            var id = new EntityID();
            id.m_Id = provider.GetNextEntityId();
            return id;
        }

        public EntityID(EntityID other)
        {
            m_Id = other.m_Id;
            m_SerializedId = m_Id;
        }

        public bool Equals(EntityID other)
        {
            return m_Id == other.m_Id;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Id.GetHashCode();
        }

        public static bool operator ==(EntityID left, EntityID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityID left, EntityID right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return m_Id.ToString();
        }

        public void OnBeforeSerialize()
        {
            m_SerializedId = m_Id;
        }

        public void OnAfterDeserialize()
        {
            m_Id = m_SerializedId;
        }
    }
}
