using System;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraPanModel : CameraTransformatorModel
    {
        const float k_Friction = 40f;
        const float k_MinVelocity = 1e-10f;

        public CameraPanModel(CameraCoordinatesModel coordinates)
            : base(coordinates, k_Friction, k_MinVelocity) { }

        protected override void OnUpdate(float deltaTime)
        {
            Pivot += Velocity.x * Coordinates.CameraRight + Velocity.y * Coordinates.CameraUp;
        }
    }
}
