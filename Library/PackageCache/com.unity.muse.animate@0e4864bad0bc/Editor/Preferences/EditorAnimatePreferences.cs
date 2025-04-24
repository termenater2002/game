using Unity.Muse.Common;
using UnityEditor;
using Unity.Muse.Common.Editor;

namespace Unity.Muse.Animate.Editor
{
    /// <summary>
    /// Initializes the Muse Animation tool preferences system for the editor.
    /// </summary>
    /// <remarks>
    /// You should not need to use this class directly.
    /// It is used to initialize the sprite preferences system for the editor.
    /// </remarks>
    [InitializeOnLoad]
    static class EditorAnimatePreferences
    {
        static EditorAnimatePreferences()
        {
            GlobalPreferences.RegisterAssetGeneratedPath("TextToAnimation", GetAssetPath);
            AnimatePreferences.Init(new EditorPreferences(
                prefix: "Unity.Muse.Animate.Preferences",
                settingsPath:  "ProjectSettings/Packages/com.unity.muse.animate/Settings.json"));
            MuseProjectSettings.RegisterSection(TextContent.animateSettingsCategory, new EditorAnimatePreferencesView());
        }
        
        static string GetAssetPath() => AnimatePreferences.AssetGeneratedFolderPath;
    }
}
