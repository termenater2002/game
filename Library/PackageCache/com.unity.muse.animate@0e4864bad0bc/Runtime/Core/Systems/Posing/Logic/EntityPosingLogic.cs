using System;
using Unity.DeepPose.Components;
using UnityEngine;
using UnityEngine.Pool;

namespace Unity.Muse.Animate
{
    class EntityPosingLogic
    {
        public EntityID EntityID => m_EntityID;
        public PosingModel PosingModel => m_PosingModel;
        public SelectionModel<DeepPoseEffectorIndex> EffectorSelectionModel => m_EffectorSelectionModel;

        public bool NeedToUpdate
        {
            get => m_NeedToUpdate;
            private set
            {
                if (value == m_NeedToUpdate)
                    return;

                m_NeedToUpdate = value;
                OnNeedToUpdateChanged?.Invoke(this, m_NeedToUpdate);
            }
        }

        public delegate void NeedToUpdateChanged(EntityPosingLogic logic, bool needToUpdate);

        public event NeedToUpdateChanged OnNeedToUpdateChanged;

        public delegate void SelectionChanged(EntityPosingLogic logic);

        public event SelectionChanged OnSelectionChanged;

        public delegate void EffectorSelectionStateChanged(EntityPosingLogic logic, DeepPoseEffectorIndex effectorIndex, bool isSelected);

        public event EffectorSelectionStateChanged OnEffectorSelectionStateChanged;

        public delegate void PosingChanged(EntityPosingLogic logic);

        public event PosingChanged OnPosingChanged;

        bool IsExclusiveSelection => InputUtils.IsExclusiveSelection;

        PosingModel m_PosingModel;
        SelectionModel<DeepPoseEffectorIndex> m_EffectorSelectionModel;

        DeepPoseComponent m_DeepPoseComponent;
        EffectorRecoveryComponent m_EffectorRecoveryComponent;

        bool m_NeedToUpdate;
        readonly EntityID m_EntityID;

        public EntityPosingLogic(EntityID entityID,
            DeepPoseComponent deepPoseComponent,
            EffectorRecoveryComponent effectorRecoveryComponent,
            ArmatureToArmatureMapping posingToCharacterArmatureMapping,
            JointMask jointMask = null)
        {
            m_EntityID = entityID;
            m_NeedToUpdate = false;

            m_DeepPoseComponent = deepPoseComponent;
            m_EffectorRecoveryComponent = effectorRecoveryComponent;

            // Create and setup posing model from component
            var armatureEffectorIndices = m_DeepPoseComponent != null
                ? m_DeepPoseComponent.GetEffectorIndices(posingToCharacterArmatureMapping)
                : Array.Empty<ArmatureEffectorIndex>();
            
            m_PosingModel = new PosingModel(armatureEffectorIndices);
            m_PosingModel.OnChanged += OnPosingModelChanged;

            m_EffectorSelectionModel = new SelectionModel<DeepPoseEffectorIndex>();
            m_EffectorSelectionModel.OnSelectionStateChanged += (model, index, selected) => OnEffectorSelectionStateChanged?.Invoke(this, index, selected);
            m_EffectorSelectionModel.OnSelectionChanged += model => OnSelectionChanged?.Invoke(this);

            SetupEffectorRecoveryComponent(posingToCharacterArmatureMapping, jointMask);

            SyncPosingFromComponent();
            SolveFully();
        }

        void SetupEffectorRecoveryComponent(ArmatureToArmatureMapping posingToCharacterArmatureMapping, JointMask jointMask)
        {
            if (jointMask == null || jointMask.Armature != posingToCharacterArmatureMapping.TargetArmature) return;
            
            var candidateEffectors = ListPool<EffectorRecoveryBase.PosingEffector>.Get();
            var mandatoryEffectors = ListPool<EffectorRecoveryBase.PosingEffector>.Get();
            candidateEffectors.Clear();
            mandatoryEffectors.Clear();

            var headIndex = posingToCharacterArmatureMapping.SourceArmature.Armature.GetJointIndex("Head");

            for (var i = 0; i < posingToCharacterArmatureMapping.NumSourceJoints; i++)
            {
                if (posingToCharacterArmatureMapping.TryGetTargetJointIndex(i,
                        out var characterJointIndex) &&
                    jointMask.PositionWeights[characterJointIndex] > 0)
                {
                    var priority = jointMask.PositionWeights[characterJointIndex] switch
                    {
                        > 0.99f => 0,
                        > 0.75f => 1,
                        > 0.5f => 2,
                        > 0.25f => 3,
                        _ => 4
                    };

                    var effectorType = i == headIndex
                        ? EffectorRecoveryBase.PosingEffector.EffectorType.LookAt
                        : EffectorRecoveryBase.PosingEffector.EffectorType.Position;

                    if (priority == 0)
                    {
                        mandatoryEffectors.Add(
                            new EffectorRecoveryBase.PosingEffector(
                                effectorType,
                                i,
                                priority));
                    }
                    else
                    {
                        candidateEffectors.Add(
                            new EffectorRecoveryBase.PosingEffector(
                                effectorType,
                                i,
                                priority));
                    }
                }
            }

            m_EffectorRecoveryComponent.MandatoryEffectors = mandatoryEffectors.ToArray();
            m_EffectorRecoveryComponent.CandidateEffectors = candidateEffectors.ToArray();
            m_EffectorRecoveryComponent.UseEffectorPriorities = true;
            m_EffectorRecoveryComponent.OnAfterDeserialize();
            ListPool<EffectorRecoveryBase.PosingEffector>.Release(candidateEffectors);
            ListPool<EffectorRecoveryBase.PosingEffector>.Release(mandatoryEffectors);
        }

