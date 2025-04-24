using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct TranslationHandleData
    {
        [FormerlySerializedAs("Circle")]
        public CircleControlModel CircleControl;
        [FormerlySerializedAs("AxisX")]
        public AxisControlModel AxisControlX;
        [FormerlySerializedAs("AxisY")]
        public AxisControlModel AxisControlY;
        [FormerlySerializedAs("AxisZ")]
        public AxisControlModel AxisControlZ;
        [FormerlySerializedAs("PlaneXY")]
        public PlaneControlModel PlaneControlXY;
        [FormerlySerializedAs("PlaneXZ")]
        public PlaneControlModel PlaneControlXZ;
        [FormerlySerializedAs("PlaneYZ")]
        public PlaneControlModel PlaneControlYZ;

        public RigidTransformModel CurrentTransform;
        public RigidTransformModel InitialTransform;
    }
}
