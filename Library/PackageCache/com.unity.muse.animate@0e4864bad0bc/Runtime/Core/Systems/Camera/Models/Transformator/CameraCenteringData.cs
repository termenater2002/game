using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct CameraCenteringData
    {
        public CameraCoordinatesModel Coordinates;

        public bool IsCentering;
        public float Duration;
        public Vector3 StartPivot;
        public Vector3 EndPivot;
        public float StartDistance;
        public float EndDistance;
        public float ElapsedTime;
    }
}
