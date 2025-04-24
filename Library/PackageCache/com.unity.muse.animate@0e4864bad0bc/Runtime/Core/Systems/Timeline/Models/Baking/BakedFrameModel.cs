using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BakedFrameModel : ICopyable<BakedFrameModel>, ISerializationCallbackReceiver
    {
        [SerializeField]
        BakedFrameData m_Data;

        [NonSerialized]
        Dictionary<BakedArmaturePoseModel, EntityID> m_ArmatureToEntity = new();

        [NonSerialized]
        Dictionary<EntityID, int> m_EntityToListIndex = new();

        [NonSerialized]
        private List<EntityPose> m_RegisteredEventTargets = new();

        public bool IsValid
        {
            get
            {
                if (m_Data.Entities == null)
                    return false;

                foreach (var pair in m_Data.Entities)
                {
                    if (!pair.ID.IsValid || pair.Model is not { IsValid: true })
                        return false;
                }

                return true;
            }
        }

        public delegate void EntityAdded(BakedFrameModel model, EntityID entityID);

        public event EntityAdded OnEntityAdded;

        public delegate void EntityRemoved(BakedFrameModel model, EntityID entityID);

        public event EntityRemoved OnEntityRemoved;

        public delegate void EntityPoseChanged(BakedFrameModel model, EntityID entityID);

        public event EntityPoseChanged OnEntityPoseChanged;

        public BakedFrameModel()
        {
            m_Data.Entities = new List<EntityPose>();
            RegisterEntities();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            UnregisterEvents();
            ResetNonSerializedData();
            RegisterEntities();
        }

        public BakedFrameModel(BakedFrameModel other)
        {
            m_Data.Entities = new List<EntityPose>();
            other.CopyTo(this);
        }

        public void AddEntity(EntityID entityID, int numJoints, int numBodies)
        {
            if (HasEntity(entityID))
                AssertUtils.Fail($"Entity is already registered: {entityID}");

            var entry = new EntityPose(entityID, new BakedArmaturePoseModel(numJoints, numBodies));
            m_Data.Entities.Add(entry);
            RegisterEntity(entry, m_Data.Entities.Count - 1);
            OnEntityAdded?.Invoke(this, entityID);
        }

        public void RemoveEntity(EntityID entityID)
        {
            if (!TryGetModel(entityID, out var poseModel))
                AssertUtils.Fail($"Entity was not registered: {entityID}");

            var index = m_EntityToListIndex[entityID];
            UnregisterEntity(entityID, poseModel, index);
            m_Data.Entities.RemoveAt(index);
            OnEntityRemoved?.Invoke(this, entityID);
        }

        int GetEntityIndex(EntityID id)
        {
            for (var i = 0; i < m_Data.Entities.Count; i++)
            {
                if (id == m_Data.Entities[i].ID)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool HasEntity(EntityID entityID)
        {
            return m_ArmatureToEntity.ContainsValue(entityID);
        }

        public void GetAllEntities(HashSet<EntityID> set)
        {
            foreach (var entityPose in m_Data.Entities)
            {
                set.Add(entityPose.ID);
            }
        }

        public bool TryGetModel(EntityID entityID, out BakedArmaturePoseModel model)
        {
            var index = GetEntityIndex(entityID);

            if (index < 0)
            {
                model = null;
                return false;
            }

            model = m_Data.Entities[index].Model;
            return true;
        }

        void RegisterEntities()
        {
            for (var index = 0; index < m_Data.Entities.Count; index++)
            {
                RegisterEntity(m_Data.Entities[index], index);
            }
        }

        void RegisterEntity(EntityPose entityPose, int listIndex)
        {
            if (m_EntityToListIndex.ContainsKey(entityPose.ID))
                AssertUtils.Fail($"Armature was already registered for entity: {entityPose.ID}");

            m_EntityToListIndex[entityPose.ID] = listIndex;
            m_ArmatureToEntity[entityPose.Model] = entityPose.ID;

            m_RegisteredEventTargets.Add(entityPose);
            RegisterEvent(entityPose);
        }

        void UnregisterEntity(EntityID entityID, BakedArmaturePoseModel poseModel, int previousIndex)
        {
            if (!m_EntityToListIndex.TryGetValue(entityID, out var index))
                AssertUtils.Fail($"Cannot unregister entity, it is not registered: {entityID}");

            var entityPose = m_Data.Entities[index];
            // Update the Entity -> ListIndex Dictionary entries affected by the removal of the entity
            m_EntityToListIndex.Remove(entityID);
            for (var newIndex = Mathf.Max(0, previousIndex - 1); newIndex < m_Data.Entities.Count; newIndex++)
            {
                m_EntityToListIndex[m_Data.Entities[newIndex].ID] = newIndex;
            }

            // Remove from Armature -> EntityID Dictionary
            m_ArmatureToEntity.Remove(poseModel);

            // Unsubscribe from model events
            m_RegisteredEventTargets.Remove(entityPose);
            UnregisterEvent(entityPose);
        }

        void OnArmaturePoseModelChanged(BakedArmaturePoseModel model, BakedArmaturePoseModel.Property property)
        {
            var entityID = m_ArmatureToEntity[model];
            OnEntityPoseChanged?.Invoke(this, entityID);
        }

        public void CopyTo(BakedFrameModel target)
        {
            CopyTo(target, false);
        }

        public void CopyTo(BakedFrameModel target, bool silent)
        {
            target.UnregisterEvents();

            target.m_Data.Entities = new();
            target.ResetNonSerializedData();

            foreach (var pose in m_Data.Entities)
            {
                target.m_Data.Entities.Add(new EntityPose(pose.ID, new BakedArmaturePoseModel(pose.Model)));
            }

            target.RegisterEntities();
        }

        void ResetNonSerializedData()
        {
            m_RegisteredEventTargets.Clear();
            m_ArmatureToEntity.Clear();
            m_EntityToListIndex.Clear();
        }

        void UnregisterEvents()
        {
            foreach (var entityPose in m_RegisteredEventTargets)
                UnregisterEvent(entityPose);
            
            m_RegisteredEventTargets.Clear();
        }
        
        void RegisterEvent(EntityPose entityPose)
        {
            entityPose.Model.OnChanged += OnArmaturePoseModelChanged;
        }
        
        void UnregisterEvent(EntityPose entityPose)
        {
            entityPose.Model.OnChanged -= OnArmaturePoseModelChanged;
        }

        public BakedFrameModel Clone()
        {
            return new BakedFrameModel(this);
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();
            var first = true;

            foreach (var entityPose in m_Data.Entities)
            {
                if (first)
                {
                    bounds = entityPose.Model.GetWorldBounds();
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(entityPose.Model.GetWorldBounds());
                }
            }

            return bounds;
        }

        public Bounds GetLocalBounds()
        {
            var bounds = new Bounds();
            var first = true;

            foreach (var entityPose in m_Data.Entities)
            {
                if (first)
                {
                    bounds = entityPose.Model.GetLocalBounds();
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(entityPose.Model.GetLocalBounds());
                }
            }

            return bounds;
        }
    }
}
