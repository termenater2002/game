using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "ArmatureToArmatureMapping", menuName = "Muse Animate Dev/Armature-to-Armature Mapping")]
#endif
    class ArmatureToArmatureMapping : ScriptableObject
    {
        public ArmatureDefinition SourceArmature => m_SourceArmature;
        public ArmatureDefinition TargetArmature => m_TargetArmature;
        public ArmatureToArmatureMappingData MappingData => m_MappingData;

        public int NumSourceJoints => m_MappingData.NumSourceJoints;
        public int NumTargetJoints => m_TargetArmature.NumJoints;

        public bool IsValid => m_SourceArmature != null
            && m_TargetArmature != null
            && m_SourceArmature.IsValid
            && m_TargetArmature.IsValid
            && m_MappingData.IsValid
            && m_SourceArmature.NumJoints == m_MappingData.NumSourceJoints;

        [SerializeField]
        ArmatureDefinition m_SourceArmature;

        [SerializeField]
        ArmatureDefinition m_TargetArmature;

        [SerializeField]
        ArmatureToArmatureMappingData m_MappingData;

        public int GetTargetJointIndex(int sourceJointIndex)
        {
            return m_MappingData.GetTargetJointIndex(sourceJointIndex);
        }
        
        public bool TryGetTargetJointIndex(int sourceJointIndex, out int targetJointIndex)
        {
            targetJointIndex = -1;
            if (sourceJointIndex < 0 || sourceJointIndex >= NumSourceJoints)
                return false;
            targetJointIndex = m_MappingData.GetTargetJointIndex(sourceJointIndex);
            return targetJointIndex >= 0;
        }

        public void FindTargetJoints()
        {
            if (SourceArmature == null || TargetArmature == null)
                return;

            m_MappingData.FindTargetJoints(SourceArmature.Armature, TargetArmature.Armature);
        }
    }
}
