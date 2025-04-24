using System;
using System.Collections.Generic;
using Unity.Sentis;
using Unity.Collections;
using Unity.DeepPose.ModelBackend;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    struct CompletionSolverData : IDisposable
    {
        public bool IsCreated { get; private set; }
        public int NumInputFrames => m_NumInputFrames;
        public int NumTargetFrames => m_NumTargetFrames;
        public int NumOutputFrames => m_NumOutputFrames;
        public int NumOutputContacts => m_NumOutputContacts;

        public bool IsEvaluating => m_ModelExecutionTask?.IsRunning ?? false;
        public float AsyncEvaluationProgress => m_ModelExecutionTask?.Progress ?? 0f;

        bool m_TransposeOrtho6D;
        int m_NumInputFrames;
        int m_NumTargetFrames;
        int m_NumInputPositions;
        int m_NumInputRotations;
        int m_NumOutputPositions;
        int m_NumOutputRotations;
        int m_NumOutputContacts;
        int m_NumOutputFrames;

        Tensor<int> m_InputFrameIndices;
        Tensor<float> m_InputPositions;
        Tensor<float> m_InputRotations;
        Tensor<int> m_TargetFrameIndices;
        Tensor<int> m_InputReferenceIndex;

        NativeArray<Quaternion> m_PredictedRotations;
        NativeArray<Vector3> m_PredictedPositions;
        NativeArray<float> m_PredictedContacts;

        HashSet<int> m_EndJointsHash;
        ModelExecutionTask m_ModelExecutionTask;

        public CompletionSolverData(int numInputFrames, int numTargetFrames, int numJoints, int numOutputContacts, bool transposeOrtho6D, int[] endJoints)
        {
            m_NumInputFrames = numInputFrames;
            m_NumTargetFrames = numTargetFrames;
            m_NumInputPositions = 1;
            m_NumInputRotations = numJoints;
            m_NumOutputPositions = 1;
            m_NumOutputRotations = numJoints;
            m_NumOutputContacts = numOutputContacts;
            m_TransposeOrtho6D = transposeOrtho6D;

            m_NumOutputFrames = m_NumInputFrames + m_NumTargetFrames;

            // TODO: Temporary hack to fix end joints issue
            m_EndJointsHash = new HashSet<int>();
            for (var i = 0; i < endJoints.Length; i++)
            {
                m_EndJointsHash.Add(endJoints[i]);
            }
            
            // Inputs
            m_InputFrameIndices = TensorUtils.Alloc<int>( m_NumInputFrames );
            m_TargetFrameIndices = TensorUtils.Alloc<int>(m_NumTargetFrames );
            m_InputReferenceIndex =TensorUtils.Alloc<int>( 1 );
            m_InputPositions = TensorUtils.Alloc<float>(1, m_NumInputFrames, m_NumInputPositions, 3 );
            m_InputRotations = TensorUtils.Alloc<float>(1, m_NumInputFrames, m_NumInputRotations, 4 );

            // Outputs
            m_PredictedRotations = new NativeArray<Quaternion>(m_NumOutputFrames * m_NumOutputRotations, Allocator.Persistent);
            m_PredictedPositions = new NativeArray<Vector3>(m_NumOutputFrames * m_NumOutputPositions, Allocator.Persistent);
            m_PredictedContacts = m_NumOutputContacts > 0 ? new NativeArray<float>(m_NumOutputFrames * m_NumOutputContacts, Allocator.Persistent) : default;

            IsCreated = true;
            m_ModelExecutionTask = null;
            
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed while not created");

            if (m_PredictedRotations.IsCreated)
                m_PredictedRotations.Dispose();
            
            if (m_PredictedPositions.IsCreated)
                m_PredictedPositions.Dispose();
            
            if (m_PredictedContacts.IsCreated)
                m_PredictedContacts.Dispose();
            
            m_InputFrameIndices.Dispose();
            m_InputPositions.Dispose();
            m_InputRotations.Dispose();
            m_TargetFrameIndices.Dispose();
            m_InputReferenceIndex.Dispose();
            
            m_ModelExecutionTask?.Dispose();
            m_ModelExecutionTask = null;
            
            IsCreated = false;
        }

        public void SetInputFrameTime(int inputFrameIdx, int timeIdx)
        {
            Assert.IsTrue(inputFrameIdx >= 0 && inputFrameIdx < m_NumInputFrames, "Invalid input frame index");

            m_InputFrameIndices[inputFrameIdx] = timeIdx;
        }

        public void SetInputFrameTimes(int[] timeIndices)
        {
            Assert.AreEqual(m_NumInputFrames, timeIndices.Length);

            for (var i = 0; i < timeIndices.Length; i++)
            {
                m_InputFrameIndices[i] = timeIndices[i];
            }
        }

        public void SetInputFrameTimes(List<int> timeIndices)
        {
            Assert.AreEqual(m_NumInputFrames, timeIndices.Count);

            for (var i = 0; i < timeIndices.Count; i++)
            {
                m_InputFrameIndices[i] = timeIndices[i];
            }
        }

        public void SetInputReferenceFrame(int referenceFrameIdx)
        {
            Assert.IsTrue(referenceFrameIdx >= 0 && referenceFrameIdx < m_NumInputFrames, "Invalid reference frame index");
            m_InputReferenceIndex[0] = referenceFrameIdx;
        }

        public void SetTargetFrameTime(int targetFrameIdx, int timeIdx)
        {
            Assert.IsTrue(targetFrameIdx >= 0 && targetFrameIdx < m_NumTargetFrames, "Invalid target frame index");
            m_TargetFrameIndices[targetFrameIdx] = timeIdx;
        }

        public void SetTargetFrameTimes(int[] timeIndices)
        {
            Assert.AreEqual(m_NumTargetFrames, timeIndices.Length);

            for (var i = 0; i < timeIndices.Length; i++)
            {
                m_TargetFrameIndices[i] = timeIndices[i];
            }
        }

        public void SetTargetFrameTimes(List<int> timeIndices)
        {
            Assert.AreEqual(m_NumTargetFrames, timeIndices.Count);

            for (var i = 0; i < timeIndices.Count; i++)
            {
                m_TargetFrameIndices[i] = timeIndices[i];
            }
        }

        public int GetTargetFrameTime(int targetFrameIdx)
        {
            return m_TargetFrameIndices[targetFrameIdx];
        }

        public void SetInputPosition(int frameIdx, int positionIdx, Vector3 position)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_NumInputFrames, "Invalid input frame index");
            Assert.IsTrue(positionIdx >= 0 && positionIdx < m_NumInputPositions, "Invalid input position index");

            m_InputPositions[0, frameIdx, positionIdx, 0] = position.x;
            m_InputPositions[0, frameIdx, positionIdx, 1] = position.y;
            m_InputPositions[0, frameIdx, positionIdx, 2] = position.z;
        }

        public void SetInputRotation(int frameIdx, int rotationIdx, Quaternion rotation)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_NumInputFrames, "Invalid input frame index");
            Assert.IsTrue(rotationIdx >= 0 && rotationIdx < m_NumInputRotations, "Invalid input rotation index");

            // TODO: this is a hot fix for unexpected model behavior with end joints
            if (IsEndJoint(rotationIdx))
                rotation = Quaternion.identity;

            m_InputRotations[0, frameIdx, rotationIdx, 0] = rotation.w;
            m_InputRotations[0, frameIdx, rotationIdx, 1] = rotation.x;
            m_InputRotations[0, frameIdx, rotationIdx, 2] = rotation.y;
            m_InputRotations[0, frameIdx, rotationIdx, 3] = rotation.z;
        }

        public NativeSlice<float> GetPredictedFrameContacts(int frameIdx)
        {
            if (m_NumOutputContacts == 0)
                return default;

            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_NumOutputFrames, "Invalid output frame index");

            var slice = new NativeSlice<float>(m_PredictedContacts, frameIdx * m_NumOutputContacts, m_NumOutputContacts);
            return slice;
        }

        public NativeSlice<Quaternion> GetPredictedFrameRotations(int frameIdx)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_NumOutputFrames, "Invalid output frame index");

            var slice = new NativeSlice<Quaternion>(m_PredictedRotations, frameIdx * m_NumOutputRotations, m_NumOutputRotations);
            return slice;
        }

        public NativeSlice<Vector3> GetPredictedFramePositions(int frameIdx)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_NumOutputFrames, "Invalid output frame index");

            var slice = new NativeSlice<Vector3>(m_PredictedPositions, frameIdx * m_NumOutputPositions, m_NumOutputPositions);
            return slice;
        }

        public void Evaluate(IModelBackend backend)
        {
            // Set the model inputs
            SetInputs(backend);

            // Run the model
            backend.Execute();

            ParseOutputs(backend);
        }

        public void StartAsyncEvaluation(IModelBackend backend, int syncProgressEachNthLayer = 0)
        {
            m_ModelExecutionTask ??= new ModelExecutionTask();

            Assert.IsFalse(m_ModelExecutionTask.IsRunning, "Async evaluation was already started");
            SetInputs(backend);
            m_ModelExecutionTask.Start(backend, true, syncProgressEachNthLayer);
        }

        public void StopAsyncEvaluation()
        {
            Assert.IsNotNull(m_ModelExecutionTask, "Async evaluation was not started");
            m_ModelExecutionTask.Stop();
        }

        public bool StepAsyncEvaluation()
        {
            Assert.IsNotNull(m_ModelExecutionTask, "Async evaluation was not started");
            var isDone = m_ModelExecutionTask.Step();
            if (isDone)
                ParseOutputs(m_ModelExecutionTask.Backend);
            return isDone;
        }

        void SetInputs(IModelBackend backend)
        {
            backend.SetInput("input_frame_indices", m_InputFrameIndices);
            backend.SetInput("target_frame_indices", m_TargetFrameIndices);
            backend.SetInput("input_root_positions", m_InputPositions);
            backend.SetInput("input_local_quats", m_InputRotations);
            backend.SetInput("input_reference_index", m_InputReferenceIndex);
        }

        void ParseOutputs(IModelBackend backend)
        {
            var predictedJointPositions = backend.PeekOutput<float>("joint_positions_global");
            var predictedJointRotations = backend.PeekOutput<float>("joint_rotations_ortho6d");
            var predictedContacts = m_NumOutputContacts > 0 ? backend.PeekOutput<float>("contacts") : null;

            for (var frameIdx = 0; frameIdx < m_NumOutputFrames; frameIdx++)
            {
                var framePositions = GetPredictedFramePositions(frameIdx);
                for (var positionIdx = 0; positionIdx < m_NumOutputPositions; positionIdx++)
                {
                    var position = new Vector3(predictedJointPositions[0, frameIdx, positionIdx, 0], predictedJointPositions[0, frameIdx, positionIdx, 1], predictedJointPositions[0, frameIdx, positionIdx, 2]);
                    framePositions[positionIdx] = position;
                }

                var frameRotations = GetPredictedFrameRotations(frameIdx);
                for (var rotationIdx = 0; rotationIdx < m_NumOutputRotations; rotationIdx++)
                {
                    var c0 = new double3(predictedJointRotations[0, frameIdx, rotationIdx, 0], predictedJointRotations[0, frameIdx, rotationIdx, 1], predictedJointRotations[0, frameIdx, rotationIdx, 2]);
                    var c1 = new double3(predictedJointRotations[0, frameIdx, rotationIdx, 3], predictedJointRotations[0, frameIdx, rotationIdx, 4], predictedJointRotations[0, frameIdx, rotationIdx, 5]);
                    var rotation = GeometryUtils.QuaternionFromOrtho6d(new double3x2(c0, c1), m_TransposeOrtho6D);
                    frameRotations[rotationIdx] = rotation;
                }

                if (predictedContacts == null)
                    continue;

                var frameContacts = GetPredictedFrameContacts(frameIdx);
                for (var contactIdx = 0; contactIdx < m_NumOutputContacts; contactIdx++)
                {
                    var contactValue = predictedContacts[0, frameIdx, contactIdx];
                    frameContacts[contactIdx] = contactValue;
                }
            }
        }

        bool IsEndJoint(int rotationIdx)
        {
            return m_EndJointsHash.Contains(rotationIdx);
        }
    }
}
