using System;
using Unity.Sentis;
using Unity.Collections;
using Unity.DeepPose.ModelBackend;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    struct DeepPoseData : IDisposable
    {
        Tensor<float> m_PositionData;
        Tensor<int> m_PositionIds;
        Tensor<float> m_PositionWeights;
        Tensor<float> m_PositionTolerances;

        Tensor<float> m_RotationData;
        Tensor<int> m_RotationIds;
        Tensor<float> m_RotationWeights;
        Tensor<float> m_RotationTolerances;

        Tensor<float> m_LookAtData;
        Tensor<int> m_LookAtIds;
        Tensor<float> m_LookAtWeights;
        Tensor<float> m_LookAtTolerances;

        Tensor<float> m_PosePositions;
        Tensor<float> m_PoseRotations;
        Tensor<int> m_PoseIds;

        public NativeArray<Quaternion> jointRotations;
        public Vector3 rootJointPosition;

        public bool IsCreated => m_IsCreated;

        bool m_IsCreated;
        bool m_TransposeOrtho6d;
        bool m_IncludePose;
        bool m_UseRaycast;
        float m_MaxRayDistance;

        public DeepPoseData(int numJoints, int numPositions, int numRotations, int numLookAts, bool includePose, bool transposeOrtho6d, bool useRaycast, float maxRayDistance)
        {
            m_TransposeOrtho6d = transposeOrtho6d;
            m_IncludePose = includePose;
            m_UseRaycast = useRaycast;
            m_MaxRayDistance = maxRayDistance;

            var posFeatureSize = m_UseRaycast ? 4 : 3;
            
            // Tensors
            m_PositionData = TensorUtils.Alloc<float>(1, numPositions, posFeatureSize);
            m_PositionIds = TensorUtils.Alloc<int>(1, numPositions);
            m_PositionWeights = TensorUtils.Alloc<float>(1, numPositions);
            m_PositionTolerances = TensorUtils.Alloc<float>(1, numPositions);

            m_RotationData = TensorUtils.Alloc<float>(1, numRotations, 6);
            m_RotationIds = TensorUtils.Alloc<int>(1, numRotations);
            m_RotationWeights = TensorUtils.Alloc<float>(1, numRotations);
            m_RotationTolerances = TensorUtils.Alloc<float>(1, numRotations);

            m_LookAtData = TensorUtils.Alloc<float>(1, numLookAts, 6);
            m_LookAtIds = TensorUtils.Alloc<int>(1, numLookAts);
            m_LookAtWeights = TensorUtils.Alloc<float>(1, numLookAts);
            m_LookAtTolerances = TensorUtils.Alloc<float>(1, numLookAts);
            
            m_PosePositions = TensorUtils.Alloc<float>(1, numJoints, 3);
            m_PoseRotations = TensorUtils.Alloc<float>(1, numJoints, 6);
            m_PoseIds = TensorUtils.Alloc<int>(1, numJoints);
            
            // Output Data
            jointRotations = new NativeArray<Quaternion>(numJoints, Allocator.Persistent);
            rootJointPosition = Vector3.zero;
            m_IsCreated = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed without being initialized");
            
            m_PositionData.Dispose();
            m_PositionIds.Dispose();
            m_PositionWeights.Dispose();
            m_PositionTolerances.Dispose();
            m_RotationData.Dispose();
            m_RotationIds.Dispose();
            m_RotationWeights.Dispose();
            m_RotationTolerances.Dispose();
            m_LookAtData.Dispose();
            m_LookAtIds.Dispose();
            m_LookAtWeights.Dispose();
            m_LookAtTolerances.Dispose();
            m_PosePositions.Dispose();
            m_PoseRotations.Dispose();
            m_PoseIds.Dispose();
            
            if (jointRotations.IsCreated)
                jointRotations.Dispose();
        }

        public void SetPoseJoint(int boneIdx, Vector3 position, Quaternion rotation)
        {
            m_PoseIds[0, boneIdx] = boneIdx;

            m_PosePositions[0, boneIdx, 0] = position.x;
            m_PosePositions[0, boneIdx, 1] = position.y;
            m_PosePositions[0, boneIdx, 2] = position.z;

                // convert to ortho6D
            var rot = Matrix4x4.Rotate(rotation);
            var r0 = m_TransposeOrtho6d ? rot.GetRow(0) : rot.GetColumn(0);
            var r1 = m_TransposeOrtho6d ? rot.GetRow(1) : rot.GetColumn(1);

            m_PoseRotations[0, boneIdx, 0] = r0.x;
            m_PoseRotations[0, boneIdx, 1] = r0.y;
            m_PoseRotations[0, boneIdx, 2] = r0.z;
            m_PoseRotations[0, boneIdx, 3] = r1.x;
            m_PoseRotations[0, boneIdx, 4] = r1.y;
            m_PoseRotations[0, boneIdx, 5] = r1.z;
        }

        public void SetPositionEffector(int idx, int boneId, Vector3 position, float weight, float tolerance, float raycastDistance)
        {
            Assert.IsTrue(IsCreated);
            Assert.IsTrue(idx >= 0 && idx < m_PositionData.shape[1]);
            Assert.IsTrue(weight >= 0f && weight <= 1f);

            m_PositionIds[0, idx] = boneId;
            m_PositionWeights[0, idx] = weight;
            m_PositionTolerances[0, idx] = tolerance;

            m_PositionData[0, idx, 0] = position.x;
            m_PositionData[0, idx, 1] = position.y;
            m_PositionData[0, idx, 2] = position.z;
            if (m_UseRaycast)
                m_PositionData[0, idx, 3] = math.min(raycastDistance, m_MaxRayDistance);
        }

        public void SetRotationEffector(int idx, int boneId, Quaternion rotation, float weight, float tolerance)
        {
            Assert.IsTrue(IsCreated);
            Assert.IsTrue(idx >= 0 && idx < m_RotationData.shape[1]);
            Assert.IsTrue(weight >= 0f && weight <= 1f);

            m_RotationIds[0, idx] = boneId;
            m_RotationWeights[0, idx] = weight;
            m_RotationTolerances[0, idx] = tolerance;

            // convert to ortho6D
            var rot = Matrix4x4.Rotate(rotation);
            var r0 = m_TransposeOrtho6d ? rot.GetRow(0) : rot.GetColumn(0);
            var r1 = m_TransposeOrtho6d ? rot.GetRow(1) : rot.GetColumn(1);

            m_RotationData[0, idx, 0] = r0.x;
            m_RotationData[0, idx, 1] = r0.y;
            m_RotationData[0, idx, 2] = r0.z;
            m_RotationData[0, idx, 3] = r1.x;
            m_RotationData[0, idx, 4] = r1.y;
            m_RotationData[0, idx, 5] = r1.z;

        }

        public void SetLookAtEffector(int idx, int boneId, Vector3 target, Vector3 localDirection, float weight, float tolerance)
        {
            Assert.IsTrue(IsCreated);
            Assert.IsTrue(idx >= 0 && idx < m_LookAtData.shape[1]);

            m_LookAtIds[0, idx] = boneId;
            m_LookAtWeights[0, idx] = weight;
            m_LookAtTolerances[0, idx] = tolerance;

            localDirection = localDirection.normalized;

            m_LookAtData[0, idx, 0] = target.x;
            m_LookAtData[0, idx, 1] = target.y;
            m_LookAtData[0, idx, 2] = target.z;
            m_LookAtData[0, idx, 3] = localDirection.x;
            m_LookAtData[0, idx, 4] = localDirection.y;
            m_LookAtData[0, idx, 5] = localDirection.z;

        }

        public void Evaluate(IModelBackend backend)
        {
            backend.SetInput("position_data", m_PositionData);
            backend.SetInput("position_weight", m_PositionWeights);
            backend.SetInput("position_tolerance", m_PositionTolerances);
            backend.SetInput("position_id", m_PositionIds);

            backend.SetInput("rotation_data", m_RotationData);
            backend.SetInput("rotation_weight", m_RotationWeights);
            backend.SetInput("rotation_tolerance", m_RotationTolerances);
            backend.SetInput("rotation_id", m_RotationIds);

            backend.SetInput("lookat_data", m_LookAtData);
            backend.SetInput("lookat_weight", m_LookAtWeights);
            backend.SetInput("lookat_tolerance", m_LookAtTolerances);
            backend.SetInput("lookat_id", m_LookAtIds);

            if (m_IncludePose)
            {
                backend.SetInput("joint_positions.1", m_PosePositions);
                backend.SetInput("joint_rotations.1", m_PoseRotations);
                backend.SetInput("joint_ids", m_PoseIds);
            }

            backend.Execute();
            
            var predictedHipPosition = backend.PeekOutput<float>("root_joint_position");
            var predictedBoneRotations = backend.PeekOutput<float>("joint_rotations");

            predictedBoneRotations.CompleteAllPendingOperations();
            predictedHipPosition.CompleteAllPendingOperations();

            for (int i = 0; i < jointRotations.Length; i++)
            {
                double3 c0, c1;
                c0 = new double3(predictedBoneRotations[0, i, 0], predictedBoneRotations[0, i, 1], predictedBoneRotations[0, i, 2]);
                c1 = new double3(predictedBoneRotations[0, i, 3], predictedBoneRotations[0, i, 4], predictedBoneRotations[0, i, 5]);

                var rotation = GeometryUtils.QuaternionFromOrtho6d(new double3x2(c0, c1), m_TransposeOrtho6d);
                jointRotations[i] = rotation;
            }

            rootJointPosition = new Vector3(predictedHipPosition[0], predictedHipPosition[1], predictedHipPosition[2]);
        }
    }
}
