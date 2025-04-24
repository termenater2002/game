using System;
using System.Collections.Concurrent;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// A disposable wrapper for a pooled object. When disposed, the object is automatically returned to the pool.
    /// </summary>
    /// <remarks>
    /// Requires that all instances of the underlying type be initialized in the same way. If you require
    /// some instances to be initialized differently, use <see cref="TempObject{TArgs,TValue}"/> instead. 
    /// </remarks>
    readonly struct TempObject<T> : IDisposable where T : class
    {
        static readonly ConcurrentBag<T> k_ObjectPool = new();
        static Func<T> s_CreateFunc;

        /// <summary>
        /// Register a function that creates instances of the pooled object. Must be called before
        /// calling <see cref="Get"/>.
        /// </summary>
        /// <param name="createFunc">Function that returns a new object instance.</param>
        public static void Register(Func<T> createFunc)
        {
            s_CreateFunc = createFunc;
        }

        TempObject(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a new temporary pooled object. If the pool is empty, a new object will be created using the
        /// registered creation function.
        /// </summary>
        /// <returns>A new <see cref="TempObject{T}"/>.</returns>
        public static TempObject<T> Get()
        {
            Assert.IsNotNull(s_CreateFunc, "Pool is not initialized. Call TempObject<T>.Register() first.");
            return new TempObject<T>(k_ObjectPool.TryTake(out var item) ? item : s_CreateFunc());
        }

        public void Dispose()
        {
            k_ObjectPool.Add(Value);
        }

        public T Value { get; }

        public static implicit operator T(TempObject<T> tempObject) => tempObject.Value;
    }

    /// <summary>
    /// A disposable wrapper for a pooled object. This version allows you to get different variants of
    /// the same type, each generated from different arguments.
    /// </summary>
    /// <typeparam name="TValue">The type of the underlying object.</typeparam>
    /// <typeparam name="TArgs">The arguments used to generate an instance of the pooled object.</typeparam>
    readonly struct TempObject<TValue, TArgs> : IDisposable where TValue : class
    {
        static readonly ConcurrentDictionary<TArgs, ConcurrentBag<TValue>> k_ObjectPools = new();
        static Func<TArgs, TValue> s_CreateFunc;

        readonly TArgs m_Key;

        TempObject(TArgs key, TValue value)
        {
            m_Key = key;
            Value = value;
        }

        /// <summary>
        /// Register a function that creates instances of the pooled object. Must be called before
        /// calling <see cref="Get"/>.
        /// </summary>
        /// <param name="createFunc">Function that takes a <typeparamref name="TArgs"/> and returns a new instance
        /// of <typeparamref name="TValue"/></param>
        public static void Register(Func<TArgs, TValue> createFunc)
        {
            s_CreateFunc = createFunc;
        }

        /// <summary>
        /// Get a temporary pooled object that was generated using the registered creation function and the
        /// specified <paramref name="args"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>A disposable temporary object.</returns>
        public static TempObject<TValue, TArgs> Get(TArgs args)
        {
            Assert.IsNotNull(s_CreateFunc, "Pool is not initialized. Call TempObject<TKey, TValue>.Register() first.");
            var queue = k_ObjectPools.GetOrAdd(args, k => new ConcurrentBag<TValue>());
            return new TempObject<TValue, TArgs>(args, queue.TryTake(out var item) ? item : s_CreateFunc(args));
        }

        public void Dispose()
        {
            k_ObjectPools[m_Key].Add(Value);
        }

        public TValue Value { get; }

        public static implicit operator TValue(TempObject<TValue, TArgs> tempObject) => tempObject.Value;
    }
}
