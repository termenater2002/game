using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Handles the logic of rendering of a Baked Timeline.
    /// </summary>
    class BakedTimelineViewLogic
    {
        struct EntityData
        {
            public GameObject PreviewGameObject;
            public ArmatureMappingComponent PreviewArmature;
            public Vector3 CumulatedTranslation;
            public Quaternion CumulatedRotation;
        }

        BakedTimelineModel m_BakedTimelineModel;
        SelectionModel<EntityID> m_EntitySelectionModel;

        Dictionary<EntityID, EntityData> m_EntityData = new();
        bool m_IsVisible;
        float m_CurrentFrameIndex;

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

        public BakedTimelineViewLogic(string name)
        {
            Name = name;
        }
        
        public BakedTimelineViewLogic(string name, BakedTimelineModel bakedTimelineModel, SelectionModel<EntityID> entitySelectionModel)
        {
            Name = name;
            SetModel(bakedTimelineModel);
            SetSelection(entitySelectionModel);
            UpdateVisibility();
        }

        public void SetModel(BakedTimelineModel bakedTimelineModel)
        {
            m_BakedTimelineModel = bakedTimelineModel;
        }
        
        public void SetSelection(SelectionModel<EntityID> entitySelectionModel)
        {
            m_EntitySelectionModel = entitySelectionModel;
        }
        
        void UpdateVisibility()
        {
            if (m_IsVisible)
            {
                DisplayCurrentFrame();
            }
            else
            {
                HideAllEntities();
            }
        }

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referenceViewArmature, int layer)
        {
            if (m_EntityData.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is already registered: {entityID}");

            var viewArmature = referenceViewArmature.Clone(layer);
            viewArmature.name = Name + " - View";
            
            var gameObject = viewArmature.gameObject;

            if (m_EntitySelectionModel != null)
            {
                EntityUtils.SetupSelectionView(m_EntitySelectionModel, entityID, gameObject);
            }

            gameObject.SetActive(false);

            var previewInfo = new EntityData
            {
                PreviewGameObject = gameObject,
                PreviewArmature = viewArmature,
                CumulatedTranslation = Vector3.zero,
                CumulatedRotation = Quaternion.identity
            };

            m_EntityData[entityID] = previewInfo;
            UpdateVisibility();
        }

        public string Name { get; set; }

        public void RemoveEntity(EntityID entityID)
        {
            if (!m_EntityData.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is not registered: {entityID}");

            var entityData = m_EntityData[entityID];

            if (entityData.PreviewGameObject != null)
                GameObjectUtils.Destroy(entityData.PreviewGameObject);

            m_EntityData.Remove(entityID);
        }

        public ArmatureMappingComponent GetPreviewArmature(EntityID entityID)
        {
            if (!m_EntityData.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is not registered: {entityID}");

            return  m_EntityData[entityID].PreviewArmature;
        }

        public GameObject GetPreviewGameObject(EntityID entityID)
        {
            if (!m_EntityData.ContainsKey(entityID))
                throw new ArgumentOutOfRangeException($"Entity is not registered: {entityID}");

            return m_EntityData[entityID].PreviewGameObject;
        }

        void HideAllEntities()
        {
            foreach (var pair in m_EntityData)
            {
                pair.Value.PreviewGameObject.SetActive(false);
            }
        }

        public void ResetAllLoopOffsets()
        {
            using var tmpList = TempList<EntityID>.Allocate();
            foreach (var pair in m_EntityData)
            {
                tmpList.Add(pair.Key);
            }

            foreach (var entityID in tmpList.List)
            {
                ResetLoopOffsets(entityID);
            }
        }

        public void ResetLoopOffsets(EntityID entityID)
        {
            var previewInfo = m_EntityData[entityID];

            previewInfo.CumulatedTranslation = Vector3.zero;
            previewInfo.CumulatedRotation = Quaternion.identity;

            m_EntityData[entityID] = previewInfo;
        }

        public void AddLoopOffset(EntityID entityID, Vector3 translation, Quaternion rotation)
        {
            var previewInfo = m_EntityData[entityID];

            var rotatedOffset = previewInfo.CumulatedRotation * translation;
            previewInfo.CumulatedTranslation += rotatedOffset;
            previewInfo.CumulatedRotation = rotation * previewInfo.CumulatedRotation;

            m_EntityData[entityID] = previewInfo;
        }

        public void DisplayFrame(float frameIndex)
        {
            var prevFrameIndex = Mathf.FloorToInt(frameIndex);
            var nextFrameIndex = Mathf.CeilToInt(frameIndex);
            var interpolationT = frameIndex - prevFrameIndex;

            // NOTE: I added a Mathf.Max because Mathf.Clamp allowed a result of -1 if FramesCount value was 0.
            // Somehow setting -1 as the max in Mathf.Clamp reverses the min and max??
            
            prevFrameIndex = Mathf.Clamp(prevFrameIndex, 0, Mathf.Max(0,m_BakedTimelineModel.FramesCount - 1));
            nextFrameIndex = Mathf.Clamp(nextFrameIndex, 0, Mathf.Max(0,m_BakedTimelineModel.FramesCount - 1));

            var prevFrame = m_BakedTimelineModel.GetFrame(prevFrameIndex);
            var nextFrame = m_BakedTimelineModel.GetFrame(nextFrameIndex);

            DisplayFrame(prevFrame, nextFrame, interpolationT);

            m_CurrentFrameIndex = frameIndex;
        }
        
        public Bounds GetBounds()
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            
            foreach (var pair in m_EntityData)
            {
                var actorBounds = pair.Value.PreviewGameObject.GetRenderersWorldBounds();
                
                // If this is the first entry, we don't encapsulate.
                if (bounds.size == Vector3.zero)
                {
                    bounds = actorBounds;
                    continue;
                }
                
                bounds.Encapsulate(actorBounds);
            }

            return bounds;
        }

        void DisplayCurrentFrame()
        {
            if (m_BakedTimelineModel == null || m_CurrentFrameIndex < 0 || m_CurrentFrameIndex >= m_BakedTimelineModel.FramesCount)
                return;

            DisplayFrame(m_CurrentFrameIndex);
        }

        void DisplayFrame(BakedFrameModel prevFrame, BakedFrameModel nextFrame, float t)
        {
            var firstFrame = m_BakedTimelineModel.GetFrame(0);

            foreach (var pair in m_EntityData)
            {
                var entityID = pair.Key;
                var previewInfo = pair.Value;

                if (!prevFrame.TryGetModel(entityID, out var prevBakedPose)
                    || !nextFrame.TryGetModel(entityID, out var nextBakedPose)
                    || !firstFrame.TryGetModel(entityID, out var firstBakedPose))
                {
                    previewInfo.PreviewGameObject.SetActive(false);
                    continue;
                }

                // Key Translation / Rotation are expressed as offsets of the root joint wrt to its original position
                // Ie, rotation does not impact the root position (ie root position is not rotated)
                // But applying a frame to a skeleton applies world space transforms, ie the root position will be rotated
                // So we must convert the translation to compensate for this rotation
                var sourceRootPosition = firstBakedPose.LocalPose.GetPosition(0);
                var correctedTranslation = sourceRootPosition + previewInfo.CumulatedTranslation - previewInfo.CumulatedRotation * sourceRootPosition;

                previewInfo.PreviewGameObject.SetActive(m_IsVisible);
                BakedArmaturePoseModel.ApplyInterpolated(previewInfo.PreviewArmature.ArmatureMappingData,
                    prevBakedPose, nextBakedPose, t, correctedTranslation, previewInfo.CumulatedRotation);
            }
        }
        
        public void DisplayFrame(BakedFrameModel frame)
        {
            var firstFrame = m_BakedTimelineModel.GetFrame(0);

            foreach (var pair in m_EntityData)
            {
                var entityID = pair.Key;
                var previewInfo = pair.Value;

                if (!frame.TryGetModel(entityID, out var framePose)
                    || !firstFrame.TryGetModel(entityID, out var firstBakedPose))
                {
                    previewInfo.PreviewGameObject.SetActive(false);
                    continue;
                }

                // Key Translation / Rotation are expressed as offsets of the root joint wrt to its original position
                // Ie, rotation does not impact the root position (ie root position is not rotated)
                // But applying a frame to a skeleton applies world space transforms, ie the root position will be rotated
                // So we must convert the translation to compensate for this rotation
                var sourceRootPosition = firstBakedPose.LocalPose.GetPosition(0);
                var correctedTranslation = sourceRootPosition + previewInfo.CumulatedTranslation - previewInfo.CumulatedRotation * sourceRootPosition;

                previewInfo.PreviewGameObject.SetActive(m_IsVisible);

                var renderer = previewInfo.PreviewGameObject.GetComponent<SkinnedMeshRenderer>();
                
                if (renderer != null)
                {
                    var update = renderer.updateWhenOffscreen;
                
                    if (!update)
                    {
                        Debug.Log("Corrected");
                        renderer.updateWhenOffscreen = true;
                    }
                }
                
                framePose.ApplyTo(previewInfo.PreviewArmature.ArmatureMappingData, correctedTranslation, previewInfo.CumulatedRotation);
            }
        }
        
        public void DisplayPose(EntityID entityID, BakedArmaturePoseModel pose)
        {
            if (m_EntityData.TryGetValue(entityID, out var entity))
            {
                entity.PreviewGameObject.SetActive(m_IsVisible);
                pose.ApplyTo(entity.PreviewArmature.ArmatureMappingData);
            }
        }
    }
}
