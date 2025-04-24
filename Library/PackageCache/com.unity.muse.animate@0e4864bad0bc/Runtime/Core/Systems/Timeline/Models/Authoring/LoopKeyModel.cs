using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a loop key, ie a key that points to a past frame, with a transform
    /// </summary>
    [Serializable]
    class LoopKeyModel : ICopyable<LoopKeyModel>, ISerializationCallbackReceiver
    {
        const int k_DefaultNumBakingLoopbacks = 1;

        public enum Property
        {
            StartFrame,
            Transform,
            NumBakingLoopbacks,
            EntityList
        }

        [SerializeField]
        LoopKeyData m_Data;

        [NonSerialized]
        Dictionary<RigidTransformModel, EntityID> m_TransformToEntity = new();
        
        [NonSerialized]
        Dictionary<EntityID, int> m_EntityToListIndex = new();

        /// <summary>
        /// Checks if the key is in a valid state
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (m_Data.StartFrame < 0)
                    return false;

                if (m_Data.Entities == null)
                    return false;

                foreach (var pair in m_Data.Entities)
                {
                    if (!pair.EntityID.IsValid || pair.RigidTransformModel == null || !pair.RigidTransformModel.IsValid)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// This is the number of times that a loop is played back and baked again to ensure better continuity of the loop
        /// 0 means the loop is only baked forward (ie using past prediction only)
        /// Any other value means that the loop is rewinded to its start and baked again using its own future prediction
        /// </summary>
        public int NumBakingLoopbacks
        {
            get => m_Data.NumBakingLoopbacks;

            set
            {
                var correctedValue = value;
                if (correctedValue < 0)
                    correctedValue = k_DefaultNumBakingLoopbacks;

                if (correctedValue == m_Data.NumBakingLoopbacks)
                    return;

                m_Data.NumBakingLoopbacks = correctedValue;
                OnChanged?.Invoke(this, Property.NumBakingLoopbacks);
            }
        }

        /// <summary>
        /// The index of the first frame of the loop
        /// </summary>
        public int StartFrame
        {
            get => m_Data.StartFrame;
            set
            {
                if (value == m_Data.StartFrame)
                    return;

                m_Data.StartFrame = value;
                OnChanged?.Invoke(this, Property.StartFrame);
            }
        }

        public delegate void Changed(LoopKeyModel model, Property property);
        public event Changed OnChanged;

        /// <summary>
        /// Creates a new loop key
        /// </summary>
        /// <param name="translation">The position offset to be applied to the source frame root joint.</param>
        /// <param name="rotation">The rotation offset to be applied to the source frame root joint.</param>
        /// <param name="startFrame">The index of the first frame of the loop.</param>
        public LoopKeyModel(Vector3 translation, Quaternion rotation, int startFrame = 0)
        {
            m_Data.Entities = new List<LoopKeyEntity>();
            m_Data.StartFrame = startFrame;
            m_Data.NumBakingLoopbacks = k_DefaultNumBakingLoopbacks;

            RegisterEvents();
        }

        /// <summary>
        /// Creates a new loop key
        /// </summary>
        /// <param name="translation">The position offset to be applied to the source frame root joint.</param>
        /// <param name="startFrame">The index of the first frame of the loop.</param>
        public LoopKeyModel(Vector3 translation, int startFrame = 0) : this(translation, Quaternion.identity, startFrame) { }

        /// <summary>
        /// Creates a new loop key
        /// </summary>
        /// <param name="rotation">The rotation offset to be applied to the source frame root joint.</param>
        /// <param name="startFrame">The index of the first frame of the loop.</param>
        public LoopKeyModel(Quaternion rotation, int startFrame = 0) : this(Vector3.zero, rotation, startFrame) { }

        /// <summary>
        /// Creates a new loop key
        /// </summary>
        /// <param name="startFrame">The index of the first frame of the loop.</param>
        public LoopKeyModel(int startFrame = 0) : this(Vector3.zero, Quaternion.identity, startFrame) { }

        public void CopyTo(LoopKeyModel target)
        {
            target.StartFrame = StartFrame;
            target.NumBakingLoopbacks = NumBakingLoopbacks;

            // List all entities of this key
            using var myEntityIDs = TempHashSet<EntityID>.Allocate();
            GetAllEntityIDs(myEntityIDs.Set);

            // List all entities of other key
            using var otherEntityIDs = TempHashSet<EntityID>.Allocate();
            target.GetAllEntityIDs(otherEntityIDs.Set);

            // Remove entities that we don't have
            foreach (var entityID in otherEntityIDs)
            {
                if (!HasEntity(entityID))
                    target.RemoveEntity(entityID);
            }

            // Copy transforms, and add entities if required
            foreach (var entityID in myEntityIDs)
            {
                if (!target.HasEntity(entityID))
                    target.AddEntity(entityID);

                if (!target.TryGetOffset(entityID, out var otherOffset))
                    continue;

                if (!TryGetOffset(entityID, out var myOffset))
                    continue;

                myOffset.CopyTo(otherOffset);
            }
        }

        public LoopKeyModel Clone()
        {
            var clone = new LoopKeyModel();
            CopyTo(clone);
            return clone;
        }

        /// <summary>
        /// Adds an entity to the key.
        /// </summary>
        /// <param name="entityID">The ID of the entity to add</param>
        public RigidTransformModel AddEntity(EntityID entityID)
        {
            var model = new RigidTransformModel();
            AddEntityInternal(new LoopKeyEntity(entityID, model));
            OnChanged?.Invoke(this, Property.EntityList);
            return model;
        }

        /// <summary>
        /// Adds an entity to the key.
        /// </summary>
        /// <param name="entityID">The ID of the entity to add</param>
        /// <param name="transformData">The transform data of the entity</param>
        public RigidTransformModel AddEntity(EntityID entityID, RigidTransformData transformData)
        {
            var model = new RigidTransformModel { Transform = transformData };
            AddEntityInternal(new LoopKeyEntity(entityID, model));
            OnChanged?.Invoke(this, Property.EntityList);
            return model;
        }
        
        void AddEntityInternal(LoopKeyEntity entity)
        {
            if (HasEntity(entity.EntityID))
                AssertUtils.Fail($"Entity is already registered: {entity.EntityID}");
            
            m_Data.Entities.Add(entity);
            RegisterEntity(entity, m_Data.Entities.Count-1);
            OnChanged?.Invoke(this, Property.EntityList);
        }
        
        /// <summary>
        /// Removes an entity from the LoopKeyModel
        /// </summary>
        /// <param name="entityID">The ID of the entity to remove</param>
        public void RemoveEntity(EntityID entityID)
        {
            if (!TryGetOffset(entityID, out var model))
                AssertUtils.Fail($"Entity was not registered: {entityID}");
            
            var index = m_EntityToListIndex[entityID];
            m_Data.Entities.RemoveAt(index);
            UnregisterEntity(entityID, model, index);
            OnChanged?.Invoke(this, Property.EntityList);
        }

        /// <summary>
        /// Removes all entities from the key
        /// </summary>
        public void RemoveAllEntities()
        {
            using var tmpList = TempList<EntityID>.Allocate();
            foreach (var entity in m_Data.Entities)
            {
                tmpList.Add(entity.EntityID);
            }

            foreach (var entityID in tmpList.List)
            {
                RemoveEntity(entityID);
            }

            OnChanged?.Invoke(this, Property.EntityList);
        }

        /// <summary>
        /// Checks if an entity is registered in this LoopKeyModel instance.
        /// </summary>
        /// <param name="entityID">The ID of the entity.</param>
        /// <returns>True if the entity was added to this LoopKeyModel instance, False otherwise</returns>
        public bool HasEntity(EntityID entityID)
        {
            return m_TransformToEntity.ContainsValue(entityID);
        }
        
        /// <summary>
        /// Tries to retrieve a RigidTransformModel instance associated to a specific entity.
        /// </summary>
        /// <param name="entityID">The EntityID of the entity.</param>
        /// <param name="model">The retrieved RigidTransformModel instance, if it exists.</param>
        /// <returns>True if the RigidTransformModel exists and was retrieved, false otherwise.</returns>
        public bool TryGetOffset(EntityID entityID, out RigidTransformModel model)
        {
            var index = GetEntityIndex(entityID);

            if (index < 0)
            {
                model = null;
                return false;
            }

            model = m_Data.Entities[index].RigidTransformModel;
            return true;
        }
        
        /// <summary>
        /// Get the index of a LoopKeyEntity inside m_Data.Entities, matching a specified EntityID.
        /// </summary>
        /// <param name="entityID">The EntityID of the KeyEntity's index you are looking for.</param>
        /// <returns>The index of the matching LoopKeyEntity entry, if any. Otherwise, returns -1.</returns>
        public int GetEntityIndex(EntityID entityID)
        {
            for (var i = 0; i < m_Data.Entities.Count; i++)
                if (m_Data.Entities[i].EntityID == entityID)
                    return i;

            return -1;
        }
        
        /// <summary>
        /// Get all the EntityIDs registered within this LoopKeyModel instance.
        /// </summary>
        /// <param name="result">A set where to store the result.</param>
        public void GetAllEntityIDs(HashSet<EntityID> result)
        {
            foreach (var pair in m_Data.Entities)
            {
                result.Add(pair.EntityID);
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            RegisterEvents();
        }
        
        void RegisterEvents()
        {
            for (var index = 0; index < m_Data.Entities.Count; index++)
            {
                RegisterEntity(m_Data.Entities[index], index);
            }
        }

        void RegisterEntity(LoopKeyEntity loopKeyEntity, int listIndex)
        {
            if (m_TransformToEntity.ContainsKey(loopKeyEntity.RigidTransformModel))
                AssertUtils.Fail($"RigidTransformModel already registered for entity: {loopKeyEntity.EntityID}");
            
            m_EntityToListIndex[loopKeyEntity.EntityID] = listIndex;
            m_TransformToEntity[loopKeyEntity.RigidTransformModel] = loopKeyEntity.EntityID;
            
            loopKeyEntity.RigidTransformModel.OnChanged += OnEntityTransformModelChanged;
        }

        void UnregisterEntity(EntityID entityID, RigidTransformModel transformModel, int index)
        {
            if (!m_EntityToListIndex.ContainsKey(entityID))
                AssertUtils.Fail($"Cannot unregister entity, it is not registered: {entityID}");

            // Update the EntityID -> ListIndex mapping Dictionary entries affected by the removal of the entity
            m_EntityToListIndex.Remove(entityID);
            for (var newIndex = Mathf.Max(0, index - 1); newIndex < m_Data.Entities.Count; newIndex++)
            {
                m_EntityToListIndex[m_Data.Entities[newIndex].EntityID] = newIndex;
            }

            // Remove from RigidTransformModel -> EntityID mapping Dictionary
            m_TransformToEntity.Remove(transformModel);

            // Unsubscribe from RigidTransformModel
            transformModel.OnChanged -= OnEntityTransformModelChanged;
        }

        void OnEntityTransformModelChanged(RigidTransformModel model)
        {
            OnChanged?.Invoke(this, Property.Transform);
        }

    }
}
