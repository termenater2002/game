using System;

namespace Unity.Muse.Animate
{
    static class KeyModelUtils
    {
        const string k_EmptyTitle = "Extrapolated";
        const string k_EmptyDescription = "Empty key without any information. Useful to let the model extrapolate your animation.";

        const string k_FullPoseTitle = "Full Pose";
        const string k_FullPoseDescription = "A key that specifies the entire pose of the character.";

        const string k_LoopTitle = "Loop";
        const string k_LoopDescription = "A key that defines a loop to a past key, with a translation and rotation. Useful to create looping animations.";

        public static string GetTitle(this KeyData.KeyType type)
        {
            return type switch
            {
                KeyData.KeyType.Empty => k_EmptyTitle,
                KeyData.KeyType.FullPose => k_FullPoseTitle,
                KeyData.KeyType.Loop => k_LoopTitle,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string GetDescription(this KeyData.KeyType type)
        {
            return type switch
            {
                KeyData.KeyType.Empty => k_EmptyDescription,
                KeyData.KeyType.FullPose => k_FullPoseDescription,
                KeyData.KeyType.Loop => k_LoopDescription,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
