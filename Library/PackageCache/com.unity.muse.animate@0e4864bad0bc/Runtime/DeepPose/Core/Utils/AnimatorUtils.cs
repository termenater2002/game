using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class AnimatorUtils
    {
        public static HumanBodyBones GetHumanBone(this Animator animator, Transform transform)
        {
            for (var i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                var humanBone = (HumanBodyBones)i;
                var boneTransform = animator.GetBoneTransform(humanBone);
                if (boneTransform == transform)
                    return humanBone;
            }

            return HumanBodyBones.LastBone;
        }

        public static bool TryGetHumanoidAnimator(this GameObject go, out Animator animator)
        {
            animator = null;

            if (go == null)
                return false;

            animator = go.GetComponentInChildren<Animator>();
            if (animator == null)
                return false;

            var avatar = animator.avatar;
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                return false;

            return true;
        }
    }
}
