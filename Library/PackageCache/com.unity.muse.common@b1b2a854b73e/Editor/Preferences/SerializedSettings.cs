using System;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    [Serializable]
    class SerializedSettings
    {
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        public SerializedSettingsDictionary m_Dictionary;
    }
        
    [Serializable]
    class SerializedSettingsDictionary
    {
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        public SerializedSettingsValue[] m_DictionaryValues;
    }
        
    [Serializable]
    class SerializedSettingsValue
    {
        [SerializeField]
        public string type;
            
        [SerializeField]
        public string key;
            
        [SerializeField]
        public string value;
    }
}
