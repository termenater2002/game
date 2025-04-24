using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraCenteringModel
    {
        const float k_DistanceToTimeFactor = 0.4f;
        const float k_MaxTimeToCenter = 1f;

        [SerializeField]
        CameraCenteringData m_Data;

        public CameraCoordinatesModel Coordinates => m_Data.Coordinates;

        public CameraCenteringModel(CameraCoordinatesModel coordinates)
        {
            m_Data.Coordinates = coordinates;
            Reset();
        }

        public void Reset()
        {
            m_Data.StartPivot = Coordinates.Pivot;
            m_Data.EndPivot = Coordinates.Pivot;
            m_Data.StartDistance = Coordinates.DistanceFromPivot;
            m_Data.EndDistance = Coordinates.DistanceFromPivot;
            m_Data.IsCentering = false;
            m_Data.ElapsedTime = 0f;
            m_Data.Duration = 0f;
        }

        public void Center(Vector3 targetPivot, float targetDistance, float duration)
        {
            Reset();

            m_Data.EndPivot = targetPivot;
            m_Data.EndDistance = targetDistance;
            m_Data.Duration = duration;
            m_Data.IsCentering = true;

            // Instant move
            if (m_Data.Duration <= 0f)
                Update(0f);
        }

        public void Center(Vector3 targetPivot, float targetDistance, bool instant = false)
        {
            var cameraForward = Coordinates.CameraForward;
            var pivotOffset = targetPivot - Coordinates.Pivot;
            var depth = Vector3.Dot(cameraForward, pivotOffset);
            var planeOffset = pivotOffset - depth * cameraForward;

            var duration = instant ? 0f : Mathf.Min(k_MaxTimeToCenter, planeOffset.magnitude * k_DistanceToTimeFactor);

            Center(targetPivot, targetDistance, duration);
        }

        public void Update(float deltaTime)
        {
            if (!m_Data.IsCentering)
                return;

            m_Data.ElapsedTime += deltaTime;

            var progress = m_Data.Duration <= 0f ? 1f : Mathf.Min(1f, m_Data.ElapsedTime / m_Data.Duration);

            // TODO: Use a curve to define the smoothing instead of smoothing twice
            var smoothProgress = MathUtils.SmoothLerp(0f, 1f, progress);
            smoothProgress = MathUtils.SmoothLerp(0f, 1f, smoothProgress);
            var interpolationT = smoothProgress;

            Coordinates.Pivot = Vector3.Lerp(m_Data.StartPivot, m_Data.EndPivot, interpolationT);
            Coordinates.DistanceFromPivot = Mathf.Lerp(m_Data.StartDistance, m_Data.EndDistance, interpolationT);

            if (m_Data.ElapsedTime >= m_Data.Duration)
            {
                Reset();
            }
        }
    }
}
