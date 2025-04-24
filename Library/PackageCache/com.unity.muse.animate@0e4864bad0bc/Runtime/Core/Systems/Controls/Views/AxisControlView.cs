using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class AxisControlView : ControlView, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler, IDeselectHandler
    {
        AxisControlViewModel m_Model;
        CapsuleCollider m_Collider;
        ConeMesh m_ArrowTip;
        
        LineRenderer m_ArrowLine;
        LineRenderer m_DraggingLinePositive;
        LineRenderer m_DraggingLineNegative;

        Gradient m_ArrowLineGradient;
        Gradient m_DraggingLinePositiveGradient;
        Gradient m_DraggingLineNegativeGradient;

        float m_Alpha = 1f;

        const float k_ArrowPaddingFromCenter = 0.12f;
        const float k_ArrowLength = 0.6f;
        const float k_ArrowTipLength = 0.15f;
        const float k_ArrowTipRadius = 0.04f;
        const float k_ColliderLengthPadding = 0.1f;
        const float k_ColliderRadiusPadding = 0.03f;
        const float k_ColliderHeight = (k_ColliderLengthPadding * 2 + k_ArrowLength + k_ArrowTipLength);
        const float k_ColliderPaddingFromCenter = (k_ArrowPaddingFromCenter - k_ColliderLengthPadding);
        const float k_ColliderRadius = (k_ColliderRadiusPadding + k_ArrowTipRadius);
        const float k_DraggingLineLength = 100f;

        public void SetModel(AxisControlViewModel model)
        {
            base.SetModel(model);
            UnregisterModel();
            m_Model = model;
            RegisterModel();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            UpdateAll();

            m_Model.OnLocalAxisChanged += OnLocalAxisChanged;
            m_Model.OnColorSchemeChanged += OnColorSchemeChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Collider.enabled = false;
            m_ArrowLine.enabled = false;
            m_ArrowTip.enabled = false;
            m_DraggingLinePositive.enabled = false;
            m_DraggingLineNegative.enabled = false;

            m_Model.OnLocalAxisChanged -= OnLocalAxisChanged;
            m_Model.OnColorSchemeChanged -= OnColorSchemeChanged;
        }
        
        // [Section] HandleElementView Methods Overrides

        public override void CreateShapesAndColliders()
        {
            // Colliders
            var colliderObject = CreateChild("Collider");
            m_Collider = CreateCollider<CapsuleCollider>(colliderObject);
            m_Collider.center = Vector3.forward * (k_ColliderPaddingFromCenter + k_ColliderHeight / 2f);
            m_Collider.direction = 2; // Z
            m_Collider.height = k_ColliderHeight;
            m_Collider.radius = k_ColliderRadius;
            m_Collider.enabled = false;

            // Arrow Line
            m_ArrowLine = HandlesUtils.CreateLine(gameObject, "Line");
            m_ArrowLine.SetPositions(new[]
            {
                k_ArrowPaddingFromCenter * Vector3.forward,
                (k_ArrowLength + k_ArrowPaddingFromCenter) * Vector3.forward
            });

            m_ArrowLineGradient = new Gradient();

            // Arrow Tip
            m_ArrowTip = HandlesUtils.CreateCone(gameObject, "Tip");
            m_ArrowTip.Radius = k_ArrowTipRadius;
            m_ArrowTip.Height = k_ArrowTipLength;
            m_ArrowTip.RotationOffset = new Vector3(90, 0, 0);
            
            // Dragging Lines
            // Note: we split dragging line into 2 parts to make sure LineRenderers uses the origin point to compute line width
            // Otherwise line width becomes unpredictable
            m_DraggingLinePositiveGradient = new Gradient();
            m_DraggingLineNegativeGradient = new Gradient();

            m_DraggingLinePositive = HandlesUtils.CreateLine(gameObject, "Dragging Line (Positive)");
            m_DraggingLineNegative = HandlesUtils.CreateLine(gameObject, "Dragging Line (Negative)");

            m_DraggingLinePositive.SetPositions(new[]
            {
                Vector3.zero,
                k_DraggingLineLength * Vector3.forward
            });

            m_DraggingLineNegative.SetPositions(new[]
            {
                -k_DraggingLineLength * Vector3.forward,
                Vector3.zero
            });
        }

        public override void Step(float delta)
        {
            base.Step(delta);
            
            // Note: Force an update on Shape Meshes to make sure
            // they are updated AFTER the handles updates
            m_ArrowTip.Update();
        }

        protected override void UpdateCamera()
        {
            base.UpdateCamera();
            UpdateScaling();
        }
        
        protected override void UpdateShape()
        {
            base.UpdateShape();
            UpdateLocalAxis();
            UpdateScaling();
        }
        
        void UpdateLocalAxis()
        {
            var rotation = Quaternion.FromToRotation(Vector3.forward, m_Model.LocalAxis);

            m_Collider.transform.localRotation = rotation;
            m_ArrowLine.transform.localRotation = rotation;
            m_DraggingLinePositive.transform.localRotation = rotation;
            m_DraggingLineNegative.transform.localRotation = rotation;

            var tipTransform = m_ArrowTip.transform;
            tipTransform.localPosition = (k_ArrowLength + k_ArrowPaddingFromCenter) * m_Model.LocalAxis;
            tipTransform.localRotation = rotation;
        }

        void UpdateScaling()
        {
            transform.localScale = Vector3.one * m_Model.SizeRatio;
            m_DraggingLineNegative.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_DraggingLinePositive.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_ArrowLine.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
        }
        
        protected override void UpdateColor()
        {
            base.UpdateColor();
            
            var colorLine = m_Model.Color;
            colorLine.a *= m_Alpha;

            var transparentColor = colorLine;
            transparentColor.a = 0f;

            m_ArrowTip.Color = colorLine;

            // Set the Arrow Line Color
            m_ArrowLineGradient.SetKeys(
                new GradientColorKey[] { new(colorLine, 0.0f), new(colorLine, 1.0f) },
                new GradientAlphaKey[] { new(colorLine.a, 0.0f), new(colorLine.a, 1.0f) }
            );

            m_ArrowLine.colorGradient = m_ArrowLineGradient;

            // Set the Line Renderers Color Gradient
            m_DraggingLinePositiveGradient.SetKeys(
                new GradientColorKey[] { new(colorLine, 0.0f), new(transparentColor, 1.0f) },
                new GradientAlphaKey[] { new(colorLine.a, 0.0f), new(transparentColor.a, 1.0f) }
            );

            m_DraggingLineNegativeGradient.SetKeys(
                new GradientColorKey[] { new(transparentColor, 0.0f), new(colorLine, 1.0f) },
                new GradientAlphaKey[] { new(transparentColor.a, 0.0f), new(colorLine.a, 1.0f) }
            );

            m_DraggingLinePositive.colorGradient = m_DraggingLinePositiveGradient;
            m_DraggingLineNegative.colorGradient = m_DraggingLineNegativeGradient;
        }
        
        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);
        }

        protected override void UpdateAlpha()
        {
            base.UpdateAlpha();
            var worldForward = m_Collider.transform.rotation * Vector3.forward;
            m_Alpha = ComputeVisibility(worldForward, m_Model.Position, true);
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            
            var isDragging = m_Model.IsDragging;
            
            m_Collider.enabled = m_Model.IsVisible && m_Alpha > 0.25f;
            m_ArrowLine.enabled = m_Model.IsVisible && !isDragging;
            m_ArrowTip.enabled = m_Model.IsVisible && !isDragging;
            m_DraggingLinePositive.enabled = isDragging;
            m_DraggingLineNegative.enabled = isDragging;
        }

        // [Section] View Model Events Handlers

        void OnColorSchemeChanged()
        {
            QueueUpdate(UpdateFlags.Color);
        }
        
        void OnLocalAxisChanged(Vector3 newAxis)
        {
            QueueUpdate(UpdateFlags.Shape);
        }

        // [Section] User Inputs Events Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_Model?.OnViewBegunDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_Model?.OnViewEndedDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_Model?.OnViewDragged(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            m_Model?.OnViewCancelled(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_Model?.OnViewDeselected(eventData);
        }
        
        
    }
}
