using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct BallControlData
    {
        public float Radius;
        public ControlColorModel Color;
        public RigidTransformModel Transform;
    }
}
