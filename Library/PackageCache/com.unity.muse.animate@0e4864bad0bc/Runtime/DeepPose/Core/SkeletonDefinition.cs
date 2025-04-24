using UnityEngine;

namespace Unity.DeepPose.Core
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "Skeleton", menuName = "Muse Animate Dev/Skeleton Definition")]
#endif
    class SkeletonDefinition : ScriptableObject
    {
        public Skeleton Skeleton;
    }
}
