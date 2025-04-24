using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines the state of a posing effector
    /// </summary>
    [Serializable]
    struct DeepPoseEffectorData
    {
        public ArmatureEffectorIndex Index;
        public Vector3 Position;
        public Quaternion Rotation;
        public float PositionWeight;
        public float PositionTolerance;
        public float RotationWeight;
        public float LookAtWeight;
    }
}
