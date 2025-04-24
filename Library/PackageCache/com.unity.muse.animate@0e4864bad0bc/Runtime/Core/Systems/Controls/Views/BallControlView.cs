using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class BallControlView : ControlView, IBeginDragHandler, IEndDragHandler, IDragHandler, ICancelHandler, IDeselectHandler
    {
        BallControlViewModel m_Model;
        SphereCollider m_Collider;

        //TODO: Replace sphere shape with a procedural sphere (Must create a HandleMeshSphere)
        //Sphere m_Ball;

        public void SetModel(BallControlViewModel model)
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
            m_Model.OnTransformChanged += OnTransformChanged;
            m_Model.OnDraggingStateChanged += OnDraggingStateChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Collider.enabled = false;

            //m_Ball.enabled = false;

            m_Model.OnColorSchemeChanged -= OnColorSchemeChanged;
            m_Model.OnTransformChanged -= OnTransformChanged;
            m_Model.OnDraggingStateChanged -= OnDraggingStateChanged;
        }

        // [Section] HandleElementView Methods Overrides

        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();
            m_Collider = CreateCollider<SphereCollider>(gameObject);

            /*
             Note: See note at top of document
            m_Ball = CreateShape<Sphere>(gameObject);
            m_Ball.DetailLevel = DetailLevel.Medium;
            m_Ball.RadiusSpace = ThicknessSpace.Meters;
            m_Ball.enabled = false;
            */
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Collider.radius = m_Model.Radius;
            UpdateScaling();
            /*
             Note: See note at top of document
            m_Ball.Radius = m_Model.Radius;
            */
        }

        protected override void UpdateColor()
        {
            base.UpdateColor();

            var fillColor = m_Model.Color;
            fillColor.a *= HandleColors.FillAlphaFactor;
            /*
             Note: See note at top of document
            m_Ball.Color = fillColor;
            */
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);
        }

        protected void UpdateScaling()
        {
            transform.localScale = Vector3.one * m_Model.SizeRatio;
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            m_Collider.enabled = m_Model.IsVisible;
            /*
             Note: See note at top of document
            m_Ball.enabled = m_Model.IsVisible;
            */
        }

        // [Section] View Model Events Handlers

        void OnDraggingStateChanged(ControlViewModel control)
        {
            ResetUpdate(UpdateFlags.Visibility);
        }

        void OnColorSchemeChanged()
        {
            ResetUpdate(UpdateFlags.Color);
        }

        void OnTransformChanged(Vector3 newPosition, Quaternion newRotation)
        {
            QueueUpdate(UpdateFlags.Transform);
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
