using Unity.Muse.Common;
using UnityEditor;
using Unity.Muse.Common.Editor;

namespace Unity.Muse.Sprite.Editor
{
    /// <summary>
    /// Initializes the sprite preferences system for the editor.
    /// </summary>
    /// <remarks>
    /// You should not need to use this class directly.
    /// It is used to initialize the sprite preferences system for the editor.
    /// </remarks>
    [InitializeOnLoad]
    static class EditorSpritePreferences
    {
        static EditorSpritePreferences()
        {
            GlobalPreferences.RegisterAssetGeneratedPath("TextToSprite", GetAssetPath);
            SpritePreferences.Init(new EditorPreferences(
                prefix: "Unity.Muse.Sprite.Preferences",
                settingsPath:  "ProjectSettings/Packages/com.unity.muse.sprite/Settings.json"));
            MuseProjectSettings.RegisterSection(TextContent.spriteSettingsCategory, new EditorSpritePreferencesView());
        }
        
        static string GetAssetPath() => SpritePreferences.assetGeneratedFolderPath;
    }
}
