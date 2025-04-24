using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// This represents user-authored data of a timeline
    /// </summary>
    [Serializable]
    struct TimelineData
    {
        public List<KeyModel> Keyframes;
        public List<TransitionModel> Transitions;
    }
}
