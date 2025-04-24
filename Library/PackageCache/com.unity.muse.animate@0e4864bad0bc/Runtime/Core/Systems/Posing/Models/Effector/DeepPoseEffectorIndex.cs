using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines the index of an effector in a specific posing component
    /// </summary>
    [Serializable]
    struct DeepPoseEffectorIndex : IEquatable<DeepPoseEffectorIndex>
    {
        public int PositionIndex;
        public int RotationIndex;
        public int LookAtIndex;

        public bool HasPosition => PositionIndex >= 0;
        public bool HasRotation => RotationIndex >= 0;
        public bool HasLookAt => LookAtIndex >= 0;

        public bool IsValid => (HasPosition && !HasLookAt) || (HasRotation && !HasLookAt) || (HasLookAt && !HasPosition && !HasRotation);

        public static DeepPoseEffectorIndex Invalid = new DeepPoseEffectorIndex(-1, -1, -1);

        public DeepPoseEffectorIndex(int positionIndex, int rotationIndex, int lookAtIndex)
        {
            PositionIndex = positionIndex;
            RotationIndex = rotationIndex;
            LookAtIndex = lookAtIndex;
        }

        public static DeepPoseEffectorIndex PositionEffector(int positionIndex)
        {
            var effectorIndex = Invalid;
            effectorIndex.PositionIndex = positionIndex;
            return effectorIndex;
        }

        public static DeepPoseEffectorIndex RotationEffector(int rotationIndex)
        {
            var effectorIndex = Invalid;
            effectorIndex.RotationIndex = rotationIndex;
            return effectorIndex;
        }

        public static DeepPoseEffectorIndex LookAtEffector(int lookAtIndex)
        {
            var effectorIndex = Invalid;
            effectorIndex.LookAtIndex = lookAtIndex;
            return effectorIndex;
        }

        public bool Equals(DeepPoseEffectorIndex other)
        {
            return PositionIndex == other.PositionIndex && RotationIndex == other.RotationIndex && LookAtIndex == other.LookAtIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is DeepPoseEffectorIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionIndex, RotationIndex, LookAtIndex);
        }

        public static bool operator ==(DeepPoseEffectorIndex left, DeepPoseEffectorIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DeepPoseEffectorIndex left, DeepPoseEffectorIndex right)
        {
            return !left.Equals(right);
        }
    }
}
