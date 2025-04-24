using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    abstract class CameraTransformatorModel
    {
        [SerializeField]
        CameraTransformatorData m_Data;

        public CameraCoordinatesModel Coordinates => m_Data.CameraCoordinates;

        public Vector3 Pivot
        {
            get => m_Data.CameraCoordinates.Pivot;
            set => m_Data.CameraCoordinates.Pivot = value;
        }

        public Vector2 Orbit
        {
            get => m_Data.CameraCoordinates.Orbit;
            set => m_Data.CameraCoordinates.Orbit = value;
        }

        public float DistanceFromPivot
        {
            get => m_Data.CameraCoordinates.DistanceFromPivot;
            set => m_Data.CameraCoordinates.DistanceFromPivot = value;
        }

        public float Friction
        {
            get => m_Data.Friction;
            set => m_Data.Friction = Mathf.Clamp(value, 0f, 1f);
        }

        public Vector3 Velocity
        {
            get => m_Data.Velocity;
            set
            {
                if (value.sqrMagnitude < m_Data.MinVelocitySq)
                {
                    m_Data.Velocity = Vector3.zero;
                    return;
                }

                m_Data.Velocity = value;
            }
        }

        public float MinVelocity
        {
            get => m_Data.MinVelocity;
            set
            {
                if (Math.Abs(m_Data.MinVelocity - value) < Mathf.Epsilon)
                    return;

                m_Data.MinVelocity = value;
                m_Data.MinVelocitySq = value * value;
                
                // Force velocity update (as it will be forced to 0 if under new min velocity)
                Velocity = m_Data.Velocity;
            }
        }

        protected CameraTransformatorModel(CameraCoordinatesModel coordinates, float friction, float minVelocity)
        {
            m_Data.CameraCoordinates = coordinates;
            m_Data.Friction = friction;
            m_Data.MinVelocity = minVelocity;
            m_Data.MinVelocitySq = minVelocity * minVelocity;
            m_Data.Velocity = Vector3.zero;
        }

        public void Reset()
        {
            Velocity = Vector3.zero;
        }
        
        public void Move(Vector3 velocity, float smoothFactor = 0.9f)
        {
#if UNITY_MUSE_ANIMATE_USE_CAMERA_PHYSICS
                Velocity = velocity * smoothFactor + ((1f - smoothFactor) * Velocity);
#else
                Velocity = velocity;
#endif
        }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
            ApplyFriction(deltaTime);
        }

        protected abstract void OnUpdate(float deltaTime);

        void ApplyFriction(float deltaTime)
        {
#if UNITY_MUSE_ANIMATE_USE_CAMERA_PHYSICS
            Velocity /= 1f + (Friction * deltaTime);
#else
            Velocity = Vector3.zero;
#endif
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            AssertUtils.Fail("Camera transformators should not be deserialized");
        }
    }
}
