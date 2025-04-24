using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a key of multiple entities
    /// </summary>
    [Serializable]
    class KeyModel : ICopyable<KeyModel>, ISerializationCallbackReceiver
    {
        [SerializeField]
        KeyData m_Data;

        public enum Property
        {
            Type,
            EntityList,
            EntityKey,
            Loop,
            Thumbnail
        }

        public ThumbnailModel Thumbnail => m_Data.Thumbnail;

        [NonSerialized]
        Dictionary<EntityKeyModel, EntityID> m_KeyToEntity = new();

        [NonSerialized]
        Dictionary<EntityID, int> m_EntityToListIndex = new();

        /// <summary>
        /// Checks if the key is in a valid state
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (m_Data.Entities == null)
                    return false;

                foreach (var key in m_Data.Entities)
                {
                    if (!key.ID.IsValid || key.Model == null || !key.Model.IsValid)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// The type of this key
        /// </summary>
        public KeyData.KeyType Type
        {
            get => m_Data.Type;
            set => SetType(value, false);
        }

        /// <summary>
        /// The loop key model
        /// </summary>
        public LoopKeyModel Loop => m_Data.LoopKey;

        /// <summary>
        /// The index of this key in its timeline
        /// </summary>
        public int ListIndex { get; set; }

        public delegate void Changed(KeyModel model, Property property);

        public event Changed OnChanged;

        /// <summary>
        /// Creates a new key
        /// </summary>
        public KeyModel()
        {
            m_Data.Type = KeyData.KeyType.FullPose;
            m_Data.Entities = new List<KeyEntity>();
            m_Data.LoopKey = new LoopKeyModel();
            m_Data.Thumbnail = new ThumbnailModel();

            RegisterEvents();
        }

        public void OnAfterDeserialize()
        {
            RegisterEvents();
        }

        public void OnBeforeSerialize() { }

        public void CopyTo(KeyModel other)
        {
            CopyTo(other, false);
        }
        
        public void CopyTo(KeyModel other, bool silent)
        {
            other.SetType(Type, silent);
            
            other.ListIndex = ListIndex;

            // List all entities of this key
            using var myEntityIDs = TempHashSet<EntityID>.Allocate();
            GetAllEntityIDs(myEntityIDs.Set);

            // List all entities of other key
            using var otherEntityIDs = TempHashSet<EntityID>.Allocate();
            other.GetAllEntityIDs(otherEntityIDs.Set);

            // Remove entities that we don't have
            foreach (var entityID in otherEntityIDs)
            {
                if (!HasEntity(entityID))
                    other.RemoveEntity(entityID);
            }

            // Copy transforms, and add entities if required
            foreach (var entityID in myEntityIDs)
            {
                if (!TryGetModel(entityID, out var myKey))
                    continue;

                if (!other.HasEntity(entityID))
                    other.AddEntity(entityID, myKey.Posing, myKey.LocalPose, myKey.GlobalPose);

                if (!other.TryGetModel(entityID, out var otherKey))
                    continue;

                myKey.CopyTo(otherKey);
            }

            Loop.CopyTo(other.Loop);
            Thumbnail.CopyTo(other.Thumbnail);
            
            if(!silent)
                other.OnChanged?.Invoke(other, Property.EntityKey);
        }

        internal void SetType(KeyData.KeyType value, bool silent)
        {
            if (value == m_Data.Type)
                return;

            m_Data.Type = value;
            
            if(!silent)
                OnChanged?.Invoke(this, Property.Type);
        }

        /// <summary>
        /// Create a duplicate of this key
        /// </summary>
        /// <returns>A new key instance that is a copy of this key</returns>
        public KeyModel Clone()
        {
            var newKey = new KeyModel();
            CopyTo(newKey);
            return newKey;
        }

        /// <summary>
        /// Adds an entity to the key.
        /// </summary>
        /// <param name="entityID">The ID of the entity to add</param>
        /// <param name="posing">The initial posing state of the key, which will be copied to the key.</param>
        /// <param name="numJoints">The number of joints of the entity.</param>
        public void AddEntity(EntityID entityID, PosingModel posing, int numJoints)
        {
            var keyModel = new EntityKeyModel(posing, numJoints);
            AddEntityInternal(new KeyEntity(entityID, keyModel));
        }

        /// <summary>
        /// Adds an entity to the key.
        /// </summary>
        /// <param name="entityID">The ID of the entity to add</param>
        /// <param name="posing">The initial posing state of the key, which will be copied to the key.</param>
        /// <param name="localPose">The initial local pose state of the key, which will be copied to the key.</param>
        /// <param name="globalPose">The initial global pose state of the key, which will be copied to the key.</param>
        public EntityKeyModel AddEntity(EntityID entityID, PosingModel posing, ArmatureStaticPoseModel localPose, ArmatureStaticPoseModel globalPose)
        {
            var keyModel = new EntityKeyModel(posing, localPose, globalPose);
            AddEntityInternal(new KeyEntity(entityID, keyModel));
            
            return keyModel;
        }

        /// <summary>
        /// Removes an entity from the key
        /// </summary>
        /// <param name="entityID">The ID of the entity to remove</param>
        /// <param name="silent">If this call will trigger a change event or not.</param>
        public void RemoveEntity(EntityID entityID, bool silent = false)
        {
            if (!TryGetModel(entityID, out var model))
                AssertUtils.Fail($"Entity was not registered: {entityID}");
            var index = m_EntityToListIndex[entityID];
            m_Data.Entities.RemoveAt(index);
            UnregisterEntity(entityID, model, index);
            m_Data.LoopKey.RemoveEntity(entityID);
            
            if(!silent)
                OnChanged?.Invoke(this, Property.EntityList);
        }

        /// <summary>
        /// Checks if an entity is registered in the key
        /// </summary>
        /// <param name="entityID">The ID of the entity</param>
        /// <returns>True if the entity was added to the key, False otherwise</returns>
        public bool HasEntity(EntityID entityID)
        {
            return m_KeyToEntity.ContainsValue(entityID);
        }

        /// <summary>
        /// Checks if all given entities are registered in the key
        /// </summary>
        /// <param name="entityIds">The ID of the entities</param>
        /// <returns>True if all the entities were added to the key, False otherwise</returns>
        public bool HasEntities(IEnumerable<EntityID> entityIds)
        {
            foreach (var entityID in entityIds)
            {
                if (!HasEntity(entityID))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if both keys have the exact same list of entities
        /// </summary>
        /// <param name="other">The other key to check with</param>
        /// <returns>true if both keys have the same entities, false otherwise</returns>
        public bool HasSameEntitiesAs(KeyModel other)
        {
            {
                using var entityIds = TempHashSet<EntityID>.Allocate();
                GetAllEntityIDs(entityIds.Set);

                if (!other.HasEntities(entityIds.Set))
                    return false;
            }

            {
                using var entityIds = TempHashSet<EntityID>.Allocate();
                other.GetAllEntityIDs(entityIds.Set);

                if (!HasEntities(entityIds.Set))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get all the EntityIDs registered within this KeyModel instance.
        /// </summary>
        /// <param name="result">A set where to store the result.</param>
        public void GetAllEntityIDs(HashSet<EntityID> result)
        {
            foreach (var pair in m_Data.Entities)
            {
                result.Add(pair.ID);
            }
        }

        /// <summary>
        /// Get the index of a KeyEntity inside m_Data.Entities, matching a specified EntityID.
        /// </summary>
        /// <param name="entityID">The EntityID of the KeyEntity's index you are looking for.</param>
        /// <returns>The index of the matching KeyEntity entry, if any. Otherwise, returns -1.</returns>
        public int GetEntityIndex(EntityID entityID)
        {
            for (var i = 0; i < m_Data.Entities.Count; i++)
                if (m_Data.Entities[i].ID == entityID)
                    return i;

            return -1;
        }

        /// <summary>
        /// Tries to retrieve an entity key
        /// </summary>
        /// <param name="entityID">The entity ID</param>
        /// <param name="model">The retrieved EntityKeyModel if it exists</param>
        /// <returns>True if the EntityKeyModel exists and was retrieved, False otherwise</returns>
        public bool TryGetModel(EntityID entityID, out EntityKeyModel model)
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

        void AddEntityInternal(KeyEntity keyEntity, bool silent = false)
        {
            if (HasEntity(keyEntity.ID))
                AssertUtils.Fail($"Entity is already registered: {keyEntity.ID}");
            
            m_Data.Entities.Add(keyEntity);
            RegisterEntity(keyEntity, m_Data.Entities.Count-1);
            
            m_Data.LoopKey.AddEntity(keyEntity.ID);
            
            if(!silent)
                OnChanged?.Invoke(this, Property.EntityList);
        }

        void RegisterEvents()
        {
            m_Data.LoopKey.OnChanged += OnLoopKeyChanged;
            m_Data.Thumbnail.OnChanged += OnThumbnailChanged;

            if (m_Data.Entities == null)
            {
                return;
            }

            for (int i = 0; i < m_Data.Entities.Count; i++)
            {
                RegisterEntity(m_Data.Entities[i], i);
            }
        }

        void OnThumbnailChanged()
        {
            OnChanged?.Invoke(this, Property.Thumbnail);
        }

        void UnregisterEvents()
        {
            m_Data.LoopKey.OnChanged -= OnLoopKeyChanged;
            m_Data.Thumbnail.OnChanged -= OnThumbnailChanged;

            if (m_Data.Entities == null)
            {
                return;
            }

            for (var i = 0; i < m_Data.Entities.Count; i++)
            {
                UnregisterEntity(m_Data.Entities[i].ID, m_Data.Entities[i].Model, i);
            }
        }

        void RemoveAllEntities()
        {
            using var tmpList = TempList<EntityID>.Allocate();
            
            foreach (var pair in m_KeyToEntity)
            {
                tmpList.Add(pair.Value);
            }

            foreach (var entityID in tmpList)
            {
                RemoveEntity(entityID);
            }
        }

        void OnLoopKeyChanged(LoopKeyModel model, LoopKeyModel.Property property)
        {
            OnChanged?.Invoke(this, Property.Loop);
        }

        void RegisterEntity(KeyEntity keyEntity, int listIndex)
        {
            if (m_EntityToListIndex.ContainsKey(keyEntity.ID))
                AssertUtils.Fail($"EntityID was already registered for entity: {keyEntity.ID}");

            m_KeyToEntity[keyEntity.Model] = keyEntity.ID;
            m_EntityToListIndex[keyEntity.ID] = listIndex;

            keyEntity.Model.OnChanged += OnEntityKeyModelChanged;
        }

        void UnregisterEntity(EntityID entityID, EntityKeyModel entityKeyModel, int previousIndex)
        {
            if (!m_EntityToListIndex.ContainsKey(entityID))
                AssertUtils.Fail($"Cannot unregister entity, it is not registered: {entityID}");

            // Update the Entity -> ListIndex Dictionary entries affected by the removal of the entity
            m_EntityToListIndex.Remove(entityID);
            for (var newIndex = Mathf.Max(0, previousIndex - 1); newIndex < m_Data.Entities.Count; newIndex++)
            {
                m_EntityToListIndex[m_Data.Entities[newIndex].ID] = newIndex;
            }

            // Remove from EntityKeyModel -> EntityID mapping Dictionary
            m_KeyToEntity.Remove(entityKeyModel);

            // Unsubscribe from EntityKeyModel
            entityKeyModel.OnChanged -= OnEntityKeyModelChanged;
        }

        void OnEntityKeyModelChanged(EntityKeyModel model, EntityKeyModel.Property property)
        {
            OnChanged?.Invoke(this, Property.EntityKey);
        }

        public void SetThumbnailCameraTransform(Vector3 position, Quaternion rotation)
        {
            m_Data.Thumbnail.Position = position;
            m_Data.Thumbnail.Rotation = rotation;
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();
            var first = true;
            foreach (var entry in m_Data.Entities)
            {
                if (first)
                {
                    bounds = entry.Model.GetWorldBounds();
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(entry.Model.GetWorldBounds());
                }
            }

            return bounds;
        }

        public bool TryGetKey(EntityID entityID, out EntityKeyModel keyModel)
        {
            return TryGetModel(entityID, out keyModel);
        }
    }
}
