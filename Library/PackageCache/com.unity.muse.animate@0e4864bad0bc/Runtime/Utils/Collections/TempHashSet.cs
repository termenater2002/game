using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    struct TempHashSet<T> : IDisposable, IEnumerable<T>
    {
        public static readonly TempHashSet<T> Invalid = new TempHashSet<T>();

        static readonly Queue<HashSet<T>> k_ReadyQueue = new Queue<HashSet<T>>(2);

        HashSet<T> m_Inner;

        public TempHashSet(HashSet<T> inner)
        {
            m_Inner = inner;
        }

        public HashSet<T> Set => m_Inner;

        public int Count => m_Inner.Count;

        public static TempHashSet<T> Allocate()
        {
            return k_ReadyQueue.Count == 0
                ? new TempHashSet<T>(new HashSet<T>())
                : new TempHashSet<T>(k_ReadyQueue.Dequeue());
        }

        public static TempHashSet<T> Allocate(IEnumerable<T> entries)
        {
            if (k_ReadyQueue.Count == 0)
            {
                return new TempHashSet<T>(new HashSet<T>(entries));
            }

            HashSet<T> inner = k_ReadyQueue.Dequeue();
            inner.Clear();

            if (entries != null)
            {
                foreach (T entry in entries)
                {
                    inner.Add(entry);
                }
            }

            return new TempHashSet<T>(inner);
        }

        public void Dispose()
        {
            m_Inner.Clear();
            k_ReadyQueue.Enqueue(Set);
            m_Inner = null;
        }

        public bool Contains(T entry)
        {
            return m_Inner.Contains(entry);
        }

        public void Add(T entry)
        {
            m_Inner.Add(entry);
        }

        public bool Remove(T entry)
        {
            return m_Inner.Remove(entry);
        }

        public void UnionWith(IEnumerable<T> set)
        {
            m_Inner.UnionWith(set);
        }
        
        /// <summary>
        /// Get non-allocating enumerator (used by the compiler in foreach loops).
        /// </summary>
        public HashSet<T>.Enumerator GetEnumerator()
        {
            return m_Inner.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
