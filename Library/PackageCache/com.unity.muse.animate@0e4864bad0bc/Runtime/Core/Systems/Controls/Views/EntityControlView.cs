using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    [ExecuteAlways]
    class EntityControlView : ControlView, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler, IDeselectHandler
    {
        const float k_EffectorSize = 0.05f;
        BallLogic m_BallLogic;
        RingLogic m_RingLogic;
        EntityControlViewModel m_Model;

        public void SetModel(EntityControlViewModel model)
        {
            base.SetModel(model);

            UnregisterModel();
            m_Model = model;
            m_RingLogic.SetModel(model);
            RegisterModel();
        }

        void RegisterModel()
        {
            UpdateAll();

            if (m_Model == null)
                return;
            
            m_Model.OnStateChanged += OnStateChanged;
            m_Model.OnEffectorChanged += OnEffectorChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnStateChanged -= OnStateChanged;
            m_Model.OnEffectorChanged -= OnEffectorChanged;
        }

        void OnEffectorChanged()
        {
            QueueUpdate(UpdateFlags.Transform);
            QueueUpdate(UpdateFlags.Camera);
            QueueUpdate(UpdateFlags.Shape);
            QueueUpdate(UpdateFlags.Visibility);
        }

        void OnStateChanged()
        {
            QueueUpdate(UpdateFlags.Visibility);
        }

        // [Section] HandleElementView Methods Overrides

        public override void Step(float delta)
        {
            base.Step(delta);
            m_RingLogic?.Step(delta);
        }
        
        public override void CreateShapesAndColliders()
        {
            base.CreateShapesAndColliders();

            //var o = gameObject;
            var target = gameObject;
            m_BallLogic = new BallLogic(target);
            m_RingLogic = new RingLogic(target);
        }
        
        
        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            
            if (m_Model == null)
                return;
            
            transform.SetPositionAndRotation(m_Model.Position, m_Model.Rotation);
            
            m_BallLogic?.UpdateTransform(this, m_Model);
            m_RingLogic?.UpdateTransform(this, m_Model);
        }
        
        protected override void UpdateShape()
        {
            base.UpdateShape();

            m_BallLogic?.UpdateShape(this, m_Model);
            m_RingLogic?.UpdateShape(this, m_Model);

            if (m_Model == null)
                return;
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();

            m_BallLogic?.UpdateVisibility(this, m_Model);
            m_RingLogic?.UpdateVisibility(this, m_Model);
        }

        protected override void UpdateColor()
        {
            base.UpdateColor();

            if (m_Model == null)
                return;

            m_BallLogic.UpdateColor(this, m_Model);
            m_RingLogic.UpdateColor(this, m_Model);
        }

        public override int GetPhysicsRaycastDepth(Vector2 screenPosition, RaycastHit hit)
        {
            // Note: for effectors we pick the closest to the cursor instead of by depth
            // TODO: exception if cursor is inside the circle (return default 0 depth)?
            var distanceToCursor = (screenPosition - GetScreenPosition()).magnitude;
            return -Mathf.FloorToInt(distanceToCursor);
        }

        
        
        // [Section] User Inputs Methods
        
        public void OnPointerClick(PointerEventData eventData)
        {
            m_Model?.OnPointerClick(eventData);
        }

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
        
        
        
        Color ComputeColor(Color baseColor, float distanceFromCursor)
        {
            if (!m_Model.IsSelected)
            {
                baseColor.a = HandlesUtils.GetAlphaFromDistanceFromCursor(distanceFromCursor);
            }

            return baseColor;
        }

        interface IShapeLogic
        {
            void UpdateColor(EntityControlView view, EntityControlViewModel model);
            void UpdateTransform(EntityControlView view, EntityControlViewModel model);
            void UpdateVisibility(EntityControlView view, EntityControlViewModel model);
        }

        class BallLogic : IShapeLogic
        {
            /*
            TODO: Must replace Sphere shape with a new HandleMeshSphere component
            Sphere m_Shape;
            */
            SphereCollider m_Collider;

            public BallLogic(GameObject gameObject)
            {
                /*
                Note: See note at top of document
                    m_Shape = ShapeUtils.CreateDefaultShape<Sphere>(gameObject, "Effector");
                    m_Shape.Radius = k_EffectorSize;
                    m_Shape.RadiusSpace = ThicknessSpace.Meters;
                    m_Shape.enabled = false;
                    m_Shape.gameObject.layer = ApplicationLayers.LayerHandles;
                */
                m_Collider = gameObject.AddComponent<SphereCollider>();
                m_Collider.radius = k_EffectorSize;
                m_Collider.enabled = false;
            }

            public void UpdateTransform(EntityControlView view, EntityControlViewModel model) { }

            public void UpdateShape(EntityControlView view, EntityControlViewModel model) { }

            public void UpdateColor(EntityControlView view, EntityControlViewModel model)
            {
                if (model == null)
                    return;

                var distanceFromCursor = HandlesUtils.GetWorldToViewportPixelDistanceFromCursor(model.Camera, model.Position, false);
                distanceFromCursor = HandlesUtils.PixelsToNoots(distanceFromCursor);

                var color = view.ComputeColor(model.Color, distanceFromCursor);
                /*
                    Note: See note at top of document
                    m_Shape.Color = color;
                    */
            }

            public void UpdateVisibility(EntityControlView entityControlView, EntityControlViewModel model)
            {
                if (model is { IsVisible: true, WidgetType: WidgetType.Ball })
                {   
                    /*
                    Note: See note at top of document
                    m_Shape.enabled = true;
                    */
                    m_Collider.enabled = true;
                }
                else
                {
                    /*
                    Note: See note at top of document
                    m_Shape.enabled = false;
                    */
                    m_Collider.enabled = false;
                }
            }
        }

        class RingLogic : IShapeLogic
        {
            RingControlView m_Ring;

            public RingLogic(GameObject gameObject)
            {
                m_Ring = HandlesUtils.CreateElement<RingControlView>("Ring", gameObject.transform);
            }

            public void SetModel(EntityControlViewModel model)
            {
                m_Ring.SetModel(model.RingControlViewModel);
            }
            
            public void Step(float delta)
            {
                m_Ring.Step(delta);
            }

            public void UpdateColor(EntityControlView view, EntityControlViewModel model)
            {
                m_Ring.QueueUpdate(UpdateFlags.Color);
            }

            public void UpdateTransform(EntityControlView view, EntityControlViewModel model)
            {
                //model?.RingControlModel.SetTransform(model.Position, model.Rotation);
                m_Ring.QueueUpdate(UpdateFlags.Transform);
            }

            public void UpdateShape(EntityControlView view, EntityControlViewModel model)
            {
                m_Ring.QueueUpdate(UpdateFlags.Shape);
            }

            public void UpdateVisibility(EntityControlView view, EntityControlViewModel model)
            {
                m_Ring.QueueUpdate(UpdateFlags.Visibility);
            }
        }
    }
}
