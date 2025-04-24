using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    internal struct OperatorData
    {
        public string type;
        public string key;
        public string version;
        public string[] settings;
        public bool enabled;
        public string assembly;
        public bool hideable;
        public bool hidden;
        public bool isRefinement;

        public OperatorData(string type, string version, string[] settings, bool enabled, string key = null, 
            bool hideable = false, bool isRefinement = false)
        {
            this.type = type;
            this.key = string.IsNullOrEmpty(key) ? type : key;
            this.version = version;
            this.settings = settings;
            this.enabled = enabled;
            this.hideable = hideable;
            this.hidden = hideable;
            this.isRefinement = isRefinement;
            this.assembly = String.Empty;
        }

        public OperatorData(string type, string assembly, string version, string[] settings, bool enabled,
            string key = null, bool hideable = false, bool isRefinement = false)
        {
            this.type = type;
            this.key = string.IsNullOrEmpty(key) ? type : key;
            this.version = version;
            this.settings = settings;
            this.enabled = enabled;
            this.hideable = hideable;
            this.hidden = hideable;
            this.isRefinement = isRefinement;
            this.assembly = assembly;
        }

        public void FromJson(string json)
        {
            this = JsonUtility.FromJson<OperatorData>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}