using Unity.DeepPose.Core;
using UnityEngine;

namespace Unity.Muse.Animate
{
    static class ArmatureDefinitionUtils
    {
        public static void FromSkeleton(this ArmatureDefinition armatureDefinition, Skeleton skeleton)
        {
            var armatureData = new ArmatureData(skeleton.Count);
            armatureData.FromSkeleton(skeleton);
            armatureDefinition.Armature = armatureData;
        }

        public static void FromHierarchy(this ArmatureDefinition armatureDefinition, Transform rootTransform)
        {
            var armatureData = new ArmatureData(0);
            armatureData.FromHierarchy(rootTransform);
            armatureDefinition.Armature = armatureData;
        }

        public static bool HaveSameJointNames(this ArmatureDefinition armature1, ArmatureDefinition armature2)
        {
            if (armature1 == null || armature2 == null)
                return false;

            return armature1.Armature.HaveSameJointNames(armature2.Armature);
        }

        public static void FindJoints(this ArmatureDefinition armatureDefinition, Transform rootTransform, Transform[] result)
        {
            armatureDefinition.Armature.FindJoints(rootTransform, result);
        }
    }
}
