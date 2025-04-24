#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common
{
    static class NetworkState
    {
        public static event Action OnChanged;
        public static bool IsAvailable => Application.internetReachability != NetworkReachability.NotReachable;

        static bool s_PreviousAvailability;
        public static bool cancel;

        [InitializeOnLoadMethod]
        public static void Init()
        {
            s_PreviousAvailability = IsAvailable;
            EditorApplication.quitting += () => cancel = true;
            AsyncUtils.SafeExecute(CheckNetworkAction);
        }

        // Keep checking availability.
        // Tried using NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged instead but Unity kept crashing with it.
        static async Task CheckNetworkAction()
        {
            await Task.Delay(2000);
            CheckNetworkAvailability();

            // Make sure to stop this method when quitting otherwise builds won't complete.
            if (!cancel)
                EditorApplication.delayCall += () => AsyncUtils.SafeExecute(CheckNetworkAction);
        }

        static void CheckNetworkAvailability()
        {
            if (s_PreviousAvailability != IsAvailable)
            {
                s_PreviousAvailability = IsAvailable;
                OnChanged?.Invoke();
            }
        }
    }
}
#endif