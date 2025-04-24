using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    class SkeletonView : MonoBehaviour
    {
        const float k_BoneThickness = 1.5f;
        static readonly Color k_BaseColor = new Color(0f, 0f, 0f, 0.7f);

        SkeletonViewModel m_Model;

        List<LineRenderer> m_Bones;
        List<Gradient> m_BoneGradients;

        public void Initialize()
        {
            m_Bones = new();
            m_BoneGradients = new();
        }

        public void Update()
        {
            if (IsUpdateQueued(UpdateFlags.Visibility))
                UpdateVisibility();

            if (IsUpdateQueued(UpdateFlags.Pose))
                UpdatePose();

            if (IsUpdateQueued(UpdateFlags.SizeRatios))
                UpdateSizeRatios();

            if (IsUpdateQueued(UpdateFlags.Alpha))
                UpdateAlpha();
        }

        public void SetModel(SkeletonViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();

            UpdateVisibility();
            UpdatePose();
            UpdateAlpha();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            CreateBones();

            m_Model.Camera.OnTransformChanged += OnCameraTransformChanged;
            m_Model.Camera.OnViewportCursorChanged += OnCameraCursorChanged;
            m_Model.Camera.OnViewportSizeChanged += OnCameraViewportSizeChanged;
            m_Model.OnPoseChanged += OnPoseChanged;
            m_Model.OnStateChanged += OnStateChanged;
            m_Model.OnViewStep += OnViewStep;
        }

        void OnViewStep(float obj)
        {
            Update();
        }

        void UnregisterModel()
        {
            // Disable bones but keep them for later to avoid GC allocs
            RecycleBones();

            if (m_Model == null)
                return;

            m_Model.Camera.OnTransformChanged -= OnCameraTransformChanged;
            m_Model.Camera.OnViewportCursorChanged -= OnCameraCursorChanged;
            m_Model.Camera.OnViewportSizeChanged -= OnCameraViewportSizeChanged;
            m_Model.OnPoseChanged -= OnPoseChanged;
            m_Model.OnStateChanged -= OnStateChanged;
        }

        void CreateBones()
        {
            // Create any missing bone
            while (m_Bones.Count < m_Model.NumBones)
            {
                var bone = CreateNewBone();
                m_Bones.Add(bone);
                m_BoneGradients.Add(new Gradient());
            }

            // Active all required bones
            for (var i = 0; i < m_Model.NumBones; i++)
            {
                var bone = m_Bones[i];
                bone.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Disable bones but keep them for later to avoid GC allocations.
        /// </summary>
        void RecycleBones()
        {
            foreach (var bone in m_Bones)
            {
                bone.gameObject.SetActive(false);
            }
        }
        
        // [Section] Models Events Handlers

        void OnCameraViewportSizeChanged(CameraModel cameraModel, Vector2 size)
        {
            QueueUpdate(UpdateFlags.SizeRatios);
            QueueUpdate(UpdateFlags.Alpha);
        }

        void OnCameraCursorChanged(CameraModel cameraModel, Vector2 position)
        {
            QueueUpdate(UpdateFlags.SizeRatios);
            QueueUpdate(UpdateFlags.Alpha);
        }

        void OnCameraTransformChanged(CameraModel cameraModel)
        {
            QueueUpdate(UpdateFlags.SizeRatios);
            QueueUpdate(UpdateFlags.Alpha);
        }

        void OnStateChanged()
        {
            QueueUpdate(UpdateFlags.Visibility);
        }

        void OnPoseChanged()
        {
            QueueUpdate(UpdateFlags.Pose);
            QueueUpdate(UpdateFlags.SizeRatios);
            QueueUpdate(UpdateFlags.Alpha);
        }

        // [Section] Update Methods

        /// <summary>
        /// Update the size of the bones lines relative to the camera
        /// </summary>
        void UpdateSizeRatios()
        {
            if (m_Model == null || !m_Model.IsVisible)
                return;

            // Note: In this case, we only clear the flag if the pose was actually updated
            QueueUpdate(UpdateFlags.SizeRatios);

            // Active all required bones
            for (var i = 0; i < m_Model.NumBones; i++)
            {
                m_Model.GetBone(i, out var fromPos, out var toPos);
                var sizeStart = HandlesUtils.ComputeSizeRatio(m_Model.Camera, fromPos);
                var sizeEnd = HandlesUtils.ComputeSizeRatio(m_Model.Camera, toPos);
                m_Bones[i].startWidth = sizeStart * HandlesUtils.LineSize;
                m_Bones[i].endWidth = sizeEnd * HandlesUtils.LineSize;
            }
        }

        void UpdateVisibility()
        {
            ResetUpdate(UpdateFlags.Visibility);

            if (m_Model == null || !m_Model.IsVisible)
            {
                for (var i = 0; i < m_Bones.Count; i++)
                {
                    var bone = m_Bones[i];
                    bone.enabled = false;
                }

                return;
            }

            for (var i = 0; i < m_Model.NumBones; i++)
            {
                m_Bones[i].enabled = true;
            }
        }

        void UpdatePose()
        {
            if (m_Model == null || !m_Model.IsVisible)
                return;

            // Note: In this case, we only clear the flag if the pose was actually updated
            ResetUpdate(UpdateFlags.Pose);

            // Note: we need to inverse apply the rootTransform as pose is in world space but drawing happens in local space
            var rootTransform = transform;

            for (var i = 0; i < m_Model.NumBones; i++)
            {
                var bone = m_Bones[i];
                m_Model.GetBone(i, out var fromPos, out var toPos);
                
                bone.SetPosition(0, rootTransform.InverseTransformPoint(fromPos));
                bone.SetPosition(1, rootTransform.InverseTransformPoint(toPos));
            }
        }

        void UpdateAlpha()
        {
            // Note: In this case, we only clear the flag if the pose was actually updated
            if (m_Model == null || !m_Model.IsVisible)
                return;

            ResetUpdate(UpdateFlags.Alpha);

            for (var i = 0; i < m_Model.NumBones; i++)
            {
                var bone = m_Bones[i];

                m_Model.GetBone(i, out var fromPos, out var toPos);

                var colorStart = ComputeJointColor(fromPos);
                var colorEnd = ComputeJointColor(toPos);

                m_BoneGradients[i].SetKeys(
                    new GradientColorKey[] { new(colorStart, 0.0f), new(colorEnd, 1.0f) },
                    new GradientAlphaKey[] { new(colorStart.a, 0.0f), new(colorEnd.a, 1.0f) }
                );

                bone.colorGradient = m_BoneGradients[i];
            }
        }

        
        
        LineRenderer CreateNewBone()
        {
            var bone = HandlesUtils.CreateLine(gameObject, "Bone");
            var boneColorGradient = new Gradient();
            bone.widthMultiplier = HandlesUtils.LineSize;
            boneColorGradient.SetKeys(
                new GradientColorKey[] { new(Color.black, 0.0f), new(Color.black, 1.0f) },
                new GradientAlphaKey[] { new(1f, 0.0f), new(1f, 1.0f) }
            );
            bone.colorGradient = boneColorGradient;

            return bone;
        }

        Color ComputeJointColor(Vector3 jointPosition)
        {
            var distance = HandlesUtils.GetWorldToViewportPixelDistanceFromCursor(m_Model.Camera, jointPosition, false);
            distance = HandlesUtils.PixelsToNoots(distance);
            var color = k_BaseColor;
            color.a = k_BaseColor.a * Mathf.Clamp01(GetAlphaFromDistanceFromCursor(distance) - 0.4f);
            return color;
        }

        static float GetAlphaFromDistanceFromCursor(float distance)
        {
            return Mathf.Max(0f, Mathf.Min(1f, 1f - (distance - 2f) / 4f));
        }
        
        // [Section] Update Flags

        public event Action<UpdateFlags, bool> OnUpdateFlagsChanged;

        /// <summary>
        /// Test
        /// </summary>
        [Flags]
        internal enum UpdateFlags
        {
            Pose = 1,
            SizeRatios = 2,
            Alpha = 4,
            Visibility = 8,
        }

        UpdateFlags m_UpdateFlags;

        bool IsUpdateQueued(UpdateFlags updateFlag) => m_UpdateFlags.HasFlag(updateFlag);

        internal void QueueUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, true);

        void ResetUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, false);

        void SetUpdateFlag(UpdateFlags flag, bool value)
        {
            if (value == m_UpdateFlags.HasFlag(flag))
                return;

            if (value)
            {
                m_UpdateFlags |= flag;
            }
            else
            {
                m_UpdateFlags &= ~flag;
            }

            OnUpdateFlagsChanged?.Invoke(flag, value);
        }

        
    }
}
