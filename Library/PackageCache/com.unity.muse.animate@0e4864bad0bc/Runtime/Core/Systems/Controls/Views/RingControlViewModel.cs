using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class RingControlViewModel : ControlViewModel
    {
        public event Action OnColorSchemeChanged;
        public event Action<Vector3> OnLocalAxisChanged;

        public DragModeType DragMode { get; }
        public Vector3 DragPreviousAxisPosition { get; private set; }
        public Vector3 DragStartAxisPosition { get; private set; }
        public Quaternion LocalRingRotation => m_ControlModel.LocalRingRotation;
        public override Quaternion Rotation => m_ControlModel.Rotation;
        public bool ShowRingBack => !m_ControlModel.HideBack;
        public override Vector3 Position => m_ControlModel.Position;
        float Radius => m_ControlModel.Radius;
        Vector3 LocalAxis => m_ControlModel.LocalAxis;
        Vector3 WorldAxis => m_ControlModel.WorldAxis;
        Quaternion WorldRingRotation => m_ControlModel.WorldRingRotation;

        public Color Color
        {
            get
            {
                if (IsHighlighted)
                    return m_ControlModel.HighlightedColor;

                return m_ControlModel.BaseColor;
            }
        }

        RingControlModel m_ControlModel;
        Vector3 m_DragAxisDirection;
        float m_DragCurrentAngleDeltaDeg;

        public enum DragModeType
        {
            Rotate,
            Translate
        }

        public RingControlViewModel(RingControlModel controlModel, CameraModel cameraModel, DragModeType dragMode, SpaceType sizeSpace = SpaceType.Screen, SpaceType thicknessSpace = SpaceType.Screen, int sortingOrder = ApplicationConstants.HandleRaycastOrder)
            :
            base(cameraModel, sizeSpace, thicknessSpace, sortingOrder)
        {
            Assert.IsNotNull(controlModel, "You must provide a HandleRingModel");
            Assert.IsNotNull(cameraModel, "You must provide a CameraModel");

            DragMode = dragMode;
            m_ControlModel = controlModel;
            m_ControlModel.OnTransformChanged += (model, position, rotation) =>
            {
                UpdateSizeRatios();
                InvokeTransformChanged();
            };

            m_ControlModel.OnAxisChanged += (model, newAxis) => OnLocalAxisChanged?.Invoke(newAxis);
            m_ControlModel.OnColorChanged += model => OnColorSchemeChanged?.Invoke();

            cameraModel.OnTransformChanged += model =>
            {
                UpdateSizeRatios();
            };
        }

        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            base.OnViewBegunDrag(eventData);

            if (DragMode == DragModeType.Rotate)
            {
                // - Variables used when dragging to rotate the ring
                var ringPosition = ScreenToRingPoint(eventData.position);
                DragStartAxisPosition = RingToWorld(ringPosition);
                DragPreviousAxisPosition = DragStartAxisPosition;

                var localAxisDirection = Vector3.Cross(Vector3.forward, new Vector3(ringPosition.x, ringPosition.y, 0f).normalized);
                m_DragAxisDirection = WorldRingRotation * localAxisDirection;
                m_DragCurrentAngleDeltaDeg = 0f;
            }
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);

            if (!IsDragging)
                return;

            if (DragMode == DragModeType.Rotate)
            {
                var axisPosition = ScreenToDragAxisPoint(eventData.position);
                DragPreviousAxisPosition = axisPosition;
                var worldOffset = DragPreviousAxisPosition - DragStartAxisPosition;
                var localAxisOffset = Vector3.Dot(worldOffset, m_DragAxisDirection);
                m_DragCurrentAngleDeltaDeg = localAxisOffset * 100f;
                var localRotation = Quaternion.AngleAxis(m_DragCurrentAngleDeltaDeg, LocalAxis);
                m_ControlModel.Rotation = DragStartRotation * localRotation;
            }
            else
            {
                m_ControlModel.Position += DragDelta;
            }
        }

        Vector3 ScreenToDragAxisPoint(Vector3 screenPoint)
        {
            return ScreenUtils.ScreenToAxisPoint(Camera, screenPoint, DragStartPointerPosition, m_DragAxisDirection);
        }

        public Vector3 GetRingLocalPosition(float angle)
        {
            var position = Radius * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            return position;
        }

        Vector2 ScreenToRingPoint(Vector3 screenPoint)
        {
            var worldRingRotation = WorldRingRotation;
            var worldRingRight = worldRingRotation * Vector3.right;
            var worldRingUp = worldRingRotation * Vector3.up;

            var worldPlanePosition = ScreenUtils.ScreenToPlanePoint(Camera, screenPoint, WorldAxis, Position);
            var worldOffset = worldPlanePosition - Position;
            var planeLocalPosition = new Vector2(Vector3.Dot(worldOffset, worldRingRight), Vector3.Dot(worldOffset, worldRingUp));
            var ringPosition = Radius * planeLocalPosition.normalized;

            return ringPosition;
        }

        Vector3 RingToWorld(Vector2 ringPoint)
        {
            // We need to take into account the size ratio
            var scaledRingPoint = SizeRatio * ringPoint;
            return Position + WorldRingRotation * new Vector3(scaledRingPoint.x, scaledRingPoint.y, 0f);
        }
    }
}
