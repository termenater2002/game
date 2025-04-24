#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    static class MuseWindowTracker
    {
        static List<EditorWindow> s_Windows = new();

        public static void Register(EditorWindow window) => s_Windows.Add(window);

        public static bool IsAnyWindowRegistered()
        {
            s_Windows = s_Windows.Where(IsValid).ToList();
            return s_Windows.Any();
        }

        static bool IsValid(EditorWindow window) =>
            window != null && window.rootVisualElement.Q<Panel>() != null;
    }
}
#endif