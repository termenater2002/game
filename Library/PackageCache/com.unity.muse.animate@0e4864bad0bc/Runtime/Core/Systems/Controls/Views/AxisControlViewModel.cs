using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class AxisControlViewModel : ControlViewModel
    {
        public Vector3 LocalAxis => m_Model.LocalAxis;
        Vector3 WorldAxis => m_Model.WorldAxis;

        public Color Color
        {
            get
            {
                if (IsHighlighted)
                    return m_Model.HighlightedColor;

                return m_Model.BaseColor;
            }
        }

        public override Vector3 Position => m_Model.Position;
        public override Quaternion Rotation => m_Model.Rotation;

        public delegate void ColorSchemeChanged();
        public event ColorSchemeChanged OnColorSchemeChanged;
        public delegate void LocalAxisChanged(Vector3 newAxis);
        public event LocalAxisChanged OnLocalAxisChanged;

        AxisControlModel m_Model;
        bool m_IsDragging;
        Vector3 m_LastAxisPosition;

        public AxisControlViewModel(AxisControlModel model, CameraModel cameraModel, SpaceType sizeSpace = SpaceType.Screen, SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder) :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            m_Model = model;
            m_Model.OnTransformChanged += (_, _, _) =>
            {
                InvokeTransformChanged();
            };

            m_Model.OnAxisChanged += (_, newAxis) => OnLocalAxisChanged?.Invoke(newAxis);
            m_Model.OnColorChanged += _ => OnColorSchemeChanged?.Invoke();
        }

        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            base.OnViewBegunDrag(eventData);
            
            var dragPosition = ScreenToAxisPoint(eventData.position);
            m_LastAxisPosition = dragPosition;
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);
            
            if (!IsDragging)
                return;

            var axisPosition = ScreenToAxisPoint(eventData.position);
            var offset = axisPosition - m_LastAxisPosition;
            m_LastAxisPosition = axisPosition;
            m_Model.TranslateAlongAxis(offset);

            eventData.Use();
        }

        protected override void OnViewDragCancelled()
        {
            base.OnViewDragCancelled();
            m_Model.Position = DragStartPointerPosition;
        }

        Vector3 ScreenToAxisPoint(Vector3 screenPoint)
        {
            var worldOrigin = Position;
            var worldDirection = WorldAxis;

            return ScreenUtils.ScreenToAxisPoint(Camera, screenPoint, worldOrigin, worldDirection);
        }
    }
}
