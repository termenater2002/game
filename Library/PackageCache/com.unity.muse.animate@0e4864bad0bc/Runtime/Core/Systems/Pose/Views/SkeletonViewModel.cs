using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class SkeletonViewModel
    {
        public CameraModel Camera => m_CameraModel;
        public int NumBones => m_Bones.Count;
        
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnStateChanged?.Invoke();
            }
        }

        public delegate void StateChanged();
        public event StateChanged OnStateChanged;
        public delegate void PoseChanged();
        public event PoseChanged OnPoseChanged;
        public event Action<float> OnViewStep;
        struct Bone
        {
            public int ChildIndex;
            public int ParentIndex;
        }

        List<Bone> m_Bones = new ();
        ArmatureStaticPoseModel m_StaticPoseModel;
        CameraModel m_CameraModel;
        bool m_IsVisible;

        public SkeletonViewModel(ArmatureStaticPoseModel staticPoseModel, ArmatureDefinition armatureDefinition, CameraModel cameraModel)
        {
            Assert.IsNotNull(staticPoseModel, "No pose provided");
            Assert.IsNotNull(armatureDefinition, "No armature provided");
            Assert.IsNotNull(cameraModel, "No camera model provided");

            Assert.AreEqual(armatureDefinition.NumJoints, staticPoseModel.NumJoints);
            Assert.IsTrue(staticPoseModel.Type == ArmatureStaticPoseData.PoseType.Global, "Pose must be Global");

            m_CameraModel = cameraModel;

            m_StaticPoseModel = staticPoseModel;
            m_StaticPoseModel.OnChanged += OnPoseModelChanged;

            for (var i = 0; i < armatureDefinition.NumJoints; i++)
            {
                var parentIndex = armatureDefinition.GetJointParentIndex(i);
                if (parentIndex < 0)
                    continue;

                m_Bones.Add(new Bone
                {
                    ChildIndex = i,
                    ParentIndex = parentIndex
                });
            }
        }

        void OnPoseModelChanged(ArmatureStaticPoseModel model)
        {
            OnPoseChanged?.Invoke();
        }

        public void GetBone(int boneIdx, out Vector3 parentPosition, out Vector3 childPosition)
        {
            if (boneIdx < 0 || boneIdx  >= m_Bones.Count)
                AssertUtils.Fail($"Invalid bone index: {boneIdx}");

            var bone = m_Bones[boneIdx];
            parentPosition = m_StaticPoseModel.GetPosition(bone.ParentIndex);
            childPosition = m_StaticPoseModel.GetPosition(bone.ChildIndex);
        }

        public void Step(float delta)
        {
            OnViewStep?.Invoke(delta);
        }
    }
}
