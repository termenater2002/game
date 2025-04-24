using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat
{
    [FilePath("MuseChatEnv", FilePathAttribute.Location.PreferencesFolder)]
    internal class MuseChatEnvironment : ScriptableSingleton<MuseChatEnvironment>
    {
        const string k_DefaultApiUrl = "https://rest-api.prd.azure.muse.unity.com";

        [SerializeField]
        public string ApiUrl = k_DefaultApiUrl;

        [SerializeField]
        public bool DebugModeEnabled;

        internal void SetApi(string apiUrl, string backend)
        {
            ApiUrl = apiUrl;
            Save(true);
        }

        internal void Reset()
        {
            ApiUrl = k_DefaultApiUrl;
            DebugModeEnabled = false;
            Save(true);
        }
    }
}