        void OnPosingModelChanged(PosingModel model, DeepPoseEffectorModel effectorModel, DeepPoseEffectorModel.Property property)
        {
            var activePositionChanged = effectorModel.PositionEnabled &&
                property is DeepPoseEffectorModel.Property.Position or DeepPoseEffectorModel.Property.PositionTolerance or DeepPoseEffectorModel.Property.PositionWeight;
            var activeRotationChanged = effectorModel.RotationEnabled &&
                property is DeepPoseEffectorModel.Property.Rotation or DeepPoseEffectorModel.Property.PositionTolerance or DeepPoseEffectorModel.Property.RotationWeight;
            var activeLookAtChanged = effectorModel.LookAtEnabled &&
                property is DeepPoseEffectorModel.Property.Position or DeepPoseEffectorModel.Property.Rotation or DeepPoseEffectorModel.Property.LookAtWeight;

            var enabledStateMightHaveChanged = property is DeepPoseEffectorModel.Property.PositionWeight or DeepPoseEffectorModel.Property.RotationWeight or DeepPoseEffectorModel.Property.LookAtWeight;

            var needToUpdate = activePositionChanged
                || activeRotationChanged
                || activeLookAtChanged
                || enabledStateMightHaveChanged;

            if (needToUpdate)
            {
                NeedToUpdate = true;
                OnPosingChanged?.Invoke(this);
            }
        }

        public void Update()
        {
            if (m_DeepPoseComponent != null)
            {
                m_PosingModel.ApplyToComponent(m_DeepPoseComponent);
                m_DeepPoseComponent.Solve();
            }

            NeedToUpdate = false;
        }

        public void SolveFully()
        {
            while (NeedToUpdate)
            {
                Update();
            }
        }

        public void SkipPendingSolve()
        {
            NeedToUpdate = false;
        }

        void SyncPosingFromComponent()
        {
            if (m_DeepPoseComponent != null)
                m_PosingModel.SyncFromComponent(m_DeepPoseComponent);
        }

        public void ClearEffectorSelection()
        {
            m_EffectorSelectionModel.Clear();
        }

        public DeepPoseEffectorModel GetEffector(DeepPoseEffectorIndex idxEffectorIndex)
        {
            var effectorModel = m_PosingModel.GetEffectorModel(idxEffectorIndex);
            return effectorModel;
        }

        public void HandleEffectorContextSelection(DeepPoseEffectorIndex effectorIndex)
        {
            // From effector context selection:

            // (1) If one or multiple effectors are selected and user right-click on one of these,
            // the selection is not cleared, the contextual menu will apply to the original selection

            if (m_EffectorSelectionModel.IsSelected(effectorIndex))
            {
                // nothing to change:
                return;
            }

            // (2) If other effectors are selected, right-clicking another effector clears
            // the selection if it is an exclusive selection and selects the newly clicked on,
            // on which the contextual menu will apply

            if (IsExclusiveSelection)
            {
                ClearEffectorSelection();
            }

            m_EffectorSelectionModel.Select(effectorIndex);
        }

        public void AddEffectorToSelection(DeepPoseEffectorIndex effectorIndex)
        {
            if (IsExclusiveSelection)
            {
                ClearEffectorSelection();
                m_EffectorSelectionModel.Select(effectorIndex);
            }
            else
            {
                var isSelected = m_EffectorSelectionModel.IsSelected(effectorIndex);
                m_EffectorSelectionModel.SetSelected(effectorIndex, !isSelected);
            }
        }

        public void SetEffectorSelected(DeepPoseEffectorIndex effectorIndex, bool isSelected)
        {
            m_EffectorSelectionModel.SetSelected(effectorIndex, isSelected);
        }

        public void Translate(Vector3 offset)
        {
            m_PosingModel.Translate(offset);
        }

        public void Rotate(Vector3 pivot, Quaternion offset)
        {
            m_PosingModel.Rotate(pivot, offset);
        }

        public void DoEffectorRecovery()
        {
            if (m_EffectorRecoveryComponent == null)
                return;

            m_EffectorRecoveryComponent.CapturePose();
            m_EffectorRecoveryComponent.Solve();
            SyncPosingFromComponent();
        }
    }
}
