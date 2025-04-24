using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    class EditorPreferences : IMusePreferences
    {
        public event Action changed;
        
        const string k_SessionKeyPrefix = "Unity.Muse.Common.Preferences";
        
        const string k_SettingsPath = "ProjectSettings/Packages/com.unity.muse.common/Settings.json";
        
        readonly string m_KeyPrefix;
        
        readonly string m_SettingsPath;
        
        string Key(string key) => $"{m_KeyPrefix}.{key}";

        public EditorPreferences(string prefix = k_SessionKeyPrefix, string settingsPath = k_SettingsPath)
        {
            m_KeyPrefix = prefix;
            m_SettingsPath = settingsPath;
        }

        public T Get<T>(string key, PreferenceScope scope = PreferenceScope.Project, T defaultValue = default)
        {
            return scope == PreferenceScope.User 
                ? EditorPrefsGet<T>(Key(key), defaultValue) 
                : ProjectPrefsGet<T>(Key(key), defaultValue);
        }

        public void Set<T>(string key, T value, PreferenceScope scope = PreferenceScope.Project)
        {
            if (scope == PreferenceScope.User)
                EditorPrefsSet(Key(key), new PreferenceDataWrapper<T> { value = value });
            else
                ProjectPrefsSet(Key(key), new PreferenceDataWrapper<T> { value = value });
            
            changed?.Invoke();
        }

        public void Delete<T>(string key, PreferenceScope scope = PreferenceScope.Project)
        {
            if (scope == PreferenceScope.User)
                EditorPrefs.DeleteKey(Key(key));
            else
                ProjectPrefsDelete(Key(key));
            
            changed?.Invoke();
        }

        T EditorPrefsGet<T>(string key, T defaultValue)
        {
            var json = EditorPrefs.GetString(key, JsonUtility.ToJson(defaultValue));
            return JsonUtility.FromJson<PreferenceDataWrapper<T>>(json).value;
        }
        
        void EditorPrefsSet<T>(string key, PreferenceDataWrapper<T> value)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(value));
        }

        T ProjectPrefsGet<T>(string key, T defaultValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(m_SettingsPath), 
                "Settings path is null or empty");
            
            var dir = Path.GetDirectoryName(m_SettingsPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            if (!File.Exists(m_SettingsPath))
                return defaultValue;
            
            var json = File.ReadAllText(m_SettingsPath);
            var settings = JsonUtility.FromJson<SerializedSettings>(json);
            if (settings.m_Dictionary?.m_DictionaryValues == null)
                return defaultValue;
            
            foreach (var value in settings.m_Dictionary.m_DictionaryValues)
            {
                if (value.key == key && value.type == typeof(PreferenceDataWrapper<T>).FullName)
                    return JsonUtility.FromJson<PreferenceDataWrapper<T>>(value.value).value;
            }
            
            return defaultValue;
        }
        
        void ProjectPrefsSet<T>(string key, T value)
        {
            Debug.Assert(!string.IsNullOrEmpty(m_SettingsPath), 
                "Settings path is null or empty");
            
            var dir = Path.GetDirectoryName(m_SettingsPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(m_SettingsPath))
            {
                var s = new SerializedSettings
                {
                    m_Dictionary = new SerializedSettingsDictionary
                    {
                        m_DictionaryValues = new[]
                        {
                            new SerializedSettingsValue
                            {
                                key = key,
                                type = typeof(T).FullName,
                                value = JsonUtility.ToJson(value)
                            }
                        }
                    }
                };
                File.WriteAllText(m_SettingsPath, JsonUtility.ToJson(s, true));
                return;
            }
            
            var json = File.ReadAllText(m_SettingsPath);
            var settings = JsonUtility.FromJson<SerializedSettings>(json);
            if (settings.m_Dictionary?.m_DictionaryValues == null)
            {
                settings.m_Dictionary = new SerializedSettingsDictionary
                {
                    m_DictionaryValues = new[]
                    {
                        new SerializedSettingsValue
                        {
                            key = key,
                            type = typeof(T).FullName,
                            value = JsonUtility.ToJson(value)
                        }
                    }
                };
                File.WriteAllText(m_SettingsPath, JsonUtility.ToJson(settings, true));
                return;
            }
            
            var found = false;
            foreach (var val in settings.m_Dictionary.m_DictionaryValues)
            {
                if (val.key == key)
                {
                    val.value = JsonUtility.ToJson(value);
                    val.type = typeof(T).FullName;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var newValues = new SerializedSettingsValue[settings.m_Dictionary.m_DictionaryValues.Length + 1];
                Array.Copy(
                    settings.m_Dictionary.m_DictionaryValues, 
                    newValues, 
                    settings.m_Dictionary.m_DictionaryValues.Length);
                newValues[^1] = new SerializedSettingsValue
                {
                    key = key,
                    type = typeof(T).FullName,
                    value = JsonUtility.ToJson(value)
                };
                settings.m_Dictionary.m_DictionaryValues = newValues;
            }
            
            File.WriteAllText(m_SettingsPath, JsonUtility.ToJson(settings, true));
        }
        
        void ProjectPrefsDelete(string key)
        {
            Debug.Assert(!string.IsNullOrEmpty(m_SettingsPath), 
                "Settings path is null or empty");
            
            var dir = Path.GetDirectoryName(m_SettingsPath);
            if (!Directory.Exists(dir))
                return;

            if (!File.Exists(m_SettingsPath))
                return;
            
            var json = File.ReadAllText(m_SettingsPath);
            var settings = JsonUtility.FromJson<SerializedSettings>(json);
            if (settings.m_Dictionary?.m_DictionaryValues == null)
                return;
            
            var newValues = settings.m_Dictionary.m_DictionaryValues
                .Where(v => v.key != key).ToArray();
            settings.m_Dictionary.m_DictionaryValues = newValues;
            File.WriteAllText(m_SettingsPath, JsonUtility.ToJson(settings, true));
        }
    }
}
