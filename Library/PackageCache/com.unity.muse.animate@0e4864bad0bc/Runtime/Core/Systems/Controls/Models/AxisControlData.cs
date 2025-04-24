using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct AxisControlData
    {
        public Vector3 LocalAxis;
        public ControlColorModel Color;
        public RigidTransformModel Transform;
    }
}
