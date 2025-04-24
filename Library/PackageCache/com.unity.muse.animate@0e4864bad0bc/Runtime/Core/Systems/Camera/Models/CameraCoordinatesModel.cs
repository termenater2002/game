using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraCoordinatesModel: ICopyable<CameraCoordinatesModel>, ISerializationCallbackReceiver
    {
        [SerializeField]
        CameraCoordinatesData m_Data;

        [NonSerialized]
        public const float k_MinDistanceFromPivot = 0.5f;

        [NonSerialized]
        const float k_MaxDistanceFromPivot = 10f;

        [NonSerialized]
        const float k_MinPivotY = 0.15f;

        [NonSerialized]
        const float k_MaxOrbitX = 90f;

        public Vector2 Orbit
        {
            get => m_Data.Orbit;
            set
            {
                var correctedOrbit = RestrainOrbit(value);
                if (m_Data.Orbit == correctedOrbit)
                    return;

                m_Data.Orbit = correctedOrbit;
                OnOrbitChanged?.Invoke(this, m_Data.Orbit);
            }
        }

        public Vector3 Pivot
        {
            get => m_Data.Pivot;
            set
            {
                var correctedPivot = new Vector3(value.x, Mathf.Max(k_MinPivotY, value.y), value.z);
                if (m_Data.Pivot == correctedPivot)
                    return;

                m_Data.Pivot = correctedPivot;
                CorrectOrbit();
                OnPivotChanged?.Invoke(this, m_Data.Pivot);
            }
        }

        public Vector2 ViewportOffset
        {
            set
            {
                if (m_Data.ViewportOffset == value)
                    return;

                m_Data.ViewportOffset = value;
                OnViewportOffsetChanged?.Invoke(this, m_Data.ViewportOffset);
            }
            get => m_Data.ViewportOffset;
        }

        public float DistanceFromPivot
        {
            get => m_Data.DistanceFromPivot;
            set
            {
                var correctedDistance = Mathf.Clamp(value, k_MinDistanceFromPivot, k_MaxDistanceFromPivot);
                if (correctedDistance == m_Data.DistanceFromPivot)
                    return;

                m_Data.DistanceFromPivot = correctedDistance;
                CorrectOrbit();
                OnDistanceFromPivotChanged?.Invoke(this, m_Data.DistanceFromPivot);
            }
        }

        public Vector3 CameraPosition
        {
            get
            {
                var pivotOffset = CameraRotation * (DistanceFromPivot * Vector3.back);
                var position = Pivot + pivotOffset;
                return position;
            }
        }

        public Quaternion CameraRotation
        {
            get
            {
                var rotation = Quaternion.Euler(Orbit.x, Orbit.y, 0f);
                return rotation;
            }
        }
        
        public Vector3 CameraRight => CameraRotation * Vector3.right;

        public Vector3 CameraUp => CameraRotation * Vector3.up;

        public Vector3 CameraForward => CameraRotation * Vector3.forward;

        public delegate void OrbitChanged(CameraCoordinatesModel model, Vector2 newOrbit);
        public event OrbitChanged OnOrbitChanged;
        
        public delegate void ViewportOffsetChanged(CameraCoordinatesModel model, Vector2 newViewportOffset);
        public event ViewportOffsetChanged OnViewportOffsetChanged;
        
        public delegate void PivotChanged(CameraCoordinatesModel model, Vector3 newPivot);
        public event PivotChanged OnPivotChanged;

        public delegate void DistanceFromPivotChanged(CameraCoordinatesModel model, float newDistance);
        public event DistanceFromPivotChanged OnDistanceFromPivotChanged;

        public CameraCoordinatesModel(Vector3 cameraPosition, Quaternion cameraRotation, float distanceFromPivot)
        {
            m_Data.DistanceFromPivot = distanceFromPivot;
            SetPivotAndOrbit(cameraPosition, cameraRotation);
        }
        
        public CameraCoordinatesModel(CameraCoordinatesModel source)
        {
            source.CopyTo(this);
        }

        public void SetCoordinates(Vector3 pivot, Vector3 cameraPosition)
        {
            var forwardVector = pivot - cameraPosition;

            Pivot = pivot;
            DistanceFromPivot = forwardVector.magnitude;

            var rotation = Quaternion.LookRotation(forwardVector.normalized, Vector3.up);
            var eulerAngles = rotation.eulerAngles;
            Orbit = new Vector2(eulerAngles.x, eulerAngles.y);
        }
        
        public void Capture(Transform sourceTransform)
        {
            SetPivotAndOrbit(sourceTransform.position, sourceTransform.rotation);
        }

        public void SetPivotAndOrbit(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            var pivotOffset = CameraRotation * (DistanceFromPivot * Vector3.back);
            Pivot = cameraPosition - pivotOffset;
            var eulerAngles = cameraRotation.eulerAngles;
            Orbit = new Vector3(eulerAngles.x, eulerAngles.y, 0f);
        }

        public void SetOrbitAndUpdatePivot(Vector2 newOrbit)
        {
            var cameraPosition = CameraPosition;
            Orbit = newOrbit;
            var pivotOffset = CameraRotation * (DistanceFromPivot * Vector3.back);
            Pivot = cameraPosition - pivotOffset;
        }

        Vector2 RestrainOrbit(Vector2 orbit)
        {
            var newOrbit = orbit;

            // Note: Sometimes FitToTransform will return values over 180
            // and it causes problems with pitch restrictions.
            if (newOrbit.x > 180)
            {
                newOrbit.x -= 360;
            }

            // Prevents going over the top
            if (newOrbit.x > k_MaxOrbitX)
            {
                newOrbit.x = k_MaxOrbitX;
            }

            // Prevents going under the floor
            var maxA = m_Data.Pivot.y > m_Data.DistanceFromPivot
                ? -(Mathf.Sin(m_Data.DistanceFromPivot / m_Data.Pivot.y) * (180 / Mathf.PI))
                : -(Mathf.Sin(m_Data.Pivot.y / m_Data.DistanceFromPivot) * (180 / Mathf.PI));

            if (newOrbit.x < maxA)
            {
                newOrbit.x = maxA;
            }

            return newOrbit;
        }

        void CorrectOrbit()
        {
            // Note: this will force the orbit to be corrected and notify any change
            Orbit = m_Data.Orbit;
        }

        public void CopyTo(CameraCoordinatesModel target)
        {
            target.m_Data.Orbit = m_Data.Orbit;
            target.m_Data.DistanceFromPivot = m_Data.DistanceFromPivot;
            target.m_Data.Pivot = m_Data.Pivot;
            target.m_Data.ViewportOffset = m_Data.ViewportOffset;
        }

        public CameraCoordinatesModel Clone()
        {
            return new CameraCoordinatesModel(this);
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Log(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Info, GetType().Name + " -> " + msg);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogError(string msg)
        {
            DevLogger.LogError(GetType().Name + " -> " + msg);
        }
    }
}
