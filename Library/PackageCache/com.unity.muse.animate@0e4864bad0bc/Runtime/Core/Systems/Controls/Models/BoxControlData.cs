using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct BoxControlData
    {
        public Vector3 LocalNormal;
        public Vector3 LocalFirstAxis;
        public Vector3 LocalSecondAxis;
        public Vector3 LocalThirdAxis;
        public Bounds LocalBounds;
        public ControlColorModel FirstAxisColor;
        public ControlColorModel SecondAxisColor;
        public ControlColorModel ThirdAxisColor;
        public ControlColorModel NormalColor;
        public RigidTransformModel Transform;
    }
}
