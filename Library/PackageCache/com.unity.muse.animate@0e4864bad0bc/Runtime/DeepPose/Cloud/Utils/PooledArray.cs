using System;
using System.Collections.Generic;

namespace Unity.DeepPose.Cloud
{
    struct PooledArray<T> : IDisposable
    {
        static Dictionary<int, List<T[]>> s_Pool = new();

        public T[] Array { get; private set; }

        public PooledArray(int length)
        {
            Array = GetOrCreateArray(length);
        }

        public void Dispose()
        {
            if (Array == null)
                return;

            ReturnArray(Array);
            Array = null;
        }

        static T[] GetOrCreateArray(int length)
        {
            if (!s_Pool.TryGetValue(length, out var arrayList))
            {
                arrayList = new List<T[]>();
                s_Pool[length] = arrayList;
            }

            if (arrayList.Count > 0)
            {
                var array =  arrayList[0];
                arrayList.RemoveAt(0);
                return array;
            }

            var newArray = new T[length];
            return newArray;
        }

        static void ReturnArray(T[] array)
        {
            s_Pool[array.Length].Add(array);
        }
    }
}
