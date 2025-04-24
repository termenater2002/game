using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    struct TempList<T> : IDisposable, IEnumerable<T>
    {
        const int k_DefaultCapacity = 100;

        public static readonly TempList<T> Invalid = new TempList<T>();

        static readonly Queue<List<T>> k_ReadyQueue = new Queue<List<T>>(2);

        List<T> m_Inner;

        public TempList(List<T> inner)
        {
            m_Inner = inner;
        }

        public List<T> List => m_Inner;

        public int Count => m_Inner.Count;

        public static TempList<T> Allocate()
        {
            return k_ReadyQueue.Count == 0
                ? new TempList<T>(new List<T>(k_DefaultCapacity))
                : new TempList<T>(k_ReadyQueue.Dequeue());
        }

        public static TempList<T> Allocate(IEnumerable<T> entries)
        {
            if (k_ReadyQueue.Count == 0)
            {
                return new TempList<T>(new List<T>(entries));
            }

            List<T> inner = k_ReadyQueue.Dequeue();
            inner.Clear();

            if (entries != null)
            {
                foreach (T entry in entries)
                {
                    inner.Add(entry);
                }
            }

            return new TempList<T>(inner);
        }

        public void Dispose()
        {
            m_Inner.Clear();
            k_ReadyQueue.Enqueue(List);
            m_Inner = null;
        }

        public void Add(T entry)
        {
            m_Inner.Add(entry);
        }

        public void Add(List<T> entries)
        {
            foreach (var entry in entries)
            {
                m_Inner.Add(entry);
            }
        }

        public bool Remove(T entry)
        {
            return m_Inner.Remove(entry);
        }

        public bool Contains(T entry)
        {
            return m_Inner.Contains(entry);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_Inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
