using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class MultiMotionCompletionRequest : IProcessingRequest
    {
        public IProcessingRequest.ProcessState State => m_State;
        public float Progress => m_Progress;
        public float WaitDelay => 0f;
        public DateTime WaitStartTime => DateTime.Now;
        public struct Input
        {
            public List<BakedFrameModel> Frames;
            public List<int> Indices;
            public int ReferenceIndex;
        }

        public struct Output
        {
            public List<BakedFrameModel> Frames;
            public List<int> Indices;
        }

        IProcessingRequest.ProcessState m_State;
        float m_Progress;

        Dictionary<EntityID, IProcessingRequest> m_Requests;

        public MultiMotionCompletionRequest(Dictionary<EntityID, SingleMotionCompletionRequest.Settings> settings, Input input, Output output)
        {
            // TODO: remove memory allocations
            m_Requests = new Dictionary<EntityID, IProcessingRequest>();
            foreach (var pair in settings)
            {
                var inputPoses = new List<BakedArmaturePoseModel>();
                foreach (var frame in input.Frames)
                {
                    frame.TryGetModel(pair.Key, out var pose);
                    inputPoses.Add(pose);
                }

                var outputPoses = new List<BakedArmaturePoseModel>();
                foreach (var frame in output.Frames)
                {
                    frame.TryGetModel(pair.Key, out var pose);
                    outputPoses.Add(pose);
                }

                if (pair.Value.MotionCompletionComponent == null)
                {
                    // Linear interpolation fallback
                    var entityInput = new SingleLinearInterpolationRequest.Input
                    {
                        Indices = input.Indices,
                        Poses = inputPoses
                    };

                    var entityOutput = new SingleLinearInterpolationRequest.Output
                    {
                        Indices = output.Indices,
                        Poses = outputPoses
                    };

                    var entitySettings = new SingleLinearInterpolationRequest.Settings
                    {
                        MotionArmature = pair.Value.MotionArmature
                    };

                    m_Requests[pair.Key] = new SingleLinearInterpolationRequest(entitySettings, entityInput, entityOutput);
                }
                else
                {
                    var entityInput = new SingleMotionCompletionRequest.Input
                    {
                        Indices = input.Indices,
                        ReferenceIndex = input.ReferenceIndex,
                        Poses = inputPoses
                    };

                    var entityOutput = new SingleMotionCompletionRequest.Output
                    {
                        Indices = output.Indices,
                        Poses = outputPoses
                    };

                    m_Requests[pair.Key] = new SingleMotionCompletionRequest(pair.Value, entityInput, entityOutput);
                }
            }

            m_State = IProcessingRequest.ProcessState.Unknown;
            m_Progress = 0f;
        }

        public void Start()
        {
            Assert.IsTrue( m_State == IProcessingRequest.ProcessState.Unknown, "Request already started");

            foreach (var pair in m_Requests)
            {
                pair.Value.Start();
            }
            m_State = IProcessingRequest.ProcessState.InProgress;
        }

        public void Stop()
        {
            if (State != IProcessingRequest.ProcessState.InProgress)
                return;

            foreach (var pair in m_Requests)
            {
                var request = pair.Value;
                request.Stop();
            }

            m_State = IProcessingRequest.ProcessState.Failed;
            m_Requests.Clear();
        }

        public void Step()
        {
            var allDone = true;

            m_Progress = 0f;

            foreach (var pair in m_Requests)
            {
                m_Progress += pair.Value.Progress / m_Requests.Count;

                if (pair.Value.State != IProcessingRequest.ProcessState.InProgress)
                    continue;

                pair.Value.Step();

                if (pair.Value.State == IProcessingRequest.ProcessState.InProgress)
                    allDone = false;
            }

            if (allDone)
            {
                m_State = IProcessingRequest.ProcessState.Done;
                m_Progress = 1f;
                m_Requests.Clear();
            }
        }

        public bool CanSkipToNextFrame => false;
    }
}
