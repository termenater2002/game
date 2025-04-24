using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct PropID : IEquatable<PropID>
    {
        public EntityID EntityID => m_Id;

        public bool IsValid => m_Id.IsValid;

        [SerializeField]
        EntityID m_Id;

        public static PropID Generate(IEntityIDProvider idProvider)
        {
            var id = new PropID();
            id.m_Id = EntityID.Generate(idProvider);
            return id;
        }

        public PropID(PropID other)
        {
            m_Id = other.m_Id;
        }

        public bool Equals(PropID other)
        {
            return m_Id == other.m_Id;
        }

        public override bool Equals(object obj)
        {
            return obj is PropID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return m_Id.GetHashCode();
        }

        public static bool operator ==(PropID left, PropID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PropID left, PropID right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return m_Id.ToString();
        }
    }
}
