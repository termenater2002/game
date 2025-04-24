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
    struct AutoregressiveSolverData : IDisposable
    {
        public bool IsCreated { get; private set; }
        public int SequenceLength => m_InputFrameIndices.SequenceLength;
        public SequenceTensorInt InputFrameIndices => m_InputFrameIndices;
        public SequenceTensorFloat InputPositions => m_InputPositions;
        public SequenceTensorFloat InputRotations => m_InputRotations;
        public SequenceTensorFloat InputRaycasts => m_InputRaycasts;
        public int InputReferenceFrame => m_InputReferenceIndex[0];

        public bool IsEvaluating => m_ModelExecutionTask?.IsRunning ?? false;
        public float AsyncEvaluationProgress => m_ModelExecutionTask?.Progress ?? 0f;

        bool m_TransposeOrtho6D;
        bool m_UseRaycasts;
        bool m_UseTolerance;
        bool m_InputAllPositions;
        bool m_OutputAllPositions;
        int m_NumInputPositions;
        int m_NumInputRotations;
        int m_NumInputRaycasts;
        int m_NumOutputPositions;
        int m_NumOutputRotations;
        int m_NumOutputContacts;
        int m_NumOutputFrames;

        SequenceTensorInt m_InputFrameIndices;
        SequenceTensorFloat m_InputPositions;
        SequenceTensorFloat m_InputRotations;
        SequenceTensorFloat m_InputRaycasts;
        Tensor<int> m_InputReferenceIndex;
        Tensor<float> m_InputTolerance;

        Tensor<int> m_AllocatedInputFrameIndices;
        Tensor<float> m_AllocatedInputPositions;
        Tensor<float> m_AllocatedInputRotations;
        Tensor<float> m_AllocatedInputRaycasts;

        NativeArray<Quaternion> m_PredictedRotations;
        NativeArray<Vector3> m_PredictedPositions;
        NativeArray<float> m_PredictedContacts;

        HashSet<int> m_EndJointsHash;
        ModelExecutionTask m_ModelExecutionTask;

        public AutoregressiveSolverData(
            int maxSequenceLength,
            int numJoints,
            int numOutputContacts,
            int numRaycastJoints,
            bool useRaycasts,
            bool transposeOrtho6D,
            int[] endJoints,
            bool useTolerance,
            bool inputAllPositions,
            bool outputAllPositions)
        {
            m_InputAllPositions = inputAllPositions;
            m_OutputAllPositions = outputAllPositions;
            m_NumInputPositions = m_InputAllPositions ? numJoints : 1;
            m_NumInputRotations = numJoints;
            m_NumInputRaycasts = numRaycastJoints;
            m_NumOutputPositions = m_OutputAllPositions ? numJoints : 1;
            m_NumOutputRotations = numJoints;
            m_NumOutputContacts = numOutputContacts;
            m_TransposeOrtho6D = transposeOrtho6D;
            m_UseRaycasts = useRaycasts;
            m_UseTolerance = useTolerance;

            // Single step prediction for now
            m_NumOutputFrames = 1;

            // TODO: Temporary hack to fix end joints issue
            m_EndJointsHash = new HashSet<int>();
            for (var i = 0; i < endJoints.Length; i++)
            {
                m_EndJointsHash.Add(endJoints[i]);
            }

            // Inputs
            m_InputReferenceIndex = TensorUtils.NewTensorInt(new TensorShape(new[] { 1 }));
            m_InputTolerance = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numJoints }));
            m_InputFrameIndices = new SequenceTensorInt(0, maxSequenceLength);
            m_InputPositions = new SequenceTensorFloat(1, 0, maxSequenceLength, m_NumInputPositions, 3);
            m_InputRotations = new SequenceTensorFloat(1, 0, maxSequenceLength, m_NumInputRotations, 4);
            m_InputRaycasts = new SequenceTensorFloat(1, 0, maxSequenceLength, m_NumInputRaycasts);

            // Outputs
            m_PredictedRotations = new NativeArray<Quaternion>(m_NumOutputFrames * m_NumOutputRotations, Allocator.Persistent);
            m_PredictedPositions = new NativeArray<Vector3>(m_NumOutputFrames * m_NumOutputPositions, Allocator.Persistent);
            m_PredictedContacts = m_NumOutputContacts > 0 ? new NativeArray<float>(m_NumOutputFrames * m_NumOutputContacts, Allocator.Persistent) : default;

            // Temporary tensors, holding reference to properly dispose them after async evaluation
            m_AllocatedInputFrameIndices = null;
            m_AllocatedInputPositions = null;
            m_AllocatedInputRotations = null;
            m_AllocatedInputRaycasts = null;

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

            m_InputReferenceIndex?.Dispose();
            m_InputReferenceIndex = null;

            m_ModelExecutionTask?.Dispose();
            m_ModelExecutionTask = null;

            DisposeInputs();

            IsCreated = false;
        }

        public void InsertFrame(int frameIdx)
        {
            m_InputFrameIndices.InsertSample(frameIdx);
            m_InputPositions.InsertSample(frameIdx);
            m_InputRotations.InsertSample(frameIdx);
            m_InputRaycasts.InsertSample(frameIdx);
        }

        public void RemoveFrame(int frameIdx)
        {
            m_InputFrameIndices.RemoveSample(frameIdx);
            m_InputPositions.RemoveSample(frameIdx);
            m_InputRotations.RemoveSample(frameIdx);
            m_InputRaycasts.RemoveSample(frameIdx);
        }

        public void ResizeSequence(int length)
        {
            m_InputFrameIndices.Resize(length);
            m_InputPositions.Resize(length);
            m_InputRotations.Resize(length);
            m_InputRaycasts.Resize(length);
        }

        public void SetInputFrameTime(int inputFrameIdx, int timeIdx)
        {
            m_InputFrameIndices[0, inputFrameIdx] = timeIdx;
        }

        public void SetInputReferenceFrame(int referenceFrameIdx)
        {
            Assert.IsTrue(referenceFrameIdx >= 0 && referenceFrameIdx < m_InputFrameIndices.SequenceLength, "Invalid reference frame index");
            m_InputReferenceIndex[0] = referenceFrameIdx;
        }

        public void SetInputTolerance(int jointIdx, float tolerance)
        {
            m_InputTolerance[0, jointIdx] = tolerance;
        }

        public void SetInputPosition(int frameIdx, int positionIdx, Vector3 position)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_InputFrameIndices.SequenceLength, "Invalid input frame index");
            Assert.IsTrue(positionIdx >= 0 && positionIdx < m_NumInputPositions, "Invalid input position index");

            m_InputPositions[0, frameIdx, positionIdx, 0] = position.x;
            m_InputPositions[0, frameIdx, positionIdx, 1] = position.y;
            m_InputPositions[0, frameIdx, positionIdx, 2] = position.z;
        }

        public void SetInputRotation(int frameIdx, int rotationIdx, Quaternion rotation)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_InputFrameIndices.SequenceLength, "Invalid input frame index");
            Assert.IsTrue(rotationIdx >= 0 && rotationIdx < m_NumInputRotations, "Invalid input rotation index");

            // TODO: this is a hot fix for unexpected model behavior with end joints
            if (IsEndJoint(rotationIdx))
                rotation = Quaternion.identity;

            m_InputRotations[0, frameIdx, rotationIdx, 0] = rotation.w;
            m_InputRotations[0, frameIdx, rotationIdx, 1] = rotation.x;
            m_InputRotations[0, frameIdx, rotationIdx, 2] = rotation.y;
            m_InputRotations[0, frameIdx, rotationIdx, 3] = rotation.z;
        }

        public void SetInputRaycast(int frameIdx, int raycastIdx, float raycastDistance)
        {
            Assert.IsTrue(frameIdx >= 0 && frameIdx < m_InputFrameIndices.SequenceLength, "Invalid input frame index");
            Assert.IsTrue(raycastIdx >= 0 && raycastIdx < m_NumInputRaycasts, "Invalid input raycast index");

            m_InputRaycasts[0, frameIdx, raycastIdx] = raycastDistance;
        }

        bool IsEndJoint(int rotationIdx)
        {
            return m_EndJointsHash.Contains(rotationIdx);
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
            StartAsyncEvaluationInternal(backend, 0, false);
            while (!StepAsyncEvaluation())
            {
                // Wait for completion
            }
        }

        public void StartAsyncEvaluation(IModelBackend backend, int syncProgressEachNthLayer = 0)
        {
            StartAsyncEvaluationInternal(backend, syncProgressEachNthLayer, true);
        }

        public void StopAsyncEvaluation()
        {
            Assert.IsNotNull(m_ModelExecutionTask, "Async evaluation was not started");

            if (!m_ModelExecutionTask.IsDone)
            {
                m_ModelExecutionTask.Stop();
            }

            DisposeInputs();
        }

        public bool StepAsyncEvaluation()
        {
            Assert.IsNotNull(m_ModelExecutionTask, "Async evaluation was not started");
            var isDone = m_ModelExecutionTask.Step();

            if (isDone)
            {
                ParseOutputs(m_ModelExecutionTask.Backend);
                DisposeInputs();
            }

            return isDone;
        }

        /// <summary>
        /// Returns the index of the input frame with the given frame time index
        /// </summary>
        /// <param name="frameIdx">The time index of the frame to look for</param>
        /// <returns>The index of the frame with the given time index in the input frames list</returns>
        public int FindInputFrameIndex(int frameIdx)
        {
            for (var i = 0; i < m_InputFrameIndices.SequenceLength; i++)
            {
                if (m_InputFrameIndices[0, i] == frameIdx)
                    return i;
            }

            return -1;
        }

        void StartAsyncEvaluationInternal(IModelBackend backend, int syncProgressEachNthLayer, bool asyncModelExecution)
        {
            m_ModelExecutionTask ??= new ModelExecutionTask();

            Assert.IsFalse(m_ModelExecutionTask.IsRunning, "Async evaluation was already started");

            AllocateInputs();
            SetInputs(backend);

            m_ModelExecutionTask.Start(backend, asyncModelExecution, syncProgressEachNthLayer);
        }

        void AllocateInputs()
        {
            DisposeInputs();
            m_AllocatedInputFrameIndices = m_InputFrameIndices.AllocateTensor();
            m_AllocatedInputPositions = m_InputPositions.AllocateTensor();
            m_AllocatedInputRotations = m_InputRotations.AllocateTensor();
            m_AllocatedInputRaycasts = m_InputRaycasts.AllocateTensor();
        }

        void DisposeInputs()
        {
            m_AllocatedInputFrameIndices?.Dispose();
            m_AllocatedInputPositions?.Dispose();
            m_AllocatedInputRotations?.Dispose();
            m_AllocatedInputRaycasts?.Dispose();

            m_AllocatedInputFrameIndices = null;
            m_AllocatedInputPositions = null;
            m_AllocatedInputRotations = null;
            m_AllocatedInputRaycasts = null;
        }

        void SetInputs(IModelBackend backend)
        {
            backend.SetInput("input_frame_indices", m_AllocatedInputFrameIndices);
            backend.SetInput(m_InputAllPositions ? "input_positions": "input_root_positions", m_AllocatedInputPositions);
            backend.SetInput("input_local_quats", m_AllocatedInputRotations);
            backend.SetInput("input_reference_index", m_InputReferenceIndex);
            if (m_UseRaycasts && m_NumInputRaycasts > 0)
                backend.SetInput("input_raycasts", m_AllocatedInputRaycasts);
            if (m_UseTolerance)
                backend.SetInput("input_tolerance", m_InputTolerance);
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
                    var position = new Vector3(
                        predictedJointPositions[0, frameIdx, positionIdx, 0],
                        predictedJointPositions[0, frameIdx, positionIdx, 1],
                        predictedJointPositions[0, frameIdx, positionIdx, 2]
                    );

                    framePositions[positionIdx] = position;
                }

                var frameRotations = GetPredictedFrameRotations(frameIdx);
                for (var rotationIdx = 0; rotationIdx < m_NumOutputRotations; rotationIdx++)
                {
                    var c0 = new double3(
                        predictedJointRotations[0, frameIdx, rotationIdx, 0],
                        predictedJointRotations[0, frameIdx, rotationIdx, 1],
                        predictedJointRotations[0, frameIdx, rotationIdx, 2]
                    );
                    var c1 = new double3(
                        predictedJointRotations[0, frameIdx, rotationIdx, 3],
                        predictedJointRotations[0, frameIdx, rotationIdx, 4],
                        predictedJointRotations[0, frameIdx, rotationIdx, 5]
                    );

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
    }
}
