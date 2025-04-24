using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct PlaneControlData
    {
        public Vector3 LocalNormal;
        public Vector3 LocalFirstAxis;
        public Vector3 LocalSecondAxis;
        public ControlColorModel FirstAxisColor;
        public ControlColorModel SecondAxisColor;
        public ControlColorModel NormalColor;
        public RigidTransformModel Transform;
    }
}
