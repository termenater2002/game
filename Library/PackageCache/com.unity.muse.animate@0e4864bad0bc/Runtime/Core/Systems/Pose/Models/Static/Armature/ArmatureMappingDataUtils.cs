using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class ArmatureMappingDataUtils
    {
        public static void FindJoints(this ref ArmatureMappingData armatureMapping, in ArmatureData armature, Transform rootTransform)
        {
            Assert.AreEqual(armature.NumJoints, armatureMapping.NumJoints);
            armature.FindJoints(rootTransform, armatureMapping.Transforms);
        }
    }
}
