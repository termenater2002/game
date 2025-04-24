using System;
using System.Linq;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Defines an abstract armature
    /// </summary>
    [Serializable]
    struct ArmatureData : IEquatable<ArmatureData>
    {
        [Serializable]
        public struct Joint
        {
            public string Name;
            public string Path;
            public int ParentIndex;
        }

        public Joint[] Joints;
        public int NumJoints => Joints.Length;

        public bool IsValid
        {
            get
            {
                if (Joints == null)
                    return false;

                for (var i = 0; i < Joints.Length; i++)
                {
                    if (string.IsNullOrEmpty(Joints[i].Name))
                        return false;
                }

                return true;
            }
        }

        public ArmatureData(int numJoints)
        {
            Joints = new Joint[numJoints];
        }

        public ArmatureData(ArmatureData other)
            : this(other.NumJoints)
        {
            other.CopyTo(ref this);
        }

        public void CopyTo(ref ArmatureData other)
        {
            Assert.AreEqual(NumJoints, other.NumJoints);
            Joints.CopyTo(other.Joints, 0);
        }

        public int GetJointIndex(string jointName, bool cleanupNames = false)
        {
            if (cleanupNames)
                jointName = CleanJointName(jointName);

            for (var i = 0; i < Joints.Length; i++)
            {
                var candidateJointName = Joints[i].Name;
                if (cleanupNames)
                    candidateJointName = CleanJointName(candidateJointName);

                if (candidateJointName == jointName)
                    return i;
            }

            return -1;
        }

        static string CleanJointName(string name)
        {
            name = name.ToLower();
            name = name.Replace(".", "");
            name = name.Replace("_", "");
            name = name.Replace(" ", "");
            name = name.Replace("right", "r");
            name = name.Replace("left", "l");
            return name;
        }

        public bool Equals(ArmatureData other) => Joints.SequenceEqual(other.Joints);

        public override bool Equals(object obj)
        {
            return obj is ArmatureData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Joints != null ? Joints.GetHashCode() : 0);
        }
    }
}
