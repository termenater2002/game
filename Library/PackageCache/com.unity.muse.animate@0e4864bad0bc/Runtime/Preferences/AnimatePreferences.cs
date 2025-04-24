using System;
using System.IO;
using Unity.Muse.Common;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Provides access to Muse Animate tool preferences.
    /// </summary>
    /// <remarks>
    /// To access to Muse global preferences, use the <see cref="GlobalPreferences"/> class.
    /// </remarks>
    static class AnimatePreferences
    {
        const string k_DefaultFolderNameForGeneratedAssets = "Animate";

#pragma warning disable 67
        internal static event Action PreferencesChanged;
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

        internal static string AssetGeneratedFolderPath
        {
            get => s_Preferences.Get<string>(nameof(AssetGeneratedFolderPath), PreferenceScope.Project, GetDefaultAnimateGeneratedAssetPath());
            set => s_Preferences.Set(nameof(AssetGeneratedFolderPath), value, PreferenceScope.Project);
        }

        internal static string GetDefaultAnimateGeneratedAssetPath()
        {
            return Path.Join(GlobalPreferences.GetMuseAssetsDefaultPath(), k_DefaultFolderNameForGeneratedAssets).Replace("\\", "/");
        }

        static void OnPreferencesChanged() => PreferencesChanged?.Invoke();
    }
}
