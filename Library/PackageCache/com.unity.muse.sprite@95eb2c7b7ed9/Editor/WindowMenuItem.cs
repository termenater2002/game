using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Editor
{
    internal static class WindowMenuItem
    {
        public const string menuItemPath = "Muse/New Sprite Generator";

        [MenuItem(menuItemPath, false, 100)]
        public static void CreateSpriteWindowMenuItem()
        {
            CreateSpriteWindow();
        }

        internal static MuseEditor CreateSpriteWindow()
        {
            var museWindow = EditorModelAssetEditor.OpenWindowForMode(UIMode.UIMode.modeKey);
            museWindow.DiscardChanges();
            return museWindow;
        }

        [MenuItem(menuItemPath, true)]
        public static bool ValidateCreateSpriteWindow()
        {
            return ModesFactory.GetModeIndexFromKey(UIMode.UIMode.modeKey) > -1;
        }
    }
}
