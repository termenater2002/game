using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class PropDefinitionComponent : MonoBehaviour
    {
        public ArmatureMappingComponent ReferenceViewArmature => m_ViewArmature;
        public ArmatureMappingComponent ReferencePosingArmature => m_PosingArmature;
        public ArmatureMappingComponent ReferencePhysicsArmature => m_PhysicsArmature;
        public int NumJoints => m_PosingArmature.NumJoints;

        [SerializeField]
        ArmatureMappingComponent m_ViewArmature;

        [SerializeField]
        ArmatureMappingComponent m_PosingArmature;

        [SerializeField]
        ArmatureMappingComponent m_PhysicsArmature;
    }
}
