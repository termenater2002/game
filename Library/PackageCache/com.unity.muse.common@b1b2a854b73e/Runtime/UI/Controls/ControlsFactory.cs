using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine;

namespace Unity.Muse.Common
{
    internal static class ControlsFactory
    {
        static Dictionary<string, Type> s_AvailableControlTypes;

        public static bool RegisterControl<T>() where T : IControl
        {
            var controlType = typeof(T);

            s_AvailableControlTypes ??= new Dictionary<string, Type>();
            return s_AvailableControlTypes.TryAdd(controlType.Name, controlType);
        }

        public static IControl GetControlInstance(string controlTypeName)
        {
            if (s_AvailableControlTypes == null || !s_AvailableControlTypes.TryGetValue(controlTypeName, out var controlType))
                return null;

            // Create the instance only when actually needed
            try
            {
                return (IControl)Activator.CreateInstance(controlType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating instance of {controlTypeName}: {ex.Message}");
                return null;
            }
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [Preserve]
        public static void RegisterDefaultControls()
        {
            RegisterControl<AssetsList>();
        }
    }
}
