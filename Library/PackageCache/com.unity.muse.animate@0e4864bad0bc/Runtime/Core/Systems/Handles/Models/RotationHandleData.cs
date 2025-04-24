using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct RotationHandleData
    {
        [FormerlySerializedAs("Ball")]
        public BallControlModel BallControl;

        public RingControlModel Front;

        public RingControlModel AxisX;
        public RingControlModel AxisY;
        public RingControlModel AxisZ;

        public RigidTransformModel CurrentTransform;
        public RigidTransformModel InitialTransform;
    }
}
