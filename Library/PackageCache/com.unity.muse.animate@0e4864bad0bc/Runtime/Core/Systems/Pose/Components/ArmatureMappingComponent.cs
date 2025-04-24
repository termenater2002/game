using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class ArmatureMappingComponent : MonoBehaviour
    {
        [SerializeField]
        ArmatureDefinition m_ArmatureDefinition;

        [SerializeField]
        Transform m_ArmatureRoot;

        [SerializeField]
        ArmatureMappingData m_ArmatureMappingData;

        public Transform RootTransform => m_ArmatureRoot;
        public int NumJoints => m_ArmatureMappingData.NumJoints;

        public ArmatureMappingData ArmatureMappingData => m_ArmatureMappingData;
        public ArmatureDefinition ArmatureDefinition => m_ArmatureDefinition;

        public bool IsValid => ArmatureDefinition != null && ArmatureDefinition.IsValid && ArmatureMappingData.IsValid && ArmatureMappingData.NumJoints == ArmatureDefinition.NumJoints;

        public Transform GetJointTransform(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= m_ArmatureMappingData.NumJoints)
                AssertUtils.Fail($"Invalid joint index: {jointIndex}");

            return m_ArmatureMappingData.Transforms[jointIndex];
        }

        public ArmatureMappingComponent Clone(int moveToLayer = -1, bool includeChildrenInLayer = true)
        {
            var parentTransform = transform.parent;
            Assert.IsNotNull(parentTransform, "Could not find parent");

            var go = Instantiate(gameObject, parentTransform, true);

            if (moveToLayer >= 0)
                go.SetLayer(moveToLayer, includeChildrenInLayer);

            go.SetActive(true);

            var armatureComponent = go.GetComponent<ArmatureMappingComponent>();
            return armatureComponent;
        }
    }
}
