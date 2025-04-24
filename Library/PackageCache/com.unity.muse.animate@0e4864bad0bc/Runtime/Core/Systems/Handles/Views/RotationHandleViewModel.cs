using UnityEngine;

namespace Unity.Muse.Animate
{
    class RotationHandleViewModel : HandleViewModel
    {
        public BallControlViewModel Ball => m_Ball;
        public RingControlViewModel RingFront => m_RingFront;
        public RingControlViewModel RingX => m_RingX;
        public RingControlViewModel RingY => m_RingY;
        public RingControlViewModel RingZ => m_RingZ;

        public bool IsDragging => m_RingX.IsDragging
            || m_RingY.IsDragging
            || m_RingZ.IsDragging
            || m_RingFront.IsDragging
            || m_Ball.IsDragging;

        BallControlViewModel m_Ball;
        RingControlViewModel m_RingFront;
        RingControlViewModel m_RingX;
        RingControlViewModel m_RingY;
        RingControlViewModel m_RingZ;

        RotationHandleModel m_RotationHandleModel;

        public RotationHandleViewModel(RotationHandleModel rotationHandleModel, CameraModel cameraModel) :
            base(cameraModel)
        {
            m_RotationHandleModel = rotationHandleModel;
            m_RotationHandleModel.OnRotationChanged += OnRotationHandleTransformChanged;

            cameraModel.OnTransformChanged += OnCameraTransformChanged;
            InitializeAxes();

            OnVisibilityChanged += UpdateElementsVisibility;

            UpdateElementsVisibility();
            UpdateFrontRing();
        }

        void OnCameraTransformChanged(CameraModel model)
        {
            UpdateFrontRing();
        }

        void UpdateFrontRing()
        {
            m_RotationHandleModel.Front.WorldAxis = CameraModel.Rotation * Vector3.forward;
        }

        void InitializeAxes()
        {
            m_Ball = new BallControlViewModel(m_RotationHandleModel.Ball, CameraModel);
            m_RingFront = new RingControlViewModel(m_RotationHandleModel.Front, CameraModel, RingControlViewModel.DragModeType.Rotate);
            m_RingX = new RingControlViewModel(m_RotationHandleModel.RingX, CameraModel, RingControlViewModel.DragModeType.Rotate);
            m_RingY = new RingControlViewModel(m_RotationHandleModel.RingY, CameraModel, RingControlViewModel.DragModeType.Rotate);
            m_RingZ = new RingControlViewModel(m_RotationHandleModel.RingZ, CameraModel, RingControlViewModel.DragModeType.Rotate);

            m_Ball.OnDraggingStateChanged += OnDraggingStateChanged;
            m_RingFront.OnDraggingStateChanged += OnDraggingStateChanged;
            m_RingX.OnDraggingStateChanged += OnDraggingStateChanged;
            m_RingY.OnDraggingStateChanged += OnDraggingStateChanged;
            m_RingZ.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void OnDraggingStateChanged(ControlViewModel control)
        {
            UpdateElementsVisibility();
        }

        void UpdateElementsVisibility()
        {
            var isSomethingDragging = IsDragging;

            m_Ball.IsVisible = IsVisible && (!isSomethingDragging || m_Ball.IsDragging);
            m_RingFront.IsVisible = IsVisible && (!isSomethingDragging || m_RingFront.IsDragging);
            m_RingX.IsVisible = IsVisible && (!isSomethingDragging || m_RingX.IsDragging);
            m_RingY.IsVisible = IsVisible && (!isSomethingDragging || m_RingY.IsDragging);
            m_RingZ.IsVisible = IsVisible && (!isSomethingDragging || m_RingZ.IsDragging);
        }
        
        public void CancelDrag()
        {
            m_Ball.CancelDrag();
            m_RingFront.CancelDrag();
            m_RingX.CancelDrag();
            m_RingY.CancelDrag();
            m_RingZ.CancelDrag();
        }
        
        void OnRotationHandleTransformChanged(RotationHandleModel model, Quaternion newRotation)
        {
            UpdateFrontRing();
            InvokeRotationChanged();
        }
    }
}
