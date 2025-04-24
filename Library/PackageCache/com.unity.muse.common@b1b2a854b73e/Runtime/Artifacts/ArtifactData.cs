using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    [Serializable]
    internal struct ArtifactData
    {
        public string guid;
        public OperatorData[] operators;
        public ArtifactData(string guid, OperatorData[] operators)
        {
            this.guid = guid;
            this.operators = operators;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            this = JsonUtility.FromJson<ArtifactData>(json);
        }
    }
}
