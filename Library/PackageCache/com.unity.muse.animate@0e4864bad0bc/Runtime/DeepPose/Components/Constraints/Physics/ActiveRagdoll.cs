using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Components
{
    class ActiveRagdoll
    {
        [Serializable]
        public struct RigidBodySettings
        {
            public float MaxAngularVelocity;
            public CollisionDetectionMode CollisionDetectionMode;
            public int SolverVelocityIterations;
            public int SolverIterations;

            public static RigidBodySettings Default = new RigidBodySettings(100f, 5, 10);

            public RigidBodySettings(float maxAngularVelocity, int solverIterations, int solverVelocityIterations,
                CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete)
            {
                MaxAngularVelocity = maxAngularVelocity;
                CollisionDetectionMode = collisionDetectionMode;
                SolverIterations = solverIterations;
                SolverVelocityIterations = solverVelocityIterations;
            }
        }

        [Serializable]
        public struct SimulationSettings
        {
            public bool ApplyForceToParent;
            public bool UseLocalTorque;
            public bool UseConfigurableJoint;

            public float MaxForce;
            public float MaxTorque;

            public float PositionTolerance;
            public float RotationTolerance;

            public float ForceFactor;
            public float TorqueFactor;

            public ForceMode PositionForceMode;
            public ForceMode RotationForceMode;

            public static SimulationSettings Default = new SimulationSettings(0.008f, 1f, 100f, 100f);

            public SimulationSettings(float forceFactor, float torqueFactor, float maxForce, float maxTorque,
                ForceMode forceMode = ForceMode.VelocityChange, bool applyForceToParent = true,
                float positionTolerance = 1e-3f, float rotationTolerance = 5e-3f)
            {
                ForceFactor = forceFactor;
                TorqueFactor = torqueFactor;

                MaxForce = maxForce;
                MaxTorque = maxTorque;

                PositionTolerance = positionTolerance;
                RotationTolerance = rotationTolerance;

                PositionForceMode = forceMode;
                RotationForceMode = forceMode;

                ApplyForceToParent = applyForceToParent;
                UseConfigurableJoint = false;
                UseLocalTorque = false;
            }
        }

        struct TransformData
        {
            public Transform TargetTransform;
            public Transform Transform;
            public Rigidbody Rigidbody;
            public Rigidbody ParentRigidbody;
            public ConfigurableJoint ConfigurableJoint;
            public Quaternion OriginalGlobalRotation;
            public Quaternion OriginalLocalRotation;
            public int Depth;
            public bool MustBeKinematic;
        }

        List<TransformData> m_Transforms = new();
        Transform m_TargetRoot;
        Transform m_PhysicsRoot;
        SimulationSettings m_SimulationSettings = SimulationSettings.Default;
        RigidBodySettings m_RigidBodySettings = RigidBodySettings.Default;

        public Transform TargetRoot => m_TargetRoot;
        public Transform PhysicsRoot => m_PhysicsRoot;

        public SimulationSettings SimulationConfig
        {
            get => m_SimulationSettings;
            set => m_SimulationSettings = value;
        }

        public RigidBodySettings RigidBodyConfig
        {
            get => m_RigidBodySettings;
            set => m_RigidBodySettings = value;
        }

        public ActiveRagdoll()
        { }

        public void Initialize(Transform physicsRoot, Transform targetRoot)
        {
            Assert.IsNotNull(physicsRoot, "You must specify a physics root");
            Assert.IsNotNull(targetRoot, "You must specify a target root");

            m_PhysicsRoot = physicsRoot;
            m_TargetRoot = targetRoot;

            BuildTransformData();
            SetupRigidBodies();
        }

        public bool IsValid => m_PhysicsRoot.gameObject.activeInHierarchy && m_TargetRoot.gameObject.activeInHierarchy;

        public bool ApplyForces(float deltaTime)
        {
            if (!IsValid)
                return false;

            var anyForceApplied = false;

            foreach (var transformData in m_Transforms)
            {
                if (transformData.Rigidbody == null)
                {
                    MatchTransform(in transformData);
                }
                else if (transformData.MustBeKinematic)
                {
                    MatchTransform(in transformData);
                }
                else
                {
                    if (!m_SimulationSettings.UseConfigurableJoint || transformData.ConfigurableJoint == null)
                    {
                        if (ApplyForce(in transformData, deltaTime))
                            anyForceApplied = true;
                    }
                    else
                    {
                        if (ApplyJointTarget(in transformData))
                            anyForceApplied = true;
                    }
                }
            }

            return anyForceApplied;
        }

        void MatchTransform(in TransformData transformData)
        {
            if (transformData.Depth == 0)
            {
                transformData.Transform.position = transformData.TargetTransform.position;
                transformData.Transform.rotation = transformData.TargetTransform.rotation;
            }
            else
            {
                transformData.Transform.localPosition = transformData.TargetTransform.localPosition;
                transformData.Transform.localRotation = transformData.TargetTransform.localRotation;
            }
        }

        bool ApplyForce(in TransformData transformData, float deltaTime)
        {
            var anyForceApplied = false;
            var positionDelta = transformData.TargetTransform.position - transformData.Rigidbody.position;
            if (positionDelta.magnitude > m_SimulationSettings.PositionTolerance)
            {
                var velocity = m_SimulationSettings.ForceFactor * (positionDelta / deltaTime);
                var velocityMagnitude = velocity.magnitude;
                if (velocityMagnitude > m_SimulationSettings.MaxForce)
                {
                    velocity = m_SimulationSettings.MaxForce * (velocity / velocityMagnitude);
                }

                transformData.Rigidbody.AddForceAtPosition(velocity, transformData.Rigidbody.position, m_SimulationSettings.PositionForceMode);
                if (m_SimulationSettings.ApplyForceToParent && transformData.ParentRigidbody != null)
                    transformData.ParentRigidbody.AddForceAtPosition(velocity, transformData.Rigidbody.position, m_SimulationSettings.PositionForceMode);

                anyForceApplied = true;
            }

            var sourceRotation = transformData.Rigidbody.rotation;
            var targetRotation = transformData.TargetTransform.rotation;

            var useLocalTorque = false;
            if (m_SimulationSettings.UseLocalTorque && transformData.ParentRigidbody != null)
            {
                sourceRotation = Quaternion.Inverse(transformData.ParentRigidbody.rotation) * transformData.Rigidbody.rotation;
                targetRotation = transformData.TargetTransform.localRotation;
                useLocalTorque = true;
            }

            GetAxisAngleRotation(sourceRotation, targetRotation, out var angle, out var axis);
            if (Mathf.Abs(angle) > m_SimulationSettings.RotationTolerance)
            {
                var angularVelocity = (m_SimulationSettings.TorqueFactor / deltaTime) * angle * axis;

                var angularVelocityMagnitude = angularVelocity.magnitude;
                if (angularVelocityMagnitude > m_SimulationSettings.MaxTorque)
                {
                    angularVelocity = m_SimulationSettings.MaxTorque *
                        (angularVelocity / angularVelocityMagnitude);
                }

                if (useLocalTorque)
                {
                    transformData.Rigidbody.AddRelativeTorque(angularVelocity, m_SimulationSettings.RotationForceMode);
                }
                else
                {
                    transformData.Rigidbody.AddTorque(angularVelocity, m_SimulationSettings.RotationForceMode);
                }

                anyForceApplied = true;
            }

            return anyForceApplied;
        }

        bool ApplyJointTarget(in TransformData transformData)
        {
            if (transformData.ConfigurableJoint == null)
                return false;

            if (transformData.Depth == 0)
                transformData.ConfigurableJoint.targetPosition = transformData.TargetTransform.position;

            if (transformData.ConfigurableJoint.configuredInWorldSpace)
            {
                transformData.ConfigurableJoint.SetTargetRotation(transformData.TargetTransform.rotation, transformData.OriginalGlobalRotation);
            }
            else
            {
                transformData.ConfigurableJoint.SetTargetRotationLocal(transformData.TargetTransform.localRotation, transformData.OriginalLocalRotation);
            }

            return true;
        }

        void SetupRigidBodies()
        {
            foreach (var data in m_Transforms)
            {
                if (data.Rigidbody == null)
                    continue;

                SetKinematic(data.Rigidbody, true);

                if (data.MustBeKinematic)
                    continue;

                data.Rigidbody.maxAngularVelocity = m_RigidBodySettings.MaxAngularVelocity;
                data.Rigidbody.interpolation = RigidbodyInterpolation.None;
                //data.Rigidbody.drag = 0f;
                //data.Rigidbody.angularDrag = 0f;
                data.Rigidbody.collisionDetectionMode = m_RigidBodySettings.CollisionDetectionMode;
                data.Rigidbody.solverVelocityIterations = m_RigidBodySettings.SolverVelocityIterations;
                data.Rigidbody.solverIterations = m_RigidBodySettings.SolverIterations;
            }
        }

        public void SnapToTarget()
        {
            foreach (var transformData in m_Transforms)
            {
                if (transformData.Rigidbody != null)
                {
                    transformData.Rigidbody.position = transformData.TargetTransform.position;
                    transformData.Rigidbody.rotation = transformData.TargetTransform.rotation;
                }

                transformData.Transform.position = transformData.TargetTransform.position;
                transformData.Transform.rotation = transformData.TargetTransform.rotation;
            }
        }

        public void FreezeRigidBodies(bool freeze = true)
        {
            foreach (var data in m_Transforms)
            {
                if (data.Rigidbody == null)
                    continue;

                SetKinematic(data.Rigidbody, data.MustBeKinematic || freeze);

                if (!freeze)
                    data.Rigidbody.WakeUp();
            }
        }

        public void ResetVelocities()
        {
            foreach (var transformData in m_Transforms)
            {
                if (transformData.Rigidbody == null || transformData.Rigidbody.isKinematic)
                    continue;

#if UNITY_6000_0_OR_NEWER
                transformData.Rigidbody.linearVelocity = Vector3.zero;
#else
                transformData.Rigidbody.velocity = Vector3.zero;
#endif
                transformData.Rigidbody.angularVelocity = Vector3.zero;
            }
        }

        static void GetAxisAngleRotation(Quaternion source, Quaternion target, out float angle, out Vector3 axis)
        {
            var delta = target * Quaternion.Inverse(source);
            delta.ToAngleAxis(out angle, out axis);

            // already aligned
            if (float.IsInfinity(axis.x))
            {
                angle = 0f;
                axis = Vector3.zero;
                return;
            }

            if (angle > 180f)
                angle -= 360f;

            angle = Mathf.Deg2Rad * angle;
            axis = axis.normalized;
        }

        void BuildTransformData()
        {
            var allMeshColliders = m_PhysicsRoot.GetComponentsInChildren<MeshCollider>();

            m_Transforms.Clear();
            BuildTransformDataRecursive(m_PhysicsRoot, "", 0, allMeshColliders);

            // Sort by depth
            m_Transforms.Sort((x, y) => x.Depth.CompareTo(y.Depth));
        }

        void BuildTransformDataRecursive(Transform parentTransform, string parentPath, int parentDepth, MeshCollider[] allMeshColliders)
        {
            AddTransformData(parentTransform, parentPath, parentDepth, allMeshColliders);

            foreach (Transform childTransform in parentTransform)
            {
                var childPath = string.IsNullOrEmpty(parentPath)
                    ? childTransform.name
                    : parentPath + "/" + childTransform.name;
                BuildTransformDataRecursive(childTransform, childPath, parentDepth + 1, allMeshColliders);
            }
        }

        void AddTransformData(Transform transform, string transformPath, int depth, MeshCollider[] allMeshColliders)
        {
            var targetTransform = m_TargetRoot.Find(transformPath);
            if (targetTransform == null)
                return;

            var parent = transform.parent;

            var rigidBody = transform.GetComponent<Rigidbody>();
            var mustBeKinematic = false;
            if (rigidBody != null)
            {
                foreach (var meshCollider in allMeshColliders)
                {
                    var colliderRigidBody = meshCollider.GetComponentInParent<Rigidbody>(true);
                    if (colliderRigidBody != rigidBody)
                        continue;

                    if (!meshCollider.convex)
                    {
                        mustBeKinematic = true;
                        break;
                    }
                }
            }

            var transformData = new TransformData();
            transformData.Rigidbody = rigidBody;
            transformData.ParentRigidbody = parent == null ? null : parent.GetComponent<Rigidbody>();
            transformData.Transform = transform;
            transformData.TargetTransform = targetTransform;
            transformData.Depth = depth;
            transformData.ConfigurableJoint = transform.GetComponent<ConfigurableJoint>();
            transformData.OriginalGlobalRotation = transform.rotation;
            transformData.OriginalLocalRotation = transform.localRotation;
            transformData.MustBeKinematic = mustBeKinematic;
            m_Transforms.Add(transformData);
        }

        static void SetKinematic(Rigidbody rigidBody, bool kinematic)
        {
            rigidBody.collisionDetectionMode = kinematic ? CollisionDetectionMode.Discrete : CollisionDetectionMode.Continuous;
            rigidBody.isKinematic = kinematic;
        }
    }
}
