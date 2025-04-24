using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class PlaneControlViewModel : ControlViewModel
    {
        public Vector3 LocalNormal => m_ControlModel.LocalNormal;
        public Vector3 WorldNormal => m_ControlModel.WorldNormal;
        public Vector3 LocalFirstAxis => m_ControlModel.LocalFirstAxis;
        public Vector3 LocalSecondAxis => m_ControlModel.LocalSecondAxis;
        
        public Color NormalColor
        {
            get
            {
                if (IsHighlighted)
                    return m_ControlModel.NormalHighlightedColor;

                return m_ControlModel.NormalBaseColor;
            }
        }

        public Color FirstAxisColor => m_ControlModel.FirstAxisBaseColor;

        public Color SecondAxisColor => m_ControlModel.SecondAxisBaseColor;

        public override Vector3 Position => m_ControlModel.Position;
        public override Quaternion Rotation => m_ControlModel.Rotation;

        public delegate void ColorSchemeChanged();
        public event ColorSchemeChanged OnColorSchemeChanged;

        PlaneControlModel m_ControlModel;
        bool m_IsDragging;
        Vector3 m_DragStartPlanePosition;

        public PlaneControlViewModel(PlaneControlModel controlModel, CameraModel cameraModel, SpaceType sizeSpace = SpaceType.Screen, SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder) :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            m_ControlModel = controlModel;
            m_ControlModel.OnTransformChanged += (model, position, rotation) =>
            {
                UpdateSizeRatios();
                InvokeTransformChanged();
            };

            m_ControlModel.OnColorChanged += model => OnColorSchemeChanged?.Invoke();
        }

        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            base.OnViewBegunDrag(eventData);
            var dragPosition = ScreenToPlanePoint(eventData.position);
            m_DragStartPlanePosition = dragPosition;
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            if (!IsDragging)
                return;

            var dragPosition = ScreenToPlanePoint(eventData.position);
            var offset = dragPosition - m_DragStartPlanePosition;
            m_DragStartPlanePosition = dragPosition;
            m_ControlModel.TranslateAlongPlane(offset);
            eventData.Use();
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (!m_IsDragging)
                return;

            CancelDragging();
            eventData.Use();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!m_IsDragging)
                return;

            CancelDragging();
            eventData.Use();
        }

        void CancelDragging()
        {
            IsDragging = false;
            m_ControlModel.Position = DragStartPointerPosition;
        }

        Vector3 ScreenToPlanePoint(Vector3 screenPoint)
        {
            return ScreenUtils.ScreenToPlanePoint(Camera, screenPoint, WorldNormal, Position);
        }
    }
}
