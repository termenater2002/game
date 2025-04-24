using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct ActorID : IEquatable<ActorID>
    {
        public EntityID EntityID => m_Id;

        public bool IsValid => m_Id.IsValid;

        [SerializeField]
        EntityID m_Id;

        public static ActorID Generate(IEntityIDProvider idProvider)
        {
            var actorId = new ActorID();
            actorId.m_Id = EntityID.Generate(idProvider);
            return actorId;
        }

        public ActorID(ActorID other)
        {
            m_Id = other.m_Id;
        }

        public bool Equals(ActorID other)
        {
            return m_Id == other.m_Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Id.GetHashCode();
        }

        public static bool operator ==(ActorID left, ActorID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorID left, ActorID right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return m_Id.ToString();
        }
    }
}
