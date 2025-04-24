#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class ExploreWindow : EditorWindow
    {
        [UnityEditor.MenuItem("Muse/Explore Muse", false, 50)]
        public static void ShowExplore()
        {
            var window = GetWindow<ExploreWindow>(utility:true, title: TextContent.exploreWindowTitle, focus: true);
            window.minSize = new Vector2(350, 200);
            window.ShowUtility();
        }

        void CreateGUI()
        {
            rootVisualElement.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));

            var panel = new Panel();
            panel.theme = EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light";

            rootVisualElement.Add(panel);

            AccountController.Register(this);
            panel.Add(new ExploreMuseDialog {OnAccept = Close});
        }
    }
}
#endif