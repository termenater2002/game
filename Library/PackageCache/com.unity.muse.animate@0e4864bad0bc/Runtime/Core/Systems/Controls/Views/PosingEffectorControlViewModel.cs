using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    class PosingEffectorControlViewModel : ControlViewModel
    {
        const float k_MouseOverColorAdd = 0.75f;

        // [Section] Data Events

        public event Action OnJointTransformChanged;
        public event Action OnEffectorChanged;

        

        // [Section] User Interaction Events

        public event Action<PosingEffectorControlViewModel> OnEffectorClicked;
        public event Action<PosingEffectorControlViewModel, Vector2> OnEffectorRightClicked;
        public event Action<PosingEffectorControlViewModel, Vector2> OnEffectorBeginDrag;

        

        public float JointSizeRatio
        {
            get => m_JointSizeRatio;

            private set
            {
                if (m_JointSizeRatio.Equals(value))
                    return;

                m_JointSizeRatio = value;
                InvokeSizeRatiosChanged();
            }
        }

        float JointColliderSizeRatio
        {
            get => m_JointColliderSizeRatio;

            set
            {
                if (m_JointColliderSizeRatio.Equals(value))
                    return;

                m_JointColliderSizeRatio = value;
                InvokeSizeRatiosChanged();
            }
        }

        public DeepPoseEffectorIndex Index => m_EffectorModel.Index.EffectorIndex;
        public bool IsActive => m_EffectorModel.IsActive;
        public override Vector3 Position => m_EffectorModel.Position;
        public override Quaternion Rotation => m_EffectorModel.Rotation;
        public bool HandlesPosition => m_EffectorModel.HandlesPosition;
        public bool PositionEnabled => m_EffectorModel.PositionEnabled;
        public bool HandlesRotation => m_EffectorModel.HandlesRotation;
        public bool RotationEnabled => m_EffectorModel.RotationEnabled;
        public bool HandlesLookAt => m_EffectorModel.HandlesLookAt;
        public bool LookAtEnabled => m_EffectorModel.LookAtEnabled;
        public float PositionTolerance => m_EffectorModel.PositionTolerance;
        public Vector3 SourceJointPosition => m_JointTransformModel.Position;
        public Quaternion SourceJointRotation => m_JointTransformModel.Rotation;
        public Color PositionColor => ComputeEffectorColor(PositionEnabled);
        public Color ToleranceColor => ComputeEffectorToleranceColor(PositionEnabled, PositionTolerance);
        public Color RotationColor => ComputeEffectorColor(RotationEnabled);
        public Color LookAtColor => ComputeEffectorColor(LookAtEnabled);

        DeepPoseEffectorModel m_EffectorModel;
        SelectionModel<DeepPoseEffectorIndex> m_SelectionModel;
        RigidTransformModel m_JointTransformModel;

        Color m_ColorDisabledUnselected;
        Color m_ColorEnabledUnselected;
        Color m_ColorDisabledSelected;
        Color m_ColorEnabledSelected;

        float m_JointSizeRatio;
        float m_JointColliderSizeRatio;
        float m_SizeRatio;
        float m_ColliderSizeRatio;

        public PosingEffectorControlViewModel(
            DeepPoseEffectorModel effectorModel,
            SelectionModel<DeepPoseEffectorIndex> selectionModel,
            CameraModel cameraModel,
            RigidTransformModel solvedJointGlobalTransform)
            : base(cameraModel, SpaceType.Screen,  SpaceType.Screen, ApplicationConstants.PosingEffectorRaycastOrder)
        {
            Assert.IsNotNull(effectorModel, "You must provide an DeepPoseEffectorModel");
            Assert.IsNotNull(selectionModel, "You must provide a SelectionModel<DeepPoseEffectorIndex>");
            Assert.IsNotNull(cameraModel, "You must provide a CameraModel");
            Assert.IsNotNull(solvedJointGlobalTransform, "You must provide a RigidTransformModel");

            m_EffectorModel = effectorModel;
            m_SelectionModel = selectionModel;
            m_JointTransformModel = solvedJointGlobalTransform;
            m_JointSizeRatio = 1f;
            
            ColorUtility.TryParseHtmlString("#000000", out m_ColorDisabledUnselected);
            ColorUtility.TryParseHtmlString("#ff8a00", out m_ColorEnabledUnselected);
            ColorUtility.TryParseHtmlString("#003b52", out m_ColorDisabledSelected);
            ColorUtility.TryParseHtmlString("#00ffdd", out m_ColorEnabledSelected);

            m_EffectorModel.OnPropertyChanged += OnEffectorPropertyChangedInternal;
            m_SelectionModel.OnSelectionChanged += OnEffectorSelectionChangedInternal;
            m_JointTransformModel.OnChanged += OnJointTransformChangedInternal;
        }

        void OnJointTransformChangedInternal(RigidTransformModel model)
        {
            UpdateSizeRatios();
            OnJointTransformChanged?.Invoke();
        }

        void OnEffectorPropertyChangedInternal(DeepPoseEffectorModel model, DeepPoseEffectorModel.Property property)
        {
            UpdateSizeRatios();
            OnEffectorChanged?.Invoke();
        }

        void OnEffectorSelectionChangedInternal(SelectionModel<DeepPoseEffectorIndex> model)
        {
            IsSelected = m_SelectionModel.IsSelected(Index);
        }

        protected override void UpdateSizeRatios()
        {
            base.UpdateSizeRatios();
            JointColliderSizeRatio = HandlesUtils.ComputeSizeRatio(Camera, SourceJointPosition);
            JointSizeRatio = SizeSpace == SpaceType.World ? 1f : JointColliderSizeRatio;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount != 1)
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnEffectorRightClicked?.Invoke(this, eventData.position);
            }
            else
            {
                OnEffectorClicked?.Invoke(this);
            }

            eventData.Use();
        }

        public override void OnViewBegunDrag(PointerEventData eventData)
        {
            if (eventData.button != 0)
                return;

            if (!m_EffectorModel.HandlesPosition && !m_EffectorModel.HandlesLookAt)
                return;

            base.OnViewBegunDrag(eventData);

            if (m_EffectorModel.HandlesPosition)
            {
                m_EffectorModel.PositionEnabled = true;
            }
            else if (m_EffectorModel.HandlesLookAt)
            {
                m_EffectorModel.LookAtEnabled = true;
            }

            OnEffectorBeginDrag?.Invoke(this, eventData.position);
        }

        public override void OnViewDragged(PointerEventData eventData)
        {
            base.OnViewDragged(eventData);

            if (!IsDragging)
                return;

            m_EffectorModel.Position += DragDelta;
        }

        protected override void OnViewDragCancelled()
        {
            base.OnViewDragCancelled();
            m_EffectorModel.Position = DragStartPosition;
        }

        Color ComputeEffectorColor(bool isEnabled)
        {
            var color = GetBaseColor(isEnabled);

            if (IsHighlighted || IsDragging)
            {
                color.r = Mathf.Min(1f, color.r + k_MouseOverColorAdd);
                color.g = Mathf.Min(1f, color.g + k_MouseOverColorAdd);
                color.b = Mathf.Min(1f, color.b + k_MouseOverColorAdd);
            }

            return color;
        }
        
        Color ComputeEffectorToleranceColor(bool isEnabled, float tolerance)
        {
            var color = ComputeEffectorColor(isEnabled);
            var toleranceFactor = 1f-Mathf.Min(1f, Mathf.Max(0f, tolerance*30f));
            color.a *= Mathf.Lerp(0.02f, 0.3f, toleranceFactor);
            return color;
        }
        
        Color GetBaseColor(bool isEnabled)
        {
            Color color;
            if (isEnabled)
            {
                color = IsSelected ? m_ColorEnabledSelected : m_ColorEnabledUnselected;
            }
            else
            {
                color = IsSelected ? m_ColorDisabledSelected : m_ColorDisabledUnselected;
            }

            return color;
        }
    }
}
