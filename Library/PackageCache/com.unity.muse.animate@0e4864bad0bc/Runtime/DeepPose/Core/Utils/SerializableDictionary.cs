using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        struct Pair
        {
            public TKey Key;
            public TValue Value;
            public Pair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
        
        [SerializeField]
        List<Pair> m_SerializedData = new();
        
        public SerializableDictionary()
        {
        }
        
        public SerializableDictionary(IDictionary<TKey, TValue> input) : base(input)
        {
        }
        
        public void OnBeforeSerialize()
        {
            m_SerializedData.Clear();
            foreach (var pair in this)
            {
                m_SerializedData.Add(new Pair(pair.Key, pair.Value));
            }
        }
        
        public void OnAfterDeserialize()
        {
            Clear();
            for (var i = 0; i < m_SerializedData.Count; i++)
            {
                var pair = m_SerializedData[i];
                Add(pair.Key, pair.Value);
            }
        }
    }
}
