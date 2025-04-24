using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    [Serializable]
    struct RetargetingData
    {
        public Transform SourceTransform;
        public Transform TargetTransform;
        public Avatar SourceAvatar;
        public Avatar TargetAvatar;
    }
}
