using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct ActorData
    {
        public ActorID ID;
        public string PrefabID;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
    }
}
