using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class CircleControlViewModel : ControlViewModel
    {
        public float Radius => m_ControlModel.Radius;
        public float RadiusPadding => m_ControlModel.ColliderRadiusPadding;
        public Color Color
        {
            get
            {
                if (IsHighlighted)
                    return m_ControlModel.HighlightedColor;

                return m_ControlModel.BaseColor;
            }
        }
        public override Vector3 Position => m_ControlModel.Position;
        
        public override Quaternion Rotation => Quaternion.identity;

        public delegate void ShapeChanged();
        public event ShapeChanged OnShapeChanged;

        public delegate void ColorSchemeChanged();
        public event ColorSchemeChanged OnColorSchemeChanged;

        public delegate void PositionChanged(Vector3 newPosition);
        public event PositionChanged OnPositionChanged;

        CircleControlModel m_ControlModel;

        public CircleControlViewModel(CircleControlModel controlModel, CameraModel cameraModel, SpaceType sizeSpace = SpaceType.Screen, SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder) :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            m_ControlModel = controlModel;
            m_ControlModel.OnPositionChanged += (model, position) =>
            {
                UpdateSizeRatios();
                OnPositionChanged?.Invoke(position);
            };

            m_ControlModel.OnShapeChanged += model => OnShapeChanged?.Invoke();
            m_ControlModel.OnColorChanged += model => OnColorSchemeChanged?.Invoke();
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);
            
            if (!IsDragging)
                return;
            
            m_ControlModel.Position += DragDelta;
        }

        protected override void OnViewDragCancelled()
        {
            base.OnViewDragCancelled();
            
            m_ControlModel.Position = DragStartPosition;
        }
    }
}
