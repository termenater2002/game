using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a sequence of keys with transitions between them
    /// </summary>
    [Serializable]
    class TimelineModel : ICopyable<TimelineModel>, ISerializationCallbackReceiver
    {
        public enum Property
        {
            ItemType,
            IsValid,
            IsEditable,
            IsBaking,
            Title,
            Description,
            Progress,
            Thumbnail,
            OutputAnimationData,
            InputAnimationData
        }
        
        static SequenceTransition NewTransition()
        {
            return NewTransition(new TransitionModel());
        }

        static SequenceTransition NewTransition(TransitionModel transitionModel)
        {
            var sequenceTransition = new SequenceTransition(transitionModel);
            return sequenceTransition;
        }

        static SequenceKey NewKey()
        {
            var keyModel = new KeyModel();
            return NewKey(keyModel);
        }

        static SequenceKey NewKey(KeyModel keyModel)
        {
            var sequenceKey = new SequenceKey(keyModel);
            return sequenceKey;
        }

        /// <summary>
        /// Represents a key in a sequence
        /// </summary>
        public class SequenceKey
        {
            /// <summary>
            /// The actual key model.
            /// </summary>
            public KeyModel Key { get; }

            /// <summary>
            /// The transition before the key, if any.
            /// </summary>
            public SequenceTransition InTransition { get; internal set; }

            /// <summary>
            /// The transition after the key, if any.
            /// </summary>
            public SequenceTransition OutTransition { get; internal set; }

            /// <summary>
            /// The next key in the sequence, if any.
            /// </summary>
            public SequenceKey NextKey => OutTransition?.ToKey;

            /// <summary>
            /// The previous key in the sequence, if any.
            /// </summary>
            public SequenceKey PreviousKey => InTransition?.FromKey;

            public ThumbnailModel Thumbnail => Key.Thumbnail;

            internal SequenceKey(KeyModel keyModel)
            {
                Key = keyModel;
                InTransition = null;
                OutTransition = null;
            }

            /// <summary>
            /// Retrieves the next key that has the given entity
            /// </summary>
            /// <param name="entityID">The entity ID to look for</param>
            /// <param name="includeSelf">If true, this key will be included in the search</param>
            /// <returns>The first key that contains the entity, null if none</returns>
            public SequenceKey GetNextKey(EntityID entityID, bool includeSelf = true)
            {
                var startKey = includeSelf ? this : NextKey;
                while (startKey != null && !startKey.Key.HasEntity(entityID))
                {
                    startKey = startKey.NextKey;
                }

                return startKey;
            }

            /// <summary>
            /// Retrieves the previous key that has the given entity
            /// </summary>
            /// <param name="entityID">The entity ID to look for</param>
            /// <param name="includeSelf">If true, this key will be included in the search</param>
            /// <returns>The first key that contains the entity, null if none</returns>
            public SequenceKey GetPreviousKey(EntityID entityID, bool includeSelf = true)
            {
                var startKey = includeSelf ? this : PreviousKey;
                while (startKey != null && !startKey.Key.HasEntity(entityID))
                {
                    startKey = startKey.PreviousKey;
                }

                return startKey;
            }
        }

        /// <summary>
        /// Represents a transition in a sequence
        /// </summary>
        public class SequenceTransition
        {
            /// <summary>
            /// The actual transition model
            /// </summary>
            public TransitionModel Transition { get; }

            /// <summary>
            /// The key at the beginning of the transition.
            /// </summary>
            public SequenceKey FromKey { get; internal set; }

            /// <summary>
            /// The key at the end of the transition
            /// </summary>
            public SequenceKey ToKey { get; internal set; }

            internal SequenceTransition(TransitionModel transitionModel)
            {
                Transition = transitionModel;
                FromKey = null;
                ToKey = null;
            }
        }

        [SerializeField]
        TimelineData m_Data;

        [NonSerialized]
        List<SequenceKey> m_SequenceKeys = new();

        [NonSerialized]
        List<SequenceTransition> m_SequenceTransitions = new();

        [NonSerialized]
        ReadOnlyCollection<SequenceKey> m_SequenceKeysReadOnly;

        [NonSerialized]
        ReadOnlyCollection<SequenceTransition> m_SequenceTransitionsReadOnly;

        /// <summary>
        /// Checks if the timeline data is in a valid state
        /// </summary>
        public bool IsValid => m_Data.Keyframes != null
            && m_Data.Transitions != null
            && m_Data.Transitions.Count == Mathf.Max(0, m_Data.Keyframes.Count - 1)
            && m_SequenceKeys.Count == m_Data.Keyframes.Count
            && m_SequenceTransitions.Count == m_Data.Transitions.Count;

        /// <summary>
        /// The total number of keys in the sequence
        /// </summary>
        public int KeyCount => m_Data.Keyframes.Count;

        /// <summary>
        /// The total number of transitions in the sequence
        /// </summary>
        public int TransitionCount => m_Data.Transitions.Count;

        /// <summary>
        /// The list of all keys in the sequence, in order.
        /// </summary>
        public ReadOnlyCollection<SequenceKey> Keys => m_SequenceKeysReadOnly;

        /// <summary>
        /// The list of all transitions in the sequence, in order.
        /// </summary>
        public ReadOnlyCollection<SequenceTransition> Transitions => m_SequenceTransitionsReadOnly;

        public event Action<TimelineModel, Property> OnTimelineChanged;
        public event Action<TimelineModel, SequenceKey> OnKeyAdded;
        public event Action<TimelineModel, SequenceKey> OnKeyRemoved;
        public event Action<TimelineModel, KeyModel, KeyModel.Property> OnKeyChanged;
        public event Action<TimelineModel, TransitionModel, TransitionModel.Property> OnTransitionChanged;
        public event Action<TimelineModel, SequenceTransition> OnTransitionAdded;
        public event Action<TimelineModel, SequenceTransition> OnTransitionRemoved;

        /// <summary>
        /// Create a new timeline
        /// </summary>
        public TimelineModel()
        {
            m_Data.Keyframes = new List<KeyModel>();
            m_Data.Transitions = new List<TransitionModel>();
            UpdateSequenceData();
            RegisterEvents();
        }

        public TimelineModel(TimelineModel source)
        {
            m_Data.Keyframes = new List<KeyModel>();
            m_Data.Transitions = new List<TransitionModel>();

            m_SequenceKeysReadOnly = new ReadOnlyCollection<SequenceKey>(m_SequenceKeys);                       
            m_SequenceTransitionsReadOnly = new ReadOnlyCollection<SequenceTransition>(m_SequenceTransitions);  
            

            source.CopyTo(this);
        }

        public TimelineModel Clone()
        {
            var clone = new TimelineModel();
            CopyTo(clone);
            return clone;
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            UpdateSequenceData();
            RegisterEvents();
        }

        public void CopyTo(TimelineModel target)
        {
            CopyTo(target, false);
        }

        public void CopyTo(TimelineModel target, bool silent)
        {
            target.Clear();

            foreach (var key in Keys)
            {
                var targetKey = NewKey();
                target.AddKeyCopy(targetKey);
                key.Key.CopyTo(targetKey.Key, silent);
            }

            var index = 0;

            foreach (var transition in Transitions)
            {
                var targetTransition = NewTransition();
                target.AddTransitionCopy(targetTransition);
                transition.Transition.CopyTo(targetTransition.Transition, silent);

                var from = target.GetKey(index);
                var to = target.GetKey(index + 1);

                targetTransition.FromKey = from;
                targetTransition.ToKey = to;

                from.OutTransition = targetTransition;
                to.InTransition = targetTransition;

                index++;
            }

            target.DisableLoopingKeysIfNotLast(silent);
        }

        /// <summary>
        /// Removes all keys and transitions from the sequence (silent, force-full)
        /// </summary>
        public void Clear()
        {
            foreach (var key in Keys)
            {
                UnregisterKey(key.Key);
            }

            foreach (var transition in Transitions)
            {
                UnregisterTransition(transition.Transition);
            }

            m_SequenceKeys.Clear();
            m_SequenceTransitions.Clear();

            m_Data.Keyframes.Clear();
            m_Data.Transitions.Clear();
        }

        /// <summary>
        /// Removes all keys and transitions from the sequence
        /// </summary>
        /// <param name="silent">If change events related to this action are skipped.</param>
        public void RemoveAllKeys(bool silent)
        {
            using var tmpList = TempList<SequenceKey>.Allocate();
            foreach (var sequenceKey in m_SequenceKeys)
            {
                tmpList.Add(sequenceKey);
            }

            foreach (var sequenceKey in tmpList.List)
            {
                RemoveKey(sequenceKey, silent);
            }
        }

        /// <summary>
        /// Inserts a new key, cloned from a provided key, at the given index.
        /// </summary>
        /// <param name="atIndex">The index at which to insert the cloned key.</param>
        /// <param name="sourceKey">The source key to clone and insert.</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        /// <param name="sourceOutTransition">The source key out transition to apply to the newly inserted key.</param>
        /// <param name="splitTransition">If true, existing transition will be split into 2 when inserting a key between two existing keys</param>
        /// <param name="splitTimePercent">
        /// Used to indicate in which proportion transition will be split when splitTransition is set to true.
        /// Transitions are split when inserting a frame between two frames.
        /// The percentage indicates where the splitting should occur, 0 meaning the beginning and 1 the end of the original transition.
        /// 0 and 1 are both invalid values as no splitting can occur in these cases.
        /// </param>
        /// <returns>The inserted key.</returns>
        public SequenceKey InsertKeyCopy(int atIndex, KeyModel sourceKey, bool silent, TransitionModel sourceOutTransition = null, bool splitTransition = false, float splitTimePercent = 1f)
        {
            if (atIndex < 0 || atIndex > KeyCount)
                AssertUtils.Fail($"Invalid key insertion index: {atIndex}");

            var sequenceKey = NewKey();
            InsertKeyAndUpdateTransitions(atIndex, sequenceKey, silent, false, 0);

            // Copy the data from the source to the newly inserted key
            sourceKey.CopyTo(sequenceKey.Key, silent);

            if (sequenceKey.OutTransition != null && !splitTransition && sourceOutTransition != null)
            {
                sourceOutTransition.CopyTo(sequenceKey.OutTransition.Transition, silent);
            }

            UpdateIndices();
            
            if (!silent)
            {
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
                OnKeyAdded?.Invoke(this, sequenceKey);
            }

            return sequenceKey;
        }

        /// <summary>
        /// Inserts a new key at the given index
        /// </summary>
        /// <param name="atIndex">The index at which to insert a new key</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        /// <param name="splitTransition">If true, existing transition will be split into 2 when inserting a key between two existing keys</param>
        /// <param name="splitTimePercent">
        /// Used to indicate in which proportion transition will be split when splitTransition is set to true.
        /// Transitions are split when inserting a frame between two frames.
        /// The percentage indicates where the splitting should occur, 0 meaning the beginning and 1 the end of the original transition.
        /// 0 and 1 are both invalid values as no splitting can occur in these cases.
        /// </param>
        /// <returns>The newly created key</returns>
        public SequenceKey InsertKey(int atIndex, bool silent, bool splitTransition = false, float splitTimePercent = 1f)
        {
            if (atIndex < 0 || atIndex > KeyCount)
                AssertUtils.Fail($"Invalid key insertion index: {atIndex}");

            var sequenceKey = NewKey();
            InsertKeyAndUpdateTransitions(atIndex, sequenceKey, silent, splitTransition, splitTimePercent);
            
            if (!silent)
            {
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
                OnKeyAdded?.Invoke(this, sequenceKey);
            }

            return sequenceKey;
        }

        /// <summary>
        /// Inserts a new key at the given index
        /// </summary>
        /// <param name="fromIndex">The index of the key to duplicate</param>
        /// <param name="atIndex">The index at which to insert the new duplicated key</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        /// <param name="splitTransition">If true, existing transition will be split into 2 when inserting a key between two existing keys</param>
        /// <param name="splitTimePercent">
        /// Used to indicate in which proportion transition will be split when splitTransition is set to true.
        /// Transitions are split when inserting a frame between two frames.
        /// The percentage indicates where the splitting should occur, 0 meaning the beginning and 1 the end of the original transition.
        /// 0 and 1 are both invalid values as no splitting can occur in these cases.
        /// </param>
        /// <returns>The newly created key</returns>
        public SequenceKey DuplicateKey(int fromIndex, int atIndex, bool silent, bool splitTransition = false, float splitTimePercent = 1f)
        {
            if (atIndex < 0 || atIndex > KeyCount)
                AssertUtils.Fail($"Invalid key insertion index: {atIndex}");

            if (fromIndex < 0 || fromIndex >= KeyCount)
                AssertUtils.Fail($"Invalid key duplicate source index: {fromIndex}");

            var keyToDuplicate = GetKey(fromIndex);
            var keyModel = keyToDuplicate.Key.Clone();
            var sequenceKey = new SequenceKey(keyModel);

            InsertKeyAndUpdateTransitions(atIndex, sequenceKey, silent, splitTransition, splitTimePercent);

            if (!silent)
            {
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
                OnKeyAdded?.Invoke(this, sequenceKey);
            }

            return sequenceKey;
        }

        /// <summary>
        /// Inserts a new key at the end of the sequence.
        /// </summary>
        /// <param name="silent">If change events related to this action are skipped.</param>
        /// <returns>The newly created key.</returns>
        public SequenceKey AddKey(bool silent)
        {
            var atIndex = KeyCount;
            return InsertKey(atIndex, silent);
        }

        /// <summary>
        /// Adds a key to the model, used when CopyTo() is used.
        /// </summary>
        /// <param name="key">The key to add.</param>
        public void AddKeyCopy(SequenceKey key)
        {
            m_Data.Keyframes.Add(key.Key);
            m_SequenceKeys.Add(key);
            RegisterKey(key.Key);
        }

        /// <summary>
        /// Adds a transition to the model, used when CopyTo() is used.
        /// </summary>
        /// <param name="transition">The transition to add.</param>
        public void AddTransitionCopy(SequenceTransition transition)
        {
            m_Data.Transitions.Add(transition.Transition);
            m_SequenceTransitions.Add(transition);
            RegisterTransition(transition.Transition);
        }

        /// <summary>
        /// Moves an existing key in the sequence.
        /// This will update existing transitions, adding or removing transitions as needed
        /// </summary>
        /// <param name="oldIndex">The index in the original sequence</param>
        /// <param name="newIndex">The index in the final sequence</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        public void MoveKey(int oldIndex, int newIndex, bool silent)
        {
            if (oldIndex < 0 || oldIndex >= KeyCount)
                AssertUtils.Fail($"Invalid initial key index: {oldIndex}");

            if (newIndex < 0 || newIndex >= KeyCount)
                AssertUtils.Fail($"Invalid destination key index: {newIndex}");

            if (oldIndex == newIndex)
                return;

            var sequenceKey = GetKey(oldIndex);

            RemoveKeyAndUpdateTransitions(sequenceKey, silent);
            InsertKeyAndUpdateTransitions(newIndex, sequenceKey, silent);

            if (!silent)
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
        }

        /// <summary>
        /// Removes the key at the specified index from the sequence
        /// </summary>
        /// <param name="keyIndex">The index of the key to remove</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        public void RemoveKey(int keyIndex, bool silent)
        {
            var sequenceKey = GetKey(keyIndex);
            RemoveKey(sequenceKey, silent);
        }

        /// <summary>
        /// Removes a key from a sequence. The key must exist.
        /// </summary>
        /// <param name="sequenceKey">The key to remove.</param>
        /// <param name="silent">If change events related to this action are skipped.</param>
        public void RemoveKey(SequenceKey sequenceKey, bool silent)
        {
            RemoveKeyAndUpdateTransitions(sequenceKey, silent);
        }

        /// <summary>
        /// Get a sequence key at a specific index
        /// </summary>
        /// <param name="keyIndex">The index of the key. Index must be valid.</param>
        /// <returns>The sequence key</returns>
        public SequenceKey GetKey(int keyIndex)
        {
            if (keyIndex < 0 || keyIndex >= m_SequenceKeys.Count)
                AssertUtils.Fail($"Invalid key index: {keyIndex}");

            var key = m_SequenceKeys[keyIndex];
            return key;
        }

        /// <summary>
        /// Get a transition at a specific index
        /// </summary>
        /// <param name="transitionIndex">The index of the transition. Must be valid.</param>
        /// <returns>The sequence transition</returns>
        public SequenceTransition GetTransition(int transitionIndex)
        {
            if (transitionIndex < 0 || transitionIndex >= m_SequenceTransitions.Count)
                AssertUtils.Fail($"Invalid transition index: {transitionIndex}");

            var transition = m_SequenceTransitions[transitionIndex];
            return transition;
        }

        /// <summary>
        /// Add an entity to the timeline keys
        /// </summary>
        /// <param name="entityID">The entity ID to add</param>
        /// <param name="posing">The initial posing state of the key, which will be copied to the key.</param>
        /// <param name="numJoints">The number of joints of the entity.</param>
        public void AddEntity(EntityID entityID, PosingModel posing, int numJoints)
        {
            foreach (var key in m_Data.Keyframes)
            {
                if (key.HasEntity(entityID))
                    continue;

                key.AddEntity(entityID, posing, numJoints);
            }
        }

        /// <summary>
        /// Removes an entity from the entire timeline keys
        /// </summary>
        /// <param name="entityID">The entity ID to remove</param>
        public void RemoveEntity(EntityID entityID)
        {
            foreach (var key in m_Data.Keyframes)
            {
                if (!key.HasEntity(entityID))
                    continue;

                key.RemoveEntity(entityID);
            }
        }

        /// <summary>
        /// Retrieves all entity IDs that have at least one key
        /// </summary>
        /// <param name="entitySet">A set to store the result</param>
        public void GetAllEntities(HashSet<EntityID> entitySet)
        {
            foreach (var key in m_Data.Keyframes)
            {
                key.GetAllEntityIDs(entitySet);
            }
        }

        /// <summary>
        /// Retrieves the first key
        /// </summary>
        /// <returns>The first key in the sequence, null if no keys exist</returns>
        public SequenceKey GetFirstKey()
        {
            return KeyCount > 0 ? GetKey(0) : null;
        }

        /// <summary>
        /// Retrieves the first key that has the given entity
        /// </summary>
        /// <param name="entityID">The entity ID to look for</param>
        /// <returns>The first key that contains the entity, null if none</returns>
        public SequenceKey GetFirstKey(EntityID entityID)
        {
            if (KeyCount == 0)
                return null;

            var firstKey = GetKey(0);
            return firstKey.GetNextKey(entityID);
        }

        /// <summary>
        /// Retrieves the last key
        /// </summary>
        /// <returns>The last key in the sequence, null if no keys exist</returns>
        public SequenceKey GetLastKey()
        {
            return KeyCount > 0 ? GetKey(KeyCount - 1) : null;
        }

        /// <summary>
        /// Returns the index of a given key
        /// </summary>
        /// <param name="sequenceKey">The sequence key for which to retrieve the index</param>
        /// <returns>The index of the key if found, an invalid index otherwise</returns>
        public int IndexOf(SequenceKey sequenceKey)
        {
            return m_SequenceKeys.IndexOf(sequenceKey);
        }

        /// <summary>
        /// Returns the index of a given transition
        /// </summary>
        /// <param name="sequenceTransition">The sequence transition for which to retrieve the index</param>
        /// <returns>The index of the transition if found, an invalid index otherwise</returns>
        public int IndexOf(SequenceTransition sequenceTransition)
        {
            return m_SequenceTransitions.IndexOf(sequenceTransition);
        }

        void InsertKeyAndUpdateTransitions(int atIndex, SequenceKey sequenceKey, bool silent, bool splitTransition = false, float splitTimePercent = 1f)
        {
            Assert.IsFalse(m_SequenceKeys.Contains(sequenceKey), "Key already in sequence");

            // Inserting at the start
            if (atIndex == 0)
            {
                if (KeyCount > 0)
                {
                    var nextSequenceKey = GetKey(0);

                    var transitionModel = new TransitionModel();
                    var sequenceTransition = new SequenceTransition(transitionModel);

                    sequenceKey.OutTransition = sequenceTransition;
                    nextSequenceKey.InTransition = sequenceTransition;

                    sequenceTransition.FromKey = sequenceKey;
                    sequenceTransition.ToKey = nextSequenceKey;

                    InsertTransitionInternal(0, sequenceTransition, silent);
                }
            }

            // Inserting at the end
            else if (atIndex == KeyCount)
            {
                var prevSequenceKey = GetKey(atIndex - 1);

                var transitionModel = new TransitionModel();
                var sequenceTransition = new SequenceTransition(transitionModel);

                prevSequenceKey.OutTransition = sequenceTransition;
                sequenceKey.InTransition = sequenceTransition;

                sequenceTransition.FromKey = prevSequenceKey;
                sequenceTransition.ToKey = sequenceKey;

                InsertTransitionInternal(atIndex - 1, sequenceTransition, silent);
            }

            // Inserting at the middle
            else
            {
                var prevSequenceKey = GetKey(atIndex - 1);
                var nextSequenceKey = GetKey(atIndex);

                var transitionToNextModel = new TransitionModel();
                var sequenceTransitionToNext = new SequenceTransition(transitionToNextModel);

                var sequenceTransitionFromPrev = prevSequenceKey.OutTransition;
                var transitionFromPrevModel = sequenceTransitionFromPrev.Transition;

                // Split transition if required
                if (splitTransition)
                {
                    transitionFromPrevModel.Split(transitionToNextModel, splitTimePercent);
                }

                sequenceTransitionFromPrev.ToKey = sequenceKey;
                sequenceTransitionToNext.FromKey = sequenceKey;
                sequenceTransitionToNext.ToKey = nextSequenceKey;

                sequenceKey.InTransition = sequenceTransitionFromPrev;
                sequenceKey.OutTransition = sequenceTransitionToNext;
                nextSequenceKey.InTransition = sequenceTransitionToNext;

                InsertTransitionInternal(atIndex, sequenceTransitionToNext, silent);
            }

            InsertKeyInternal(atIndex, sequenceKey, silent);
        }

        void RemoveKeyAndUpdateTransitions(SequenceKey sequenceKey, bool silent)
        {
            Assert.IsTrue(m_SequenceKeys.Contains(sequenceKey), "Key not found");

            // First key
            if (sequenceKey.PreviousKey == null)
            {
                // Should not have any transition before
                Assert.IsNull(sequenceKey.InTransition);

                // If there is more than one key there will be an out transition
                if (sequenceKey.OutTransition != null)
                {
                    // Get next key
                    var nextSequenceKey = sequenceKey.NextKey;

                    Assert.IsNotNull(nextSequenceKey);
                    Assert.AreEqual(sequenceKey.OutTransition, nextSequenceKey.InTransition);

                    // Update next key to remove in transition
                    nextSequenceKey.InTransition = null;

                    // Remove transition
                    RemoveTransitionInternal(sequenceKey.OutTransition, silent);
                }
            }

            // Last key
            else if (sequenceKey.NextKey == null)
            {
                Assert.IsNull(sequenceKey.OutTransition);
                Assert.IsNotNull(sequenceKey.InTransition); // if we are here there are at least 2 frames

                // Get prev key
                var prevSequenceKey = sequenceKey.PreviousKey;

                Assert.IsNotNull(prevSequenceKey);
                Assert.AreEqual(sequenceKey.InTransition, prevSequenceKey.OutTransition);

                // Update prev key to remove out transition
                prevSequenceKey.OutTransition = null;

                // Remove transition
                RemoveTransitionInternal(sequenceKey.InTransition, silent);
            }

            // General case
            else
            {
                Assert.IsNotNull(sequenceKey.InTransition);
                Assert.IsNotNull(sequenceKey.OutTransition);

                // We fuse both transition together
                // TODO: Disabled transition duration fusing temporarily until the solver handles long transitions better
                // sequenceKey.InTransition.Transition.Fuse(sequenceKey.OutTransition.Transition);

                // Get next key
                var nextSequenceKey = sequenceKey.NextKey;
                Assert.IsNotNull(nextSequenceKey);

                // Update next key to use fused transition
                sequenceKey.InTransition.ToKey = nextSequenceKey;
                nextSequenceKey.InTransition = sequenceKey.InTransition;

                // Remove transition
                RemoveTransitionInternal(sequenceKey.OutTransition, silent);
            }

            RemoveKeyInternal(sequenceKey, silent);
        }

        void InsertKeyInternal(int atIndex, SequenceKey sequenceKey, bool silent)
        {
            m_Data.Keyframes.Insert(atIndex, sequenceKey.Key);
            m_SequenceKeys.Insert(atIndex, sequenceKey);

            RegisterKey(sequenceKey.Key);
            
            if(!silent)
                OnKeyAdded?.Invoke(this, sequenceKey);
            
            DisableLoopingKeysIfNotLast(silent);
        }

        internal void UpdateIndices(int from = 0)
        {
            for (var i = Mathf.Max(0, from); i < m_Data.Keyframes.Count; i++)
            {
                m_Data.Keyframes[i].ListIndex = i;
            }

            for (var i = Mathf.Max(0, from); i < m_Data.Transitions.Count; i++)
            {
                m_Data.Transitions[i].ListIndex = i;
            }
        }

        void InsertTransitionInternal(int atIndex, SequenceTransition sequenceTransition, bool silent)
        {
            m_Data.Transitions.Insert(atIndex, sequenceTransition.Transition);
            m_SequenceTransitions.Insert(atIndex, sequenceTransition);
            RegisterTransition(sequenceTransition.Transition);

            OnTransitionAdded?.Invoke(this, sequenceTransition);

            DisableLoopingKeysIfNotLast(silent);
        }

        void RemoveKeyInternal(SequenceKey key, bool silent)
        {
            // Invalid key context
            key.InTransition = null;
            key.OutTransition = null;

            m_SequenceKeys.Remove(key);
            m_Data.Keyframes.Remove(key.Key);
            
            UnregisterKey(key.Key);

            if (!silent)
            {
                OnKeyRemoved?.Invoke(this, key);
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
            }

            
            
            DisableLoopingKeysIfNotLast(silent);
        }

        void RemoveTransitionInternal(SequenceTransition transition, bool silent)
        {
            // Invalid transition context
            transition.FromKey = null;
            transition.ToKey = null;

            UnregisterTransition(transition.Transition);
            m_SequenceTransitions.Remove(transition);
            m_Data.Transitions.Remove(transition.Transition);
            
            if(!silent)
                OnTransitionRemoved?.Invoke(this, transition);

            DisableLoopingKeysIfNotLast(silent);
        }

        void UpdateSequenceData()
        {
            m_SequenceKeys?.Clear();
            m_SequenceTransitions?.Clear();
            
            m_SequenceKeys ??= new List<SequenceKey>();
            m_SequenceTransitions ??= new List<SequenceTransition>();

            // First create all keys
            foreach (var t in m_Data.Keyframes)
            {
                var key = new SequenceKey(t);
                m_SequenceKeys.Add(key);
            }

            // And all transitions
            foreach (var t in m_Data.Transitions)
            {
                var transition = new SequenceTransition(t);
                m_SequenceTransitions.Add(transition);
            }

            // Then linked them together
            for (var i = 0; i < m_SequenceKeys.Count; i++)
            {
                var key = m_SequenceKeys[i];
                key.InTransition = i > 0 ? m_SequenceTransitions[i - 1] : null;
                key.OutTransition = i < m_Data.Transitions.Count ? m_SequenceTransitions[i] : null;
            }

            for (var i = 0; i < m_SequenceTransitions.Count; i++)
            {
                var transition = m_SequenceTransitions[i];
                transition.FromKey = m_SequenceKeys[i];
                transition.ToKey = m_SequenceKeys[i + 1];
            }
            
            m_SequenceKeysReadOnly = new ReadOnlyCollection<SequenceKey>(m_SequenceKeys);
            m_SequenceTransitionsReadOnly = new ReadOnlyCollection<SequenceTransition>(m_SequenceTransitions);
        }

        void RegisterEvents()
        {
            foreach (var key in m_Data.Keyframes)
            {
                RegisterKey(key);
            }

            foreach (var transition in m_Data.Transitions)
            {
                RegisterTransition(transition);
            }
        }

        void RegisterKey(KeyModel keyModel)
        {
            keyModel.OnChanged += OnKeyModelChanged;
            UpdateIndices();
        }

        void UnregisterKey(KeyModel keyModel)
        {
            keyModel.OnChanged -= OnKeyModelChanged;
            UpdateIndices();
        }

        void RegisterTransition(TransitionModel transitionModel)
        {
            transitionModel.OnChanged += OnTransitionModelChanged;
            UpdateIndices();
        }

        void UnregisterTransition(TransitionModel transitionModel)
        {
            transitionModel.OnChanged -= OnTransitionModelChanged;
            UpdateIndices();
        }

        void OnTransitionModelChanged(TransitionModel model, TransitionModel.Property property)
        {
            OnTransitionChanged?.Invoke(this, model, property);
            OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
        }

        void OnKeyModelChanged(KeyModel model, KeyModel.Property property)
        {
            OnKeyChanged?.Invoke(this, model, property);
            if (property is not KeyModel.Property.Thumbnail)
            {
                OnTimelineChanged?.Invoke(this, Property.InputAnimationData);
            }
        }

        void DisableLoopingKeysIfNotLast(bool silent)
        {
            for (var i = 0; i < KeyCount - 1; i++)
            {
                var key = GetKey(i);
                if (key.Key.Type == KeyData.KeyType.Loop)
                    key.Key.SetType(KeyData.KeyType.FullPose, silent);
            }
        }

        public Bounds GetWorldBounds()
        {
            var bounds = new Bounds();
            var first = true;

            foreach (var key in m_Data.Keyframes)
            {
                if (key.Type is KeyData.KeyType.FullPose)
                {
                    if (first)
                    {
                        bounds = key.GetWorldBounds();
                        first = false;
                    }
                    else
                    {
                        bounds.Encapsulate(key.GetWorldBounds());
                    }
                }
            }

            return bounds;
        }
    }
}
