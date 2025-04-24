using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct CameraTransformatorData
    {
        public CameraCoordinatesModel CameraCoordinates;
        public Vector3 Velocity;
        public float Friction;
        public float MinVelocity;
        public float MinVelocitySq;
    }
}
