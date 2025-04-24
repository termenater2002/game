using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines the index of an effector in a specific posing component and for a specific armature
    /// </summary>
    [Serializable]
    struct ArmatureEffectorIndex : IEquatable<ArmatureEffectorIndex>
    {
        public DeepPoseEffectorIndex EffectorIndex;
        public int JointIndex;

        public bool IsValid => EffectorIndex.IsValid && JointIndex >= 0;

        public static ArmatureEffectorIndex Invalid = new ArmatureEffectorIndex(DeepPoseEffectorIndex.Invalid, -1);

        public ArmatureEffectorIndex(DeepPoseEffectorIndex effectorIndex, int jointIndex)
        {
            EffectorIndex = effectorIndex;
            JointIndex = jointIndex;
        }

        public bool Equals(ArmatureEffectorIndex other)
        {
            return EffectorIndex == other.EffectorIndex && JointIndex == other.JointIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is ArmatureEffectorIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EffectorIndex, JointIndex);
        }

        public static bool operator ==(ArmatureEffectorIndex left, ArmatureEffectorIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArmatureEffectorIndex left, ArmatureEffectorIndex right)
        {
            return !left.Equals(right);
        }
    }
}
