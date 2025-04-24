using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a kinematic (ie static) pose of an armature
    /// First transform is ALWAYS assumed to be a single root transform
    /// </summary>
    [Serializable]
    struct ArmatureStaticPoseData
    {
        public enum PoseType
        {
            Local = 0,
            Global = 1
        }

        public PoseType Type;
        public RigidTransformModel[] TransformModels;
    }
}
