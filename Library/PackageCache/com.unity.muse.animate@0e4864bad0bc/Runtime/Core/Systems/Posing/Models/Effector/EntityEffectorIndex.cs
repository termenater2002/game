using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    // Represent an effector of a specific actor
    [Serializable]
    struct EntityEffectorIndex : IEquatable<EntityEffectorIndex>
    {
        public DeepPoseEffectorIndex EffectorIndex => m_EffectorIndex;
        public EntityID EntityID => m_EntityID;

        public bool IsValid => EffectorIndex.IsValid && EntityID.IsValid;

        [SerializeField]
        DeepPoseEffectorIndex m_EffectorIndex;

        [SerializeField]
        EntityID m_EntityID;

        public EntityEffectorIndex(EntityID entityID, DeepPoseEffectorIndex effectorIndex)
        {
            m_EntityID = entityID;
            m_EffectorIndex = effectorIndex;
        }

        public bool Equals(EntityEffectorIndex other)
        {
            return m_EffectorIndex.Equals(other.m_EffectorIndex) && m_EntityID.Equals(other.m_EntityID);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityEffectorIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_EffectorIndex, m_EntityID);
        }

        public static bool operator ==(EntityEffectorIndex left, EntityEffectorIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityEffectorIndex left, EntityEffectorIndex right)
        {
            return !left.Equals(right);
        }
    }
}
