using System.Collections.Generic;

namespace Unity.Muse.Animate
{
    static class DeepPoseAnalyticsUtils
    {
        public static bool TryGetSelectedEffectorNames(EntityID entityID, PoseAuthoringLogic poseLogic, out List<string> result)
        {
            result = null;
            
            var effectorSelectionCount = poseLogic.EffectorSelectionCount;
            if (effectorSelectionCount == 0)
                return false;

            var armatureDefinition = poseLogic.GetPosingArmature(entityID)?.ArmatureDefinition;
            if (armatureDefinition == null)
                return false;

            result = new List<string>(effectorSelectionCount);

            for (var i = 0; i < effectorSelectionCount; ++i)
            {
                var effectorModel = poseLogic.GetSelectedEffector(i);
                var jointName = armatureDefinition.GetJointName(effectorModel.Index.JointIndex);
                result.Add(jointName);
            }

            return true;
        }
    }
}
