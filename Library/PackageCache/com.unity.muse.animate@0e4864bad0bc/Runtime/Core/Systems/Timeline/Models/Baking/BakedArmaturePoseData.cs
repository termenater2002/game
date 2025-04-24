using System;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct BakedArmaturePoseData
    {
        public ArmatureStaticPoseModel LocalPose;
        public ArmatureStaticPoseModel GlobalPose;
        public ArmaturePhysicsPoseModel PhysicsState;
    }
}
