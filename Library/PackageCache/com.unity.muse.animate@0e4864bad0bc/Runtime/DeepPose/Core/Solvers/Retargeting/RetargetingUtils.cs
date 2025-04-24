using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class RetargetingUtils
    {
        public static void Retarget(this Animator sourceAnimator, Animator targetAnimator)
        {
            var retargetingSolver = new RetargetingSolver();
            var retargetingData = new RetargetingData
            {
                SourceAvatar = sourceAnimator.avatar,
                SourceTransform = sourceAnimator.transform,
                TargetAvatar = targetAnimator.avatar,
                TargetTransform = targetAnimator.transform
            };

            retargetingSolver.Initialize(retargetingData);
            retargetingSolver.Solve();
            retargetingSolver.Dispose();
        }
    }
}
