using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a transition between two keyframes
    /// </summary>
    [Serializable]
    struct TransitionData
    {
        public enum TransitionType
        {
            Linear = 0,
            MotionSynthesis = 1
        }

        public int Duration;
        public TransitionType Type;
    }
}
