#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    delegate AccountState StateTransition(AccountState toState, AccountState fromState);

    class AccountController
    {
        /// <summary>
        /// Method signature only kept for API compatibility. Only window is actually required.
        /// Can be safely changed after muse common 1.1.0 is released.
        /// </summary>
        public static void Register(EditorWindow window, StateTransition transition = null, bool allowNoAccount = false, VisualElement container = null)
        {
            MuseWindowTracker.Register(window);

            var element = window.rootVisualElement.Q<Panel>();
            if (element is null)
                throw new Exception("The window must have a visual element with a Panel type to be apple to display modal dialogs.");

            element.theme = EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light";
            element.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));
            element.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.accountStyleSheet));
        }
    }
}
#endif