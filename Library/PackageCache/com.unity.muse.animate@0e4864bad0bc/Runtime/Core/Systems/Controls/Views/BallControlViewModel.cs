using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class BallControlViewModel : ControlViewModel
    {
        public Color Color
        {
            get
            {
                if (IsHighlighted)
                    return m_BallControlModel.HighlightedColor;

                return m_BallControlModel.BaseColor;
            }
        }

        public override Vector3 Position => m_BallControlModel.Position;
        public override Quaternion Rotation => m_BallControlModel.Rotation;
        public float Radius => m_BallControlModel.Radius;

        public delegate void ColorSchemeChanged();
        public event ColorSchemeChanged OnColorSchemeChanged;

        BallControlModel m_BallControlModel;

        public BallControlViewModel(BallControlModel ballControlModel, CameraModel cameraModel, SpaceType sizeSpace = SpaceType.Screen, SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder) :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            m_BallControlModel = ballControlModel;
            m_BallControlModel.OnTransformChanged += (model, position, rotation) =>
            {
                UpdateSizeRatios();
                InvokeTransformChanged();
            };

            m_BallControlModel.OnColorChanged += model => OnColorSchemeChanged?.Invoke();
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);
            
            if (!IsDragging)
                return;

            var cameraForward = CameraModel.Rotation * Vector3.forward;
            var rotationAxis = Vector3.Cross(DragDeltaTotal.normalized, cameraForward);
            var rotationAngleDeg = DragDeltaTotal.magnitude * 100f;
            var worldRotation = Quaternion.AngleAxis(rotationAngleDeg, rotationAxis);
            
            m_BallControlModel.Rotation = worldRotation * DragStartRotation;
        }

        protected override void OnViewDragCancelled()
        {
            base.OnViewDragCancelled();
            m_BallControlModel.Rotation = DragStartRotation;
        }
    }
}
