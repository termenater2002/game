using System;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct PlaybackData
    {
        public bool IsPlaying;
        public bool IsLooping;
        public float MinTime;
        public float MaxTime;
        public float CurrentTime;
        public float TimeWhenStartedPlaying;
        public float FramesPerSecond;
        public float PlaybackSpeed;
    }
}
