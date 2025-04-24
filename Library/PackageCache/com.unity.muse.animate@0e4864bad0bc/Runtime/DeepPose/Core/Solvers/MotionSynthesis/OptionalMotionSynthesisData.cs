using System;
using Unity.Sentis;
using Unity.Collections;
using Unity.DeepPose.ModelBackend;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    struct OptionalMotionSynthesisData : IDisposable
    {
        public Tensor<float> FrameTimes;
        public Tensor<float> InputPositions;
        public Tensor<float> InputPositionsTolerance;
        public Tensor<float> InputPositionsMask;
        public Tensor<float> InputRotations;
        public Tensor<float> InputRotationsTolerance;
        public Tensor<float> InputRotationsMask;

        public NativeArray<Quaternion> PredictedRotations;
        public NativeArray<Vector3> PredictedPositions;
        public NativeArray<bool> PredictedContacts;

        public bool IsCreated => FrameTimes != null;

        bool m_HasContacts;
        private bool m_HasTolerance;
        bool m_TransposeOrtho6d;

        int m_NumFrames;
        int m_NumInputPositions;
        int m_NumInputRotations;
        int m_NumOutputPositions;
        int m_NumOutputRotations;
        int m_NumOutputContacts;

        public OptionalMotionSynthesisData(int numFrames, int numInputPositions, int numInputRotations, int numOutputPositions, int numOutputRotations, bool transposeOrtho6d, bool hasTolerance, bool hasContacts, int numOutputContacts = 0)
        {
            m_TransposeOrtho6d = transposeOrtho6d;
            m_HasContacts = hasContacts;
            m_HasTolerance = hasTolerance;

            m_NumFrames = numFrames;
            m_NumInputPositions = numInputPositions;
            m_NumInputRotations = numInputRotations;
            m_NumOutputPositions = numOutputPositions;
            m_NumOutputRotations = numOutputRotations;
            m_NumOutputContacts = numOutputContacts;

            PredictedRotations = new NativeArray<Quaternion>(numFrames * numOutputRotations, Allocator.Persistent);
            PredictedPositions = new NativeArray<Vector3>(numFrames * numOutputPositions, Allocator.Persistent);
            PredictedContacts = new NativeArray<bool>(numFrames * numOutputContacts, Allocator.Persistent);

            FrameTimes = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames }));

            InputPositions = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputPositions, 3 }));
            InputPositionsTolerance = m_HasTolerance ? TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputPositions })) : null;
            InputPositionsMask = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputPositions }));

            InputRotations = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputRotations, 6 }));
            InputRotationsTolerance = m_HasTolerance ? TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputRotations })) : null;
            InputRotationsMask = TensorUtils.NewTensorFloat(new TensorShape(new[] { 1, numFrames, numInputRotations }));

            ResetAllEffectors();
        }

        public void Dispose()
        {
            if (PredictedRotations.IsCreated)
                PredictedRotations.Dispose();
            if (PredictedPositions.IsCreated)
                PredictedPositions.Dispose();
            if (PredictedContacts.IsCreated)
                PredictedContacts.Dispose();

            FrameTimes?.Dispose();
            InputPositions?.Dispose();
            InputPositionsTolerance?.Dispose();
            InputPositionsMask?.Dispose();
            InputRotations?.Dispose();
            InputRotationsTolerance?.Dispose();
            InputRotationsMask?.Dispose();
        }

        public void ResetAllEffectors(int frameIdx = -1)
        {
            if (frameIdx >= 0)
            {
                for (var i = 0; i < m_NumInputPositions; i++)
                {
                    if (m_HasTolerance)
                        InputPositionsTolerance[0, frameIdx, i] = 0f;
                    InputPositionsMask[0, frameIdx, i] = 0f;
                }

                for (var i = 0; i < m_NumInputRotations; i++)
                {
                    if (m_HasTolerance)
                        InputRotationsTolerance[0, frameIdx, i] = 0f;
                    InputRotationsMask[0, frameIdx, i] = 0f;
                }
            }
            else
            {
                TensorUtils.Fill(InputPositionsMask, 0f);
                TensorUtils.Fill(InputRotationsMask, 0f);

                if (m_HasTolerance)
                {
                    TensorUtils.Fill(InputPositionsTolerance, 0f);
                    TensorUtils.Fill(InputRotationsTolerance, 0f);
                }
            }
        }

        public void SetFrameTime(int frameIdx, float time)
        {
            FrameTimes[0, frameIdx] = time;
        }

        public void SetPositionEffector(int frameIdx, int id, Vector3 position, float tolerance = 0f, bool enabled = true)
        {
            InputPositions[0, frameIdx, id, 0] = position.x;
            InputPositions[0, frameIdx, id, 1] = position.y;
            InputPositions[0, frameIdx, id, 2] = position.z;
            if (m_HasTolerance)
                InputPositionsTolerance[0, frameIdx, id] = tolerance;
            InputPositionsMask[0, frameIdx, id] = enabled ? 1f : 0f;
        }

        public void SetRotationEffector(int frameIdx, int id, Quaternion rotation, float tolerance = 0f, bool enabled = true)
        {
            // convert to ortho6D
            var rot = Matrix4x4.Rotate(rotation);
            var r0 = m_TransposeOrtho6d ? rot.GetRow(0) : rot.GetColumn(0);
            var r1 = m_TransposeOrtho6d ? rot.GetRow(1) : rot.GetColumn(1);

            InputRotations[0, frameIdx, id, 0] = r0.x;
            InputRotations[0, frameIdx, id, 1] = r0.y;
            InputRotations[0, frameIdx, id, 2] = r0.z;
            InputRotations[0, frameIdx, id, 3] = r1.x;
            InputRotations[0, frameIdx, id, 4] = r1.y;
            InputRotations[0, frameIdx, id, 5] = r1.z;
            if (m_HasTolerance)
                InputRotationsTolerance[0, frameIdx, id] = tolerance;
            InputRotationsMask[0, frameIdx, id] = enabled ? 1f : 0f;
        }

        public float GetFrameTime(int frameIdx)
        {
            return FrameTimes[0, frameIdx];
        }

        public NativeSlice<Quaternion> GetFrameRotations(int frameIdx)
        {
            var slice = new NativeSlice<Quaternion>(PredictedRotations, frameIdx * m_NumOutputRotations, m_NumOutputRotations);
            return slice;
        }

        public NativeSlice<Vector3> GetFramePositions(int frameIdx)
        {
            var slice = new NativeSlice<Vector3>(PredictedPositions, frameIdx * m_NumOutputPositions, m_NumOutputPositions);
            return slice;
        }

        public NativeSlice<bool> GetFrameContacts(int frameIdx)
        {
            var slice = new NativeSlice<bool>(PredictedContacts, frameIdx * m_NumOutputContacts, m_NumOutputContacts);
            return slice;
        }

        public void Evaluate(IModelBackend backend)
        {
            // Set the model inputs
            backend.SetInput("input_frame_times", FrameTimes);
            backend.SetInput("input_position_effectors", InputPositions);
            backend.SetInput("input_position_mask", InputPositionsMask);
            backend.SetInput("input_rotation_effectors", InputRotations);
            backend.SetInput("input_rotation_mask", InputRotationsMask);
            if (m_HasTolerance)
            {
                backend.SetInput("input_position_tolerances", InputPositionsTolerance);
                backend.SetInput("input_rotation_tolerances", InputRotationsTolerance);
            }

            // Run the model
            backend.Execute();

            var predictedJointPositions = backend.PeekOutput<float>("joint_positions_global");
            var predictedJointRotations = backend.PeekOutput<float>("joint_rotations_ortho6d");
            var predictedContacts = m_HasContacts ? backend.PeekOutput<float>("contact_predictions") : null;

            for (var frameIdx = 0; frameIdx < m_NumFrames; frameIdx++)
            {
                var framePositions = GetFramePositions(frameIdx);
                for (var positionIdx = 0; positionIdx < m_NumOutputPositions; positionIdx++)
                {
                    var position = new Vector3(predictedJointPositions[0, frameIdx, positionIdx, 0], predictedJointPositions[0, frameIdx, positionIdx, 1], predictedJointPositions[0, frameIdx, positionIdx, 2]);
                    framePositions[positionIdx] = position;
                }

                var frameRotations = GetFrameRotations(frameIdx);
                for (var rotationIdx = 0; rotationIdx < m_NumOutputRotations; rotationIdx++)
                {
                    var c0 = new double3(predictedJointRotations[0, frameIdx, rotationIdx, 0], predictedJointRotations[0, frameIdx, rotationIdx, 1], predictedJointRotations[0, frameIdx, rotationIdx, 2]);
                    var c1 = new double3(predictedJointRotations[0, frameIdx, rotationIdx, 3], predictedJointRotations[0, frameIdx, rotationIdx, 4], predictedJointRotations[0, frameIdx, rotationIdx, 5]);
                    var rotation = GeometryUtils.QuaternionFromOrtho6d(new double3x2(c0, c1), m_TransposeOrtho6d);
                    frameRotations[rotationIdx] = rotation;
                }

                if (predictedContacts == null)
                    continue;

                var frameContacts = GetFrameContacts(frameIdx);
                for (var contactIdx = 0; contactIdx < m_NumOutputContacts; contactIdx++)
                {
                    var contactValue = predictedContacts[0, frameIdx, contactIdx];
                    var isContact = contactValue >= 0.5f;
                    frameContacts[contactIdx] = isContact;
                }
            }
        }
    }
}
