using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct ToleranceHandleData
    {
        [FormerlySerializedAs("Radius")]
        public RadiusControlModel RadiusControl;
        public RigidTransformModel Transform;
    }
}
