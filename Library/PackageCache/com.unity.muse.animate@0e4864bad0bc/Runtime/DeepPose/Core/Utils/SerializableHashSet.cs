using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<T> m_SerializedData = new();

        public SerializableHashSet()
            : base() {}

        public SerializableHashSet(IEnumerable<T> collection)
            : base(collection) { }

        public SerializableHashSet(int capacity)
            : base(capacity) { }

        public void OnBeforeSerialize()
        {
            m_SerializedData.Clear();
            foreach (var pair in this)
            {
                m_SerializedData.AddRange(this);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var value in m_SerializedData)
            {
                Add(value);
            }
        }
    }
}
