using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraOrbitModel : CameraTransformatorModel
    {
        const float k_Friction = 7f;
        const float k_MinVelocity = 1e-5f;

        public CameraOrbitModel(CameraCoordinatesModel coordinates)
            : base(coordinates, k_Friction, k_MinVelocity)
        {

        }

        protected override void OnUpdate(float deltaTime)
        {
            Orbit += new Vector2(-Velocity.y, Velocity.x);
        }
    }
}
