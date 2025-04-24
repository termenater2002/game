using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Common
{
    internal static class ObjectUtils
    {
        static Dictionary<Object, List<object>> s_ObjectPool = new();

        /// <summary>
        /// Reference-count a unity object according to a given context.
        /// </summary>
        /// <param name="obj">The unity object.</param>
        /// <param name="context">Any object related to the context of object (eg: an editor panel).</param>
        internal static void Retain(Object obj, object context = null)
        {
            if (obj == null)
                return;

            obj.hideFlags = HideFlags.DontSave;

            if (context == null)
                return;

            if (!s_ObjectPool.TryGetValue(obj, out var contexts))
            {
                contexts = new List<object>();
                s_ObjectPool.Add(obj, contexts);
            }

            if (!contexts.Contains(context))
                contexts.Add(context);
        }

        /// <summary>
        /// Releases all unity objects according to a given context. If the unity object has no more related context, it will be disposed.
        /// </summary>
        /// <param name="context">Any object related to the context of object (eg: an editor panel).</param>
        internal static void Release(object context)
        {
            if (context == null)
                return;

            var remove = new List<Object>();
            foreach (var (obj, contexts) in s_ObjectPool)
            {
                contexts.Remove(context);
                if (contexts.Count == 0)
                    remove.Add(obj);
            }

            foreach (var texture in remove)
            {
                s_ObjectPool.Remove(texture);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
