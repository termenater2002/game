using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class SingleLinearInterpolationRequest : IProcessingRequest
    {
        public IProcessingRequest.ProcessState State => m_State;
        public float Progress => m_Progress;
        public float WaitDelay => 0f;
        public DateTime WaitStartTime => DateTime.Now;
        public struct Settings
        {
            public ArmatureMappingComponent MotionArmature;
        }

        public struct Input
        {
            public List<BakedArmaturePoseModel> Poses;
            public List<int> Indices;
        }

        public struct Output
        {
            public List<BakedArmaturePoseModel> Poses;
            public List<int> Indices;
        }

        IProcessingRequest.ProcessState m_State;
        float m_Progress;

        Settings m_Settings;
        Input m_Input;
        Output m_Output;
        int m_InternalStepIndex;

        public SingleLinearInterpolationRequest(Settings settings, Input input, Output output)
        {
            m_State = IProcessingRequest.ProcessState.Unknown;
            m_Progress = 0f;

            Assert.IsNotNull(settings.MotionArmature, "No motion armature specified");

            Assert.IsNotNull(input.Indices, "No input indices");
            Assert.IsTrue(input.Poses != null && input.Poses.Count > 0, "No input poses");
            Assert.AreEqual(input.Poses.Count, input.Indices.Count);
            Assert.IsTrue(input.Poses.Count > 0, "No input pose");

            Assert.IsNotNull(output.Indices, "No output indices");
            Assert.IsTrue(output.Poses != null && output.Poses.Count > 0, "No output poses");
            Assert.AreEqual(output.Poses.Count, output.Indices.Count);

            m_Settings = settings;
            m_Input = input;
            m_Output = output;

            m_InternalStepIndex = 0;
        }

        public void Start()
        {
            Assert.IsTrue( m_State == IProcessingRequest.ProcessState.Unknown, "Request already started");

            m_State = IProcessingRequest.ProcessState.InProgress;
            m_InternalStepIndex = 0;
            m_Progress = 0f;
        }

        public void Stop()
        {
            if (State != IProcessingRequest.ProcessState.InProgress)
                return;

            m_State = IProcessingRequest.ProcessState.Failed;
        }

        public void Step()
        {
            var frameIndex = m_Output.Indices[m_InternalStepIndex];
            FindInterpolationPoses(frameIndex, out var prevPose, out var nextPose, out var interpolationT);

            // interpolate local poses
            var outputPose = m_Output.Poses[m_InternalStepIndex];
            outputPose.LocalPose.Interpolate(prevPose.LocalPose, nextPose.LocalPose, interpolationT);

            // Synchronize local & global poses
            outputPose.LocalPose.ApplyTo(m_Settings.MotionArmature.ArmatureMappingData);
            outputPose.Capture(m_Settings.MotionArmature.ArmatureMappingData);

            // Go to next frame
            m_InternalStepIndex++;
            m_Progress += 1f / m_Output.Indices.Count;

            // Done
            if (m_InternalStepIndex >= m_Output.Indices.Count)
            {
                m_State = IProcessingRequest.ProcessState.Done;
                m_Progress = 1f;
            }
        }

        public bool CanSkipToNextFrame => false;

        void FindInterpolationPoses(int frameIndex, out BakedArmaturePoseModel prevPose, out BakedArmaturePoseModel nextPose, out float interpolationT)
        {
            var prevFrameIndex = int.MinValue;
            var nextFrameIndex = int.MaxValue;
            prevPose = null;
            nextPose = null;
            for (var i = 0; i < m_Input.Indices.Count; i++)
            {
                var idx = m_Input.Indices[i];
                if (idx <= frameIndex && idx > prevFrameIndex)
                {
                    prevFrameIndex = idx;
                    prevPose = m_Input.Poses[i];
                }

                if (idx >= frameIndex && idx < nextFrameIndex)
                {
                    nextFrameIndex = idx;
                    nextPose = m_Input.Poses[i];
                }
            }

            if (prevPose == null)
                throw new Exception($"Could not find previous input index for frame: {frameIndex}");
            if (nextPose == null)
                throw new Exception($"Could not find next input index for frame: {frameIndex}");

            interpolationT = (frameIndex - prevFrameIndex) / (float)(nextFrameIndex - prevFrameIndex);
        }
    }
}
