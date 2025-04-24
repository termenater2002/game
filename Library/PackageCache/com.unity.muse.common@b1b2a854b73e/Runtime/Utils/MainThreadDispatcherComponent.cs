using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    class MainThreadDispatcherComponent : MonoBehaviour
    {
        static MainThreadDispatcherComponent s_Instance;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitialize()
        {
            if (Application.isEditor || s_Instance)
                return;
            
            var go = new GameObject("[Muse] Main Thread Dispatcher");
            s_Instance = go.AddComponent<MainThreadDispatcherComponent>();
            DontDestroyOnLoad(go);
        }

        void OnApplicationQuit()
        {
            if (Application.isEditor)
                return;
            
            MainThreadDispatcher.Update();
            
            s_Instance = null;
            Destroy(gameObject);
        }

        void Update()
        {
            if (Application.isEditor)
                return;
            
            MainThreadDispatcher.Update();
        }
    }
}
