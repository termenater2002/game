using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class EntityControlViewModel: ControlViewModel
    {
        public event Action OnEffectorChanged;
        public event Action<EntityControlViewModel> OnEffectorBeginDrag;
        public event Action<EntityControlViewModel, Vector3> OnEffectorDrag;
        public event Action<EntityControlViewModel, Vector3> OnEffectorEndDrag;
        public event Action<EntityControlViewModel> OnClicked;
        
        public EntityID ID { get; }
        public WidgetType WidgetType { get; }
        public RingControlViewModel RingControlViewModel { get; }
        public RingControlModel RingControlModel { get; }
        public override Vector3 Position => m_EntityEffectorModel.Position;
        public override Quaternion Rotation => m_EntityEffectorModel.Rotation;
        public Color Color => ComputeEffectorColor();
        
        public ControlColorModel ColorsDefault;
        public ControlColorModel ColorsSelected;

        SelectionModel<EntityID> m_SelectionModel;
        GenericEffectorModel m_EntityEffectorModel;
        
        public EntityControlViewModel(EntityID entityID, GenericEffectorModel entityEffectorModel, SelectionModel<EntityID> selectionModel, CameraModel cameraModel, WidgetType widgetType = WidgetType.Ring) : 
            base(cameraModel, SpaceType.World, SpaceType.World, ApplicationConstants.EntityEffectorRaycastOrder)
        {
            Assert.IsNotNull(entityEffectorModel, "You must provide a GenericEffectorModel");
            Assert.IsNotNull(selectionModel, "You must provide a SelectionModel<EntityID>");
            Assert.IsNotNull(cameraModel, "You must provide a CameraModel");
            
            ID = entityID;
            WidgetType = widgetType;
            
            m_EntityEffectorModel = entityEffectorModel;
            m_SelectionModel = selectionModel;

            // Colors
            ColorsDefault = new ControlColorModel(new Color(0f, 0f, 0f, 0.6f));
            ColorsSelected = new ControlColorModel(new Color(0.3f, 1f, 1f, 0.8f));

            // Ring Models
            RingControlModel = new RingControlModel(Vector3.up, 0.2f, true, ColorsDefault.Base);
            RingControlModel.Position = m_EntityEffectorModel.Position;
            RingControlModel.Rotation = m_EntityEffectorModel.Rotation;
            
            RingControlViewModel = new RingControlViewModel(RingControlModel, cameraModel, RingControlViewModel.DragModeType.Translate, ControlViewModel.SpaceType.World, SpaceType.World, ApplicationConstants.EntityEffectorRaycastOrder);
            RingControlViewModel.OnTransformChanged += RingControlTransformChanged;
            RingControlViewModel.OnDraggingStateChanged += RingControlDraggingStateChanged;

            // Register to events
            m_SelectionModel.OnSelectionChanged += OnSelectionModelChanged;
            m_EntityEffectorModel.OnPropertyChanged += OnEntityEffectorModelChanged;
            
            OnStateChanged += UpdateElements;
            OnVisibilityChanged += UpdateElements;
            
            UpdateElements();
        }

        void UpdateElements()
        {
            if (!IsVisible)
            {
                RingControlViewModel.IsVisible = false;
                return;
            }
            
            if (WidgetType == WidgetType.Ring)
            {
                RingControlModel.SetColorScheme(IsSelected ? ColorsSelected.Base : ColorsDefault.Base);
            }

            RingControlViewModel.IsHighlighted = IsHighlighted;
            RingControlViewModel.IsVisible = WidgetType == WidgetType.Ring;
        }

        void RefreshSelected()
        {
            IsSelected = m_SelectionModel.IsSelected(ID);
        }

        void Translate(Vector3 offset)
        {
            m_EntityEffectorModel.Position += offset;
            
            OnEffectorDrag?.Invoke(this, offset);
        }
        
        Color ComputeEffectorColor()
        {
            var color = IsSelected ? ColorsSelected : ColorsDefault;
            return IsHighlighted ? color.Highlighted : color.Base;
        }
        
        // [Section] Models Event Handlers
        
        /// <summary>
        /// Used to track the dragging when <see cref="WidgetType"/>. Ring is used as the widget.
        /// The dragging is handled by the ring's <see cref="RingControlViewModel"/> and relayed here.
        /// </summary>
        void RingControlDraggingStateChanged(ControlViewModel control)
        {
            if (RingControlViewModel.IsDragging)
                OnEffectorBeginDrag?.Invoke(this);
        }
        
        /// <summary>
        /// Tracks the transform changes when dragging the <see cref="RingControlViewModel"/>.
        /// Performs the translation on the character accordingly.
        /// </summary>
        void RingControlTransformChanged()
        {
            var offset = RingControlModel.Position - m_EntityEffectorModel.Position;
            if (offset.sqrMagnitude <= float.Epsilon)
                return;
            
            Translate(offset);
        }

        void OnEntityEffectorModelChanged(GenericEffectorModel model, GenericEffectorModel.Property property)
        {
            RingControlModel.Position = model.Position;
            OnEffectorChanged?.Invoke();
        }

        void OnSelectionModelChanged(SelectionModel<EntityID> model)
        {
            RefreshSelected();
        }
        
        
        
        // [Section] Unique User Input Methods
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount != 1)
                return;

            OnClicked?.Invoke(this);
            eventData.Use();
        }
        
        
        
        // [Section] HandleElementViewModel User Input Methods Overrides
        
        public override void OnViewPointerEntered(PointerEventData eventData)
        {
            base.OnViewPointerEntered(eventData);
            RingControlViewModel.OnViewPointerEntered(eventData);
        }

        public override void OnViewPointerExited(PointerEventData eventData)
        {
            base.OnViewPointerExited(eventData);
            RingControlViewModel.OnViewPointerExited(eventData);
        }

        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            base.OnViewBegunDrag(eventData);
            
            if (eventData.button != 0)
                return;
            
            OnEffectorBeginDrag?.Invoke(this);
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);
            
            if (!IsDragging)
                return;
            
            Translate(DragDelta);
        }
        
        public override void OnViewEndedDrag(PointerEventData eventData)
        {
            base.OnViewEndedDrag(eventData);
            OnEffectorEndDrag?.Invoke(this, eventData.position);
        }
        
        protected override void OnViewDragCancelled()
        {
            base.OnViewDragCancelled();
            m_EntityEffectorModel.Position = DragStartPosition;
        }

        
    }

    enum WidgetType
    {
        Ball,
        Ring
    }
}
