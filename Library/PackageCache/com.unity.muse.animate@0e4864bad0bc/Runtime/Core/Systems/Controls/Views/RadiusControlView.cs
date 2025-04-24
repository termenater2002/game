using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class RadiusControlView : ControlView, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler, IDeselectHandler
    {
        RadiusControlViewModel m_Model;
        SphereCollider m_Collider;
        DiscMesh m_CircleShape;

        public void SetModel(RadiusControlViewModel model)
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

            m_Model.OnRadiusChanged += OnRadiusChanged;
            m_Model.OnColorSchemeChanged += OnColorSchemeChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Collider.enabled = false;
            m_CircleShape.enabled = false;

            m_Model.OnRadiusChanged -= OnRadiusChanged;
            m_Model.OnColorSchemeChanged -= OnColorSchemeChanged;
        }

        // [Section] HandleElementView Methods Overrides

        public override void Step(float delta)
        {
            base.Step(delta);
            
            // Note: Force an update on Shape Meshes to make sure
            // they are updated AFTER the handles updates
            m_CircleShape.Update();
        }
        
        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();
            
            m_Collider = CreateCollider<SphereCollider>(gameObject);
            m_Collider.enabled = false;

            m_CircleShape = HandlesUtils.CreateDisc(gameObject, "Circle");
            m_CircleShape.RotationOffset = new Vector3(-90f, 0f, 0f);
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Collider.radius = m_Model.ColliderRadius;
            m_CircleShape.Radius = m_Model.Radius;
            m_CircleShape.BorderSize = HandlesUtils.LineSize * m_Model.SizeRatio;
        }

        protected override void UpdateColor()
        {
            base.UpdateColor();
            var colorLine = m_Model.Color;
            m_CircleShape.ColorBorder = colorLine;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            transform.position = m_Model.Position;
        }

        protected override void UpdateCamera()
        {
            base.UpdateCamera();
            var modelCamera = m_Model.Camera;
            m_CircleShape.transform.rotation = modelCamera.Target.transform.rotation;
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            m_Collider.enabled = m_Model.IsVisible;
            m_CircleShape.enabled = m_Model.IsVisible;
        }

        // [Section] View Model Events Handlers

        void OnRadiusChanged()
        {
            QueueUpdate(UpdateFlags.Shape);
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
