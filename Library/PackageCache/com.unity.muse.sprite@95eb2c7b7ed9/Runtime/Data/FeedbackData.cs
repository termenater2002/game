using System;
using UnityEngine;

namespace Unity.Muse.Sprite.Data
{
    [Flags]
    internal enum FeedbackFlags
    {
        None = 0,

        Disliked = 1 << 0
    }

    [Serializable]
    internal struct FeedbackData
    {
        public const int currentVersion = 0;
        
        public int version;
        public bool disliked;

        public int GetFlags()
        {
            var flags = 0;

            flags |= (int)FeedbackFlags.Disliked;

            return flags;
        }

        public override string ToString() => JsonUtility.ToJson(this);
    }
}
