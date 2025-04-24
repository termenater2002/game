using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct UniversalEffectorHandleData
    {
        public bool RotationEnabled;

        [FormerlySerializedAs("Ball")]
        public BallControlModel BallControl;
        public RingControlModel Front;
        [FormerlySerializedAs("Circle")]
        public CircleControlModel CircleControl;

        public RigidTransformModel CurrentTransform;
        public RigidTransformModel InitialTransform;
    }
}
