using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraWalkModel : CameraTransformatorModel
    {
        const float k_Friction = 20f;
        const float k_MinVelocity = 1e-2f;

        public bool OnPlane
        {
            get => m_Data.OnPlane;
            set => m_Data.OnPlane = value;
        }

        [SerializeField]
        CameraWalkData m_Data;

        public CameraWalkModel(CameraCoordinatesModel coordinates) :
            base(coordinates, k_Friction, k_MinVelocity) { }
        protected override void OnUpdate(float deltaTime)
        {
            var offset = Velocity * deltaTime;  // local space
            offset = Coordinates.CameraRotation * offset;  // world space

            if (OnPlane)
                offset.y = 0f;

            Pivot += offset;
        }
    }
}
