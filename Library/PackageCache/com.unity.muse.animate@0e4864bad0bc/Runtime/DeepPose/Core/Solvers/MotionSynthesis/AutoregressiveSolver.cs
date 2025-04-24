using System;
using System.Collections.Generic;
using Unity.DeepPose.ModelBackend;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class AutoregressiveSolver : IDisposable
    {
        private const int k_MaxSequenceLength = 256;

        public bool IsCreated { get; private set; }

        public PoseSequence SolvedSequence => m_Sequence;

        public bool IsSolving => m_ASyncRunInfo.IsSolving;
        public float SolveProgress => m_ASyncRunInfo.Progress;

        public bool InputFkPositions;

        int m_NumJoints;
        int m_NumContacts;
        int m_NumRaycasts;
        int[] m_RaycastsIndices;
        float m_RaycastClampValue;
        bool m_TransposeOrtho6D;
        bool m_UsesRayCasts;
        bool m_InputAllPositions;

        AutoregressiveSolverData m_SolverData;
        IModelBackend m_Backend;
        Transform[] m_Joints;
        PoseSequence m_Sequence;

        bool m_InputDirty;
        int[] m_EndJoints;

        struct ASyncRunInfo
        {
            public bool IsSolving;
            public float Progress;

            public int FirstFrame;
            public int LastFrame;
            public int NumPastFrames;
            public int NumTargetFrames;
            public int CurrentTargetIdx;
            public int ApplyFrameIdx;
            public int SyncProgressEachNthLayer;
        }

        ASyncRunInfo m_ASyncRunInfo;

        public void Initialize(in AutoregressiveSolverSettings settings)
        {
            Assert.IsFalse(IsCreated, "Already initialized");
            Assert.IsTrue(settings.IsValid, "Invalid settings");

            var modelDefinition = new ModelDefinition(settings.Config.Model);
            m_Backend = new ModelBackend.ModelBackend(modelDefinition, settings.Config.BarracudaBackend);

            m_TransposeOrtho6D = settings.Config.TransposeOrtho6D;
            m_NumJoints = settings.Config.Skeleton.Count;
            m_NumContacts = settings.Config.PredictsFootContacts ? settings.Config.ContactJoints.Length : 0;
            var contactIndices = settings.Config.PredictsFootContacts ? settings.Config.ContactJoints : null;
            m_UsesRayCasts = settings.Config.UseRaycasts;
            m_NumRaycasts = m_UsesRayCasts ? settings.Config.RaycastJoints.Length : 0;
            m_RaycastsIndices = m_UsesRayCasts ? settings.Config.RaycastJoints : null;
            m_RaycastClampValue = settings.Config.RaycastClampValue;
            m_EndJoints = settings.Config.Skeleton.GetEndJointIndices();
            var useTolerance= settings.Config.HasTolerance;
            m_InputAllPositions = settings.Config.InputAllPositions;
            var outputAllPositions = settings.Config.OutputAllPositions;
            InputFkPositions = settings.InputFkPositions;

            m_Sequence = new PoseSequence(m_NumJoints, contactIndices, m_RaycastsIndices);

            SetJoints(settings.Joints);

            m_SolverData = new AutoregressiveSolverData(k_MaxSequenceLength, m_NumJoints, m_NumContacts, m_NumRaycasts, m_UsesRayCasts, m_TransposeOrtho6D, m_EndJoints, useTolerance, m_InputAllPositions, outputAllPositions);
            m_ASyncRunInfo.IsSolving = false;

            m_InputDirty = true;
            IsCreated = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed without being initialized");

            if (m_ASyncRunInfo.IsSolving)
                StopAsyncSolve();

            if (m_SolverData.IsCreated)
                m_SolverData.Dispose();

            m_Backend?.Dispose();

            m_Sequence.Clear();

            IsCreated = false;
        }

        public void SetJoints(ICollection<Transform> joints)
        {
            Assert.IsTrue(joints.Count == 0 || joints.Count == m_NumJoints, "Wrong number of joints");

            m_Joints = new Transform[joints.Count];
            joints.CopyTo(m_Joints, 0);
        }

        void InsertPredictionToInput(int targetTime)
        {
            var targetIdxInBuffer = m_SolverData.FindInputFrameIndex(targetTime);
            if (targetIdxInBuffer > -1)
            {
                Assert.AreEqual(targetTime, targetIdxInBuffer);
            }
            else
            {
                m_SolverData.InsertFrame(targetTime);
            }

            CopyPredictionsToInput(targetTime);
            m_SolverData.SetInputReferenceFrame(targetTime);
        }

        void CopyPredictionsToInput(int targetTime)
        {
            // We may need FK:
            if (m_UsesRayCasts || (m_InputAllPositions && InputFkPositions))
                ApplySolvedPoseToJoints(0);

            m_SolverData.SetInputFrameTime(targetTime, targetTime);

            var predictedPositions = m_SolverData.GetPredictedFramePositions(0);
            for (var i = 0; i < predictedPositions.Length; i++)
            {
                Vector3 jointPosition;
                if(i > 0 && InputFkPositions)
                    jointPosition = m_Joints[i].position;
                else
                    jointPosition = predictedPositions[i];
                m_SolverData.SetInputPosition(targetTime, i, jointPosition);
            }

            var predictedRotations = m_SolverData.GetPredictedFrameRotations(0);
            for (var i = 0; i < predictedRotations.Length; i++)
            {
                m_SolverData.SetInputRotation(targetTime, i, predictedRotations[i]);
            }

            if (m_UsesRayCasts)
            {

                for (var i = 0; i < m_RaycastsIndices.Length; i++)
                {
                    var raycastDistance = RaycastsUtils.GetRaycastDistance(m_Joints[m_RaycastsIndices[i]].position, m_RaycastClampValue);
                    m_SolverData.SetInputRaycast(targetTime, i, raycastDistance);
                }
            }
        }

        public void Solve(int lastFrameIdx, int frameIdx = -1)
        {
            StartAsyncSolve(lastFrameIdx, frameIdx, 0);
            while (!StepAsyncSolve())
            {
                // Wait for completion
            }
        }

        public void StartAsyncSolve(int lastFrameIdx, int frameIdx = -1, int syncProgressEachNthLayer = 0)
        {
            Assert.IsFalse(m_ASyncRunInfo.IsSolving, "An async solve is already running");

            if (m_UsesRayCasts && m_NumRaycasts > 0)
            {
                Assert.IsTrue(m_Joints.Length > 0, "You need to setup joints transforms to use raycast inputs!");
            }

            var sequenceLength = lastFrameIdx - m_ASyncRunInfo.FirstFrame + 1;
            Assert.IsTrue(sequenceLength < k_MaxSequenceLength, "Max sequence length reached");
            Assert.IsFalse(m_ASyncRunInfo.IsSolving, "An async solve is already running");

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            Assert.IsTrue(m_SolverData.SequenceLength > 0, "No input data provided");

            m_ASyncRunInfo.FirstFrame = m_SolverData.InputFrameIndices[0];
            Assert.IsTrue(lastFrameIdx >= m_ASyncRunInfo.FirstFrame, "Invalid last frame index");

            m_ASyncRunInfo.ApplyFrameIdx = frameIdx;
            m_ASyncRunInfo.SyncProgressEachNthLayer = syncProgressEachNthLayer;

            if (m_ASyncRunInfo.LastFrame != lastFrameIdx)
            {
                m_ASyncRunInfo.LastFrame = lastFrameIdx;
                m_InputDirty = true;
            }

            if (m_InputDirty)
            {
                var frameCount = m_ASyncRunInfo.LastFrame - m_ASyncRunInfo.FirstFrame + 1;
                m_Sequence.Resize(frameCount);

                m_ASyncRunInfo.NumPastFrames = m_SolverData.InputReferenceFrame + 1;
                Assert.IsTrue(GetPastContextSize() >= m_ASyncRunInfo.NumPastFrames, "All input indices from the first one up to the reference frame must be contiguous");
                CapturePastFrames(m_ASyncRunInfo.NumPastFrames);

                var lastPastFrameIdx = m_SolverData.InputFrameIndices[m_ASyncRunInfo.NumPastFrames - 1];
                m_ASyncRunInfo.NumTargetFrames = lastFrameIdx - lastPastFrameIdx;
                m_ASyncRunInfo.Progress = 0f;

                m_InputDirty = false;
            }
            else
            {
                m_ASyncRunInfo.NumTargetFrames = 0;
                m_ASyncRunInfo.Progress = 1f;
            }

            m_ASyncRunInfo.CurrentTargetIdx = 0;
            m_ASyncRunInfo.IsSolving = true;
        }

        public void StopAsyncSolve()
        {
            Assert.IsTrue(m_ASyncRunInfo.IsSolving, "Async solve was not started or is already done");
            m_SolverData.StopAsyncEvaluation();
            m_ASyncRunInfo.IsSolving = false;
        }

        public bool StepAsyncSolve()
        {
            if (m_ASyncRunInfo.CurrentTargetIdx < m_ASyncRunInfo.NumTargetFrames)
            {
                var currentTargetFrameIdx = m_ASyncRunInfo.NumPastFrames + m_ASyncRunInfo.CurrentTargetIdx;
                var currentTargetTime = m_ASyncRunInfo.FirstFrame + currentTargetFrameIdx;

                if (!m_SolverData.IsEvaluating)
                {
                    // Start solver
                    m_SolverData.StartAsyncEvaluation(m_Backend, m_ASyncRunInfo.SyncProgressEachNthLayer);

                    m_ASyncRunInfo.Progress = (float)m_ASyncRunInfo.CurrentTargetIdx / (float)m_ASyncRunInfo.NumTargetFrames;
                }
                else
                {
                    m_ASyncRunInfo.Progress = ((float)m_ASyncRunInfo.CurrentTargetIdx + m_SolverData.AsyncEvaluationProgress) / (float)m_ASyncRunInfo.NumTargetFrames;

                    // Run solver until completion
                    if (!m_SolverData.StepAsyncEvaluation())
                        return false;

                    // Store output
                    CaptureOutputFrame(currentTargetFrameIdx);

                    // Insert prediction if not last frame
                    if (m_ASyncRunInfo.CurrentTargetIdx < m_ASyncRunInfo.NumTargetFrames - 1)
                        InsertPredictionToInput(currentTargetTime);

                    // Process next frame
                    m_ASyncRunInfo.CurrentTargetIdx++;
                }
            }
            else
            {
                // Apply solve result at desired frame
                if (m_Joints.Length > 0 && m_ASyncRunInfo.ApplyFrameIdx >= 0)
                    ApplySolvedPoseToJoints(m_ASyncRunInfo.ApplyFrameIdx);

                // Solving is done
                m_ASyncRunInfo.IsSolving = false;
                m_ASyncRunInfo.Progress = 1f;
            }

            return !m_ASyncRunInfo.IsSolving;
        }

        public void SetInputLength(int newLength)
        {
            m_SolverData.ResizeSequence(newLength);
            m_InputDirty = true;
        }

        public void SetInputTolerance(int jointIdx, float tolerance)
        {
            m_SolverData.SetInputTolerance(jointIdx, tolerance);
            m_InputDirty = true;
        }

        public void RemoveAllInputsBefore(int lastPastFrameIdx, bool inclusive = false)
        {
            var currentIdx = 0;
            while (currentIdx < m_SolverData.SequenceLength)
            {
                var frameIdx = m_SolverData.InputFrameIndices[currentIdx];
                if (frameIdx < lastPastFrameIdx || (frameIdx == lastPastFrameIdx && inclusive))
                {
                    m_SolverData.RemoveFrame(currentIdx);
                    m_InputDirty = true;
                }
                else
                {
                    currentIdx++;
                }
            }
        }

        public void RemoveAllInputsAfter(int firstFutureFrameIdx, bool inclusive = false)
        {
            var currentIdx = 0;
            while (currentIdx < m_SolverData.SequenceLength)
            {
                var frameIdx = m_SolverData.InputFrameIndices[currentIdx];
                if (frameIdx > firstFutureFrameIdx || (frameIdx == firstFutureFrameIdx && inclusive))
                {
                    m_SolverData.RemoveFrame(currentIdx);
                    m_InputDirty = true;
                }
                else
                {
                    currentIdx++;
                }
            }
        }

        public int GetOrInsertFrame(int frameTimeIdx)
        {
            var frameIdx = m_SolverData.FindInputFrameIndex(frameTimeIdx);
            if (frameIdx != -1)
                return frameIdx;

            if (m_SolverData.SequenceLength == 0)
            {
                m_SolverData.ResizeSequence(1);
                m_SolverData.SetInputFrameTime(0, frameTimeIdx);
                m_InputDirty = true;
                return 0;
            }

            frameIdx = m_SolverData.SequenceLength;
            for (var i = 0; i < m_SolverData.SequenceLength; i++)
            {
                if (m_SolverData.InputFrameIndices[i] > frameTimeIdx)
                {
                    frameIdx = i;
                    break;
                }
            }

            m_SolverData.InsertFrame(frameIdx);
            m_SolverData.SetInputFrameTime(frameIdx, frameTimeIdx);
            m_InputDirty = true;

            return frameIdx;
        }

        public void CaptureInputFrame(int inputIdx)
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");

            m_SolverData.SetInputPosition(inputIdx, 0, m_Joints[0].position);
            m_SolverData.SetInputRotation(inputIdx, 0, m_Joints[0].rotation);
            for (var i = 1; i < m_Joints.Length; i++)
            {
                var rotation = m_Joints[i].localRotation;
                m_SolverData.SetInputRotation(inputIdx, i, rotation);

                if (m_InputAllPositions)
                {
                    var position = m_Joints[i].position;
                    m_SolverData.SetInputPosition(inputIdx, i, position);
                }
            }

            if (m_UsesRayCasts && m_NumRaycasts > 0)
            {
                for (var i = 0; i < m_NumRaycasts; i++)
                {
                    var raycastDistance = RaycastsUtils.GetRaycastDistance(m_Joints[m_RaycastsIndices[i]].position, m_RaycastClampValue);
                    m_SolverData.SetInputRaycast(inputIdx, i, raycastDistance);
                }
            }

            m_InputDirty = true;
        }

        public void SetInputPosition(int inputIdx, int positionIdx, Vector3 position)
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputPosition(inputIdx, positionIdx, position);

            m_InputDirty = true;
        }

        public void SetInputRotation(int inputIdx, int rotationIdx, Quaternion rotation)
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputRotation(inputIdx, rotationIdx, rotation);

            m_InputDirty = true;
        }

        public void SetInputRaycast(int inputIdx, int raycastIdx, float raycastDistance)
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputRaycast(inputIdx, raycastIdx, raycastDistance);

            m_InputDirty = true;
        }

        public void SetInputFrameTime(int inputIdx, int timeIdx, bool isReferenceFrame = false)
        {
            if (isReferenceFrame)
                SetInputReferenceFrame(inputIdx);

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputFrameTime(inputIdx, timeIdx);

            m_InputDirty = true;
        }

        public void SetInputReferenceFrame(int inputIdx)
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputReferenceFrame(inputIdx);

            m_InputDirty = true;
        }

        int GetPastContextSize()
        {
            if (m_SolverData.InputFrameIndices.SequenceLength == 0)
                return 0;

            var count = 1;
            var nextIdx = m_SolverData.InputFrameIndices[0];
            for (var i = 1; i < m_SolverData.InputFrameIndices.SequenceLength; i++)
            {
                nextIdx++;
                if (m_SolverData.InputFrameIndices[i] != nextIdx)
                    break;

                count++;
            }

            return count;
        }

        void CaptureOutputFrame(int frameIdx)
        {
            var frame = m_Sequence.GetFrame(frameIdx);
            CaptureSolvedFrame(frame, 0);
        }

        void CaptureSolvedFrame(PoseFrame frame, int frameIdx)
        {
            if (m_Joints.Length > 0)
            {
                ApplySolvedPoseToJoints(frameIdx);
                frame.CaptureTransforms(m_Joints);
            }
            else
            {
                ApplySolvedPoseToFrame(frameIdx, frame);
            }
        }

        void ApplySolvedPoseToFrame(int frameIdx, PoseFrame frame)
        {
            var jointPositions = m_SolverData.GetPredictedFramePositions(frameIdx);
            var jointRotations = m_SolverData.GetPredictedFrameRotations(frameIdx);
            var contacts = m_SolverData.GetPredictedFrameContacts(frameIdx);

            frame.GlobalPositions[0] = jointPositions[0];
            frame.GlobalRotations[0] = frame.LocalRotations[0];
            jointRotations.CopyTo(frame.LocalRotations);
            contacts.CopyTo(frame.Contacts);
        }

        public void ApplySolvedPoseToJoints(int frameIdx)
        {
            var jointPositions = m_SolverData.GetPredictedFramePositions(frameIdx);
            var jointRotations = m_SolverData.GetPredictedFrameRotations(frameIdx);

            m_Joints[0].position = jointPositions[0];
            m_Joints[0].rotation = jointRotations[0];
            for (var i = 1; i < m_Joints.Length; i++)
            {
                m_Joints[i].localRotation = jointRotations[i];
            }
        }

        void ApplyFrameToJoints(PoseFrame frame)
        {
            m_Joints[0].position = frame.GlobalPositions[0];
            m_Joints[0].rotation = frame.LocalRotations[0];
            for (var i = 1; i < m_Joints.Length; i++)
            {
                m_Joints[i].localRotation = frame.LocalRotations[i];
            }
        }

        void CapturePastFrames(int numPastFrames)
        {
            for (var frameIdx = 0; frameIdx < numPastFrames; frameIdx++)
            {
                var frame = m_Sequence.GetFrame(frameIdx);

                frame.GlobalPositions[0].x = m_SolverData.InputPositions[0, frameIdx, 0, 0];
                frame.GlobalPositions[0].y = m_SolverData.InputPositions[0, frameIdx, 0, 1];
                frame.GlobalPositions[0].z = m_SolverData.InputPositions[0, frameIdx, 0, 2];

                for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
                {
                    frame.LocalRotations[jointIdx].w = m_SolverData.InputRotations[0, frameIdx, jointIdx, 0];
                    frame.LocalRotations[jointIdx].x = m_SolverData.InputRotations[0, frameIdx, jointIdx, 1];
                    frame.LocalRotations[jointIdx].y = m_SolverData.InputRotations[0, frameIdx, jointIdx, 2];
                    frame.LocalRotations[jointIdx].z = m_SolverData.InputRotations[0, frameIdx, jointIdx, 3];
                }
            }
        }

        public RaycastsUtils.RaySequence GetRaycastInputs()
        {

            Assert.IsTrue(m_UsesRayCasts && m_NumRaycasts > 0, "Trying to retrieve raycasts but the model does not use raycast inputs.");
            Assert.IsTrue(!m_ASyncRunInfo.IsSolving, "Trying to retrieve raycasts during solve.");

            var raySequence = new RaycastsUtils.RaySequence(m_Sequence.Length);

            var frameIdx = 0;
            foreach (var frame in m_Sequence.Frames)
            {
                var rayFrame = new RaycastsUtils.RayFrame(m_NumRaycasts);

                // No raycast inputs on last frame
                if (frameIdx == m_Sequence.Length - 1)
                {
                    for (var i = 0; i < m_NumRaycasts; i++)
                    {
                        rayFrame.Rays[i].Origin = new Vector3(0f,0f,0f);
                        rayFrame.Rays[i].Distance = 0f;
                    }
                }

                else
                {
                    // Set joints correctly to retrieve ray origins for that frame
                    ApplyFrameToJoints(frame);

                    for (var i = 0; i < m_NumRaycasts; i++)
                    {
                        rayFrame.Rays[i].Origin = m_Joints[m_RaycastsIndices[i]].position;
                        rayFrame.Rays[i].Distance = m_SolverData.InputRaycasts[0, frameIdx, i];
                    }

                    raySequence.RayFrames[frameIdx] = rayFrame;
                    frameIdx++;
                }

            }

            return raySequence;

        }
    }
}
