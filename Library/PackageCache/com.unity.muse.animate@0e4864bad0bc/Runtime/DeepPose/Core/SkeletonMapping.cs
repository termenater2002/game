using System;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    class SkeletonMapping : MonoBehaviour
    {
        public Transform SkeletonRoot
        {
            get => m_SkeletonRoot;
            set => m_SkeletonRoot = value;
        }

        public SkeletonDefinition SkeletonDefinition
        {
            get => m_SkeletonDefinition;
            set
            {
                if (m_SkeletonDefinition == value)
                    return;

                m_SkeletonDefinition = value;
                m_Joints = new Transform[Skeleton?.Count ?? 0];
            }
        }

        public Transform[] Joints => m_Joints;

        public Skeleton Skeleton => m_SkeletonDefinition != null ? m_SkeletonDefinition.Skeleton : null;

        [SerializeField]
        Transform m_SkeletonRoot;

        [SerializeField]
        SkeletonDefinition m_SkeletonDefinition;

        [SerializeField]
        Transform[] m_Joints = Array.Empty<Transform>();

        public void FindJoints()
        {
            var skeleton = Skeleton;
            if (skeleton == null)
            {
                m_Joints = Array.Empty<Transform>();
                return;
            }

            if (m_Joints.Length != skeleton.Count)
                m_Joints = new Transform[skeleton.Count];

            var rootTransform = m_SkeletonRoot != null ? m_SkeletonRoot : SkeletonUtils.FindRootTransform(gameObject);
            var jointTransforms = skeleton.FindTransforms(rootTransform);

            for (var i = 0; i < m_Joints.Length; i++)
            {
                var joint = skeleton.FindJoint(i);
                if (joint == null)
                    continue;

                m_Joints[i] = jointTransforms.TryGetValue(joint, out var jointTransform) ? jointTransform : null;
            }
        }

        public bool IsValid
        {
            get
            {
                for (var i = 0; i < m_Joints.Length; i++)
                {
                    if (m_Joints[i] == null)
                        return false;
                }

                return true;
            }
        }
    }
}
