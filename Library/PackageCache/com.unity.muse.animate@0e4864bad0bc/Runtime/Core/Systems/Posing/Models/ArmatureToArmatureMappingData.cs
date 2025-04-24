using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct ArmatureToArmatureMappingData
    {
        [SerializeField]
        int[] SourceToTarget;

        public int NumSourceJoints => SourceToTarget.Length;

        public bool IsValid => SourceToTarget != null;

        public ArmatureToArmatureMappingData(int numSourceJoints)
        {
            SourceToTarget = new int[numSourceJoints];
            for (var i = 0; i < numSourceJoints; i++)
            {
                SourceToTarget[i] = -1;
            }
        }

        public int GetTargetJointIndex(int sourceJointIndex)
        {
            if (sourceJointIndex < 0 || sourceJointIndex > SourceToTarget.Length)
                AssertUtils.Fail($"Invalid source joint index: {sourceJointIndex.ToString()}");
            return SourceToTarget[sourceJointIndex];
        }

        public bool HasTargetJointIndex(int sourceJointIndex)
        {
            if (sourceJointIndex < 0 || sourceJointIndex > SourceToTarget.Length)
                AssertUtils.Fail($"Invalid source joint index: {sourceJointIndex.ToString()}");
            return SourceToTarget[sourceJointIndex] >= 0;
        }

        public void SetTargetJointIndex(int sourceJointIndex, int targetJointIndex)
        {
            if (sourceJointIndex < 0 || sourceJointIndex > SourceToTarget.Length)
                AssertUtils.Fail($"Invalid source joint index: {sourceJointIndex.ToString()}");
            SourceToTarget[sourceJointIndex] = targetJointIndex;
        }
    }
}
