using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Handles the display logic of a Key on the timeline, displaying the solved poses. Used for thumbnails rendering.
    /// </summary>
    class TimelineKeyViewLogic
    {
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                UpdateEntities();
            }
        }

        struct EntityData
        {
            public ArmatureStaticPoseModel GlobalPose;
            public ArmatureMappingComponent Armature;
            public StaticPoseView View;
        }

        Dictionary<EntityID, EntityData> m_Entities = new();

        bool m_IsVisible;

        public void AddEntity(EntityID entityID, ArmatureMappingComponent referenceViewArmature, int layer)
        {
            if (m_Entities.ContainsKey(entityID))
                AssertUtils.Fail($"Entity is already registered: {entityID}");

            var armature = referenceViewArmature.Clone(layer);
            var globalPose = new ArmatureStaticPoseModel(armature.NumJoints, ArmatureStaticPoseData.PoseType.Global);
            var view = SetupSolvedPoseView(globalPose, armature.gameObject, layer);

            armature.gameObject.SetActive(false);

            var entityData = new EntityData()
            {
                GlobalPose = globalPose,
                View = view,
                Armature = armature
            };

            m_Entities[entityID] = entityData;
            
            UpdateEntityView(entityID);
        }

        public void RemoveEntity(EntityID entityID)
        {
            if (!m_Entities.ContainsKey(entityID))
                AssertUtils.Fail($"Entity is not registered: {entityID}");

            var entityData = m_Entities[entityID];

            if (entityData.Armature != null)
                GameObjectUtils.Destroy(entityData.Armature.gameObject);

            m_Entities.Remove(entityID);
        }

        public void ApplyKey(KeyModel keyModel)
        {
            foreach (var pair in m_Entities)
            {
                var entityID = pair.Key;

                if (!keyModel.TryGetKey(entityID, out var entityKeyModel))
                    continue;

                RestoreEntityKey(entityID, entityKeyModel);
            }

            UpdateEntities();
        }

        void RestoreEntityKey(EntityID entityID, EntityKeyModel entityKeyModel)
        {
            var entityData = m_Entities[entityID];
            entityKeyModel.GlobalPose.CopyTo(entityData.GlobalPose);
        }

        static StaticPoseView SetupSolvedPoseView(ArmatureStaticPoseModel solvedPoseGlobal, GameObject gameObject, int layer)
        {
            var poseViewModel = new StaticPoseViewModel(solvedPoseGlobal);
            var poseView = gameObject.AddComponent<StaticPoseView>();
            poseView.Setup(poseViewModel, "Timeline Key - Static Pose", false, true, layer);
            return poseView;
        }

        void UpdateEntities()
        {
            foreach (var pair in m_Entities)
            {
                UpdateEntityView(pair.Key);
            }
        }

        void UpdateEntityView(EntityID entityID)
        {
            var entityData = m_Entities[entityID];
            entityData.Armature.gameObject.SetActive(m_IsVisible);
            entityData.View.enabled = m_IsVisible;

            if (m_IsVisible)
            {
                // Force the pose to be updated right away in order for the camera to pick it up
                entityData.View.ApplyPose();
            }
        }

        public Bounds GetBounds()
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            
            foreach (var pair in m_Entities)
            {
                var actorBounds = pair.Value.View.gameObject.GetRenderersWorldBounds();
                
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
    }
}
