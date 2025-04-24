using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class PlaneControlView : ControlView, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler, IDeselectHandler
    {
        PlaneControlViewModel m_Model;
        BoxCollider m_Collider;
        QuadMesh m_FillShape;
        
        LineRenderer m_OutlineShape;
        LineRenderer m_DraggingLineFirstPositive;
        LineRenderer m_DraggingLineFirstNegative;
        LineRenderer m_DraggingLineSecondPositive;
        LineRenderer m_DraggingLineSecondNegative;
        
        Gradient m_OutlineShapeGradient;
        Gradient m_DraggingLineFirstPositiveGradient;
        Gradient m_DraggingLineFirstNegativeGradient;
        Gradient m_DraggingLineSecondPositiveGradient;
        Gradient m_DraggingLineSecondNegativeGradient;
        
        float m_Alpha = 1f;

        const float k_Size = 0.1f;
        const float k_PaddingFromCenter = 0.3f;
        const float k_ColliderSizePadding = 0.025f;
        const float k_ColliderDepth = 0.05f;
        const float k_DraggingLineLength = 100f;

        public void SetModel(PlaneControlViewModel model)
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
            m_Model.OnColorSchemeChanged += OnColorSchemeChanged;
            m_Model.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Collider.enabled = false;
            m_FillShape.enabled = false;
            m_OutlineShape.enabled = false;
            m_DraggingLineFirstPositive.enabled = false;
            m_DraggingLineFirstNegative.enabled = false;
            m_DraggingLineSecondPositive.enabled = false;
            m_DraggingLineSecondNegative.enabled = false;

            m_Model.OnColorSchemeChanged -= OnColorSchemeChanged;
            m_Model.OnDraggingStateChanged -= OnDraggingStateChanged;
        }

        // [Section] HandleElementView Methods Overrides

        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();
            
            m_OutlineShapeGradient = new Gradient();
            m_DraggingLineFirstPositiveGradient = new Gradient();
            m_DraggingLineFirstNegativeGradient = new Gradient();
            m_DraggingLineSecondPositiveGradient = new Gradient();
            m_DraggingLineSecondNegativeGradient = new Gradient();
            
            var colliderObject = CreateChild("Collider");
            m_Collider = CreateCollider<BoxCollider>(colliderObject);
            m_Collider.enabled = false;

            m_FillShape = HandlesUtils.CreateQuad(gameObject, "Fill");
            m_FillShape.SetQuadVertex(0, Vector3.zero);
            m_FillShape.SetQuadVertex(1, Vector3.zero);
            m_FillShape.SetQuadVertex(2, Vector3.zero);
            m_FillShape.SetQuadVertex(3, Vector3.zero);

            m_OutlineShape = HandlesUtils.CreateLine(gameObject, "Outline");
            m_OutlineShape.loop = true;
            m_OutlineShape.positionCount = 4;
            
            // Create the dragging line shapes
            m_DraggingLineFirstPositive = HandlesUtils.CreateLine(gameObject, "DraggingFirstPositive");
            m_DraggingLineFirstPositive.positionCount = 2;
            m_DraggingLineFirstPositive.SetPositions(new[] { Vector3.zero, k_DraggingLineLength * Vector3.right });

            m_DraggingLineFirstNegative = HandlesUtils.CreateLine(gameObject, "DraggingFirstNegative");
            m_DraggingLineFirstNegative.positionCount = 2;
            m_DraggingLineFirstNegative.SetPositions(new[] { -k_DraggingLineLength * Vector3.right, Vector3.zero });

            m_DraggingLineSecondPositive = HandlesUtils.CreateLine(gameObject, "DraggingSecondPositive");
            m_DraggingLineSecondPositive.positionCount = 2;
            m_DraggingLineSecondPositive.SetPositions(new[] { Vector3.zero, k_DraggingLineLength * Vector3.up });

            m_DraggingLineSecondNegative = HandlesUtils.CreateLine(gameObject, "DraggingSecondNegative");
            m_DraggingLineSecondNegative.positionCount = 2;
            m_DraggingLineSecondNegative.SetPositions(new[] { -k_DraggingLineLength * Vector3.up, Vector3.zero });
        }

        public override void Step(float delta)
        {
            base.Step(delta);
            
            // Note: Force an update on Shape Meshes to make sure
            // they are updated AFTER the handles updates
            m_FillShape.Update();
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            
            // Update the local plane
            UpdatePlane();
            
            // Update the collider
            var size = (k_ColliderSizePadding * 2 + k_Size) * m_Model.SizeRatio;
            var padding = (k_PaddingFromCenter - k_ColliderSizePadding) * m_Model.SizeRatio;
            var depth = k_ColliderDepth * m_Model.SizeRatio;
            var localPlaneUV = m_Model.LocalFirstAxis + m_Model.LocalSecondAxis;
            m_Collider.center = localPlaneUV * (size / 2f + padding);
            m_Collider.size = localPlaneUV * size + m_Model.LocalNormal * depth;
            
            // Update the shape geometry
            var firstVector = m_Model.LocalFirstAxis;
            var secondVector = m_Model.LocalSecondAxis;
            var cornerVector = secondVector + firstVector;

            var corner0 = k_PaddingFromCenter * (cornerVector * m_Model.SizeRatio);
            var corner1 = corner0 + k_Size * (secondVector * m_Model.SizeRatio);
            var corner2 = corner0 + k_Size * (cornerVector * m_Model.SizeRatio);
            var corner3 = corner0 + k_Size * (firstVector * m_Model.SizeRatio);

            m_FillShape.SetQuadVertex(0, corner0);
            m_FillShape.SetQuadVertex(1, corner1);
            m_FillShape.SetQuadVertex(2, corner2);
            m_FillShape.SetQuadVertex(3, corner3);

            m_OutlineShape.SetPosition(0, corner0);
            m_OutlineShape.SetPosition(1, corner1);
            m_OutlineShape.SetPosition(2, corner2);
            m_OutlineShape.SetPosition(3, corner3);
            
            m_OutlineShape.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            
            m_DraggingLineFirstPositive.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_DraggingLineFirstNegative.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_DraggingLineSecondPositive.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
            m_DraggingLineSecondNegative.widthMultiplier = HandlesUtils.LineSize * m_Model.SizeRatio;
        }

        void UpdatePlane()
        {
            var rotation = Quaternion.FromToRotation(Vector3.forward, m_Model.LocalNormal);
            m_DraggingLineFirstPositive.transform.localRotation = rotation;
            m_DraggingLineFirstNegative.transform.localRotation = rotation;
            m_DraggingLineSecondPositive.transform.localRotation = rotation;
            m_DraggingLineSecondNegative.transform.localRotation = rotation;
        }
        
        protected override void UpdateColor()
        {
            base.UpdateColor();
            
            var normalLineColor = m_Model.NormalColor;
            normalLineColor.a *= m_Alpha;

            var normalFillColor = normalLineColor;
            normalFillColor.a *= HandleColors.FillAlphaFactor;

            m_FillShape.Color = normalFillColor;
            
            m_OutlineShapeGradient.SetKeys(
                new GradientColorKey[] { new(normalFillColor, 0.0f), new(normalFillColor, 1.0f) },
                new GradientAlphaKey[] { new(normalFillColor.a, 0.0f), new(normalFillColor.a, 1.0f) }
            );
            
            m_OutlineShape.colorGradient = m_OutlineShapeGradient;

            // First Axis Drag Line
            var firstAxisColor = m_Model.FirstAxisColor;
            var firstAxisTransparentColor = firstAxisColor;
            firstAxisTransparentColor.a = 0f;
            
            m_DraggingLineFirstPositiveGradient.SetKeys(
                new GradientColorKey[] { new(firstAxisColor, 0.0f), new(firstAxisTransparentColor, 1.0f) },
                new GradientAlphaKey[] { new(firstAxisColor.a, 0.0f), new(firstAxisTransparentColor.a, 1.0f) }
            );
            
            m_DraggingLineFirstNegativeGradient.SetKeys(
                new GradientColorKey[] { new(firstAxisTransparentColor, 0.0f), new(firstAxisColor, 1.0f) },
                new GradientAlphaKey[] { new(firstAxisTransparentColor.a, 0.0f), new(firstAxisColor.a, 1.0f) }
            );
            
            m_DraggingLineFirstPositive.colorGradient = m_DraggingLineFirstPositiveGradient;
            m_DraggingLineFirstNegative.colorGradient = m_DraggingLineFirstNegativeGradient;
            
            // Second Axis Drag Line
            var secondAxisColor = m_Model.SecondAxisColor;
            var secondAxisTransparentColor = secondAxisColor;
            secondAxisTransparentColor.a = 0f;
            
            m_DraggingLineSecondPositiveGradient.SetKeys(
                new GradientColorKey[] { new(secondAxisColor, 0.0f), new(secondAxisTransparentColor, 1.0f) },
                new GradientAlphaKey[] { new(secondAxisColor.a, 0.0f), new(secondAxisTransparentColor.a, 1.0f) }
            );
            
            m_DraggingLineSecondNegativeGradient.SetKeys(
                new GradientColorKey[] { new(secondAxisTransparentColor, 0.0f), new(secondAxisColor, 1.0f) },
                new GradientAlphaKey[] { new(secondAxisTransparentColor.a, 0.0f), new(secondAxisColor.a, 1.0f) }
            );
            
            m_DraggingLineSecondPositive.colorGradient = m_DraggingLineSecondPositiveGradient;
            m_DraggingLineSecondNegative.colorGradient = m_DraggingLineSecondNegativeGradient;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            
            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);

            if (m_Model.IsDragging)
                UpdateDraggingLinePosition();
        }

        void UpdateDraggingLinePosition()
        {
            m_DraggingLineFirstNegative.transform.position = m_Model.DragStartPointerPosition;
            m_DraggingLineFirstPositive.transform.position = m_Model.DragStartPointerPosition;
            m_DraggingLineSecondNegative.transform.position = m_Model.DragStartPointerPosition;
            m_DraggingLineSecondPositive.transform.position = m_Model.DragStartPointerPosition;
        }

        protected override void UpdateAlpha()
        {
            base.UpdateAlpha();
            var worldForward = m_Model.WorldNormal;
            m_Alpha = ComputeVisibility(worldForward, m_Model.Position, false);
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            
            var isDragging = m_Model.IsDragging;
            UpdateDraggingLinePosition();
            m_Collider.enabled = m_Model.IsVisible && m_Alpha > 0.25f;
            m_FillShape.enabled = m_Model.IsVisible && !isDragging;
            m_OutlineShape.enabled = m_Model.IsVisible && !isDragging;

            m_DraggingLineFirstPositive.enabled = isDragging;
            m_DraggingLineFirstNegative.enabled = isDragging;
            m_DraggingLineSecondPositive.enabled = isDragging;
            m_DraggingLineSecondNegative.enabled = isDragging;
        }

        // [Section] ViewModel Events Handlers

        void OnDraggingStateChanged(ControlViewModel control)
        {
            QueueUpdate(UpdateFlags.Visibility);
        }

        void OnColorSchemeChanged()
        {
            QueueUpdate(UpdateFlags.Color);
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
            m_Model?.OnCancel(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_Model?.OnDeselect(eventData);
        }

        
    }
}
