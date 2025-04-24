using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Components
{
    class ActiveRagdollSetup : MonoBehaviour
    {
        // NOTE: This must match python json format
        [Serializable]
        struct SerializedJointLimit
        {
            public string joint;
            public float twist_low;
            public float twist_high;
            public float swing1;
            public float swing2;
        }

        // NOTE: This must match python json format
        [Serializable]
        struct SerializedData
        {
            public SerializedJointLimit[] limits;
        }

        [Serializable]
        struct JointInfo
        {
            public string Name;
            public Transform Transform;
            public float TwistLow;
            public float TwistHigh;
            public float Swing1;
            public float Swing2;
        }

        [SerializeField]
        Vector3 m_Axis = new Vector3(1f, 0f, 0f);

        [SerializeField]
        Vector3 m_SwingAxis = new Vector3(0f, 0f, -1f);

        [SerializeField]
        Transform m_Root;

        [SerializeField]
        SkeletonDefinition m_SkeletonDefinition;

        [SerializeField]
        List<JointInfo> m_JointInfos = new();

        public void LoadJointLimits(string filepath)
        {
            m_JointInfos.Clear();

            var json = File.ReadAllText(filepath);
            var data = JsonConvert.DeserializeObject<SerializedData>(json);

            if (data.limits == null)
                return;

            foreach (var joint in data.limits)
            {
                var info = new JointInfo();
                info.Name = joint.joint;
                info.TwistLow = joint.twist_low;
                info.TwistHigh = joint.twist_high;
                info.Swing1 = joint.swing1;
                info.Swing2 = joint.swing2;
                info.Transform = null;

                m_JointInfos.Add(info);
            }

            FindJoints();
        }

        public void FindJoints()
        {
            if (m_SkeletonDefinition == null || m_SkeletonDefinition.Skeleton == null)
                return;

            var skeleton = m_SkeletonDefinition.Skeleton;
            var rootTransform = SkeletonUtils.FindRootTransform(m_Root == null ? gameObject : m_Root.gameObject);
            var jointTransforms = skeleton.FindTransforms(rootTransform);

            for (var i = 0; i < m_JointInfos.Count; i++)
            {
                var info = m_JointInfos[i];
                info.Transform = null;

                for (var j = 0; j < skeleton.Count; j++)
                {
                    var joint = skeleton.FindJoint(j);
                    Assert.IsNotNull(joint, $"Cannot find joint with index: {j}");

                    if (joint.Name != info.Name)
                        continue;

                    info.Transform = jointTransforms.TryGetValue(joint, out var jointTransform) ? jointTransform : null;
                    break;
                }

                m_JointInfos[i] = info;
            }
        }

        public void DoSetup()
        {
            HashSet<Transform> allTransforms = new();
            foreach (var info in m_JointInfos)
            {
                if (info.Transform == null)
                    continue;

                allTransforms.Add(info.Transform);
            }

            foreach (var info in m_JointInfos)
            {
                if (info.Transform == null)
                    continue;

                Transform parentTransform = null;
                var currentTransform = info.Transform;
                while (currentTransform.parent != null)
                {
                    currentTransform = currentTransform.parent;
                    if (allTransforms.Contains(currentTransform))
                    {
                        parentTransform = currentTransform;
                        break;
                    }
                }

                var rigidbody = info.Transform.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = info.Transform.gameObject.AddComponent<Rigidbody>();

                SetupRigidBody(rigidbody);

                if (parentTransform == null)
                    continue;

                var joint = info.Transform.GetComponent<CharacterJoint>();
                if (joint == null)
                    joint = info.Transform.gameObject.AddComponent<CharacterJoint>();

                SetupJoint(joint, parentTransform, info, m_Axis, m_SwingAxis);
            }
        }

        public void ConvertToConfigurableJoints()
        {
            ConvertToConfigurableJointsRecursive(m_Root);
        }

        void ConvertToConfigurableJointsRecursive(Transform root)
        {
            if (root == null)
                return;

            var characterJoint = root.GetComponent<CharacterJoint>();
            if (characterJoint != null)
            {
                var configurableJoint = root.GetComponent<ConfigurableJoint>();
                if (configurableJoint == null)
                    configurableJoint = root.gameObject.AddComponent<ConfigurableJoint>();

                characterJoint.ConvertJoint(configurableJoint);

#if UNITY_EDITOR
                DestroyImmediate(characterJoint);
#else
                Destroy(characterJoint);
#endif
            }

            foreach (Transform child in root)
            {
                ConvertToConfigurableJointsRecursive(child);
            }
        }

        void SetupRigidBody(Rigidbody rigidbody)
        {
            rigidbody.isKinematic = true;
            rigidbody.mass = 1f;
#if UNITY_6000_0_OR_NEWER
            rigidbody.linearDamping = 0f;
            rigidbody.angularDamping = 0f;
#else
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
#endif
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;
        }

        void SetupJoint(CharacterJoint joint, Transform parentTransform, JointInfo info, Vector3 axis, Vector3 swingAxis)
        {
            joint.connectedBody = parentTransform.GetComponent<Rigidbody>();
            joint.axis = axis;
            joint.swingAxis = swingAxis;
            joint.autoConfigureConnectedAnchor = true;

            var twistLimitSpring = new SoftJointLimitSpring();
            twistLimitSpring.damper = 0f;
            twistLimitSpring.spring = 0f;
            joint.twistLimitSpring = twistLimitSpring;

            var lowTwistLimit = new SoftJointLimit();
            lowTwistLimit.bounciness = 0f;
            lowTwistLimit.contactDistance = 0f;
            lowTwistLimit.limit = Mathf.Clamp(info.TwistLow, -177f, 177f);
            joint.lowTwistLimit = lowTwistLimit;

            var highTwistLimit = new SoftJointLimit();
            highTwistLimit.bounciness = 0f;
            highTwistLimit.contactDistance = 0f;
            highTwistLimit.limit = Mathf.Clamp(info.TwistHigh, -177f, 177f);
            joint.highTwistLimit = highTwistLimit;

            var swing1Limit = new SoftJointLimit();
            swing1Limit.bounciness = 0f;
            swing1Limit.contactDistance = 0f;
            swing1Limit.limit = Mathf.Min(177f, info.Swing1);
            joint.swing1Limit = swing1Limit;

            var swing2Limit = new SoftJointLimit();
            swing2Limit.bounciness = 0f;
            swing2Limit.contactDistance = 0f;
            swing2Limit.limit = Mathf.Min(177f, info.Swing2);
            joint.swing2Limit = swing2Limit;

            joint.enableProjection = false;
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            joint.massScale = 1f;
            joint.connectedMassScale = 1f;
        }

        void SetupJoint(ConfigurableJoint joint, Transform parentTransform, JointInfo info)
        {
            joint.SetupAsCharacterJoint();

            joint.connectedBody = parentTransform.GetComponent<Rigidbody>();
            joint.axis = new Vector3(1f, 0f, 0f);
            joint.secondaryAxis = new Vector3(0f, 0f, -1f);
            joint.autoConfigureConnectedAnchor = true;

            var twistLimitSpring = new SoftJointLimitSpring();
            twistLimitSpring.damper = 0f;
            twistLimitSpring.spring = 0f;
            joint.angularXLimitSpring = twistLimitSpring;

            var lowTwistLimit = new SoftJointLimit();
            lowTwistLimit.bounciness = 0f;
            lowTwistLimit.contactDistance = 0f;
            lowTwistLimit.limit = Mathf.Clamp(info.TwistLow, -177f, 177f);
            joint.lowAngularXLimit = lowTwistLimit;

            var highTwistLimit = new SoftJointLimit();
            highTwistLimit.bounciness = 0f;
            highTwistLimit.contactDistance = 0f;
            highTwistLimit.limit = Mathf.Clamp(info.TwistHigh, -177f, 177f);
            joint.highAngularXLimit = highTwistLimit;

            var swing1Limit = new SoftJointLimit();
            swing1Limit.bounciness = 0f;
            swing1Limit.contactDistance = 0f;
            swing1Limit.limit = Mathf.Min(177f, info.Swing1);
            joint.angularYLimit = swing1Limit;

            var swing2Limit = new SoftJointLimit();
            swing2Limit.bounciness = 0f;
            swing2Limit.contactDistance = 0f;
            swing2Limit.limit = Mathf.Min(177f, info.Swing2);
            joint.angularZLimit = swing2Limit;

            joint.projectionMode = JointProjectionMode.None;
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            joint.massScale = 1f;
            joint.connectedMassScale = 1f;
        }

        public bool IsValid
        {
            get
            {
                foreach (var info in m_JointInfos)
                {
                    if (info.Transform == null)
                        return false;
                }
                return true;
            }
        }
    }
}
