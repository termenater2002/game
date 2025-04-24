using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    class RuntimePreferences : IMusePreferences
    {
        public event Action changed;
        
        const string k_SessionKeyPrefix = "Unity.Muse.Common.Preferences";

        readonly string m_KeyPrefix;
        
        string Key(string key) => $"{m_KeyPrefix}.{key}";

        public RuntimePreferences(string prefix = k_SessionKeyPrefix)
        {
            m_KeyPrefix = prefix;
        }

        public T Get<T>(string key, PreferenceScope scope = PreferenceScope.Project, T defaultValue = default)
        {
            if (!PlayerPrefs.HasKey(Key(key)))
                Set(key, defaultValue);

            var json = PlayerPrefs.GetString(Key(key));
            return JsonUtility.FromJson<PreferenceDataWrapper<T>>(json).value;
        }

        public void Set<T>(string key, T value, PreferenceScope scope = PreferenceScope.Project)
        {
            var wrapper = new PreferenceDataWrapper<T> { value = value };
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(Key(key), json);
            changed?.Invoke();
        }

        public void Delete<T>(string key, PreferenceScope scope = PreferenceScope.Project)
        {
            if (PlayerPrefs.HasKey(Key(key)))
            {
                PlayerPrefs.DeleteKey(Key(key));
                changed?.Invoke();
            }
        }
    }
}
