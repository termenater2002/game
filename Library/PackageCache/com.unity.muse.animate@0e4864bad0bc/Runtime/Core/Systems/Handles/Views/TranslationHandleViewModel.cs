using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class TranslationHandleViewModel : HandleViewModel
    {
        public CircleControlViewModel Circle { get; private set; }
        public AxisControlViewModel AxisX { get; private set; }
        public AxisControlViewModel AxisY { get; private set; }
        public AxisControlViewModel AxisZ { get; private set; }
        public PlaneControlViewModel PlaneXY { get; private set; }
        public PlaneControlViewModel PlaneXZ { get; private set; }
        public PlaneControlViewModel PlaneYZ { get; private set; }

        public bool IsDragging => Circle.IsDragging
            || AxisX.IsDragging
            || AxisY.IsDragging
            || AxisZ.IsDragging
            || PlaneXY.IsDragging
            || PlaneXZ.IsDragging
            || PlaneYZ.IsDragging;

        TranslationHandleModel m_TranslationHandleModel;

        public TranslationHandleViewModel(TranslationHandleModel translationHandleModel, CameraModel cameraModel) :
            base(cameraModel)
        {
            m_TranslationHandleModel = translationHandleModel;
            m_TranslationHandleModel.OnPositionChanged += OnTranslationHandleTransformChanged;

            InitializeCircle();
            InitializeAxes();
            InitializePlanes();

            OnVisibilityChanged += UpdateElementsVisibility;
            UpdateElementsVisibility();
        }

        void InitializeCircle()
        {
            Circle = new CircleControlViewModel(m_TranslationHandleModel.CircleControlModel, CameraModel);
            Circle.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void InitializeAxes()
        {
            AxisX = new AxisControlViewModel(m_TranslationHandleModel.AxisControlXModel, CameraModel);
            AxisY = new AxisControlViewModel(m_TranslationHandleModel.AxisControlYModel, CameraModel);
            AxisZ = new AxisControlViewModel(m_TranslationHandleModel.AxisControlZModel, CameraModel);

            AxisX.OnDraggingStateChanged += OnDraggingStateChanged;
            AxisY.OnDraggingStateChanged += OnDraggingStateChanged;
            AxisZ.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void InitializePlanes()
        {
            PlaneXY = new PlaneControlViewModel(m_TranslationHandleModel.PlaneControlXYModel, CameraModel);
            PlaneXZ = new PlaneControlViewModel(m_TranslationHandleModel.PlaneControlXZModel, CameraModel);
            PlaneYZ = new PlaneControlViewModel(m_TranslationHandleModel.PlaneControlYZModel, CameraModel);

            PlaneXY.OnDraggingStateChanged += OnDraggingStateChanged;
            PlaneXZ.OnDraggingStateChanged += OnDraggingStateChanged;
            PlaneYZ.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void OnDraggingStateChanged(ControlViewModel control)
        {
            UpdateElementsVisibility();
        }

        void UpdateElementsVisibility()
        {
            var isSomethingDragging = IsDragging;

            Circle.IsVisible = IsVisible && (!isSomethingDragging);
            AxisX.IsVisible = IsVisible && (!isSomethingDragging || AxisX.IsDragging);
            AxisY.IsVisible = IsVisible && (!isSomethingDragging || AxisY.IsDragging);
            AxisZ.IsVisible = IsVisible && (!isSomethingDragging || AxisZ.IsDragging);
            PlaneXY.IsVisible = IsVisible && (!isSomethingDragging || PlaneXY.IsDragging);
            PlaneXZ.IsVisible = IsVisible && (!isSomethingDragging || PlaneXZ.IsDragging);
            PlaneYZ.IsVisible = IsVisible && (!isSomethingDragging || PlaneYZ.IsDragging);
        }

        void OnTranslationHandleTransformChanged(TranslationHandleModel model, Vector3 newPosition)
        {
            InvokePositionChanged();
        }

        public void CancelDrag()
        {
            Circle.CancelDrag();
            AxisX.CancelDrag();
            AxisY.CancelDrag();
            AxisZ.CancelDrag();
            PlaneXY.CancelDrag();
            PlaneXZ.CancelDrag();
            PlaneYZ.CancelDrag();
        }
    }
}
