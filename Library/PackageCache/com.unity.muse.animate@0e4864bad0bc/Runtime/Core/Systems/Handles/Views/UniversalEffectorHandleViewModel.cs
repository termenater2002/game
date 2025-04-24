using UnityEngine;

namespace Unity.Muse.Animate
{
    class UniversalEffectorHandleViewModel : HandleViewModel
    {
        public CircleControlViewModel CircleControlViewModel => m_CircleControlViewModel;
        public BallControlViewModel BallControlViewModel => m_BallControlViewModel;
        public RingControlViewModel RingControlViewModelFront => m_RingControlViewModelFront;

        public bool IsDragging => m_CircleControlViewModel.IsDragging
            || m_RingControlViewModelFront.IsDragging
            || m_BallControlViewModel.IsDragging;

        bool IsRotationEnabled => m_UniversalEffectorHandleModel.RotationEnabled;

        CircleControlViewModel m_CircleControlViewModel;
        BallControlViewModel m_BallControlViewModel;
        RingControlViewModel m_RingControlViewModelFront;

        UniversalEffectorHandleModel m_UniversalEffectorHandleModel;

        public UniversalEffectorHandleViewModel(UniversalEffectorHandleModel rotationHandleModel, CameraModel cameraModel) :
            base(cameraModel)
        {
            m_UniversalEffectorHandleModel = rotationHandleModel;
            m_UniversalEffectorHandleModel.OnRotationChanged += OnHandleRotationChanged;
            m_UniversalEffectorHandleModel.OnChanged += OnHandleChanged;

            cameraModel.OnTransformChanged += OnCameraTransformChanged;

            InitializeCircle();
            InitializeAxes();

            OnVisibilityChanged += UpdateElementsVisibility;

            UpdateElementsVisibility();
            UpdateFrontRing();
        }

        void OnHandleChanged(UniversalEffectorHandleModel model)
        {
            UpdateElementsVisibility();
        }

        void OnCameraTransformChanged(CameraModel model)
        {
            UpdateFrontRing();
        }

        void UpdateFrontRing()
        {
            m_UniversalEffectorHandleModel.FrontControlModel.WorldAxis = CameraModel.Rotation * Vector3.forward;
        }

        void InitializeCircle()
        {
            m_CircleControlViewModel = new CircleControlViewModel(m_UniversalEffectorHandleModel.CircleControlModel, CameraModel);
            m_CircleControlViewModel.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void InitializeAxes()
        {
            m_BallControlViewModel = new BallControlViewModel(m_UniversalEffectorHandleModel.ballControlModel, CameraModel);
            m_RingControlViewModelFront = new RingControlViewModel(m_UniversalEffectorHandleModel.FrontControlModel, CameraModel, RingControlViewModel.DragModeType.Rotate);

            m_BallControlViewModel.OnDraggingStateChanged += OnDraggingStateChanged;
            m_RingControlViewModelFront.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void OnDraggingStateChanged(ControlViewModel control)
        {
            UpdateElementsVisibility();
        }

        void UpdateElementsVisibility()
        {
            var isSomethingDragging = IsDragging;

            m_CircleControlViewModel.IsVisible = IsVisible && !isSomethingDragging;
            m_BallControlViewModel.IsVisible = IsRotationEnabled && IsVisible && (!isSomethingDragging || m_BallControlViewModel.IsDragging);
            m_RingControlViewModelFront.IsVisible = IsRotationEnabled && IsVisible && (!isSomethingDragging || m_RingControlViewModelFront.IsDragging);
        }
        
        public void CancelDrag()
        {
            m_CircleControlViewModel.CancelDrag();
            m_BallControlViewModel.CancelDrag();
            m_RingControlViewModelFront.CancelDrag();
        }
        
        void OnHandleRotationChanged(UniversalEffectorHandleModel model, Quaternion newRotation)
        {
            UpdateFrontRing();
            InvokeRotationChanged();
        }
    }
}
