using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// This class represents a ragdoll, ie a collection of rigid bodies
    /// </summary>
    [Serializable]
    struct RagdollData
    {
        public Rigidbody[] RigidBodies;
        public int NumBodies => RigidBodies.Length;

        public bool IsValid => RigidBodies != null;
    }

    static class RagdollDataUtils
    {
        public static bool IsValid(this RagdollData data)
        {
            return data.RigidBodies != null;
        }
    }
}
