using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class ActorDefinitionComponent : MonoBehaviour
    {
        public ArmatureMappingComponent ReferenceViewArmature => m_ViewArmature;
        public ArmatureMappingComponent ReferencePosingArmature => m_PosingArmature;
        public ArmatureMappingComponent ReferencePhysicsArmature => m_PhysicsArmature;
        public ArmatureMappingComponent ReferenceMotionArmature => m_MotionArmature;
        public ArmatureMappingComponent ReferenceTextToMotionArmature => m_TextToMotionArmature;

        public ArmatureToArmatureMapping PosingToCharacterArmatureMapping => m_PosingToCharacterArmatureMapping;
        public JointMask EvaluationJointMask => m_EvaluationJointMask;

        public int NumJoints => PosingToCharacterArmatureMapping.TargetArmature.NumJoints;

        [SerializeField]
        ArmatureMappingComponent m_ViewArmature;

        [SerializeField]
        ArmatureMappingComponent m_PosingArmature;

        [SerializeField]
        ArmatureMappingComponent m_PhysicsArmature;

        [SerializeField]
        ArmatureMappingComponent m_MotionArmature;

        [SerializeField]
        ArmatureMappingComponent m_TextToMotionArmature;

        [SerializeField]
        ArmatureToArmatureMapping m_PosingToCharacterArmatureMapping;

        [SerializeField, Tooltip("Joints to use for evaluating errors in recovered motion.")]
        JointMask m_EvaluationJointMask;

        void Awake()
        {
            // Hell yeah
            m_PhysicsArmature.gameObject.AddComponent<PhysicsJointFixer>();
        }
    }
}
