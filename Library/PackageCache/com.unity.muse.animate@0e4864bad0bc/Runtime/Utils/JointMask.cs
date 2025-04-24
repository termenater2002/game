using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Animate
{
#if UNITY_MUSE_DEV
    [CreateAssetMenu(fileName = "JointMask", menuName = "Muse Animate Dev/Joint Mask")]
#endif
    class JointMask : ScriptableObject
    {
        public ArmatureDefinition Armature => m_Armature;
        
        [SerializeField]
        ArmatureDefinition m_Armature;
        
        [FormerlySerializedAs("m_Mask")]
        [SerializeField, Range(0, 1)]
        float[] m_PositionWeights;
        
        [SerializeField, Range(0, 1)]
        float[] m_RotationWeights;
        
        public IReadOnlyList<float> PositionWeights => m_PositionWeights;
        public IReadOnlyList<float> RotationWeights => m_RotationWeights;

        public void Initialize()
        {
            if (m_PositionWeights.Length == m_Armature.NumJoints && m_RotationWeights.Length == m_Armature.NumJoints)
                return;
            
            m_PositionWeights = new float[m_Armature.NumJoints];
            m_RotationWeights = new float[m_Armature.NumJoints];
            
            for (var i = 0; i < m_Armature.NumJoints; i++)
            {
                m_PositionWeights[i] = 1;
                m_RotationWeights[i] = 1;
            }
        }
        
        public bool IsValid =>
            m_Armature != null &&
            m_Armature.IsValid &&
            m_PositionWeights != null &&
            m_RotationWeights != null &&
            m_PositionWeights.Length == m_Armature.NumJoints &&
            m_RotationWeights.Length == m_Armature.NumJoints;

        public int Count => m_PositionWeights.Length;
    }
}
