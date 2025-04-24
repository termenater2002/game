using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct PropData
    {
        public PropID ID;
        public string PrefabID;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
    }
}
