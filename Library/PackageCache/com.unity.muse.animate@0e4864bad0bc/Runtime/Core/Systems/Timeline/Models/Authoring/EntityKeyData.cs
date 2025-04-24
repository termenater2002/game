using System;

namespace Unity.Muse.Animate
{
    [Serializable]
    struct EntityKeyData
    {
        public PosingModel Posing;
        public ArmatureStaticPoseModel LocalPose;
        public ArmatureStaticPoseModel GlobalPose;
    }
}
