using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class RadiusControlViewModel : ControlViewModel
    {
        const float k_CircleColliderRadiusPadding = 0.05f;

        public float ColliderRadius => k_CircleColliderRadiusPadding + Mathf.Max(m_ControlModel.MinDisplayRadiusRelative * SizeRatio, m_ControlModel.Radius);
        public float Radius => Mathf.Max(m_ControlModel.MinDisplayRadiusRelative * SizeRatio, m_ControlModel.Radius);

        public Color Color
        {
            get
            {
                if (IsHighlighted)
                    return m_ControlModel.HighlightedColor;

                return m_ControlModel.BaseColor;
            }
        }
        public override Quaternion Rotation => Quaternion.identity;
        public override Vector3 Position => m_ControlModel.Position;

        public delegate void RadiusChanged();
        public event RadiusChanged OnRadiusChanged;

        public delegate void ColorSchemeChanged();
        public event ColorSchemeChanged OnColorSchemeChanged;

        RadiusControlModel m_ControlModel;
        bool m_IsDragging;
        float m_DragStartRadius;

        public RadiusControlViewModel(RadiusControlModel controlModel, CameraModel cameraModel, SpaceType sizeSpace = SpaceType.Screen,  SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder) :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            m_ControlModel = controlModel;
            m_ControlModel.OnPositionChanged += (model, position) =>
            {
                InvokeTransformChanged();
            };

            m_ControlModel.OnRadiusChanged += (model, radius) => OnRadiusChanged?.Invoke();
            m_ControlModel.OnColorChanged += model => OnColorSchemeChanged?.Invoke();
        }
        
        // [Section] HandleElementViewModel User Input Methods Overrides
        
        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            base.OnViewBegunDrag(eventData);
            m_DragStartRadius = m_ControlModel.Radius;
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);
            
            if (!IsDragging)
                return;

            var distanceFromPosition = DragStartPosition - DragPreviousPointerPosition;
            var radius = distanceFromPosition.magnitude;

            radius = Mathf.Min(radius, m_ControlModel.MaxRadius);
            var minRadius = m_ControlModel.MinDisplayRadiusRelative * SizeRatio;
            if (radius < minRadius)
                radius = m_ControlModel.SnapToZeroBelowMinRadius ? 0f : minRadius;

            m_ControlModel.Radius = radius;
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
            m_ControlModel.Radius = m_DragStartRadius;
        }
        
        
    }
}
