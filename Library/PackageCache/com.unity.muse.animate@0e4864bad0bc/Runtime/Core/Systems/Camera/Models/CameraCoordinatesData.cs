using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct CameraCoordinatesData
    {
        public Vector3 Pivot;
        public float DistanceFromPivot;
        public Vector2 Orbit; // Note: in degrees
        public Vector2 ViewportOffset; // Note: in viewport ratio (0 to 1)
    }
}
