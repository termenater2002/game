using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A visual component used in all handles and controls.
    /// Must be paired with a <see cref="ControlViewModel"/>.
    /// </summary>
    [ExecuteAlways]
    abstract class ControlView : MonoBehaviour, IPhysicsRaycastHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /*
         Note: keeping those as reference for later bugfixes/refactoring of render order
        const int k_RenderQueue = 3200;
        const CompareFunction k_ZTest = CompareFunction.Disabled;
        const StencilOp k_StencilOpPass = StencilOp.Keep;
        const CompareFunction k_StencilComp = CompareFunction.LessEqual;
        */
        
        const float k_FacingFadeOutBegin = 0.3f;
        const float k_FacingFadeOutEnd = 0.1f;
        const float k_FacingFadeOutRange = k_FacingFadeOutBegin - k_FacingFadeOutEnd;

        public int PhysicsRaycastDepth { get; set; }
        int PhysicsRaycastSortingOrder { get; set; }

        public bool IsInitialized { get; private set; }
        
        ControlViewModel m_Model;

        public void Initialize()
        {
            if (IsInitialized)
                return;
            
            IsInitialized = true;
            CreateShapesAndColliders();
        }
        
        protected internal void SetModel(ControlViewModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            PhysicsRaycastSortingOrder = m_Model.PhysicsRaycastSortingOrder;
            
            QueueUpdateAll();
            
            m_Model.OnStateChanged += OnModelStateChanged;
            m_Model.OnTransformChanged += OnModelTransformChanged;
            m_Model.OnDraggingStateChanged += OnModelDraggingStateChanged;
            m_Model.OnSizeRatiosChanged += OnSizeRatiosChanged;
            m_Model.OnCameraTransformChanged += OnCameraTransformChanged;
            m_Model.OnCameraViewportSizeChanged += OnCameraViewportSizeChanged;
            m_Model.OnCameraViewportCursorChanged += OnCameraViewportCursorChanged;
            m_Model.OnVisibilityChanged += OnVisibilityChanged;
            m_Model.OnStep += OnStep;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnStateChanged -= OnModelStateChanged;
            m_Model.OnTransformChanged -= OnModelTransformChanged;
            m_Model.OnDraggingStateChanged -= OnModelDraggingStateChanged;
            m_Model.OnSizeRatiosChanged -= OnSizeRatiosChanged;
            m_Model.OnCameraTransformChanged -= OnCameraTransformChanged;
            m_Model.OnCameraViewportSizeChanged -= OnCameraViewportSizeChanged;
            m_Model.OnCameraViewportCursorChanged -= OnCameraViewportCursorChanged;
            m_Model.OnVisibilityChanged -= OnVisibilityChanged;
            m_Model.OnStep -= OnStep;
        }

        protected void UpdateAll()
        {
            UpdateTransform();
            UpdateCamera();
            UpdateShape();
            UpdateColliders();
            UpdateAlpha();
            UpdateColor();
            UpdateVisibility();
        }

        protected void QueueUpdateAll()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Colliders);
            QueueUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
            QueueUpdate(UpdateFlags.Visibility);
        }

        protected void OnTransformChanged()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
        }
        
        void OnCameraViewportCursorChanged(CameraModel cam, Vector2 position)
        {
            QueueUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
        }

        void OnCameraViewportSizeChanged(CameraModel cam, Vector2 size)
        {
            QueueUpdateAll();
        }

        // [Section] User Input Events Handlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Model?.OnViewPointerEntered(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Model?.OnViewPointerExited(eventData);
        }

        // [Section] View Model Events Handlers
        
        void OnStep(float delta)
        {
            Step(delta);
        }
        
        void OnModelTransformChanged()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
        }
        
        void OnModelStateChanged()
        {
            QueueUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
            QueueUpdate(UpdateFlags.Visibility);
        }
        
        void OnModelDraggingStateChanged(ControlViewModel control)
        {
            QueueUpdateAll();
        }
        
        void OnCameraTransformChanged()
        {
            QueueUpdate(UpdateFlags.Camera);
        }
        
        void OnSizeRatiosChanged()
        {
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Colliders);
        }

        void OnVisibilityChanged()
        {
            ForceUpdate();
        }

        // [Section] Virtual Methods

        /// <summary>
        /// Called when creating the 3d shapes components and colliders
        /// </summary>
        public virtual void CreateShapesAndColliders()
        {
            gameObject.layer = ApplicationLayers.LayerHandles;
        }

        public virtual void Step(float delta)
        {
            if (IsUpdateQueued(UpdateFlags.Transform))
                UpdateTransform();
            
            if (IsUpdateQueued(UpdateFlags.Camera))
                UpdateCamera();

            if (IsUpdateQueued(UpdateFlags.Shape))
                UpdateShape();

            if (IsUpdateQueued(UpdateFlags.Colliders))
                UpdateColliders();

            if (IsUpdateQueued(UpdateFlags.Alpha))
                UpdateAlpha();

            if (IsUpdateQueued(UpdateFlags.Color))
                UpdateColor();

            if (IsUpdateQueued(UpdateFlags.Visibility))
                UpdateVisibility();
        }

        public virtual void ForceUpdate()
        {
            if (m_Model == null)
                return;
            
            UpdateAll();
        }

        protected virtual void UpdateColliders()
        {
            Assert.IsFalse(IsUpdateQueued(UpdateFlags.Shape), "Shape must be updated first");
            ResetUpdate(UpdateFlags.Colliders);
        }

        protected virtual void UpdateShape()
        {
            ResetUpdate(UpdateFlags.Shape);
        }

        protected virtual void UpdateCamera()
        {
            Assert.IsFalse(IsUpdateQueued(UpdateFlags.Transform), "Transform must be up to date first");
            ResetUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Alpha);
        }

        protected virtual void UpdateTransform()
        {
            ResetUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
        }

        protected virtual void UpdateColor()
        {
            Assert.IsFalse(IsUpdateQueued(UpdateFlags.Alpha), "Alpha must be up to date first");
            ResetUpdate(UpdateFlags.Color);
        }

        protected virtual void UpdateAlpha()
        {
            Assert.IsFalse(IsUpdateQueued(UpdateFlags.Transform), "Transform must be up to date first");
            ResetUpdate(UpdateFlags.Alpha);
            QueueUpdate(UpdateFlags.Color);
            QueueUpdate(UpdateFlags.Visibility);
        }

        protected virtual void UpdateVisibility()
        {
            Assert.IsFalse(IsUpdateQueued(UpdateFlags.Alpha), "Alpha must be updated first");
            ResetUpdate(UpdateFlags.Visibility);
        }

        // [Section] Raycasting Methods

        public bool ValidateRaycastHit(Vector2 screenPosition, RaycastHit hit)
        {
            if (!m_Model.IsVisible)
                return false;

            return CheckRaycastHit(screenPosition, hit);
        }

        // Child classes should reimplement this to perform custom raycast validation
        protected virtual bool CheckRaycastHit(Vector2 screenPosition, RaycastHit hit)
        {
            return true;
        }

        public int GetPhysicsRaycastSortingOrder(Vector2 screenPosition, RaycastHit hit)
        {
            return PhysicsRaycastSortingOrder;
        }

        public virtual int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit)
        {
            return PhysicsRaycastDepth;
        }

        protected Vector2 GetScreenPosition()
        {
            var modelCamera = m_Model.Camera.Target;
            var screenPosition = modelCamera.WorldToScreenPoint(m_Model.Position);
            return new Vector2(screenPosition.x, screenPosition.y);
        }

        // [Section] Helper Methods

        protected float ComputeVisibility(Vector3 worldForward, Vector3 position, bool reversed)
        {
            var modelCamera = m_Model.Camera;
            var facingRatio = HandlesUtils.ComputeCameraFacingRatio(modelCamera, worldForward, position);
            var ratio = reversed ? 1f - facingRatio : facingRatio;
            var visibility = Mathf.Clamp01((ratio - k_FacingFadeOutEnd) / k_FacingFadeOutRange);
            return visibility;
        }

        protected static T CreateCollider<T>(GameObject container) where T : Collider
        {
            var createdCollider = container.AddComponent<T>();
            return createdCollider;
        }

        protected GameObject CreateChild(string objectName)
        {
            return CreateChild(objectName, transform);
        }

        protected static GameObject CreateChild(string objectName, Transform parent)
        {
            var createdObject = new GameObject(objectName);

            createdObject.transform.SetParent(parent, false);
            createdObject.layer = ApplicationLayers.LayerHandles;

            return createdObject;
        }

        // [Section] Update Flags

        public event Action<UpdateFlags, bool> OnUpdateFlagsChanged;

        [Flags]
        public enum UpdateFlags
        {
            Transform = 1,
            Camera = 2,
            Shape = 4,
            Colliders = 8,
            Alpha = 16,
            Color = 32,
            Visibility = 64,
        }
        
        UpdateFlags m_UpdateFlags;
        
        bool IsUpdateQueued(UpdateFlags updateFlag) => m_UpdateFlags.HasFlag(updateFlag);

        internal void QueueUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, true);

        internal void ResetUpdate(UpdateFlags updateFlag) => SetUpdateFlag(updateFlag, false);

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
