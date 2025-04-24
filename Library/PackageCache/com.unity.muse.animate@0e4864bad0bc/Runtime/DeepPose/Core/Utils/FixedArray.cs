using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// This struct defines a list of things that can be animated (as opposed to System.List and C# arrays)
    /// It is implemented with a hard limit of 32 Effector elements.
    /// </summary>
    [Serializable]
    struct FixedArray32<T> : IList<T>, IList
        where T : new()
    {
        /// <summary>Maximum number of elements in the array.</summary>
        public static readonly int k_MaxLength = 32;

        [SerializeField, NotKeyable]
        private int m_Length;

        [SerializeField]
        private T m_Item0;
        [SerializeField]
        private T m_Item1;
        [SerializeField]
        private T m_Item2;
        [SerializeField]
        private T m_Item3;
        [SerializeField]
        private T m_Item4;
        [SerializeField]
        private T m_Item5;
        [SerializeField]
        private T m_Item6;
        [SerializeField]
        private T m_Item7;
        [SerializeField]
        private T m_Item8;
        [SerializeField]
        private T m_Item9;
        [SerializeField]
        private T m_Item10;
        [SerializeField]
        private T m_Item11;
        [SerializeField]
        private T m_Item12;
        [SerializeField]
        private T m_Item13;
        [SerializeField]
        private T m_Item14;
        [SerializeField]
        private T m_Item15;
        [SerializeField]
        private T m_Item16;
        [SerializeField]
        private T m_Item17;
        [SerializeField]
        private T m_Item18;
        [SerializeField]
        private T m_Item19;
        [SerializeField]
        private T m_Item20;
        [SerializeField]
        private T m_Item21;
        [SerializeField]
        private T m_Item22;
        [SerializeField]
        private T m_Item23;
        [SerializeField]
        private T m_Item24;
        [SerializeField]
        private T m_Item25;
        [SerializeField]
        private T m_Item26;
        [SerializeField]
        private T m_Item27;
        [SerializeField]
        private T m_Item28;
        [SerializeField]
        private T m_Item29;
        [SerializeField]
        private T m_Item30;
        [SerializeField]
        private T m_Item31;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="size">Size of the array. This will clamped to be a number in between 0 and k_MaxLength.</param>
        /// <seealso cref="FixedArray32.k_MaxLength"/>
        public FixedArray32(int size)
        {
            m_Length = ClampSize(size);
            m_Item0 = new T();
            m_Item1 = new T();
            m_Item2 = new T();
            m_Item3 = new T();
            m_Item4 = new T();
            m_Item5 = new T();
            m_Item6 = new T();
            m_Item7 = new T();
            m_Item8 = new T();
            m_Item9 = new T();
            m_Item10 = new T();
            m_Item11 = new T();
            m_Item12 = new T();
            m_Item13 = new T();
            m_Item14 = new T();
            m_Item15 = new T();
            m_Item16 = new T();
            m_Item17 = new T();
            m_Item18 = new T();
            m_Item19 = new T();
            m_Item20 = new T();
            m_Item21 = new T();
            m_Item22 = new T();
            m_Item23 = new T();
            m_Item24 = new T();
            m_Item25 = new T();
            m_Item26 = new T();
            m_Item27 = new T();
            m_Item28 = new T();
            m_Item29 = new T();
            m_Item30 = new T();
            m_Item31 = new T();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        /// <summary>
        /// Adds an item to the array.
        /// </summary>
        /// <param name="value">The object to add to the array.</param>
        /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
        int IList.Add(object value)
        {
            Add((T)value);
            return m_Length - 1;
        }

        /// <summary>
        /// Adds an item to the array.
        /// </summary>
        /// <param name="value">The value to add to the array.</param>
        public void Add(T value)
        {
            if (m_Length >= k_MaxLength)
                throw new ArgumentException($"This array cannot have more than '{k_MaxLength}' items.");

            Set(m_Length, value);

            ++m_Length;
        }

        /// <summary>
        /// Removes all items from the array.
        /// </summary>
        public void Clear()
        {
            m_Length = 0;
        }

        /// <summary>
        /// Determines the index of a specific item in the array.
        /// </summary>
        /// <param name="value">The object to locate in the array.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value) => IndexOf((T)value);

        /// <summary>
        /// Determines the index of a specific item in the array.
        /// </summary>
        /// <param name="value">The item to locate in the array.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(T value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Determines whether the array contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the array.</param>
        /// <returns>true if the Object is found in the array; otherwise, false.</returns>
        bool IList.Contains(object value) => Contains((T)value);

        /// <summary>
        /// Determines whether the array contains a specific value.
        /// </summary>
        /// <param name="value">The item to locate in the array.</param>
        /// <returns>true if the Object is found in the array; otherwise, false.</returns>
        public bool Contains(T value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the elements of the array to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from the array. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < m_Length; i++)
            {
                array.SetValue(Get(i), i + arrayIndex);
            }
        }

        /// <summary>
        /// Copies the elements of the array to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from the array. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < m_Length; i++)
            {
                array[i + arrayIndex] = Get(i);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the array.
        /// </summary>
        /// <param name="value">The object to remove from the array.</param>
        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific item from the array.
        /// </summary>
        /// <param name="value">The item to remove from the array.</param>
        /// <returns>True if value was removed from the array, false otherwise.</returns>
        public bool Remove(T value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                {
                    for (; i < m_Length - 1; ++i)
                    {
                        Set(i, Get(i + 1));
                    }

                    --m_Length;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            CheckOutOfRangeIndex(index);

            for (int i = index; i < m_Length - 1; ++i)
            {
                Set(i, Get(i + 1));
            }

            --m_Length;
        }

        /// <summary>
        /// Inserts an item into the array at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The object to insert into the array.</param>
        void IList.Insert(int index, object value) => Insert(index, (T)value);

        /// <summary>
        /// Inserts an item into the array at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The item to insert into the array.</param>
        public void Insert(int index, T value)
        {
            if (m_Length >= k_MaxLength)
                throw new ArgumentException($"This array cannot have more than '{k_MaxLength}' items.");

            CheckOutOfRangeIndex(index);

            if (index >= m_Length)
            {
                Add(value);
                return;
            }

            for (int i = m_Length; i > index; --i)
            {
                Set(i, Get(i - 1));
            }

            Set(index, value);
            ++m_Length;
        }

        /// <summary>
        /// Clamps specified value in between 0 and k_MaxLength.
        /// </summary>
        /// <param name="size">Size value.</param>
        /// <returns>Value in between 0 and k_MaxLength.</returns>
        /// <seealso cref="EffectorArray.k_MaxLength"/>
        static int ClampSize(int size)
        {
            return Mathf.Clamp(size, 0, k_MaxLength);
        }

        /// <summary>
        /// Checks whether specified index value in within bounds.
        /// </summary>
        /// <param name="index">Index value.</param>
        /// <exception cref="IndexOutOfRangeException">Index value is not between 0 and k_MaxLength.</exception>
        /// <seealso cref="EffectorArray.k_MaxLength"/>
        void CheckOutOfRangeIndex(int index)
        {
            if (index < 0 || index >= k_MaxLength)
                throw new IndexOutOfRangeException($"Index {index} is out of range of '{m_Length}' Length.");
        }

        /// <summary>
        /// Retrieves an item at specified index.
        /// </summary>
        /// <param name="index">Index value.</param>
        /// <returns>The item value at specified index.</returns>
        T Get(int index)
        {
            CheckOutOfRangeIndex(index);

            switch (index)
            {
                case 0: return m_Item0;
                case 1: return m_Item1;
                case 2: return m_Item2;
                case 3: return m_Item3;
                case 4: return m_Item4;
                case 5: return m_Item5;
                case 6: return m_Item6;
                case 7: return m_Item7;
                case 8: return m_Item8;
                case 9: return m_Item9;
                case 10: return m_Item10;
                case 11: return m_Item11;
                case 12: return m_Item12;
                case 13: return m_Item13;
                case 14: return m_Item14;
                case 15: return m_Item15;
                case 16: return m_Item16;
                case 17: return m_Item17;
                case 18: return m_Item18;
                case 19: return m_Item19;
                case 20: return m_Item20;
                case 21: return m_Item21;
                case 22: return m_Item22;
                case 23: return m_Item23;
                case 24: return m_Item24;
                case 25: return m_Item25;
                case 26: return m_Item26;
                case 27: return m_Item27;
                case 28: return m_Item28;
                case 29: return m_Item29;
                case 30: return m_Item30;
                case 31: return m_Item31;
            }

            // Shouldn't happen.
            return m_Item0;
        }

        /// <summary>
        /// Sets an item value at specified index.
        /// </summary>
        /// <param name="index">Index value.</param>
        /// <param name="value">The item value to set.</param>
        void Set(int index, T value)
        {
            CheckOutOfRangeIndex(index);

            switch (index)
            {
                case 0:
                    m_Item0 = value;
                    break;
                case 1:
                    m_Item1 = value;
                    break;
                case 2:
                    m_Item2 = value;
                    break;
                case 3:
                    m_Item3 = value;
                    break;
                case 4:
                    m_Item4 = value;
                    break;
                case 5:
                    m_Item5 = value;
                    break;
                case 6:
                    m_Item6 = value;
                    break;
                case 7:
                    m_Item7 = value;
                    break;
                case 8:
                    m_Item8 = value;
                    break;
                case 9:
                    m_Item9 = value;
                    break;
                case 10:
                    m_Item10 = value;
                    break;
                case 11:
                    m_Item11 = value;
                    break;
                case 12:
                    m_Item12 = value;
                    break;
                case 13:
                    m_Item13 = value;
                    break;
                case 14:
                    m_Item14 = value;
                    break;
                case 15:
                    m_Item15 = value;
                    break;
                case 16:
                    m_Item16 = value;
                    break;
                case 17:
                    m_Item17 = value;
                    break;
                case 18:
                    m_Item18 = value;
                    break;
                case 19:
                    m_Item19 = value;
                    break;
                case 20:
                    m_Item20 = value;
                    break;
                case 21:
                    m_Item21 = value;
                    break;
                case 22:
                    m_Item22 = value;
                    break;
                case 23:
                    m_Item23 = value;
                    break;
                case 24:
                    m_Item24 = value;
                    break;
                case 25:
                    m_Item25 = value;
                    break;
                case 26:
                    m_Item26 = value;
                    break;
                case 27:
                    m_Item27 = value;
                    break;
                case 28:
                    m_Item28 = value;
                    break;
                case 29:
                    m_Item29 = value;
                    break;
                case 30:
                    m_Item30 = value;
                    break;
                case 31:
                    m_Item31 = value;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        object IList.this[int index]
        {
            get => Get(index);
            set => Set(index, (T)value);
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the WeightedTransform to get or set.</param>
        public T this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        /// <summary>The number of elements contained in the array.</summary>
        public int Count
        {
            get => m_Length;
        }

        /// <summary>
        /// Retrieves whether array is read-only. Always false.
        /// </summary>
        public bool IsReadOnly
        {
            get => false;
        }

        /// <summary>
        /// Retrieves whether array has a fixed size. Always false.
        /// </summary>
        public bool IsFixedSize
        {
            get => false;
        }

        bool ICollection.IsSynchronized
        {
            get => true;
        }

        object ICollection.SyncRoot
        {
            get => null;
        }

        [Serializable]
        struct Enumerator : IEnumerator<T>
        {
            FixedArray32<T> m_Array;
            int m_Index;

            public Enumerator(ref FixedArray32<T> array)
            {
                m_Array = array;
                m_Index = -1;
            }

            public bool MoveNext()
            {
                m_Index++;
                return (m_Index < m_Array.Count);
            }

            public void Reset()
            {
                m_Index = -1;
            }

            void IDisposable.Dispose() { }

            public T Current => m_Array.Get(m_Index);

            object IEnumerator.Current => Current;
        }
    }
}
