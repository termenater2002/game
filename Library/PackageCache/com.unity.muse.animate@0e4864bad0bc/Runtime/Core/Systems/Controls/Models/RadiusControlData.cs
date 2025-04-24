using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct RadiusControlData
    {
        public float Radius;
        public float MinDisplayRadiusRelative;
        public float MaxRadius;
        public bool SnapToZeroBelowMinRadius;
        public ControlColorModel Color;
        public RigidTransformModel Transform;
    }
}
