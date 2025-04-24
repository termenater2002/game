using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
#endif

namespace Unity.Muse.Common
{
    internal static class Preferences
    {
        public const string keyPrefix = "Unity.Muse.Common.Preferences.";
        public const string defaultImportFolderPath = "Assets";

        public const string packageName = "com.unity.muse.common";
        const string k_PackageVersionKey = "Unity.Muse.Common.Version";

        static Dictionary<string, string> s_PlayerPreferences = new()
        {
            { nameof(resultsTraySize), keyPrefix + nameof(resultsTraySize) },
            { nameof(autoSave), keyPrefix + nameof(autoSave) },
            { nameof(lastImportFolderPath), keyPrefix + nameof(lastImportFolderPath) }
        };

#if UNITY_EDITOR
        [UnityEditor.MenuItem("internal:Muse/Internals/Clear Preferences", false)]
#endif
        public static void ClearAll()
        {
            foreach (var (_, key) in s_PlayerPreferences)
            {
                if (PlayerPrefs.HasKey(key))
                    PlayerPrefs.DeleteKey(key);
            }

            Session.ClearAllSessionKeys();
        }

        public static float resultsTraySize
        {
            get => PlayerPrefs.GetFloat(s_PlayerPreferences[nameof(resultsTraySize)], 1.0f);
            set => PlayerPrefs.SetFloat(s_PlayerPreferences[nameof(resultsTraySize)], value);
        }

        public static bool autoSave
        {
            get => PlayerPrefs.GetInt(s_PlayerPreferences[nameof(autoSave)], 1) == 1;
            set => PlayerPrefs.SetInt(s_PlayerPreferences[nameof(autoSave)], value ? 1 : 0);
        }

        public static string lastImportFolderPath
        {
            get => PlayerPrefs.GetString(s_PlayerPreferences[nameof(lastImportFolderPath)], defaultImportFolderPath);
            set => PlayerPrefs.SetString(s_PlayerPreferences[nameof(lastImportFolderPath)], value);
        }

        /// <summary>
        /// Preferences that last only for one session.
        /// </summary>
        internal static class Session
        {
            public const string sessionKeyPrefix = "Unity.Muse.Common.SessionPreferences.";
            static Dictionary<string, string> s_SessionPlayerPreferences = new()
            {
                { nameof(deleteWithoutWarning), sessionKeyPrefix + nameof(deleteWithoutWarning) }
            };

            [RuntimeInitializeOnLoadMethod]
            [Preserve]
#if UNITY_EDITOR
            [UnityEditor.InitializeOnLoadMethod]
#endif
            public static void Init()
            {
                ClearAllSessionKeys();
#if UNITY_EDITOR
                s_EditorUpdateFrames = 0;
                EditorApplication.update += OnEditorUpdate;
                EditorApplication.quitting += OnEditorQuit; // Subscribe to the quitting event
#endif
            }

#if UNITY_EDITOR

            static int s_EditorUpdateFrames = 0;

            private static void OnEditorUpdate()
            {
                if (Application.isPlaying)
                    return;

                if (s_EditorUpdateFrames < 10)
                {
                    s_EditorUpdateFrames++;
                    return;
                }

                EditorApplication.update -= OnEditorUpdate;
                CheckPackageVersion();
            }

            private static async void CheckPackageVersion()
            {
                var request = Client.List(true, true);

                while (request.Status == StatusCode.InProgress)
                {
                    await System.Threading.Tasks.Task.Delay(100);
                }

                if (request.Status == StatusCode.Success && request.Result != null)
                {
                    foreach (var package in request.Result)
                    {
                        if (package.name == packageName)
                        {
                            var version = package.version;
                            if (PlayerPrefs.HasKey(k_PackageVersionKey))
                            {
                                var previousVersion = PlayerPrefs.GetString(k_PackageVersionKey);
                                if (previousVersion != version)
                                {
                                    PlayerPrefs.SetString(k_PackageVersionKey, version);
                                    OnPackageVersionChanged();
                                }
                            }
                            else
                            {
                                PlayerPrefs.SetString(k_PackageVersionKey, version);
                                OnPackageVersionChanged();
                            }
                            break;
                        }
                    }
                }
            }

            private static void OnPackageVersionChanged()
            {
                EditorUtility.RequestScriptReload();
            }

            private static void OnEditorQuit()
            {
                EditorApplication.update -= OnEditorUpdate;
                PlayerPrefs.SetString(k_PackageVersionKey, string.Empty);
                PlayerPrefs.Save(); // Save PlayerPrefs when the editor is closing
            }
#endif

            public static void ClearAllSessionKeys()
            {
                foreach (var (_, key) in s_SessionPlayerPreferences)
                {
                    if (PlayerPrefs.HasKey(key))
                        PlayerPrefs.DeleteKey(key);
                }
            }

            public static bool deleteWithoutWarning
            {
                get => PlayerPrefs.GetInt(s_SessionPlayerPreferences[nameof(deleteWithoutWarning)], 0) == 1;
                set => PlayerPrefs.SetInt(s_SessionPlayerPreferences[nameof(deleteWithoutWarning)], value ? 1 : 0);
            }

            public static CanvasControlScheme canvasControlScheme
            {
                get => (CanvasControlScheme)PlayerPrefs.GetInt(s_SessionPlayerPreferences[nameof(canvasControlScheme)], (int)CanvasControlScheme.Modern);
                set => PlayerPrefs.SetInt(s_SessionPlayerPreferences[nameof(canvasControlScheme)], (int)value);
            }
        }
    }
}
