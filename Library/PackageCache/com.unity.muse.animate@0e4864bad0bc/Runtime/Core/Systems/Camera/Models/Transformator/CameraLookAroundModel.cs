using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraLookAroundModel : CameraTransformatorModel
    {
        const float k_Friction = 400f;
        const float k_MinVelocity = 1e-5f;
        
        public CameraLookAroundModel(CameraCoordinatesModel coordinates)
            : base(coordinates, k_Friction, k_MinVelocity)
        {
            
        }

        protected override void OnUpdate(float deltaTime)
        {
            var angularMovement = new Vector2(-Velocity.y, Velocity.x);
            Coordinates.SetOrbitAndUpdatePivot(Orbit + angularMovement);
        }
    }
}
