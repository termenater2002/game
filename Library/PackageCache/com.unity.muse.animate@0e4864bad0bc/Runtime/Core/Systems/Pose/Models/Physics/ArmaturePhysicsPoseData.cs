using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a physics state of an armature
    /// </summary>
    [Serializable]
    struct ArmaturePhysicsPoseData
    {
        public RigidBodyStateModel[] RigidBodyStates;

        public ArmaturePhysicsPoseData(int numBodies)
        {
            RigidBodyStates = new RigidBodyStateModel[numBodies];
        }
    }
}
