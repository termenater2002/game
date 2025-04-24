using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    struct TempDictionary<TK, TV> : IDisposable, IEnumerable<TK>, IEnumerable<KeyValuePair<TK, TV>>
    {
        const int k_DefaultCapacity = 100;

        public static readonly TempDictionary<TK, TV> Invalid = new TempDictionary<TK, TV>();

        static readonly Queue<Dictionary<TK, TV>> k_ReadyQueue = new Queue<Dictionary<TK, TV>>(2);

        Dictionary<TK, TV> m_Inner;

        public TempDictionary(Dictionary<TK, TV> inner)
        {
            m_Inner = inner;
        }

        public Dictionary<TK, TV> Dictionary => m_Inner;

        public int Count => m_Inner.Count;

        public static TempDictionary<TK, TV> Allocate()
        {
            return k_ReadyQueue.Count == 0
                ? new TempDictionary<TK, TV>(new Dictionary<TK, TV>(k_DefaultCapacity))
                : new TempDictionary<TK, TV>(k_ReadyQueue.Dequeue());
        }

        public void Dispose()
        {
            m_Inner.Clear();
            k_ReadyQueue.Enqueue(Dictionary);
            m_Inner = null;
        }

        public void Add(TK key, TV value)
        {
            m_Inner.Add(key, value);
        }

        public bool Remove(TK key)
        {
            return m_Inner.Remove(key);
        }

        public bool ContainsKey(TK key)
        {
            return m_Inner.ContainsKey(key);
        }
        
        public bool TryGetValue(TK key, out TV value)
        {
            return m_Inner.TryGetValue(key, out value);
        }
        
        public TV this[TK key]
        {
            get => m_Inner[key];
            set => m_Inner[key] = value;
        }
        
        /// <summary>
        /// Get non-allocating enumerator (used by the compiler in foreach loops).
        /// </summary>
        /// <returns></returns>
        public Dictionary<TK, TV>.Enumerator GetEnumerator()
        {
            return m_Inner.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator()
        {
            return m_Inner.GetEnumerator();
        }

        IEnumerator<TK> IEnumerable<TK>.GetEnumerator()
        {
            return m_Inner.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
