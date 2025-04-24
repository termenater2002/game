using System;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct CameraMovementData
    {
        public CameraModel CameraModel;
        public CameraCoordinatesModel Coordinates;
        public CameraDollyModel Dolly;
        public CameraLookAroundModel LookAround;
        public CameraOrbitModel Orbit;
        public CameraPanModel Pan;
        public CameraWalkModel Walk;
        public CameraCenteringModel Centering;
    }
}
