using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Common.Utils
{
    /// <summary>
    /// Serializable Hash Set.
    /// </summary>
    /// <typeparam name="T">The value</typeparam>
    [Serializable]
    internal class SerializedHashSet<T> : ISerializationCallbackReceiver
    {
        [SerializeField]
        T[] m_Values = Array.Empty<T>();

        HashSet<T> m_HashSet = new HashSet<T>();

        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_Values = new T[m_HashSet.Count];

            var i = 0;
            foreach (var val in m_HashSet)
                m_Values[i++] = val;
        }

        /// <summary>
        /// OnAfterDeserialize implementation.
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_HashSet = new HashSet<T>(m_Values);
        }

        /// <summary>
        /// Adds an unique item.
        /// </summary>
        /// <param name="val">Item to be added.</param>
        public void Add(T val) => m_HashSet.Add(val);

        /// <summary>
        /// Remove an item.
        /// </summary>
        /// <param name="val">Item to be removed.</param>
        public void Remove(T val) => m_HashSet.Remove(val);

        /// <summary>
        /// Determines whether the set contains a given item.
        /// </summary>
        /// <param name="val">Item to be checked.</param>
        /// <returns>True if the item exits.</returns>
        public bool Contains(T val) => m_HashSet.Contains(val);
    }
}
