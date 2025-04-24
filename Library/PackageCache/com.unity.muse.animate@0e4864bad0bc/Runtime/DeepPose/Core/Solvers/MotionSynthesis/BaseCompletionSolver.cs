using System;
using System.Collections.Generic;
using Unity.DeepPose.ModelBackend;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    abstract class BaseCompletionSolver : IDisposable
    {
        public bool IsCreated { get; private set; }

        public int NumInputFrames
        {
            get => m_NumInputFrames;
            set
            {
                m_NumInputFrames = value;
                m_DataDirty = true;
            }
        }

        public int NumTargetFrames
        {
            get => m_NumTargetFrames;
            set
            {
                m_NumTargetFrames = value;
                m_DataDirty = true;
            }
        }

        public PoseSequence SolvedSequence => m_Sequence;

        protected int m_NumInputFrames = 0;
        protected int m_NumTargetFrames = 0;
        protected int m_NumJoints;
        protected int m_NumContacts;
        protected bool m_TransposeOrtho6D;

        protected CompletionSolverData m_SolverData;
        protected IModelBackend m_Backend;
        protected Transform[] m_Joints;
        protected PoseSequence m_Sequence;

        bool m_DataDirty;
        bool m_InputDirty;
        protected int[] m_EndJoints;

        protected bool m_IsSolving;
        int m_AsyncFrameIdx;

        public void Initialize(in CompletionSolverSettings settings)
        {
            Assert.IsFalse(IsCreated, "Already initialized");
            Assert.IsTrue(settings.IsValid, "Invalid settings");

            var modelDefinition = new ModelDefinition(settings.Config.Model);
            m_Backend = new ModelBackend.ModelBackend(modelDefinition, settings.Config.BarracudaBackend);

            m_TransposeOrtho6D = settings.Config.TransposeOrtho6D;
            m_NumJoints = settings.Config.Skeleton.Count;
            m_NumContacts = settings.Config.PredictsFootContacts ? settings.Config.ContactJoints.Length : 0;
            var contactIndices = settings.Config.PredictsFootContacts ? settings.Config.ContactJoints : null;
            m_EndJoints = settings.Config.Skeleton.GetEndJointIndices();

            m_Sequence = new PoseSequence(m_NumJoints, contactIndices);

            SetJoints(settings.Joints);

            m_DataDirty = true;
            m_InputDirty = true;
            IsCreated = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed without being initialized");

            StopAsyncSolve();

            if (m_SolverData.IsCreated)
                m_SolverData.Dispose();

            m_Sequence.Clear();

            IsCreated = false;
        }

        public void Solve(int frameIdx = -1)
        {
            Assert.IsFalse(m_IsSolving, "An async solve is already running");

            if (m_InputDirty)
            {
                SolveSequence();
                m_InputDirty = false;
            }

            if (m_Joints.Length > 0 && frameIdx >= 0)
                ApplySolvedPoseToJoints(frameIdx);
        }

        public void StartAsyncSolve(int frameIdx = -1, int syncProgressEachNthLayer = 0)
        {
            Assert.IsFalse(m_IsSolving, "An async solve is already running");
            m_AsyncFrameIdx = frameIdx;

            if (m_InputDirty)
            {
                m_SolverData.StartAsyncEvaluation(m_Backend, syncProgressEachNthLayer);
                m_InputDirty = false;
            }

            m_IsSolving = true;
        }

        public void StopAsyncSolve()
        {
            Assert.IsTrue(m_IsSolving, "Async solve was not started or is already done");
            m_SolverData.StopAsyncEvaluation();
            m_IsSolving = false;
        }

        public bool StepAsyncSolve()
        {
            if (!m_SolverData.IsEvaluating)
            {
                if (m_Joints.Length > 0 && m_AsyncFrameIdx >= 0)
                    ApplySolvedPoseToJoints(m_AsyncFrameIdx);

                m_IsSolving = false;
            }
            else
            {
                if (m_SolverData.StepAsyncEvaluation())
                    CaptureOutputSequence();
            }

            return !m_IsSolving;
        }

        public void CaptureInputFrame(int inputIdx, int timeIdx, bool isReferenceFrame)
        {
            AllocateSolverDataIfNeeded();

            SetInputFrameTime(inputIdx, timeIdx, isReferenceFrame);
            CaptureInputFramePose(inputIdx);
            m_InputDirty = true;
        }

        public void CaptureInputFramePose(int inputIdx)
        {
            AllocateSolverDataIfNeeded();

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputPosition(inputIdx, 0, m_Joints[0].position);
            m_SolverData.SetInputRotation(inputIdx, 0, m_Joints[0].rotation);
            for (var i = 1; i < m_Joints.Length; i++)
            {
                var rotation = m_Joints[i].localRotation;
                m_SolverData.SetInputRotation(inputIdx, i, rotation);
            }

            m_InputDirty = true;
        }

        public void SetInputPosition(int inputIdx, int positionIdx, Vector3 position)
        {
            AllocateSolverDataIfNeeded();

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputPosition(inputIdx, positionIdx, position);

            m_InputDirty = true;
        }

        public void SetInputRotation(int inputIdx, int rotationIdx, Quaternion rotation)
        {
            AllocateSolverDataIfNeeded();

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputRotation(inputIdx, rotationIdx, rotation);

            m_InputDirty = true;
        }

        public void SetInputFrameTime(int inputIdx, int timeIdx, bool isReferenceFrame = false)
        {
            AllocateSolverDataIfNeeded();

            if (isReferenceFrame)
                SetInputReferenceFrame(inputIdx);

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputFrameTime(inputIdx, timeIdx);

            m_InputDirty = true;
        }

        public void SetInputFrameTimes(int[] timeIndices)
        {
            NumInputFrames = timeIndices.Length;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetInputFrameTimes(timeIndices);
        }

        public void SetInputFrameTimes(List<int> timeIndices)
        {
            NumInputFrames = timeIndices.Count;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetInputFrameTimes(timeIndices);
        }

        public void SetInputReferenceFrame(int inputIdx)
        {
            AllocateSolverDataIfNeeded();

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetInputReferenceFrame(inputIdx);

            m_InputDirty = true;
        }

        public void SetTargetFrameTime(int targetIdx, int timeIdx)
        {
            AllocateSolverDataIfNeeded();

            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.SetTargetFrameTime(targetIdx, timeIdx);

            m_InputDirty = true;
        }

        public void SetTargetFrameTimes(int[] timeIndices)
        {
            NumTargetFrames = timeIndices.Length;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetTargetFrameTimes(timeIndices);
        }

        public void SetTargetFrameTimes(List<int> timeIndices)
        {
            NumTargetFrames = timeIndices.Count;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetTargetFrameTimes(timeIndices);
        }

        public void SetInputAndTargetFrameTimes(int[] inputTimeIndices, int[] targetTimeIndices)
        {
            NumInputFrames = inputTimeIndices.Length;
            NumTargetFrames = targetTimeIndices.Length;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetInputFrameTimes(inputTimeIndices);
            m_SolverData.SetTargetFrameTimes(targetTimeIndices);
        }

        public void SetInputAndTargetFrameTimes(List<int> inputTimeIndices, List<int> targetTimeIndices)
        {
            NumInputFrames = inputTimeIndices.Count;
            NumTargetFrames = targetTimeIndices.Count;
            AllocateSolverDataIfNeeded();

            m_SolverData.SetInputFrameTimes(inputTimeIndices);
            m_SolverData.SetTargetFrameTimes(targetTimeIndices);
        }

        void AllocateSolverDataIfNeeded()
        {
            if (!m_DataDirty)
                return;

            if (m_SolverData.IsCreated)
                m_SolverData.Dispose();

            if (m_NumInputFrames == 0 || m_NumTargetFrames == 0)
                return;

            m_SolverData = new CompletionSolverData(
                m_NumInputFrames,
                m_NumTargetFrames,
                m_NumJoints,
                m_NumContacts,
                m_TransposeOrtho6D,
                m_EndJoints);

            m_DataDirty = false;
        }

        void SetJoints(ICollection<Transform> joints)
        {
            Assert.IsTrue(joints.Count == 0 || joints.Count == m_NumJoints, "Wrong number of joints");

            m_Joints = new Transform[joints.Count];
            joints.CopyTo(m_Joints, 0);
        }

        protected abstract void SolveSequence();

        protected void CaptureOutputSequence()
        {
            if (m_Sequence.Length > m_SolverData.NumOutputFrames)
                m_Sequence.Clear();

            m_Sequence.Resize(m_SolverData.NumOutputFrames);

            for (var i = 0; i < m_Sequence.Length; i++)
            {
                var frame = m_Sequence.GetFrame(i);
                CaptureSolvedFrame(frame, i);
            }
        }

        protected void CaptureSolvedFrame(PoseFrame frame, int frameIdx)
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

        void ApplySolvedPoseToJoints(int frameIdx)
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
    }
}
