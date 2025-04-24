using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class LoopAuthoringLogic
    {
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                UpdateVisibility();
            }
        }

        public event Action<LoopKeyModel> OnLoopChanged;

        struct LoopInfo
        {
            public GameObject SourceGameObject;
            public GameObject TargetGameObject;
            public ArmatureMappingComponent SourceArmature;
            public ArmatureMappingComponent TargetArmature;
            public EntitySelectionView TargetSelectionView;
        }

        Dictionary<EntityID, LoopInfo> m_Infos = new();
        SelectionModel<EntityID> m_EntitySelectionModel;
        SelectionModel<TimelineModel.SequenceKey> m_KeySelectionModel;
        BakedTimelineModel m_BakedTimelineModel;
        bool m_IsVisible;
        TimelineModel.SequenceKey m_TargetKey;

        public LoopAuthoringLogic(SelectionModel<EntityID> entitySelection, BakedTimelineModel bakedTimelineModel,
            SelectionModel<TimelineModel.SequenceKey> keySelectionModel)
        {
            m_EntitySelectionModel = entitySelection;
            m_BakedTimelineModel = bakedTimelineModel;
            m_KeySelectionModel = keySelectionModel;
            
            m_BakedTimelineModel.OnChanged += OnBakedTimelineChanged;
            m_KeySelectionModel.OnSelectionChanged += OnKeySelectionChanged;
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referenceViewArmature)
        {
            if (m_Infos.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is already registered: {entityID}");

            var sourceViewArmature = referenceViewArmature.Clone();
            sourceViewArmature.name = "View - Source (Loop Authoring)";
            var sourcePreviewGameObject = sourceViewArmature.gameObject;
            sourcePreviewGameObject.OverrideAllMaterials(ApplicationConstants.LoopGhostMaterial);
            sourcePreviewGameObject.SetActive(false);

            var targetViewArmature = referenceViewArmature.Clone();
            targetViewArmature.name = "View - Target (Loop Authoring)";
            var targetPreviewGameObject = targetViewArmature.gameObject;
            var targetSelectionView = EntityUtils.SetupSelectionView(m_EntitySelectionModel, entityID, targetPreviewGameObject);
            targetPreviewGameObject.SetActive(false);

            var info = new LoopInfo
            {
                SourceArmature = sourceViewArmature,
                TargetArmature = targetViewArmature,
                SourceGameObject = sourcePreviewGameObject,
                TargetGameObject = targetPreviewGameObject,
                TargetSelectionView = targetSelectionView
            };

            m_Infos[entityID] = info;

            UpdateSourceFrame();
        }

        public void RemoveEntity(EntityID entityID)
        {
            if (!m_Infos.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is not registered: {entityID}");

            var info = m_Infos[entityID];

            if (info.SourceGameObject != null)
                GameObjectUtils.Destroy(info.SourceGameObject);
            if (info.TargetGameObject != null)
                GameObjectUtils.Destroy(info.TargetGameObject);

            m_Infos.Remove(entityID);
        }

        void OnKeySelectionChanged(SelectionModel<TimelineModel.SequenceKey> model)
        {
            ActivateSelectedKey();
        }

        void ActivateSelectedKey()
        {
            UnregisterKey();
            m_TargetKey = m_KeySelectionModel.HasSelection ? m_KeySelectionModel.GetSelection(0) : null;
            RegisterKey();
        }
        
        void RegisterKey()
        {
            if (m_TargetKey == null)
                return;

            m_TargetKey.Key.OnChanged += OnKeyChanged;

            if (m_IsVisible)
                UpdateSourceFrame();
        }

        void UnregisterKey()
        {
            if (m_TargetKey == null)
                return;

            m_TargetKey.Key.OnChanged -= OnKeyChanged;

            m_TargetKey = null;
        }

        void OnKeyChanged(KeyModel model, KeyModel.Property property)
        {
            if (!m_IsVisible)
                return;
            
            if (property == KeyModel.Property.Loop)
            {
                OnLoopChanged?.Invoke(model.Loop);
            }
            
            UpdateSourceFrame();
        }

        void OnBakedTimelineChanged(BakedTimelineModel model)
        {
            if (!m_IsVisible)
                return;

            UpdateSourceFrame();
        }

        void UpdateVisibility()
        {
            if (m_IsVisible)
            {
                UpdateSourceFrame();
            }
            else
            {
                HideAllEntities();
            }
        }

        void UpdateSourceFrame()
        {
            if (m_TargetKey == null)
            {
                HideAllEntities();
                return;
            }

            var loop = m_TargetKey.Key.Loop;

            foreach (var pair in m_Infos)
            {
                var entityID = pair.Key;
                var info = pair.Value;

                if (!loop.TryGetOffset(entityID, out var loopOffset))
                    continue;

                if (!TryGetEntitySourcePose(entityID, out var sourcePose))
                {
                    info.SourceGameObject.SetActive(false);
                    info.TargetGameObject.SetActive(false);
                    continue;
                }

                // Key Translation / Rotation are expressed as offsets of the root joint wrt to its original position
                // Ie, rotation does not impact the root position (ie root position is not rotated)
                // But applying a frame to a skeleton applies world space transforms, ie the root position will be rotated
                // So we must convert the translation to compensate for this rotation
                var sourceRootPosition = GetReferencePosition(sourcePose);
                var correctedTranslation = sourceRootPosition + loopOffset.Position - loopOffset.Rotation * sourceRootPosition;

                info.SourceGameObject.SetActive(m_IsVisible);
                sourcePose.ApplyTo(info.SourceArmature.ArmatureMappingData);

                info.TargetGameObject.SetActive(m_IsVisible);
                sourcePose.ApplyTo(info.TargetArmature.ArmatureMappingData, correctedTranslation, loopOffset.Rotation);
            }
        }

        void HideAllEntities()
        {
            foreach (var pair in m_Infos)
            {
                pair.Value.SourceGameObject.SetActive(false);
                pair.Value.TargetGameObject.SetActive(false);
            }
        }

        public bool HasEntity(EntityID entityID)
        {
            return m_Infos.ContainsKey(entityID);
        }

        public bool TryGetEntityReferencePosition(EntityID entityID, out Vector3 position)
        {
            position = Vector3.zero;

            if (!TryGetEntitySourcePose(entityID, out var sourcePose))
                return false;

            position = GetReferencePosition(sourcePose);
            return true;
        }

        bool TryGetEntitySourcePose(EntityID entityID, out BakedArmaturePoseModel sourcePose)
        {
            sourcePose = null;

            if (!HasEntity(entityID))
                AssertUtils.Fail($"Entity is not registered: {entityID}");

            var loop = m_TargetKey.Key.Loop;
            if (loop.StartFrame < 0 || loop.StartFrame >= m_BakedTimelineModel.FramesCount)
                return false;

            var sourceFrame = m_BakedTimelineModel.GetFrame(loop.StartFrame);
            if (!sourceFrame.TryGetModel(entityID, out sourcePose))
                return false;

            return true;
        }

        Vector3 GetReferencePosition(BakedArmaturePoseModel sourcePose)
        {
            return sourcePose.LocalPose.GetPosition(0);
        }

        public ArmatureMappingComponent GetViewArmature(EntityID entityID, bool sourcePose = false)
        {
            if (!HasEntity(entityID))
                AssertUtils.Fail($"Entity is not registered: {entityID}");

            var info = m_Infos[entityID];
            return sourcePose ? info.SourceArmature : info.TargetArmature;
        }
    }
}
