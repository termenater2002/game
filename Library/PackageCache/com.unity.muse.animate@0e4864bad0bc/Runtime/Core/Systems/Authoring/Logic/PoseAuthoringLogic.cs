using System;
using System.Collections.Generic;
using Unity.DeepPose.Components;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class PoseAuthoringLogic
    {
        // We dispatch physics on multiple frames so that it still snaps to large movements
        const int k_NumPhysicsSolvingIterations = 10;
        public bool IsSolving => m_NumPhysicsIterationsLeft > 0;
        public bool NeedToUpdate => IsSolving || AnyEntityNeedToUpdate();
        public bool HasEffectorSelection => m_GlobalSelectionModel.HasSelection;
        public int EffectorSelectionCount => m_GlobalSelectionModel.Count;

        public delegate void EffectorSelectionChanged(PoseAuthoringLogic logic);

        public event EffectorSelectionChanged OnEffectorSelectionChanged;

        public delegate void PosingChanged(PoseAuthoringLogic logic, EntityID entityID);

        public event PosingChanged OnPosingChanged;

        public delegate void SolveFinished(PoseAuthoringLogic logic);

        public event SolveFinished OnSolveFinished;

        bool IsExclusiveSelection => InputUtils.IsExclusiveSelection;
        public SelectionModel<EntityID> EntityManipulatorSelection => m_EntityManipulatorSelectionModel;

        public bool UsePhysics
        {
            get => m_UsePhysics;
            set
            {
                if (value == m_UsePhysics)
                    return;

                m_UsePhysics = value;
                QueuePhysicsSolve();
            }
        }

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (m_IsVisible == value)
                    return;

                m_IsVisible = value;
                UpdateAllViews();
            }
        }

        struct EntityData
        {
            public EntityPosingLogic PosingLogic;
            public PhysicsSolver.RagdollSettings Ragdoll;

            public ArmatureMappingComponent PosingArmature;
            public ArmatureMappingComponent PhysicsArmature;

            public ArmatureStaticPoseModel SolvedPoseLocal;
            public ArmatureStaticPoseModel SolvedPoseGlobal;

            public GenericEffectorModel EntityEffector;

            public ArmatureMappingComponent ViewArmature;
            public EntityControlViewModel EntityControlView;
            public PosingViewModel PosingViewModel;
            public SkeletonViewModel SkeletonView;
            public StaticPoseView SolvedPoseView;
            public JointMask JointMask;
        }

        Dictionary<EntityID, EntityData> m_Entities = new();
        SelectionModel<EntityEffectorIndex> m_GlobalSelectionModel = new();
        PhysicsSolverComponent m_PhysicsSolverComponent;

        SelectionModel<EntityID> m_EntitySelectionModel;
        SelectionModel<EntityID> m_EntityManipulatorSelectionModel;
        HashSet<EntityID> m_ActiveEntities = new();

        CameraModel m_CameraModel;
        private readonly VisualElement m_UIRoot;

        ActorContextualMenu m_ActorContextualMenu;
        EffectorsContextualMenu m_EffectorsContextualMenu;
        
        int m_NumPhysicsIterationsLeft;
        bool m_UsePhysics;
        bool m_IsVisible;

        private readonly List<ContextMenu.ActionArgs> m_EffectorContextMenuArgs;
        readonly AuthoringModel m_AuthoringModel;

        public PoseAuthoringLogic(AuthoringModel authoringModel, PhysicsSolverComponent physicsSolver, SelectionModel<EntityID> entitySelectionModel, CameraModel cameraModel, VisualElement uiRoot)
        {
            m_AuthoringModel = authoringModel;
            m_NumPhysicsIterationsLeft = 0;
            m_UsePhysics = true;
            m_PhysicsSolverComponent = physicsSolver;
            m_GlobalSelectionModel.OnSelectionStateChanged += OnGlobalSelectionChanged;

            m_EntitySelectionModel = entitySelectionModel;
            m_EntitySelectionModel.OnSelectionStateChanged += OnEntitySelectionStateChanged;

            m_CameraModel = cameraModel;
            m_UIRoot = uiRoot;

            m_EntityManipulatorSelectionModel = new SelectionModel<EntityID>();

            physicsSolver.RemoveAllRagdolls();
            
            m_ActorContextualMenu = new ActorContextualMenu();
            m_ActorContextualMenu.OnMenuAction += OnActorContextualMenuAction;
            
            m_EffectorsContextualMenu = new EffectorsContextualMenu();
            m_EffectorsContextualMenu.OnMenuAction += OnEffectorsContextualMenuAction;
        }

        void OnEffectorsContextualMenuAction(EffectorsContextualMenu.ActionType type, AuthoringModel authoring)
        {
            switch (type)
            {
                case EffectorsContextualMenu.ActionType.DisableAll:
                    DisableSelectedEffectors(true, true, true);
                    break;
                case EffectorsContextualMenu.ActionType.DisablePosition:
                    DisableSelectedEffectors(true, false, false);
                    break;
                case EffectorsContextualMenu.ActionType.DisableRotation:
                    DisableSelectedEffectors(false, true, false);
                    break;
                case EffectorsContextualMenu.ActionType.DisableLookAt:
                    DisableSelectedEffectors(false, false, true);
                    break;
            }
        }

        void OnActorContextualMenuAction(ActorContextualMenu.ActionType type, ClipboardService clipboard, AuthoringModel authoring, EntityKeyModel target)
        {
            switch (type)
            {
                case ActorContextualMenu.ActionType.CopyPose:
                    m_AuthoringModel.Timeline.AskCopyPose();
                    break;
                case ActorContextualMenu.ActionType.PastePose:
                    m_AuthoringModel.Timeline.AskPastePose();
                    break;
                case ActorContextualMenu.ActionType.ResetPose:
                    m_AuthoringModel.Timeline.AskResetPose();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        void OnEntitySelectionStateChanged(SelectionModel<EntityID> model, EntityID entityID, bool isSelected)
        {
            var entityData = m_Entities[entityID];
            var entityEffectorSelectionModel = entityData.PosingLogic.EffectorSelectionModel;

            // Taking a temporary snapshot
            using var tmpList = TempList<DeepPoseEffectorIndex>.Allocate();
            for (var i = 0; i < entityEffectorSelectionModel.Count; i++)
            {
                var effectorIndex = entityEffectorSelectionModel.GetSelection(i);
                tmpList.Add(effectorIndex);
            }

            // Disconnect selection state changes
            if (!isSelected)
            {
                entityData.PosingLogic.OnEffectorSelectionStateChanged -= EntityEffectorSelectionStateChanged;
                m_ActiveEntities.Remove(entityID);

                // Actually clear effector selection
                // Note: the above allows to restore selection when the entity is selected again
                // But we chose to clear effector selection anyway.
                entityData.PosingLogic.ClearEffectorSelection();
            }

            // Remove / Add effectors from global selection list
            for (var i = 0; i < tmpList.Count; i++)
            {
                var effectorIndex = tmpList.List[i];
                var entityEffectorIndex = new EntityEffectorIndex(entityID, effectorIndex);
                m_GlobalSelectionModel.SetSelected(entityEffectorIndex, isSelected);
            }

            // Connect selection stage changes
            if (isSelected)
            {
                entityData.PosingLogic.OnEffectorSelectionStateChanged += EntityEffectorSelectionStateChanged;
                m_ActiveEntities.Add(entityID);
            }

            // Update entity effector selection
            if (isSelected)
            {
                if (IsExclusiveSelection)
                {
                    if (m_EntityManipulatorSelectionModel.HasSelection)
                    {
                        DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Deselect);
                    }

                    m_EntityManipulatorSelectionModel.Clear();
                }

                // We unselect the entity effector if the entity has posing effectors, otherwise we select the entity effector
                var shouldSelectEntityEffector = entityData.PosingLogic.PosingModel.EffectorCount == 0;
                m_EntityManipulatorSelectionModel.SetSelected(entityID, shouldSelectEntityEffector);
            }

            UpdateViews(entityID);
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referencePosingArmature,
            ArmatureMappingComponent referenceViewArmature, ArmatureMappingComponent referencePhysicsArmature,
            ArmatureToArmatureMapping posingToCharacterMapping, Action resetPose,
            Action copyPose, Action pastePose, Func<bool> canPastePose,
            JointMask jointMask = null)
        {
            if (m_Entities.ContainsKey(entityID))
                AssertUtils.Fail($"Entity is already registered: {entityID}");

            var posingArmature = referencePosingArmature.Clone();
            posingArmature.name = "Posing (Pose Authoring)";
            
            var viewArmature = referenceViewArmature.Clone();
            viewArmature.name = "View (Pose Authoring)";
            
            var physicsArmature = referencePhysicsArmature.Clone(ApplicationLayers.LayerPosing);
            physicsArmature.name = "Physics (Pose Authoring)";
            
            var deepPoseComponent = posingArmature.GetComponentInChildren<DeepPoseComponent>();
            var effectorRecoveryComponent = posingArmature.GetComponentInChildren<EffectorRecoveryComponent>();

            var posingLogic = new EntityPosingLogic(entityID, deepPoseComponent, effectorRecoveryComponent, posingToCharacterMapping, jointMask);
            var entityEffector = new GenericEffectorModel();
            var solvedPoseLocal = new ArmatureStaticPoseModel(physicsArmature.NumJoints, ArmatureStaticPoseData.PoseType.Local);
            var solvedPoseGlobal = new ArmatureStaticPoseModel(physicsArmature.NumJoints, ArmatureStaticPoseData.PoseType.Global);

            var posingViewModel = CreatePosingView(posingLogic, solvedPoseGlobal, posingArmature.gameObject, m_CameraModel);
            
            var skeletonView = SetupSkeletonView(solvedPoseGlobal, physicsArmature, posingArmature.gameObject, m_CameraModel);
            var solvedPoseView = SetupSolvedPosedView(solvedPoseGlobal, viewArmature.gameObject);
            var selectionView = EntityUtils.SetupSelectionView(m_EntitySelectionModel, entityID, viewArmature.gameObject);
            var entityView = EntityUtils.SetupEntityView(entityID, viewArmature.gameObject, m_UIRoot, OpenActorContextualMenu);
            var entityEffectorView = SetupEntityEffectorView(entityID, entityEffector, posingArmature.gameObject, m_CameraModel);

            posingArmature.gameObject.SetActive(false);
            viewArmature.gameObject.SetActive(false);
            physicsArmature.gameObject.SetActive(false);

#if UNITY_EDITOR
            physicsArmature.gameObject.name = physicsArmature.gameObject.name + "(posing)";
#endif

            var entityData = new EntityData()
            {
                PosingLogic = posingLogic,
                Ragdoll = new PhysicsSolver.RagdollSettings
                {
                    TargetRoot = posingArmature.RootTransform,
                    PhysicsRoot = physicsArmature.RootTransform
                },
                PosingArmature = posingArmature,
                PhysicsArmature = physicsArmature,
                SolvedPoseLocal = solvedPoseLocal,
                SolvedPoseGlobal = solvedPoseGlobal,
                EntityEffector = entityEffector,
                SolvedPoseView = solvedPoseView,
                PosingViewModel = posingViewModel,
                SkeletonView = skeletonView,
                ViewArmature = viewArmature,
                EntityControlView = entityEffectorView
            };

            entityData.PosingLogic.OnEffectorSelectionStateChanged += EntityEffectorSelectionStateChanged;
            entityData.PosingLogic.OnPosingChanged += OnEntityPosingChanged;

            if (!UnityEngine.Application.isPlaying)
                m_PhysicsSolverComponent.Initialize();
            
            m_PhysicsSolverComponent.AddRagdoll(entityData.Ragdoll);

            m_Entities[entityID] = entityData;

            UpdateViews(entityID);
            SolvePhysicsFully();
        }

        public void RemoveEntity(EntityID entityID)
        {
            if (!m_Entities.ContainsKey(entityID))
                AssertUtils.Fail($"Entity is not registered: {entityID}");

            m_EntityManipulatorSelectionModel.SetSelected(entityID, false);

            var entityData = m_Entities[entityID];

            if (entityData.PosingLogic != null)
            {
                entityData.PosingLogic.ClearEffectorSelection();
                entityData.PosingLogic.OnEffectorSelectionStateChanged -= EntityEffectorSelectionStateChanged;
                entityData.PosingLogic.OnPosingChanged -= OnEntityPosingChanged;
            }

            m_PhysicsSolverComponent.RemoveRagdoll(entityData.Ragdoll);

            if (entityData.PosingArmature != null)
                GameObjectUtils.Destroy(entityData.PosingArmature.gameObject);

            if (entityData.ViewArmature != null)
                GameObjectUtils.Destroy(entityData.ViewArmature.gameObject);

            if (entityData.PhysicsArmature != null)
                GameObjectUtils.Destroy(entityData.PhysicsArmature.gameObject);

            m_Entities.Remove(entityID);

            QueuePhysicsSolve();
        }

        void QueuePhysicsSolve()
        {
            m_NumPhysicsIterationsLeft = k_NumPhysicsSolvingIterations;
        }

        public void Step(float delta)
        {
            if (NeedToUpdate)
            {
                var anyPoseChanged = UpdateEntitiesPosing();
                if (anyPoseChanged)
                    m_NumPhysicsIterationsLeft = k_NumPhysicsSolvingIterations;
            }
            
            if (m_NumPhysicsIterationsLeft > 0)
            {
                SolvePhysicsIteration();
            }
            
            StepEntitiesViews(delta);
        }

        void StepEntitiesViews(float delta)
        {
            foreach (var pair in m_Entities)
            {
                pair.Value.EntityControlView.Step(delta);
                pair.Value.SolvedPoseView.Step(delta);
                pair.Value.PosingViewModel.Step(delta);
                pair.Value.SkeletonView.Step(delta);
            }
        }

        public void SolvePhysicsFully()
        {
            m_NumPhysicsIterationsLeft = k_NumPhysicsSolvingIterations;

            while (m_NumPhysicsIterationsLeft > 0)
            {
                SolvePhysicsIteration();
            }
        }

        /// <summary>
        /// Use a combination of DeepPose and physics to re-evaluate the pose such that there
        /// is no ground penetration.
        /// </summary>
        public void ResolveGroundPenetration(EntityID entityID)
        {
            const float thresholdHeight = 0.2f;
            var entityData = m_Entities[entityID];
            var posingLogic = entityData.PosingLogic;
            var posingModel = posingLogic.PosingModel;
            
            // Find all effectors that are below the threshold height
            List<(DeepPoseEffectorModel effector, float weight)> problematicEffectors = new();
            for (var effectorIndex = 0; effectorIndex < posingModel.EffectorCount; ++effectorIndex)
            {
                var effectorModel = posingModel.GetEffectorModel(effectorIndex);
                if (effectorModel.Position.y < thresholdHeight)
                {
                    problematicEffectors.Add((effectorModel, effectorModel.PositionWeight));
                }
            }
            
            // If there are no effectors below the threshold height, we're done
            if (problematicEffectors.Count == 0)
                return;
            
            // Find a valid starting pose without penetration.
            // Move the effectors above the threshold height and solve.
            foreach (var (effector, _) in problematicEffectors)
            {
                effector.PositionWeight = 1f;
                var pos = effector.Position;
                pos.y += thresholdHeight;
                effector.Position = pos;
            }
            
            posingLogic.SolveFully();
            SnapPhysicsToPosing(entityID);

            // Temporarily enable physics
            var usePhysics = m_UsePhysics;
            m_UsePhysics = true;
            
            // Move the effectors back to their original positions.
            foreach (var (effector, _) in problematicEffectors)
            {
                var pos = effector.Position;
                pos.y -= thresholdHeight;
                effector.Position = pos;
            }
            
            // Solve and apply physics.
            posingLogic.SolveFully();
            SolvePhysicsFully();
            
            m_UsePhysics = usePhysics;
            
            // Restore the original weights
            foreach (var (effector, weight) in problematicEffectors)
            {
                effector.PositionWeight = weight;
            }
        }

        void SolvePhysicsIteration()
        {
            if (m_UsePhysics)
            {
                m_PhysicsSolverComponent.Solve(true);
            }
            else
            {
                m_NumPhysicsIterationsLeft = 1;
                m_PhysicsSolverComponent.SnapToTargets();
            }

            CaptureSolvedPoses();
            SnapInactiveEffectorsToSolvedPose();

            m_NumPhysicsIterationsLeft--;

            if (m_NumPhysicsIterationsLeft <= 0)
            {
                OnSolveFinished?.Invoke(this);
            }
        }

        public void SnapPhysicsToPosing(EntityID entityID)
        {
            var entityData = m_Entities[entityID];

            entityData.SolvedPoseLocal.Capture(entityData.PosingArmature.ArmatureMappingData);
            entityData.SolvedPoseGlobal.Capture(entityData.PosingArmature.ArmatureMappingData);

            entityData.SolvedPoseGlobal.ApplyTo(entityData.PhysicsArmature.ArmatureMappingData);

            SnapInactiveEffectorsToSolvedPose();
        }

        bool UpdateEntitiesPosing()
        {
            var didSolve = false;

            foreach (var pair in m_Entities)
            {
                var posingLogic = pair.Value.PosingLogic;
                if (!posingLogic.NeedToUpdate)
                    continue;

                posingLogic.Update();
                didSolve = true;
            }

            return didSolve;
        }

        void CaptureSolvedPoses()
        {
            foreach (var pair in m_Entities)
            {
                var solvedPoseGlobal = pair.Value.SolvedPoseGlobal;
                var solvedPoseLocal = pair.Value.SolvedPoseLocal;
                var solvedArmature = pair.Value.PhysicsArmature;

                solvedPoseLocal.Capture(solvedArmature.ArmatureMappingData);
                solvedPoseGlobal.Capture(solvedArmature.ArmatureMappingData);
            }
        }

        void SnapInactiveEffectorsToSolvedPose()
        {
            foreach (var pair in m_Entities)
            {
                var posingModel = pair.Value.PosingLogic.PosingModel;
                var solvedPoseGlobal = pair.Value.SolvedPoseGlobal;

                posingModel.SnapToPose(solvedPoseGlobal, true);

                SnapEntityEffector(pair.Key);
            }
        }

        public EntityEffectorIndex GetSelectedEffectorIndex(int idx)
        {
            return m_GlobalSelectionModel.GetSelection(idx);
        }

        public DeepPoseEffectorModel GetSelectedEffector(int idx)
        {
            var effectorIndex = GetSelectedEffectorIndex(idx);
            return GetEffectorModel(effectorIndex);
        }

        public void GetSelectedEffectorModels(List<DeepPoseEffectorModel> selectedEffectorModels)
        {
            var selectionCount = m_GlobalSelectionModel.Count;
            for (var i = 0; i < selectionCount; i++)
            {
                var effectorModel = GetSelectedEffector(i);
                selectedEffectorModels.Add(effectorModel);
            }
        }

        public DeepPoseEffectorModel GetEffectorModel(EntityEffectorIndex idx)
        {
            var posingLogic = m_Entities[idx.EntityID].PosingLogic;
            var effectorModel = posingLogic.GetEffector(idx.EffectorIndex);
            return effectorModel;
        }

        public void ClearEffectorsSelection()
        {
            m_GlobalSelectionModel.Clear();
        }

        public void SetEffectorSelected(EntityEffectorIndex idx, bool isSelected)
        {
            m_GlobalSelectionModel.SetSelected(idx, isSelected);
        }

        public bool HasEnabledEffectorsInSelection()
        {
            var selectionCount = m_GlobalSelectionModel.Count;
            for (var i = 0; i < selectionCount; i++)
            {
                var effectorModel = GetSelectedEffector(i);
                if (effectorModel.LookAtEnabled || effectorModel.PositionEnabled || effectorModel.RotationEnabled)
                    return true;
            }

            return false;
        }

        public bool TryGetPosingLogic(EntityID entityID, out EntityPosingLogic posingLogic)
        {
            if (m_Entities.TryGetValue(entityID, out var entityData))
            {
                posingLogic = entityData.PosingLogic;
                return posingLogic != null;
            }

            posingLogic = null;
            return false;
        }

        public bool TryGetSolvedPoses(EntityID entityID, out ArmatureStaticPoseModel localPose, out ArmatureStaticPoseModel globalPose)
        {
            if (m_Entities.TryGetValue(entityID, out var entityData))
            {
                localPose = entityData.SolvedPoseLocal;
                globalPose = entityData.SolvedPoseGlobal;

                return localPose != null && globalPose != null;
            }

            localPose = null;
            globalPose = null;
            return false;
        }

        void EntityEffectorSelectionStateChanged(EntityPosingLogic logic, DeepPoseEffectorIndex effectorIndex, bool isSelected)
        {
            if (IsExclusiveSelection)
            {
                if (isSelected)
                    m_EntityManipulatorSelectionModel.Clear();

                foreach (var pair in m_Entities)
                {
                    var posingLogic = pair.Value.PosingLogic;
                    if (posingLogic == logic)
                        continue;

                    posingLogic.ClearEffectorSelection();
                }
            }

            var entityEffectorIndex = new EntityEffectorIndex(logic.EntityID, effectorIndex);
            m_GlobalSelectionModel.SetSelected(entityEffectorIndex, isSelected);
            
            if (m_GlobalSelectionModel.HasSelection)
            {
                m_AuthoringModel.LastSelectionType = AuthoringModel.SelectionType.Effector;
            }
        }

        void OnEntityPosingChanged(EntityPosingLogic logic)
        {
            SnapEntityEffector(logic.EntityID);
            OnPosingChanged?.Invoke(this, logic.EntityID);
        }

        void OnGlobalSelectionChanged(SelectionModel<EntityEffectorIndex> model, EntityEffectorIndex index, bool isSelected)
        {
            TryGetPosingLogic(index.EntityID, out var posingLogic);
            
            var armatureDefinition = GetPosingArmature(index.EntityID).ArmatureDefinition;
            var jointName = armatureDefinition.GetJointName(GetEffectorModel(index).ArmatureJointIndex);

            DeepPoseAnalytics.SendEffectorUsed(
                isSelected ? DeepPoseAnalytics.EffectorAction.Select : DeepPoseAnalytics.EffectorAction.Deselect,
                jointName);

            // Skip entities that are not active
            if (m_ActiveEntities.Contains(index.EntityID))
                posingLogic.SetEffectorSelected(index.EffectorIndex, isSelected);

            OnEffectorSelectionChanged?.Invoke(this);
        }

        void SnapEntityEffector(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            var entityEffector = entityData.EntityEffector;

            var rootTransform = entityData.PosingArmature.NumJoints > 0 ? entityData.PosingArmature.GetJointTransform(0) : entityData.PosingArmature.RootTransform;
            var refTransform = entityData.PosingArmature.NumJoints > 0 ? entityData.PosingArmature.GetJointTransform(0) : rootTransform;
            entityEffector.Position = refTransform.position;
            entityEffector.Rotation = refTransform.rotation;
        }

        void AddEntityEffectorToSelection(EntityID entityID)
        {
            if (IsExclusiveSelection)
            {
                ClearEffectorsSelection();

                m_EntityManipulatorSelectionModel.Clear();
                m_EntityManipulatorSelectionModel.Select(entityID);
                
                DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Select);
            }
            else
            {
                var isSelected = m_EntityManipulatorSelectionModel.IsSelected(entityID);

                DeepPoseAnalytics.SendEntityEffectorUsed(
                    isSelected
                        ? DeepPoseAnalytics.EffectorAction.Deselect
                        : DeepPoseAnalytics.EffectorAction.Select);

                m_EntityManipulatorSelectionModel.SetSelected(entityID, !isSelected);
            }
        }

        bool AnyEntityNeedToUpdate()
        {
            foreach (var pair in m_Entities)
            {
                var posingLogic = pair.Value.PosingLogic;
                if (posingLogic.NeedToUpdate)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Applies the current pose from the posing logic to a specific key for all entities
        /// </summary>
        /// <param name="keyModel">The key model to capture the pose to</param>
        public void ApplyPosingStateToKey(KeyModel keyModel)
        {
            // Set key to full pose
            keyModel.Type = KeyData.KeyType.FullPose;

            foreach (var pair in m_Entities)
            {
                ApplyPosingStateToKey(pair.Key, keyModel);
            }
        }

        /// <summary>
        /// Applies the current pose from the posing logic to a specific key for the given entity
        /// </summary>
        /// <param name="entityID">The entity whose pose to capture</param>
        /// <param name="keyModel">The key model to capture the pose to</param>
        public void ApplyPosingStateToKey(EntityID entityID, KeyModel keyModel)
        {
            var entityData = m_Entities[entityID];

            var posingModel = entityData.PosingLogic.PosingModel;
            var localPose = entityData.SolvedPoseLocal;
            var globalPose = entityData.SolvedPoseGlobal;

            if (!keyModel.TryGetKey(entityID, out var keyframeModel))
                keyModel.AddEntity(entityID, posingModel, localPose, globalPose);
            else
                keyframeModel.Capture(posingModel, localPose, globalPose);
        }

        /// <summary>
        /// Transfer the pose stored in a key to the posing logic state for all entities
        /// </summary>
        /// <param name="keyModel">The key model to apply the pose from</param>
        public void RestorePosingStateFromKey(KeyModel keyModel)
        {
            foreach (var pair in m_Entities)
            {
                var entityID = pair.Key;

                if (!keyModel.TryGetKey(entityID, out var keyframeModel))
                    continue;

                RestoreKey(entityID, keyframeModel);
            }
        }

        internal void RestoreKey(EntityID entityID, EntityKeyModel entityKeyModel)
        {
            var entityData = m_Entities[entityID];
            var posingLogic = entityData.PosingLogic;

            entityKeyModel.Apply(entityData.SolvedPoseLocal);
            entityKeyModel.Apply(entityData.SolvedPoseGlobal);

            entityKeyModel.Apply(entityData.PhysicsArmature.ArmatureMappingData);
            entityKeyModel.Apply(entityData.PosingArmature.ArmatureMappingData);

            entityKeyModel.Apply(posingLogic.PosingModel);

            // Remove any physics iteration or pose solve
            posingLogic.SkipPendingSolve();
            m_NumPhysicsIterationsLeft = 0;
        }

        public GenericEffectorModel GetEntityEffector(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            var entityEffector = entityData.EntityEffector;
            return entityEffector;
        }

        public void Translate(EntityID entityID, Vector3 offset)
        {
            var entityData = m_Entities[entityID];
            var posingLogic = entityData.PosingLogic;

            // Special case for props that have no effectors
            if (posingLogic == null || posingLogic.PosingModel.EffectorCount == 0)
            {
                if (entityData.PosingArmature.ArmatureMappingData.NumJoints == 0)
                    return;

                var rootTransform = entityData.PosingArmature.ArmatureMappingData.Transforms[0];
                var newPosition = rootTransform.position + offset;
                rootTransform.position = newPosition;

                QueuePhysicsSolve();
            }
            else
            {
                posingLogic.Translate(offset);
            }
        }

        public void Rotate(EntityID entityID, Vector3 pivot, Quaternion offset)
        {
            var entityData = m_Entities[entityID];
            var posingLogic = entityData.PosingLogic;

            // Special case for props that have no effectors
            if (posingLogic == null || posingLogic.PosingModel.EffectorCount == 0)
            {
                if (entityData.PosingArmature.ArmatureMappingData.NumJoints == 0)
                    return;

                var rootTransform = entityData.PosingArmature.ArmatureMappingData.Transforms[0];
                var newPosition = pivot + offset * (rootTransform.position - pivot);
                var newRotation = offset * rootTransform.rotation;
                rootTransform.SetPositionAndRotation(newPosition, newRotation);

                QueuePhysicsSolve();
            }
            else
            {
                posingLogic.Rotate(pivot, offset);
            }
        }

        public void PoseFromBakedTimeline(StageModel stage, BakedTimelineModel timeline, int frameIndex)
        {
            PoseFromBakedTimelineFrame(stage, timeline.GetFrame(frameIndex), frameIndex == 0);
        }

        void PoseFromBakedTimelineFrame(StageModel stage, BakedFrameModel from, bool firstFrame)
        {
            for (var i = 0; i < stage.NumActors; i++)
            {
                var actorModel = stage.GetActorModel(i);

                if (from.TryGetModel(actorModel.EntityID, out var bakedPose))
                {
                    // Apply the pose from the baked frame on the posing armature
                    var posingArmature = GetPosingArmature(actorModel.EntityID);
                    bakedPose.ApplyTo(posingArmature.ArmatureMappingData);

                    // We snap physics BEFORE effector recovery as we know the baked frame should have a physically-accurate pose,
                    // EXCEPT for the first keyframe. See note below.
                    SnapPhysicsToPosing(actorModel.EntityID);
                    DoEffectorRecovery(actorModel.EntityID);

                    // For the first baked frame, we need to make sure there is no ground penetration. This can
                    // happen if we are extracting keys from a baked timeline does not obey physics (e.g.
                    // text-to-motion).
                    if (firstFrame)
                        ResolveGroundPenetration(actorModel.EntityID);

                    // We do not snap physics AFTER effector recovery as this could lead to penetration when effector recovery is imperfect.
                    // Instead we let the physics solve try to match the recovered pose
                }
                else
                {
                    Debug.Log($"Failed to locate pose in frame for entityID: {actorModel.EntityID}");
                }
            }
        }

        public void DoEffectorRecovery(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            var posingLogic = entityData.PosingLogic;

            posingLogic.DoEffectorRecovery();
            posingLogic.SolveFully();

            QueuePhysicsSolve();
        }
        
        internal void DisableSelectedEffectors(bool disablePosition, bool disableRotation, bool disableLookAt)
        {
            UserActionsManager.Instance.StartUserEdit("Disabling Effectors");
            
            for (var i = 0; i < EffectorSelectionCount; ++i)
            {
                var effector = GetSelectedEffector(i);

                if (disablePosition)
                {
                    effector.PositionEnabled = false;
                }

                if (disableLookAt)
                {
                    effector.LookAtEnabled = false;
                }

                if (disableRotation)
                {
                    effector.RotationEnabled = false;
                }
            }

            ClearEffectorsSelection();
        }

        private (int enabledEffectors, int positionEnabledEffectors, int rotationEnabledEffectors) CountEnabledEffectors()
        {
            var enabledEffectors = 0;
            var positionEnabledEffectors = 0;
            var rotationEnabledEffectors = 0;

            for (var i = 0; i < EffectorSelectionCount; ++i)
            {
                var effector = GetSelectedEffector(i);

                if (effector.PositionEnabled || effector.RotationEnabled || effector.LookAtEnabled)
                {
                    ++enabledEffectors;
                }

                if (effector.PositionEnabled || effector.LookAtEnabled)
                {
                    ++positionEnabledEffectors;
                }

                if (effector.RotationEnabled)
                {
                    ++rotationEnabledEffectors;
                }
            }

            return (enabledEffectors, positionEnabledEffectors, rotationEnabledEffectors);
        }

        public void OpenEffectorsContextMenu(Vector2 pointerPosition)
        {
            m_EffectorsContextualMenu.Open(m_AuthoringModel, m_UIRoot, m_CameraModel, pointerPosition);
            /*
            var (enabledEffectors, positionEnabledEffectors, rotationEnabledEffectors) = CountEnabledEffectors();

            m_EffectorContextMenuArgs[0].IsClickable = enabledEffectors > 0;
            m_EffectorContextMenuArgs[1].IsClickable = positionEnabledEffectors > 0;
            m_EffectorContextMenuArgs[2].IsClickable = rotationEnabledEffectors > 0;
            
            m_CameraModel.OpenContextMenu(m_UIRoot, pointerPosition, m_EffectorContextMenuArgs);*/
        }

        void OpenActorContextualMenu(VisualElement uiRoot, Vector2 clickPosition, EntityView entityContextMenuView)
        {
            m_ActorContextualMenu.Open(m_AuthoringModel, m_CameraModel, uiRoot, clickPosition, m_AuthoringModel.Context.Clipboard, entityContextMenuView);
        }
        
        PosingViewModel CreatePosingView(EntityPosingLogic posingLogic, ArmatureStaticPoseModel solvedPoseGlobal, GameObject gameObject, CameraModel cameraModel)
        {
            var posingViewModel = new PosingViewModel(posingLogic.PosingModel, posingLogic.EffectorSelectionModel, solvedPoseGlobal, cameraModel);
            posingViewModel.OnEffectorClicked += (model, index) => posingLogic.AddEffectorToSelection(index);
            posingViewModel.OnEffectorRightClicked +=
                (model, index, pointerPosition) =>
                {
                    posingLogic.HandleEffectorContextSelection(index);
                    OpenEffectorsContextMenu(pointerPosition);
                };
            
            // We want to also select the effector when we start dragging it
            posingViewModel.OnEffectorBeginDrag += (_, index, _) => posingLogic.AddEffectorToSelection(index);
            
            
            var posingView = gameObject.AddComponent<PosingView>();
            posingView.Initialize();
            posingView.SetModel(posingViewModel);
            
            return posingViewModel;
        }

        EntityControlViewModel SetupEntityEffectorView(EntityID entityID, GenericEffectorModel entityEffectorModel, GameObject gameObject, CameraModel cameraModel)
        {
            var effectorViewModel = new EntityControlViewModel(entityID, entityEffectorModel, m_EntityManipulatorSelectionModel, cameraModel);
            effectorViewModel.OnClicked += (model) => AddEntityEffectorToSelection(model.ID);
            effectorViewModel.OnEffectorBeginDrag += (model) => AddEntityEffectorToSelection(model.ID);
            effectorViewModel.OnEffectorDrag += (model, offset) => Translate(model.ID, offset);
            
            var view = HandlesUtils.CreateElement<EntityControlView>("EntityEffector", gameObject.transform, ApplicationConstants.EntityEffectorRaycastOrder);
            view.SetModel(effectorViewModel);
            view.enabled = false;
            return effectorViewModel;
        }

        SkeletonViewModel SetupSkeletonView(ArmatureStaticPoseModel solvedPoseGlobal,
            ArmatureMappingComponent physicsArmature, GameObject gameObject, CameraModel cameraModel)
        {
            var skeletonViewModel = new SkeletonViewModel(solvedPoseGlobal, physicsArmature.ArmatureDefinition, cameraModel);
            var skeletonView = gameObject.AddComponent<SkeletonView>();
            skeletonView.Initialize();
            skeletonView.SetModel(skeletonViewModel);

            return skeletonViewModel;
        }

        StaticPoseView SetupSolvedPosedView(ArmatureStaticPoseModel solvedPoseGlobal, GameObject gameObject)
        {
            var poseViewModel = new StaticPoseViewModel(solvedPoseGlobal);
            var poseView = gameObject.AddComponent<StaticPoseView>();
            poseView.Setup(poseViewModel, "Posing - Static Pose", true, true, gameObject.layer);
            return poseView;
        }

        public ArmatureMappingComponent GetViewArmature(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            return entityData.ViewArmature;
        }

        public ArmatureMappingComponent GetPosingArmature(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            return entityData.PosingArmature;
        }

        public void ForceUpdateAllViews()
        {
            UpdateAllViews();
        }
        
        void UpdateAllViews()
        {
            foreach (var pair in m_Entities)
            {
                UpdateViews(pair.Key);
            }
        }

        void UpdateViews(EntityID entityID)
        {
            var entityData = m_Entities[entityID];

            // Note: Leaving the posing & physics armature active
            // so we can use the deepPose component and Physics solver outside of posing mode
            entityData.PosingArmature.gameObject.SetActive(true);
            entityData.ViewArmature.gameObject.SetActive(m_IsVisible);
            entityData.PhysicsArmature.gameObject.SetActive(true);

            var isSelected = m_EntitySelectionModel.IsSelected(entityID);
            var hasAnyEffector = entityData.PosingLogic != null && entityData.PosingLogic.PosingModel.EffectorCount > 0;
            
            entityData.SolvedPoseView.enabled = m_IsVisible;
            entityData.PosingViewModel.IsVisible = m_IsVisible && isSelected;
            entityData.PosingViewModel.UpdateEffectorsVisibility();
            entityData.SkeletonView.IsVisible = m_IsVisible && isSelected;
            entityData.EntityControlView.IsVisible = m_IsVisible && isSelected;
        }

        public GameObject GetPosingGameObject(EntityID entityID)
        {
            if (!m_Entities.TryGetValue(entityID, out var entityData))
            {
                throw new ArgumentException($"Entity {entityID} is not registered");
            }
            
            return entityData.SolvedPoseView.gameObject;
        }
        
        /// <summary>
        /// Adds Ctrl+ or ⌘+ before the provided text depending on the platform.
        /// </summary>
        /// <param name="text">The text that follows the added localised command label.</param>
        /// <returns>The provided text with an added Ctrl+ or ⌘+ depending on the platform.</returns>
        static string GetCmd(string text)
        {
            return text == "" ? "" : PlatformUtils.GetCommandLabel(text);
        }
    }
}
