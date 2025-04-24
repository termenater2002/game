using System;
using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// The Service Locator pattern implementation, a.k.a. "Poor man's dependency injection"
    /// </summary>
    abstract class Locator
    {
        static readonly Dictionary<Type, object> k_Instances = new();

        public static void Provide<T>(T instance)
        {
            k_Instances[typeof(T)] = instance;
        }
        
        public static T Get<T>()
        {
            if (k_Instances.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            throw new InvalidOperationException($"No instance of {typeof(T)} registered");
        }
        
        public static bool TryGet<T>(out T instance)
        {
            if (k_Instances.TryGetValue(typeof(T), out var obj))
            {
                instance = (T)obj;
                return true;
            }

            instance = default;
            return false;
        }
    }
}
