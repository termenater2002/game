using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraMovementModel
    {
        [SerializeField]
        CameraMovementData m_Data;

        public Vector3 Pivot => m_Data.Coordinates.Pivot;
        public CameraModel Camera => m_Data.CameraModel;

        public CameraMovementModel(CameraModel cameraModel)
        {
            m_Data.CameraModel = cameraModel;
            m_Data.Coordinates = new CameraCoordinatesModel(m_Data.CameraModel.Position, m_Data.CameraModel.Rotation, 1.5f);

            m_Data.Dolly = new CameraDollyModel(m_Data.Coordinates);
            m_Data.Orbit = new CameraOrbitModel(m_Data.Coordinates);
            m_Data.Pan = new CameraPanModel(m_Data.Coordinates);
            m_Data.Walk = new CameraWalkModel(m_Data.Coordinates);
            m_Data.LookAround = new CameraLookAroundModel(m_Data.Coordinates);
            m_Data.Centering = new CameraCenteringModel(m_Data.Coordinates);

            RegisterEvents();
            UpdateCameraModel();
        }

        public void Update(float deltaTime)
        {
            m_Data.Pan.Update(deltaTime);
            m_Data.Orbit.Update(deltaTime);
            m_Data.Dolly.Update(deltaTime);
            m_Data.LookAround.Update(deltaTime);
            m_Data.Walk.Update(deltaTime);
            m_Data.Centering.Update(deltaTime);
        }

        public void SetCoordinates(Vector3 pivot, Vector3 cameraPosition)
        {
            m_Data.Coordinates.SetCoordinates(pivot, cameraPosition);
            Reset();
        }

        public void SetPivotAndOrbit(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            m_Data.Coordinates.SetPivotAndOrbit(cameraPosition, cameraRotation);
            Reset();
        }

        public void SetViewportOffset(Vector2 offset)
        {
            m_Data.Coordinates.ViewportOffset = offset;
        }

        public void Pan(Vector2 planeMovement)
        {
            m_Data.Pan.Move(new Vector3(planeMovement.x, planeMovement.y, 0f));
        }

        public void Orbit(Vector2 orbitMovement)
        {
            m_Data.Orbit.Move(new Vector3(orbitMovement.x, orbitMovement.y, 0f));
        }

        public void Dolly(float amount)
        {
            m_Data.Dolly.Move(new Vector3(amount, 0f, 0f));
        }

        public void DollyScroll(float scrollDelta)
        {
            var amount = scrollDelta * 0.3f;
            
            if (amount > 0)
            {
                var remainingDistance = m_Data.Coordinates.DistanceFromPivot - CameraCoordinatesModel.k_MinDistanceFromPivot;
                
                if (amount >= remainingDistance)
                {
                    Walk(Vector3.forward * (amount * 3f), false);
                }
                else
                {
                    Dolly(amount);
                }
            }
            else
            {
                Dolly(amount);
            }
            
           
        }
        
        public void LookAround(Vector2 lookAroundMovement)
        {
            m_Data.LookAround.Move(new Vector3(lookAroundMovement.x, lookAroundMovement.y, 0f));
        }

        public void Walk(Vector3 movement, bool onPlane)
        {
            m_Data.Walk.OnPlane = onPlane;
            m_Data.Walk.Move(movement);
        }

        public void Frame(Bounds worldBounds, float relativeMargin = 1.2f, bool instant = false)
        {
            var cameraFoV = m_Data.CameraModel.Target.fieldOfView;
            var radius = worldBounds.extents.magnitude;
            var distance = (radius * relativeMargin) / Mathf.Sin(Mathf.Deg2Rad * cameraFoV / 2.0f);

            Center(worldBounds.center, distance, instant);
        }

        public void Center(Vector3 target, float distance, bool instant = false)
        {
            m_Data.Centering.Center(target, distance, instant);
        }

        void Reset()
        {
            m_Data.Pan.Reset();
            m_Data.Orbit.Reset();
            m_Data.Dolly.Reset();
            m_Data.LookAround.Reset();
            m_Data.Walk.Reset();
            m_Data.Centering.Reset();
        }

        void RegisterEvents()
        {
            m_Data.Coordinates.OnOrbitChanged += OnCoordinatesOrbitChanged;
            m_Data.Coordinates.OnPivotChanged += OnCoordinatesPivotChanged;
            m_Data.Coordinates.OnDistanceFromPivotChanged += OnDistanceFromPivotChanged;
            m_Data.Coordinates.OnViewportOffsetChanged += OnViewportOffsetChanged;
        }
        
        void OnViewportOffsetChanged(CameraCoordinatesModel model, Vector2 newOffset)
        {
            UpdateCameraModel();
        }
        
        void OnDistanceFromPivotChanged(CameraCoordinatesModel model, float newDistance)
        {
            UpdateCameraModel();
        }

        void OnCoordinatesPivotChanged(CameraCoordinatesModel model, Vector3 newPivot)
        {
            UpdateCameraModel();
        }

        void OnCoordinatesOrbitChanged(CameraCoordinatesModel model, Vector2 newOrbit)
        {
            UpdateCameraModel();
        }

        void UpdateCameraModel()
        {
            m_Data.CameraModel.SetCoordinates(m_Data.Coordinates.CameraPosition, m_Data.Coordinates.CameraRotation);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            AssertUtils.Fail("CameraMovementModel should not be serialized");
        }
    }
}
