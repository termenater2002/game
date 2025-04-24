using System;
using System.Collections.Generic;
using Unity.DeepPose.Components;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class SingleMotionCompletionRequest : IProcessingRequest
    {
        const int k_SyncProgressEachNthLayer = 100;
        const int k_NumInternalSteps = 3;

        public IProcessingRequest.ProcessState State => m_State;
        public float Progress => m_Progress;
        public float WaitDelay => 0f;
        public DateTime WaitStartTime => DateTime.Now;
        
        public struct Settings
        {
            public MotionCompletionComponent MotionCompletionComponent;
            public ArmatureMappingComponent MotionArmature;
        }

        public struct Input
        {
            public List<BakedArmaturePoseModel> Poses;
            public List<int> Indices;
            public int ReferenceIndex;
        }

        public struct Output
        {
            public List<BakedArmaturePoseModel> Poses;
            public List<int> Indices;
        }

        enum InternalStep
        {
            Unknown,
            CapturingInput,
            RunningModel,
            CapturingOutput,
            Done
        }

        IProcessingRequest.ProcessState m_State;
        float m_Progress;

        Settings m_Settings;
        Input m_Input;
        Output m_Output;
        InternalStep m_InternalStep;
        int m_InternalStepIndex;

        public SingleMotionCompletionRequest(Settings settings, Input input, Output output)
        {
            m_State = IProcessingRequest.ProcessState.Unknown;
            m_Progress = 0f;

            Assert.IsNotNull(settings.MotionArmature, "No motion armature specified");
            Assert.IsNotNull(settings.MotionCompletionComponent, "No motion completion component specified");

            Assert.IsNotNull(input.Indices, "No input indices");
            Assert.IsTrue(input.Poses != null && input.Poses.Count > 0, "No input poses");
            Assert.AreEqual(input.Poses.Count, input.Indices.Count);
            Assert.IsTrue(input.Poses.Count > 0, "No input pose");
            Assert.IsTrue(input.ReferenceIndex >= 0 && input.ReferenceIndex < input.Indices.Count);

            Assert.IsNotNull(output.Indices, "No output indices");
            Assert.IsTrue(output.Poses != null && output.Poses.Count > 0, "No output poses");
            Assert.AreEqual(output.Poses.Count, output.Indices.Count);

            m_Settings = settings;
            m_Input = input;
            m_Output = output;

            m_InternalStep = InternalStep.Unknown;
            m_InternalStepIndex = 0;
        }

        public void Start()
        {
            Assert.IsTrue( m_State == IProcessingRequest.ProcessState.Unknown, "Request already started");

            m_Settings.MotionCompletionComponent.SetFrameTimes(m_Input.Indices, m_Input.ReferenceIndex, m_Output.Indices);

            m_State = IProcessingRequest.ProcessState.InProgress;
            m_InternalStep = InternalStep.CapturingInput;
            m_InternalStepIndex = 0;
        }

        public void Stop()
        {
            if (State != IProcessingRequest.ProcessState.InProgress)
                return;

            if (m_Settings.MotionCompletionComponent.IsRunningAsyncSolve)
                m_Settings.MotionCompletionComponent.StopAsyncSolve();

            m_State = IProcessingRequest.ProcessState.Failed;
        }

        public void Step()
        {
            switch (m_InternalStep)
            {
                case InternalStep.Unknown:
                    throw new Exception("Request not started");

                case InternalStep.CapturingInput:
                    StepCapturingInput();
                    break;

                case InternalStep.RunningModel:
                    StepRunningModel();
                    break;

                case InternalStep.CapturingOutput:
                    StepCapturingOutput();
                    break;

                case InternalStep.Done:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool CanSkipToNextFrame => false;

        void StepCapturingInput()
        {
            var inputPose = m_Input.Poses[m_InternalStepIndex];
            inputPose.ApplyTo(m_Settings.MotionArmature.ArmatureMappingData);

            m_Settings.MotionCompletionComponent.CapturePose(m_InternalStepIndex);

            // Go to next frame
            m_InternalStepIndex++;
            m_Progress += (1f / m_Input.Indices.Count) / k_NumInternalSteps;

            if (m_InternalStepIndex >= m_Input.Indices.Count)
            {
                m_Settings.MotionCompletionComponent.StartAsyncSolve(k_SyncProgressEachNthLayer);
                m_InternalStep = InternalStep.RunningModel;
                m_InternalStepIndex = 0;
            }
        }

        void StepRunningModel()
        {
            if (m_Settings.MotionCompletionComponent.StepAsyncSolve())
            {
                m_Progress = 2f / k_NumInternalSteps;

                m_InternalStep = InternalStep.CapturingOutput;
                m_InternalStepIndex = 0;
            }
            else
            {
                m_Progress = (1f + m_Settings.MotionCompletionComponent.AsyncSolveProgress) / k_NumInternalSteps;
            }
        }

        void StepCapturingOutput()
        {
            var frameIndex = m_Output.Indices[m_InternalStepIndex];
            m_Settings.MotionCompletionComponent.Apply(frameIndex);

            var outputPose = m_Output.Poses[m_InternalStepIndex];
            outputPose.Capture(m_Settings.MotionArmature.ArmatureMappingData);

            // Go to next frame
            m_InternalStepIndex++;
            m_Progress += (1f / m_Output.Indices.Count) / k_NumInternalSteps;

            if (m_InternalStepIndex >= m_Output.Indices.Count)
            {
                m_InternalStep = InternalStep.Done;
                m_State = IProcessingRequest.ProcessState.Done;
                m_Progress = 1f;
            }
        }
    }
}
