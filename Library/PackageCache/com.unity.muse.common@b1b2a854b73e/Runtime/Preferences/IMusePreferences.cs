using System;

namespace Unity.Muse.Common
{
    interface IMusePreferences
    {
        event Action changed;
        T Get<T>(string key, PreferenceScope scope = PreferenceScope.Project, T defaultValue = default);
        void Set<T>(string key, T value, PreferenceScope scope = PreferenceScope.Project);
        public void Delete<T>(string key, PreferenceScope scope = PreferenceScope.Project);
    }
}
