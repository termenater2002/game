using System;
using Unity.Muse.Common;

namespace Unity.Muse.Sprite
{
    /// <summary>
    /// Provides access to Muse Sprite tool preferences.
    /// </summary>
    /// <remarks>
    /// To access to Muse global preferences, use the <see cref="GlobalPreferences"/> class.
    /// </remarks>
    static class SpritePreferences
    {
        const string k_AssetsRoot = "Assets";
        
#pragma warning disable 67
        internal static event Action preferencesChanged;
#pragma warning restore 67
        
        static IMusePreferences s_Preferences;

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void Init() => Init(new RuntimePreferences());
#endif

        internal static void Init(IMusePreferences preferences)
        {
            s_Preferences = preferences;
            s_Preferences.changed += OnPreferencesChanged;
        }
        
        internal static string assetGeneratedFolderPath
        {
            get => s_Preferences.Get<string>(nameof(assetGeneratedFolderPath), PreferenceScope.Project, k_AssetsRoot);
            set => s_Preferences.Set(nameof(assetGeneratedFolderPath), value, PreferenceScope.Project);
        }
        
        static void OnPreferencesChanged() => preferencesChanged?.Invoke();
    }
}
