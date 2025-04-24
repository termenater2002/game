using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents a full posing states, ie all effectors and their individual state
    /// </summary>
    [Serializable]
    struct PosingData
    {
        public DeepPoseEffectorModel[] DeepPoseEffectors;
    }
}
