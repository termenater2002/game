using System;
using System.Collections.Generic;

namespace Unity.Muse.Common
{
    internal interface IUIMode
    {
        void Activate(MainUI mainUI, string modeKey);
        void Deactivate();
    }

    internal static class UIModeFactory
    {
        static Dictionary<string, Type> s_AvailableUIMode;

        public static bool RegisterUIMode<T>(string mode) where T: IUIMode, new()
        {
            s_AvailableUIMode ??= new Dictionary<string, Type>();
            return s_AvailableUIMode.TryAdd(mode, typeof(T));
        }

        public static IUIMode GetUIMode(string mode)
        {
            if (s_AvailableUIMode == null || !s_AvailableUIMode.TryGetValue(mode, out var type))
            {
                return null;
            }

            return (IUIMode)Activator.CreateInstance(type);
        }
    }
}
