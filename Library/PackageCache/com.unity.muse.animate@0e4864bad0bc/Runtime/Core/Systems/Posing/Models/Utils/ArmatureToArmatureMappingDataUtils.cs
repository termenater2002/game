using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class ArmatureToArmatureMappingDataUtils
    {
        public static void FindTargetJoints(this ref ArmatureToArmatureMappingData data, ArmatureData sourceArmature, ArmatureData targetArmature)
        {
            Assert.AreEqual(data.NumSourceJoints, sourceArmature.NumJoints);

            for (var sourceIndex = 0; sourceIndex < data.NumSourceJoints; sourceIndex++)
            {
                var sourceJointName = sourceArmature.Joints[sourceIndex].Name;
                var targetIndex = targetArmature.GetJointIndex(sourceJointName);
                if (targetIndex < 0)
                    targetIndex = targetArmature.GetJointIndex(sourceJointName, true);

                data.SetTargetJointIndex(sourceIndex, targetIndex);
            }
        }
    }
}
