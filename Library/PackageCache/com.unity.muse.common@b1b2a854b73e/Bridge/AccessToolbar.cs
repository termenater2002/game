#if !MUSE_TOOLBAR_BUTTON_ENABLED && UNITY_EDITOR
using System;
using System.Reflection;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Common.Bridge
{
    [InitializeOnLoad]
    public static class AccessToolbar
    {
        static Assembly EditorAssembly => typeof(UnityEditor.Editor).Assembly;
        static Type MuseDropDownType => Type.GetType("UnityEditor.Toolbars.MuseDropdown, UnityEditor");
        static bool s_DomainReloading;
        static bool UseNativeToolbarInterface => MuseDropDownType != null;

        static AccessToolbar()
        {
            if (Application.isBatchMode)
                return;

            if (UseNativeToolbarInterface)
                ShowNative();
            else
                ShowCustom();
        }

        static void ShowCustom() => EnsureButtonIsPresent();

        static void EnsureButtonIsPresent()
        {
            EditorApplication.delayCall += () =>
            {
                var toolbarType = EditorAssembly.GetType("UnityEditor.Toolbar");
                if (toolbarType == null)
                {
                    Debug.LogError("Could not find UnityEditor.Toolbar type. Muse will not be available.");
                    return;
                }

                var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rootField == null)
                {
                    Debug.LogError("Could not find toolbar root. Muse will not be available.");
                    return;
                }

                // Note: Not using `UnityEditor.Toolbar.get` directly here since 2023.2.20f1 does not expose `Toolbar` publicly
                var getField = toolbarType.GetField("get", BindingFlags.Public | BindingFlags.Static);
                if (getField == null)
                {
                    Debug.LogError("Could not find the Toolbar instance field. Muse will not be available.");
                    return;
                }

                var toolbarInstance = getField.GetValue(null);
                if (toolbarInstance == null)
                {
                    Debug.LogError("Could not find the Toolbar instance. Muse will not be available.");
                    return;
                }

                if (rootField.GetValue(toolbarInstance) is VisualElement toolbarRoot)
                {
                    var toolbarZoneLeftAlign = toolbarRoot.Q<VisualElement>("ToolbarZoneLeftAlign");
                    if (toolbarZoneLeftAlign != null)
                        AddCustomMuseButton(toolbarZoneLeftAlign, toolbarType, toolbarInstance);
                    else
                        Debug.LogError("Could not find toolbar left zone. Muse will not be available.");
                }
            };
        }

        static void AddCustomMuseButton(VisualElement toolbarZoneLeftAlign, Type toolbarType, object toolbarInstance)
        {
            var museContainerClass = "muse-editor-toolbar-container";
            if (toolbarZoneLeftAlign.Q<Panel>(classes: museContainerClass) != null)
                return;

            var container = new Panel();
            container.AddToClassList(museContainerClass);

            // Ensure the position is relative to avoid taking any more space than necessary.
            container.contentContainer.style.position = Position.Relative;
            container.ProvideContext(new ThemeContext(EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light"));
            container.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));

            Button museButton = new Button {title = "Muse"};
            var caretIcon = new VisualElement();
            caretIcon.AddToClassList("unity-icon-arrow");
            museButton.Add(caretIcon);
            museButton.clicked += () =>
                AccountDropdownWindow.ShowMuseAccountSettingsAsPopup(museButton.worldBound);
            museButton.AddToClassList("unity-toolbar-button");
            // After long periods, especially when closing and re-opening a laptop, the button gets destroyed,
            // so we need to make sure it gets added back after being detached.
            museButton.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                if (!s_DomainReloading)
                    EnsureButtonIsPresent();
            });
            AccountDropdownWindow.toolbarButton = museButton;

            var positionField = toolbarType.GetProperty("screenPosition", BindingFlags.Public | BindingFlags.Instance);
            AccountDropdownWindow.toolbarPosition = () =>
            {
                if (positionField?.GetValue(toolbarInstance) is Rect rect)
                    return rect;

                return Rect.zero;
            };

            container.Add(museButton);

            toolbarZoneLeftAlign.Add(container);
        }

        static void ShowNative()
        {
            var onDropdownOpenedField = MuseDropDownType.GetField("OnDropdownOpened", BindingFlags.Static | BindingFlags.NonPublic);
            if (onDropdownOpenedField == null)
                return;

            // Assign the delegate to the event field
            onDropdownOpenedField.SetValue(null, (Action<Rect, VisualElement>) NativeHandler);
        }

        static void NativeHandler(Rect rect, VisualElement visualElement) => AccountDropdownWindow.ShowMuseAccountSettingsAsPopup(rect);

        [InitializeOnLoadMethod]
        static void DomainReloadDetector()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () => s_DomainReloading = true;
            AssemblyReloadEvents.afterAssemblyReload += () => s_DomainReloading = false;
        }
    }
}
#endif
