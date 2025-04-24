using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraDollyModel : CameraTransformatorModel
    {
        const float k_Friction = 20f;
        const float k_MinVelocity = 1e-2f;

        public CameraDollyModel(CameraCoordinatesModel coordinates)
            : base(coordinates, k_Friction, k_MinVelocity)
        {

        }

        protected override void OnUpdate(float deltaTime)
        {
            var speedRatio = Mathf.Lerp(0.1f, 1f, DistanceFromPivot / 10f);
            DistanceFromPivot -= Velocity.x * speedRatio;
        }
    }
}
