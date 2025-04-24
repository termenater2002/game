using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
#if DEEPPOSE_URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    [Serializable]
    class CameraModel
    {
        public event Action<CameraModel, Vector2> OnViewportRenderScalingChanged;
        public event Action<CameraModel, Vector2> OnViewportPositionChanged;
        public event Action<CameraModel, Vector2> OnViewportCursorChanged;
        public event Action<CameraModel, bool> OnIsDraggingControlChanged;
        public event Action<CameraModel, Vector2> OnViewportSizeChanged;
        public event Action<CameraModel, Camera> OnCameraChanged;
        public event Action<CameraModel> OnContextMenu;
        
        public event Action<CameraModel> OnTransformChanged;
        
        public Camera Target
        {
            get => m_Data.Target;
            set
            {
                Assert.IsNotNull(value, "Camera must be non null");

                if (m_Data.Target == value)
                    return;

                m_Data.Target = value;
                OnCameraChanged?.Invoke(this, m_Data.Target);

                Capture();
            }
        }
        
        public Texture TargetTexture => m_Data.Target.targetTexture;

        public Vector3 Position
        {
            get => m_Data.RigidTransformModel.Position;
            private set => m_Data.RigidTransformModel.Position = value;
        }

        public Quaternion Rotation
        {
            get => m_Data.RigidTransformModel.Rotation;
            private set => m_Data.RigidTransformModel.Rotation = value;
        }
        
        public Vector2 RenderScaling
        {
            get => m_Data.RenderScaling;
            set
            {
                if (m_Data.RenderScaling.Equals(value))
                    return;

                m_Data.RenderScaling = value;
                OnViewportRenderScalingChanged?.Invoke(this, m_Data.RenderScaling);
            }
        }

        public Vector2 Dpi
        {
            get
            {
                var dpi = Vector2.one * Mathf.Max(Unity.AppUI.Core.Platform.scaleFactor, 1f);
                dpi.x = Mathf.Max(dpi.x, RenderScaling.x, Unity.AppUI.Core.Platform.scaleFactor);
                dpi.y = Mathf.Max(dpi.y, RenderScaling.y, Unity.AppUI.Core.Platform.scaleFactor);
                return dpi;
            }
        }
        
        public Vector3 ViewportCursor
        {
            get => m_Data.ViewportCursor;
            set
            {
                if (m_Data.ViewportCursor.Equals(value))
                    return;

                m_Data.ViewportCursor = value;
                OnViewportCursorChanged?.Invoke(this, m_Data.ViewportCursor);
            }
        }

        public Vector2 ViewportSize
        {
            get => m_Data.ViewportSize;
            set
            {
                if (m_Data.ViewportSize.Equals(value))
                    return;

                if (value.x == 0 || value.y == 0)
                    return;
                
                m_Data.ViewportSize = value;
                
                // Note: Changing the aspect here right away
                // allows the controls to be updated correctly
                // before we call RenderToTexture
                Target.aspect = m_Data.ViewportSize.x / m_Data.ViewportSize.y;
                OnViewportSizeChanged?.Invoke(this, m_Data.ViewportSize);
            }
        }
        
        public Vector2 ViewportPosition
        {
            get => m_Data.ViewportPosition;
            set
            {
                if (m_Data.ViewportPosition.Equals(value))
                    return;

                m_Data.ViewportPosition = value;
                OnViewportPositionChanged?.Invoke(this, m_Data.ViewportPosition);
            }
        }
        
        public bool IsDraggingControl
        {
            get => m_Data.IsDraggingControl;
            private set
            {
                if (m_Data.IsDraggingControl.Equals(value))
                    return;

                m_Data.IsDraggingControl = value;
                OnIsDraggingControlChanged?.Invoke(this, m_Data.IsDraggingControl);
            }
        }

        [SerializeField]
        CameraData m_Data;
        
        [NonSerialized]
        List<ControlViewModel> m_Controls = new();

        public CameraModel(Camera camera)
        {
            Assert.IsNotNull(camera, "Camera must be non null");
            var cameraTransform = camera.transform;

            m_Data = new CameraData
            {
                Target = camera,
                RigidTransformModel = new RigidTransformModel(cameraTransform.position, cameraTransform.rotation)
            };
            
            RegisterEvents();
        }

        public void RegisterControl(ControlViewModel control)
        {
            m_Controls.Add(control);
            control.OnDraggingStateChanged += OnControlViewDraggingStateChanged;
        }

        void OnControlViewDraggingStateChanged(ControlViewModel control)
        {
            IsDraggingControl = control.IsDragging;
        }

        public void UnregisterControl(ControlViewModel control)
        {
            m_Controls.Remove(control);
            control.OnDraggingStateChanged -= OnControlViewDraggingStateChanged;
        }
        
        [JsonConstructor]
        public CameraModel(CameraData m_Data)
        {
            this.m_Data = m_Data;
        }

        void RegisterEvents()
        {
            // Note: we update all changes as very small camera movements can result in significant view change, thus using non-null epsilon can cause jittering
            m_Data.RigidTransformModel.PositionEpsilon = 0f;
            m_Data.RigidTransformModel.RotationEpsilon = 0f;

            m_Data.RigidTransformModel.OnPositionChanged += OnRigidTransformPositionChanged;
            m_Data.RigidTransformModel.OnRotationChanged += OnRigidTransformRotationChanged;
        }

        void OnRigidTransformPositionChanged(RigidTransformModel model, Vector3 newPosition)
        {
            Apply();
            OnTransformChanged?.Invoke(this);
        }

        void OnRigidTransformRotationChanged(RigidTransformModel model, Quaternion newRotation)
        {
            Apply();
            OnTransformChanged?.Invoke(this);
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext context)
        {
            RegisterEvents();
        }

        public void Capture()
        {
            var cameraTransform = m_Data.Target.transform;
            SetCoordinates(cameraTransform.position, cameraTransform.rotation);
        }

        void Apply()
        {
            Target.transform.SetPositionAndRotation(Position, Rotation);
            Target.ResetAspect();
            Target.ResetProjectionMatrix();
        }

        public void RenderToTexture(RenderTexture renderTexture, bool permanent)
        {
            using(new RenderTextureOverride(renderTexture))
            using(new LightingSettingsOverride(ApplicationConstants.DefaultLightingSettings, Target.scene))
            {
                Target.ResetProjectionMatrix();
                Target.ResetAspect();

                if (renderTexture.width == 0 || renderTexture.height == 0)
                {
                    return;
                }

                Target.aspect = renderTexture.width / (float)renderTexture.height;
                Target.targetTexture = renderTexture;

                if (!permanent)
                {
                    Target.Render();
                    Target.targetTexture = null;
                }
                else
                {
                    
                    #if DEEPPOSE_URP
                    
                    if (RenderPipelineUtils.IsUsingUrp())
                    {
                        // Assign the new pipeline asset to the Camera's current pipeline asset
                        Target.GetUniversalAdditionalCameraData().renderPostProcessing = true;
                        Target.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        Target.GetUniversalAdditionalCameraData().antialiasingQuality = AntialiasingQuality.High;
                    }
                    
                    #endif
                    
                    Target.RenderDontRestore();
                }
            }
        }
        
        /// <summary>
        /// User is responsible for releasing the render texture.
        /// </summary>
        public void ClearPermanentRenderTextureTarget() => Target.targetTexture = null;

        public void SetCoordinates(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            Apply();
        }

        public void OpenContextMenu(VisualElement uiRoot, Vector2 pointerPosition, List<ContextMenu.ActionArgs> effectorContextMenuArgs)
        {
            var screenPosition = ViewportPosition + (pointerPosition / Dpi);
            screenPosition.y += uiRoot.layout.height - ViewportSize.y;
            
            OnContextMenu?.Invoke(this);
            
            ContextMenu.OpenContextMenu(uiRoot, screenPosition, effectorContextMenuArgs);
        }
        
        public Vector2 WorldToViewportPoint(Vector3 worldCoordinate)
        {
            var ret = Target.WorldToViewportPoint(worldCoordinate);
            return ret;
        }
    }
}
