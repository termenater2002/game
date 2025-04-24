using System;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    abstract class ControlViewModel
    {
        public event Action OnSizeRatiosChanged;
        public event Action OnStateChanged;
        public event Action OnVisibilityChanged;
        public event Action<ControlViewModel> OnDraggingStateChanged;
        public event Action OnCameraTransformChanged;
        public event Action OnTransformChanged;
        public event Action<float> OnStep;
        public event Action<CameraModel, Vector2> OnCameraViewportSizeChanged;
        public event Action<CameraModel, Vector2> OnCameraViewportCursorChanged;

        public enum SpaceType
        {
            World,
            Screen
        }

        public CameraModel Camera => m_CameraModel;

        public abstract Vector3 Position { get; }
        public abstract Quaternion Rotation { get; }
        
        public SpaceType SizeSpace
        {
            get => m_SizeSpace;
            set
            {
                if (m_SizeSpace == value)
                    return;

                m_SizeSpace = value;
                UpdateSizeRatios();
            }
        }

        public bool IsSelected
        {
            get => m_IsSelected;
            set
            {
                if (m_IsSelected == value)
                    return;

                m_IsSelected = value;
                OnStateChanged?.Invoke();
            }
        }
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (m_IsVisible == value)
                    return;

                m_IsVisible = value;
                OnVisibilityChanged?.Invoke();
            }
        }

        public bool IsDragging
        {
            get => m_IsDragging;
            protected set
            {
                if (m_IsDragging == value)
                    return;

                m_IsDragging = value;
                OnDraggingStateChanged?.Invoke(this);
            }
        }

        public bool IsHighlighted
        {
            get => m_IsHighlighted;
            set
            {
                if (m_IsHighlighted == value)
                    return;

                m_IsHighlighted = value;
                InvokeStateChanged();
            }
        }

        public float SizeRatio
        {
            get => m_SizeRatio;

            private set
            {
                if (m_SizeRatio.Equals(value))
                    return;

                m_SizeRatio = value;
                OnSizeRatiosChanged?.Invoke();
            }
        }

        public float ThicknessRatio
        {
            get => m_ThicknessRatio;

            private set
            {
                if (m_ThicknessRatio.Equals(value))
                    return;

                m_ThicknessRatio = value;
                OnSizeRatiosChanged?.Invoke();
            }
        }

        public int PhysicsRaycastSortingOrder { get; }
        protected CameraModel CameraModel => m_CameraModel;
        public Vector3 DragStartPointerPosition { get; private set; }
        public Vector3 DragPreviousPointerPosition { get; private set; }
        public Quaternion DragStartRotation { get; private set; }
        public Vector3 DragStartPosition { get; private set; }
        public Vector3 DragEndPosition { get; private set; }
        public Vector3 DragDelta { get; private set; }
        public Vector3 DragDeltaTotal { get; private set; }
        
        readonly CameraModel m_CameraModel;

        bool m_IsVisible;
        bool m_IsHighlighted;
        bool m_IsDragging;
        bool m_IsSelected;
        
        float m_SizeRatio;
        SpaceType m_SizeSpace;
        SpaceType m_ThicknessSpace;
        float m_ThicknessRatio;
        
        protected ControlViewModel(CameraModel cameraModel, SpaceType sizeSpace, SpaceType thicknessSpace, int sortingOrder)
        {
            m_CameraModel = cameraModel;
            m_SizeSpace = sizeSpace;
            PhysicsRaycastSortingOrder = sortingOrder;

            m_SizeRatio = 1f;
            m_ThicknessRatio = 1f;

            m_CameraModel.OnTransformChanged += OnCameraTransformChangedInternal;
            m_CameraModel.OnViewportCursorChanged += OnCameraViewportCursorChangedInternal;
            m_CameraModel.OnViewportSizeChanged += OnCameraViewportSizeChangedInternal;
            
            m_CameraModel.RegisterControl(this);
            m_IsVisible = true;
        }

        protected void InvokeSizeRatiosChanged()
        {
            OnSizeRatiosChanged?.Invoke();
        }

        protected void InvokeStateChanged()
        {
            OnStateChanged?.Invoke();
        }
        
        protected void InvokeTransformChanged()
        {
            UpdateSizeRatios();
            OnTransformChanged?.Invoke();
        }
        
        void OnCameraTransformChangedInternal(CameraModel model)
        {
            UpdateSizeRatios();
            OnCameraTransformChanged?.Invoke();
        }

        void OnCameraViewportSizeChangedInternal(CameraModel model, Vector2 size)
        {
            UpdateSizeRatios();
            OnCameraViewportSizeChanged?.Invoke(model, size);
        }

        void OnCameraViewportCursorChangedInternal(CameraModel model, Vector2 pointerPosition)
        {
            OnCameraViewportCursorChanged?.Invoke(model, pointerPosition);
        }
        
        public void Step(float delta)
        {
            OnStep?.Invoke(delta);
        }
        
        protected virtual void UpdateSizeRatios()
        {
            ThicknessRatio = m_ThicknessSpace == SpaceType.World ? 1f : HandlesUtils.ComputeSizeRatio(Camera, Position);
            SizeRatio = m_SizeSpace == SpaceType.World ? 1f : HandlesUtils.ComputeSizeRatio(Camera, Position);
        }

        // [Section] User Input Methods
        
        public virtual void OnViewPointerEntered(PointerEventData eventData)
        {
            IsHighlighted = true;
            eventData.Use();
        }

        public virtual void OnViewPointerExited(PointerEventData eventData)
        {
            IsHighlighted = false;
            eventData.Use();
        }

        public virtual void OnViewBegunDrag(PointerEventData eventData)
        {
            if (eventData.button != 0)
                return;
            
            DragStartRotation = Rotation;
            DragStartPosition = Position;
            
            var dragWorldPointerPosition = GetWorldPointerPosition(eventData);
            
            DragStartPointerPosition = dragWorldPointerPosition;
            DragPreviousPointerPosition = dragWorldPointerPosition;
            
            IsDragging = true;
            
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(eventData.pointerDrag, eventData);
            }

            eventData.Use();
        }

        public virtual void OnViewDragged(PointerEventData eventData)
        {
            if (!IsDragging)
            {
                DragDelta = Vector3.zero;
                DragDeltaTotal = Vector3.zero;
                return;
            }

            var dragPosition = GetWorldPointerPosition(eventData);
            DragDelta = dragPosition - DragPreviousPointerPosition;
            DragDeltaTotal = dragPosition - DragStartPointerPosition;
            DragPreviousPointerPosition = dragPosition;
            
            eventData.Use();
        }

        public virtual void OnViewEndedDrag(PointerEventData eventData)
        {
            var dragPosition = GetWorldPointerPosition(eventData);

            DragEndPosition = dragPosition;
            DragDelta = Vector3.zero;

            if (EventSystem.current != null)
            {
                if (EventSystem.current.currentSelectedGameObject == eventData.pointerDrag)
                    EventSystem.current.SetSelectedGameObject(null, eventData);
            }
            
            IsDragging = false;
            eventData.Use();
        }

        public virtual void OnViewCancelled(BaseEventData eventData)
        {
            if (!IsDragging)
                return;

            CancelDrag();
            eventData.Use();
        }

        public virtual void OnViewDeselected(BaseEventData eventData)
        {
            if (!IsDragging)
                return;

            CancelDrag();
            eventData.Use();
        }

        protected virtual void OnViewDragCancelled()
        {
            
        }

        public void CancelDrag()
        {
            if (!IsDragging)
                return;
            
            OnViewDragCancelled();
            IsDragging = false;
        }

        Vector3 GetWorldPointerPosition(PointerEventData eventData)
        {
            return ScreenUtils.ScreenToWorld(Camera, eventData.position, Position);
        }

        public void Clear()
        {
            m_CameraModel.UnregisterControl(this);
        }
        
        
    }
}
