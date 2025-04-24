using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct RingControlModelData
    {
        public bool HideBack;
        public float Radius;
        public Vector3 LocalAxis;
        public ControlColorModel Color;
        public RigidTransformModel Transform;
    }
}
