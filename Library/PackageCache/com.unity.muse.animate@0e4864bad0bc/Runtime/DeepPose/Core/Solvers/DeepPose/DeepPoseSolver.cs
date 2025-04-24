using System;
using Unity.DeepPose.ModelBackend;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class DeepPoseSolver : IDisposable
    {
        public static Vector3 DefaultLookAtDirection = Vector3.forward;

        public float Weight { get; set; }
        public bool IsCreated { get; private set; }

        DeepPoseData m_Data;
        IModelBackend m_Backend;

        public void Initialize(in DeepPoseSolverData data)
        {
            Assert.IsFalse(IsCreated, "Already initialized");

            var modelDefinition = new ModelDefinition(data.Config.Model);
            m_Backend = new ModelBackend.ModelBackend(modelDefinition, data.Config.BarracudaBackend);
            m_Data = new DeepPoseData(data.Joints.Count, data.Positions.Count, data.Rotations.Count, data.LookAts.Count, data.Config.Additive, data.Config.TransposeOrtho6D, data.Config.UseRaycast, data.Config.MaxRayDistance);
            IsCreated = true;
        }

        public void Dispose()
        {
            Assert.IsTrue(IsCreated, "Disposed without being initialized");

            if (m_Data.IsCreated)
                m_Data.Dispose();

            m_Backend?.Dispose();
            
            IsCreated = false;
        }

        void ReadCurrentPose(in DeepPoseSolverData data)
        {
            for (int i = 0; i < data.Joints.Count; i++)
            {
                var id = i;
                var position = data.Joints[i].position;
                var rotation = data.Joints[i].localRotation;
                if (id == 0) // Root joint
                    rotation = data.Joints[i].rotation; // Use world transform

                m_Data.SetPoseJoint(id, position, rotation);
            }
        }

        void ReadEffectors(in DeepPoseSolverData data)
        {
            // Position effectors
            for (var i = 0; i < data.Positions.Count; i++)
            {
                var id = data.Positions[i].id;
                var weight = data.Positions[i].weight;
                var tolerance = data.Positions[i].tolerance;
                var transform = data.Positions[i].transform;
                var position = data.Scaling * (transform.position - data.ReferencePoint) + data.ReferencePoint;
                if (data.Config.Additive)
                    position = position - data.Joints[id].position;
                var raycastDistance = data.Config.UseRaycast ? RaycastsUtils.GetRaycastDistance(transform.position, data.Config.MaxRayDistance) : 0f;  //TODO: how does that work with retargeting ???
                m_Data.SetPositionEffector(i, id, position, weight, tolerance, raycastDistance);
            }

            // Rotation effectors
            for (var i = 0; i < data.Rotations.Count; i++)
            {
                var id = data.Rotations[i].id;
                var weight = data.Rotations[i].weight;
                var tolerance = data.Rotations[i].tolerance;
                var transform = data.Rotations[i].transform;
                var rotation = transform.rotation;
                if (data.Config.Additive)
                    rotation = rotation * Quaternion.Inverse(data.Joints[id].rotation);
                m_Data.SetRotationEffector(i, id, rotation, weight, tolerance);
            }

            // Look-at effectors
            for (var i = 0; i < data.LookAts.Count; i++)
            {
                var id = data.LookAts[i].id;
                var weight = data.LookAts[i].weight;
                var tolerance = data.LookAts[i].tolerance;
                var transform = data.LookAts[i].transform;
                var targetPosition = data.Scaling * (transform.position - data.ReferencePoint) + data.ReferencePoint;
                var localDirection = data.Config.GeneralizedLookAt ? transform.localRotation * DefaultLookAtDirection : DefaultLookAtDirection;
                if (data.Config.Additive)
                    targetPosition = targetPosition - data.Joints[id].position;
                m_Data.SetLookAtEffector(i, id, targetPosition, localDirection, weight, tolerance);
            }
        }

        public void Solve(in DeepPoseSolverData data)
        {
            if (Weight <= float.Epsilon)
                return;

            if (data.Config.Additive)
                ReadCurrentPose(in data);
            ReadEffectors(in data);

            // Solve
            m_Data.Evaluate(m_Backend);

            // Apply results
            data.Joints[0].position = Vector3.Lerp(data.Joints[0].position, m_Data.rootJointPosition, Weight);
            data.Joints[0].rotation = Quaternion.Lerp(data.Joints[0].rotation, m_Data.jointRotations[0], Weight);
            for (int i = 1; i < data.Joints.Count; i++)
            {
                data.Joints[i].localRotation = Quaternion.Lerp(data.Joints[i].localRotation, m_Data.jointRotations[i], Weight);
            }
        }

        public void DrawGizmos(in DeepPoseSolverData data)
        {
            // Position effectors
            for (int i = 0; i < data.Positions.Count; i++)
            {
                var id = data.Positions[i].id;
                var transform = data.Positions[i].transform;
                var position = transform.position;
                Gizmos.DrawLine(data.Joints[id].position, position);
            }
        }
    }
}
