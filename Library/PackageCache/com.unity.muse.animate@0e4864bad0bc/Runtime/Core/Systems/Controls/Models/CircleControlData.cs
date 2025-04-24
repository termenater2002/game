using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct CircleControlData
    {
        public float Radius;
        public float ColliderRadiusPadding;
        public ControlColorModel Color;
        public RigidTransformModel Transform;
    }
}
