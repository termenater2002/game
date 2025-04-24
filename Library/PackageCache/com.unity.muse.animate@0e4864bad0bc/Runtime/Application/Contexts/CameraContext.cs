using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used by a controllable viewport camera.
    /// </summary>
    /// <remarks>
    /// Both <see cref="ThumbnailsService"/> and <see cref="ApplicationContext"/> have their own <see cref="CameraContext"/>.
    /// </remarks>
    class CameraContext
    {
        public CameraModel CameraModel { get; }
        public CameraMovementModel CameraMovementModel { get; }

        public CameraContext(CameraModel cameraModel, CameraMovementModel cameraMovementModel)
        {
            CameraModel = cameraModel;
            CameraMovementModel = cameraMovementModel;
        }
    }
}
