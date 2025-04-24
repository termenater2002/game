using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the state of a rigid body
    /// </summary>
    [Serializable]
    struct RigidBodyStateData
    {
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
    }
}
