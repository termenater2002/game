using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "Armature", menuName = "Muse Animate Dev/Armature")]
#endif
    class ArmatureDefinition : ScriptableObject
    {
        public ArmatureData Armature
        {
            get => m_Armature;
            set => m_Armature = value;
        }

        public int NumJoints => m_Armature.NumJoints;

        [SerializeField]
        ArmatureData m_Armature;

        public bool IsValid => m_Armature.IsValid;

        public string GetJointName(int index)
        {
            var joint = GetJoint(index);
            return joint.Name;
        }

        public string GetJointPath(int index)
        {
            var joint = GetJoint(index);
            return joint.Path;
        }

        public int GetJointParentIndex(int index)
        {
            var joint = GetJoint(index);
            return joint.ParentIndex;
        }

        ArmatureData.Joint GetJoint(int index)
        {
            if (index < 0 || index >= m_Armature.NumJoints)
                AssertUtils.Fail($"Invalid joint index: {index.ToString()}");
            return m_Armature.Joints[index];
        }

        public bool Equals(ArmatureDefinition other)
        {
            return m_Armature.Equals(other.m_Armature);
        }
    }
}
