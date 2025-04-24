using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    class BakedTimelineMappingModel: ICopyable<BakedTimelineMappingModel>, ISerializationCallbackReceiver
    {
        public delegate void Changed(BakedTimelineMappingModel model);
        public event Changed OnChanged;

        public delegate void AddedKey(BakedTimelineMappingModel model, BakedTimelineMappingData.MappingEntry key);
        public event AddedKey OnKeyAdded;

        public delegate void AddedTransition(BakedTimelineMappingModel model, BakedTimelineMappingData.MappingEntry transition);
        public event AddedKey OnTransitionAdded;

        [SerializeField]
        BakedTimelineMappingData m_Data;

        public BakedTimelineMappingModel()
        {
            m_Data.Keys = new List<BakedTimelineMappingData.MappingEntry>();
            m_Data.Transitions = new List<BakedTimelineMappingData.MappingEntry>();
        }
        
        public BakedTimelineMappingModel(BakedTimelineMappingModel source)
        {
            source.CopyTo(this);
        }

        public void CopyTo(BakedTimelineMappingModel target)
        {
            CopyTo(target, false);
        }

        public void CopyTo(BakedTimelineMappingModel target, bool silent)
        {
            target.m_Data.Keys = new List<BakedTimelineMappingData.MappingEntry>();
            target.m_Data.Transitions = new List<BakedTimelineMappingData.MappingEntry>();
            
            foreach (var entry in m_Data.Keys)
            {
                target.m_Data.Keys.Add(entry);
            }
            
            foreach (var entry in m_Data.Transitions)
            {
                target.m_Data.Transitions.Add(entry);
            }
        }

        public BakedTimelineMappingModel Clone()
        {
            return new BakedTimelineMappingModel(this);
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            DevLogger.LogInfo($"BakedTimelineMappingModel -> OnAfterDeserialize()");
        }
        
        public void Clear()
        {
            m_Data.Keys.Clear();
            m_Data.Transitions.Clear();
            OnChanged?.Invoke(this);
        }

        public void AddKey(int timelineIndex, int bakedFrameIndex)
        {
            var entry = new BakedTimelineMappingData.MappingEntry
            {
                TimelineIndex = timelineIndex,
                StartBakedFrameIndex = bakedFrameIndex,
                EndBakedFrameIndex = bakedFrameIndex
            };

            m_Data.Keys.Add(entry);

            OnKeyAdded?.Invoke(this, entry);
            OnChanged?.Invoke(this);
        }

        public void AddTransition(int timelineIndex, int startBakedFrameIndex, int endBakedFrameIndex)
        {
            var entry = new BakedTimelineMappingData.MappingEntry
            {
                TimelineIndex = timelineIndex,
                StartBakedFrameIndex = startBakedFrameIndex,
                EndBakedFrameIndex = endBakedFrameIndex
            };

            m_Data.Transitions.Add(entry);

            OnTransitionAdded?.Invoke(this, entry);
            OnChanged?.Invoke(this);
        }

        public bool IsAtKey(int bakedFrameIndex)
        {
            return TryGetKeyIndex(bakedFrameIndex, out _);
        }

        public bool IsInsideTransition(int bakedFrameIndex)
        {
            return TryGetTransitionIndex(bakedFrameIndex, out _);
        }

        public void GetBakedKeyProgressAt(float bakedFrame, out int keyIndex, out float transitionProgress)
        {
            var bakedFrameIndex = Mathf.FloorToInt(bakedFrame);

            // Directly at key
            if( TryGetKeyIndex(bakedFrameIndex, out keyIndex ))
            {
                transitionProgress = 0;
            }
            else if( TryGetTransitionIndex(bakedFrameIndex, out keyIndex ))
            {
                if (TryGetBakedTransitionSegment(keyIndex, out var transitionStartBakedFrameIndex, out var transitionEndBakedFrameIndex))
                {
                    var keyBakedFrameIndex = transitionStartBakedFrameIndex - 1;
                    var segmentLength = transitionEndBakedFrameIndex - keyBakedFrameIndex;
                    var segmentProgressIndex = bakedFrameIndex - keyBakedFrameIndex;

                    transitionProgress = segmentProgressIndex / (float)segmentLength;
                }
                else
                {
                    transitionProgress = 0;
                }
            }
            else if( TryGetKeyIndex(bakedFrameIndex, out keyIndex ))
            {
                transitionProgress = 0;
            }
            else
            {
                transitionProgress = 0;
                keyIndex = -1;
            }
        }

        public bool TryGetBakedKeyIndex(int keyIndex, out int bakedFrameIndex)
        {
            if (m_Data.Keys.TryGetEntryAtTimelineIndex(keyIndex, out var entry))
            {
                if (entry.StartBakedFrameIndex != entry.EndBakedFrameIndex)
                    throw new Exception("Key should be mapped to a single frame");

                bakedFrameIndex = entry.StartBakedFrameIndex;
                return true;
            }

            bakedFrameIndex = -1;
            return false;
        }

        public bool TryGetBakedTransitionSegment(int transitionIndex, out int startBakedFrameIndex, out int endBakedFrameIndex)
        {
            if (m_Data.Transitions.TryGetEntryAtTimelineIndex(transitionIndex, out var entry))
            {
                startBakedFrameIndex = entry.StartBakedFrameIndex;
                endBakedFrameIndex = entry.EndBakedFrameIndex;
                return true;
            }

            startBakedFrameIndex = -1;
            endBakedFrameIndex = -1;
            return false;
        }

        public bool TryGetKeyIndex(int bakedFrameIndex, out int keyIndex)
        {
            if (m_Data.Keys.TryGetEntryAtBakedIndex(bakedFrameIndex, out var entry))
            {
                keyIndex = entry.TimelineIndex;
                return true;
            }

            keyIndex = -1;
            return false;
        }

        public bool TryGetKey(TimelineModel timeline, int bakedFrameIndex, out TimelineModel.SequenceKey sequenceKey)
        {
            if (!TryGetKeyIndex(bakedFrameIndex, out var keyIndex))
            {
                sequenceKey = null;
                return false;
            }

            sequenceKey = timeline.GetKey(keyIndex);
            return true;
        }

        public bool TryGetTransitionIndex(int bakedFrameIndex, out int transitionIndex)
        {
            if (m_Data.Transitions.TryGetEntryAtBakedIndex(bakedFrameIndex, out var entry))
            {
                transitionIndex = entry.TimelineIndex;
                return true;
            }

            transitionIndex = -1;
            return false;
        }

        public bool TryGetTransition(TimelineModel timeline, int bakedFrameIndex, out TimelineModel.SequenceTransition sequenceTransition)
        {
            if (!TryGetTransitionIndex(bakedFrameIndex, out var transitionIndex))
            {
                sequenceTransition = null;
                return false;
            }

            sequenceTransition = timeline.GetTransition(transitionIndex);
            return true;
        }

        /// <summary>
        /// Get the first frame index that has a key before the given frame index, including the given index
        /// </summary>
        /// <param name="bakedFrameIndex">The frame index at which to start the search</param>
        /// <param name="keyBakedFrameIndex">The frame index of the key found, which could be equal to the start search index</param>
        /// <param name="keyTimelineIndex">The key index of the found key in the timeline</param>
        /// <param name="exclusive">If false, can return the key exactly at the given index</param>
        /// <returns>true if a key was found, false otherwise</returns>
        public bool TryGetFirstKeyBefore(int bakedFrameIndex, out int keyBakedFrameIndex, out int keyTimelineIndex, bool exclusive = false)
        {
            var foundIdx = -1;
            var lastIndex = int.MinValue;

            if (exclusive)
                bakedFrameIndex--;

            for (var i = 0; i < m_Data.Keys.Count; i++)
            {
                var entry = m_Data.Keys[i];
                if (entry.StartBakedFrameIndex > bakedFrameIndex)
                    continue;

                if (entry.EndBakedFrameIndex < lastIndex)
                    continue;

                foundIdx = i;
                lastIndex = entry.EndBakedFrameIndex;
            }

            if (foundIdx < 0)
            {
                keyBakedFrameIndex = -1;
                keyTimelineIndex = -1;
                return false;
            }

            var foundEntry = m_Data.Keys[foundIdx];
            keyBakedFrameIndex = foundEntry.EndBakedFrameIndex;
            keyTimelineIndex = foundEntry.TimelineIndex;
            return true;
        }

        /// <summary>
        /// Get the first frame index that has a key after the given frame index
        /// </summary>
        /// <param name="bakedFrameIndex">The frame index at which to start the search</param>
        /// <param name="keyBakedFrameIndex">The frame index of the key found, which could be equal to the start search index</param>
        /// <param name="keyTimelineIndex">The key index of the found key in the timeline</param>
        /// <param name="exclusive">If false, can return the key exactly at the given index</param>
        /// <returns>true if a key was found, false otherwise</returns>
        public bool TryGetFirstKeyAfter(int bakedFrameIndex, out int keyBakedFrameIndex, out int keyTimelineIndex, bool exclusive = false)
        {
            var foundIdx = -1;
            var lastIndex = int.MaxValue;

            if (exclusive)
                bakedFrameIndex++;

            for (var i = 0; i < m_Data.Keys.Count; i++)
            {
                var entry = m_Data.Keys[i];
                if (entry.EndBakedFrameIndex < bakedFrameIndex)
                    continue;

                if (entry.StartBakedFrameIndex > lastIndex)
                    continue;

                foundIdx = i;
                lastIndex = entry.StartBakedFrameIndex;
            }

            if (foundIdx < 0)
            {
                keyBakedFrameIndex = -1;
                keyTimelineIndex = -1;
                return false;
            }

            var foundEntry = m_Data.Keys[foundIdx];
            keyBakedFrameIndex = foundEntry.StartBakedFrameIndex;
            keyTimelineIndex = foundEntry.TimelineIndex;
            return true;
        }
    }
}
