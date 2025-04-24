using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// This component is required on ragdolls because Unity somehow reset axis computation when joint is disabled then enabled again...
    /// - Insert face palm here -
    /// See this thread for instance: https://forum.unity.com/threads/hinge-joint-limits-resets-on-activate-object.483481/
    /// </summary>
    [DefaultExecutionOrder(-100)]
    class PhysicsJointFixer : MonoBehaviour
    {
        struct JointRotation
        {
            public Quaternion ConnectedBody;
            public Quaternion Joint;
        }

        Joint[] m_AllJoints;
        JointRotation[] m_InitialRotations;
        JointRotation[] m_TargetRotations;

        void Awake()
        {
            m_AllJoints = GetComponentsInChildren<Joint>(true);
            m_InitialRotations = new JointRotation[m_AllJoints.Length];
            m_TargetRotations = new JointRotation[m_AllJoints.Length];

            CaptureInitialRotation();
        }

        void OnEnable()
        {
            CaptureRotation(m_AllJoints, m_TargetRotations);
            ApplyRotation(m_AllJoints, m_InitialRotations);

            for (var i = 0; i < m_AllJoints.Length; i++)
            {
                var joint = m_AllJoints[i];
                joint.axis = joint.axis; // Note: this trick will re-trigger some axis calculation
            }

            ApplyRotation(m_AllJoints, m_TargetRotations);
        }

        public void CaptureInitialRotation()
        {
            CaptureRotation(m_AllJoints, m_InitialRotations);
        }

        static void CaptureRotation(Joint[] joints, JointRotation[] destination)
        {
            for (var i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];
                var rotation = new JointRotation
                {
                    ConnectedBody = joint.connectedBody.transform.localRotation,
                    Joint = joint.transform.localRotation
                };
                destination[i] = rotation;
            }
        }

        static void ApplyRotation(Joint[] joints, JointRotation[] source)
        {
            for (var i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];
                var rotation = source[i];

                joint.connectedBody.transform.localRotation = rotation.ConnectedBody;
                joint.transform.localRotation = rotation.Joint;
            }
        }
    }
}
