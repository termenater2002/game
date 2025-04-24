using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Common
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    static class MainThreadDispatcher
    {
        static MainThreadDispatcher()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Update;
            UnityEditor.EditorApplication.update += Update;
#endif
        }
        
        static readonly Queue<Action> s_Actions = new Queue<Action>();
        
        internal static void Invoke(Action action)
        {
            lock (s_Actions)
            {
                s_Actions.Enqueue(action);
            }
        }

        internal static void Update()
        {
            lock (s_Actions)
            {
                while (s_Actions.Count > 0)
                {
                    s_Actions.Dequeue()?.Invoke();
                }
            }
        }
    }
}