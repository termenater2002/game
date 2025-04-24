using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class RetargetingSolver : IDisposable
    {
        public bool IsCreated { get; private set; }
        public bool IsValid { get; private set; }

        HumanPoseHandler m_SourcePoseHandler;
        HumanPoseHandler m_TargetPoseHandler;
        HumanPose m_HumanPose;

        public void Initialize(in RetargetingData data)
        {
            IsValid = false;
            Assert.IsFalse(IsCreated, "Already initialized");

            if (SkeletonUtils.IsValidNonNullHumanAvatar(data.SourceAvatar)
                && SkeletonUtils.IsValidNonNullHumanAvatar(data.TargetAvatar)
                && data.SourceTransform != null
                && data.TargetTransform != null)
            {
                m_SourcePoseHandler = new HumanPoseHandler(data.SourceAvatar, data.SourceTransform);
                m_TargetPoseHandler = new HumanPoseHandler(data.TargetAvatar, data.TargetTransform);
                m_HumanPose = new HumanPose();
                IsValid = true;
            }

            IsCreated = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed without being initialized");

            m_SourcePoseHandler?.Dispose();
            m_TargetPoseHandler?.Dispose();

            IsCreated = false;
            IsValid = false;
        }

        public void Solve(bool inverse = false)
        {
            Assert.IsTrue(IsCreated, "Running solver that was not initialized");

            if (!IsValid)
                return;

            if (m_SourcePoseHandler == null || m_TargetPoseHandler == null)
                return;

            if (inverse)
            {
                m_TargetPoseHandler.GetHumanPose(ref m_HumanPose);
                m_SourcePoseHandler.SetHumanPose(ref m_HumanPose);
            }
            else
            {
                m_SourcePoseHandler.GetHumanPose(ref m_HumanPose);
                m_TargetPoseHandler.SetHumanPose(ref m_HumanPose);
            }
        }
    }
}
