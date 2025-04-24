using System;
using System.Collections.Generic;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Unity.DeepPose.Components
{
    [ExecuteAlways]
    class DeepPoseComponent : MonoBehaviour
    {
        [Serializable]
        public struct Joint
        {
            public Transform Transform;
            public bool Stiff;
        }

        [SerializeField, Range(0f, 1f)]
        protected float m_Weight = 1f;

        [SerializeField]
        Animator m_Animator;

        [SerializeField]
        bool m_CcdEnabled = true;

        [SerializeField]
        bool m_RetargetingEnabled;

        [SerializeField, Range(0f, 1f)]
        float m_CcdWeight = 1f;

        [SerializeField]
        bool m_AutomaticRetargetingScale = true;

        [SerializeField]
        float m_RetargetingScale = 1f;

        [FormerlySerializedAs("m_NeuralConstraintData")]
        [SerializeField]
        DeepPoseSolverData m_DeepPoseSolverData;

        [SerializeField]
        List<Joint> m_Joints = new List<Joint>();

        Vector3[] m_JointLookAtDirections;

        public float weight
        {
            get => m_Weight;
            set => m_Weight = Mathf.Clamp01(value);
        }

        public ref DeepPoseSolverData DeepPoseSolverData => ref m_DeepPoseSolverData;
        public List<Joint> Joints => m_Joints;

        [SerializeField]
        public bool IsSolverAlwaysActive = true;

        public int Iterations = 1;

        DeepPoseSolver m_DeepPoseSolver;

        MultiCcdSolver m_MultiCcdSolver;
        MultiCcdData m_MultiCcdData = new MultiCcdData(1f, 10, 0f);

        RetargetingSolver m_RetargetingSolver;
        RetargetingData m_RetargetingData;
        float m_AutoScale;

        GameObject m_SourceSkeleton;

        void Reset()
        {
            m_Animator = GetComponentInParent<Animator>();
            if (m_Animator != null && SkeletonUtils.IsValidNonNullHumanAvatar(m_Animator.avatar))
                m_RetargetingEnabled = true;

            m_AutomaticRetargetingScale = true;
            m_RetargetingScale = 1f;

            m_Joints = new List<Joint>();
        }

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            Dispose();
        }

        void Initialize()
        {
            Assert.IsNotNull(m_DeepPoseSolverData.Config, "You must set a configuration");
            Assert.IsNotNull(m_DeepPoseSolverData.Config.Skeleton, "Configuration has no skeleton");
            Assert.IsTrue(m_DeepPoseSolverData.Config.IsValid(), "Configuration has no model");

            if (m_AutomaticRetargetingScale)
                m_RetargetingScale = 1f;
            m_DeepPoseSolverData.ReferencePoint = Vector3.zero;

            // By default, we use the joints set by the user
            // This will be overriden if retargeting is on, using internal skeleton joints instead
            m_DeepPoseSolverData.Joints = new List<Transform>(m_Joints.Count);
            foreach (var joint in m_Joints)
                m_DeepPoseSolverData.Joints.Add(joint.Transform);

            if (m_RetargetingEnabled)
                InitializeRetargeting();
            else if (m_CcdEnabled && m_Joints.Count > 0)
                InitializeCcd(m_Joints[0].Transform);

            if (m_DeepPoseSolver != null)
            {
                m_DeepPoseSolver.Dispose();
            }

            m_DeepPoseSolver = new DeepPoseSolver();
            m_DeepPoseSolver.Initialize(m_DeepPoseSolverData);

            if (m_RetargetingEnabled)
            {
                ComputeLookAtDirections();
                m_DeepPoseSolver.Solve(in m_DeepPoseSolverData);
                m_RetargetingSolver.Solve();
            }
        }

        void InitializeCcd(Transform targetTransform)
        {
            m_MultiCcdSolver = null;

            if (targetTransform == null)
                return;

            // Build look-up for stiff transforms
            var isStiff = new Dictionary<Transform, bool>();
            foreach (var joint in m_Joints)
            {
                if (joint.Transform == null)
                    continue;

                isStiff[joint.Transform] = joint.Stiff;
            }

            var allTransforms = targetTransform.GetAllChildren();
            m_MultiCcdData.Joints = new List<DynamicCcdJoint>();
            foreach (var t in allTransforms)
            {
                if (!isStiff.TryGetValue(t, out var stiff))
                    stiff = false;

                m_MultiCcdData.Joints.Add(new DynamicCcdJoint(t, stiff));
            }

            // Include all positional effectors in CCD
            m_MultiCcdData.Positions = new List<DynamicCcdEffector>();
            foreach (var effector in m_DeepPoseSolverData.Positions)
            {
                var effectorTransform = effector.transform;
                var jointTransform = m_Joints[effector.id].Transform;
                var jointId = allTransforms.IndexOf(jointTransform);

                m_MultiCcdData.Positions.Add(new DynamicCcdEffector(effectorTransform, jointId, effector.weight, 0f, false));
            }

            m_MultiCcdSolver = new MultiCcdSolver();
            m_MultiCcdSolver.Initialize(in m_MultiCcdData);
        }

        void InitializeRetargeting()
        {
            if (!m_RetargetingEnabled)
                return;

            var skeleton = m_DeepPoseSolverData.Config.Skeleton;

            if (m_Animator != null && SkeletonUtils.IsValidNonNullHumanAvatar(m_Animator.avatar))
            {
                m_RetargetingData.TargetAvatar = m_Animator.avatar;
                m_RetargetingData.TargetTransform = m_Animator.transform;
            }

            m_SourceSkeleton = skeleton.Instantiate("SourceSkeleton");
            m_SourceSkeleton.transform.SetParent(transform, false);
            m_SourceSkeleton.hideFlags = HideFlags.HideAndDontSave;

            m_RetargetingData.SourceAvatar = m_DeepPoseSolverData.Config.Avatar;
            m_RetargetingData.SourceTransform = m_SourceSkeleton.transform;

            m_RetargetingSolver = new RetargetingSolver();
            m_RetargetingSolver.Initialize(m_RetargetingData);

            // Initialize CCD
            var rootTransform = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
            InitializeCcd(rootTransform);

            // Compute scaling
            var sourceHumanScale = m_RetargetingData.SourceAvatar != null ? m_RetargetingData.SourceAvatar.GetHumanScale() : 1f;
            var targetHumanScale = m_RetargetingData.TargetAvatar != null ? m_RetargetingData.TargetAvatar.GetHumanScale() : 1f;
            m_AutoScale = sourceHumanScale / targetHumanScale;

            // When retargeting is performed, the joints used by the neural solver are always that of its internal skeleton
            var jointTransforms = skeleton.FindTransforms(m_SourceSkeleton.transform);
            m_DeepPoseSolverData.Joints = new List<Transform>();
            for (var i = 0; i < skeleton.Count; i++)
            {
                var joint = skeleton.FindJoint(i);
                Assert.IsNotNull(joint, $"Cannot find joint with index: {i}");

                jointTransforms.TryGetValue(joint, out var jointTransform);
                m_DeepPoseSolverData.Joints.Add(jointTransform);
            }

            ComputeLookAtDirections();
        }

        void ComputeLookAtDirections()
        {
            ResetToBindPose();

            m_JointLookAtDirections = new Vector3[Joints.Count];

            for (var i = 0; i < Joints.Count; i++)
            {
                var targetTransform = Joints[i].Transform;
                if (targetTransform == null)
                {
                    m_JointLookAtDirections[i] = DeepPoseSolver.DefaultLookAtDirection;
                }
                else
                {
                    var sourceTransform = m_DeepPoseSolverData.Joints[i];
                    var worldLookAtDirection = sourceTransform.rotation * DeepPoseSolver.DefaultLookAtDirection;
                    var localLookAtDirection = Quaternion.Inverse(targetTransform.rotation) * worldLookAtDirection;
                    m_JointLookAtDirections[i] = localLookAtDirection;
                }
            }
        }

        void ResetToBindPose()
        {
            for (var i = 0; i < m_DeepPoseSolverData.Joints.Count; i++)
            {
                var jointTransform = m_DeepPoseSolverData.Joints[i];
                jointTransform.localRotation = Quaternion.identity;
            }

            if (m_RetargetingEnabled && IsRetargetingValid() && m_RetargetingSolver != null)
            {
                m_RetargetingSolver.Solve();
            }
        }

        void Dispose()
        {
            if (m_DeepPoseSolver != null)
            {
                m_DeepPoseSolver.Dispose();
                m_DeepPoseSolver = null;
            }

            DisposeRetargeting();
        }

        void DisposeRetargeting()
        {
            if (m_RetargetingSolver != null)
            {
                m_RetargetingSolver.Dispose();
                m_RetargetingSolver = null;
            }

            m_MultiCcdSolver = null;

            if (m_SourceSkeleton != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(m_SourceSkeleton);
#else
                Destroy(m_SourceSkeleton);
#endif
            }
        }

        void LateUpdate()
        {
            if (IsSolverAlwaysActive)
            {
                Solve();
            }
        }

        public void Solve()
        {
            if (m_DeepPoseSolver == null)
                return;

            if (m_DeepPoseSolverData.EnabledPositionEffectorsCount() == 0)
                return;

            if (m_RetargetingEnabled && m_RetargetingSolver.IsValid)
            {
                if (m_AutomaticRetargetingScale)
                    m_RetargetingScale = m_AutoScale; // Note: updates serialized value so that it shows in the UI
                m_DeepPoseSolverData.Scaling = m_RetargetingScale;
                m_DeepPoseSolverData.ReferencePoint = m_RetargetingData.TargetTransform.position;
            }
            else
            {
                m_DeepPoseSolverData.Scaling = 1f;
                m_DeepPoseSolverData.ReferencePoint = Vector3.zero;
            }

            // Solve the deep pose solver
            m_DeepPoseSolver.Weight = weight;

            for (var i = 0; i < Iterations; i++)
                m_DeepPoseSolver.Solve(in m_DeepPoseSolverData);

            if (m_RetargetingEnabled)
            {
                if (m_RetargetingSolver.IsValid)
                {
                    SyncCcdWeights();
                    m_RetargetingSolver.Solve();
                    m_MultiCcdSolver?.Solve(in m_MultiCcdData);
                }
            }
            else if (m_CcdEnabled && m_MultiCcdSolver != null)
            {
                SyncCcdWeights();
                m_MultiCcdSolver?.Solve(in m_MultiCcdData);
            }
        }

        void SyncCcdWeights()
        {
            for (var i = 0; i < m_DeepPoseSolverData.Positions.Count; i++)
            {
                var effectorWeight = m_DeepPoseSolverData.Positions[i].weight;
                var effectorTolerance = m_DeepPoseSolverData.Positions[i].tolerance;

                var effector = m_MultiCcdData.Positions[i];
                effector.Weight = effectorTolerance > 1e-4f ? 0f : effectorWeight;
                effector.Tolerance = effectorTolerance;
                m_MultiCcdData.Positions[i] = effector;
            }

            m_MultiCcdData.Weight = m_CcdWeight;
        }

        void OnDrawGizmos()
        {
            if (!m_RetargetingEnabled)
                m_DeepPoseSolver?.DrawGizmos(in m_DeepPoseSolverData);
        }

        public void FindJoints()
        {
            if (m_RetargetingEnabled)
                FindJointsForRetargeting();
            else
                FindJointsWithoutRetargeting();
        }

        void FindJointsWithoutRetargeting()
        {
            if (!HasValidSkeleton())
                return;

            InitializeJointsListIfNeeded();

            // Find root of skeleton (or some parent of the root)
            var skeleton = m_DeepPoseSolverData.Config.Skeleton;
            var rootTransform = SkeletonUtils.FindRootTransform(gameObject);
            var jointTransforms = skeleton.FindTransforms(rootTransform);

            // For each joint, find a transform with matching name
            for (var i = 0; i < skeleton.Count; i++)
            {
                var joint = skeleton.FindJoint(i);
                Assert.IsNotNull(joint, $"Cannot find joint with index: {i}");

                var jointData = m_Joints[i];
                jointData.Transform = jointTransforms.TryGetValue(joint, out var jointTransform) ? jointTransform : null;
                m_Joints[i] = jointData;
            }
        }

        void FindJointsForRetargeting()
        {
            if (!IsRetargetingValid())
                return;

            var sourceToAvatar = m_DeepPoseSolverData.Config.Avatar.RigToAvatarNames();
            var targetAvatarTransforms = m_Animator.GetAvatarJointTransforms();

            InitializeJointsListIfNeeded();

            // Find joints
            for (var i = 0; i < m_DeepPoseSolverData.Config.Skeleton.Count; i++)
            {
                var joint = m_DeepPoseSolverData.Config.Skeleton.FindJoint(i);
                Assert.IsNotNull(joint, $"Cannot find joint with index: {i}");

                Transform targetTransform = null;
                if (sourceToAvatar.TryGetValue(joint.Name, out var avatarJointName))
                    targetAvatarTransforms.TryGetValue(avatarJointName, out targetTransform);

                var myJoint = m_Joints[i];
                myJoint.Transform = targetTransform;
                m_Joints[i] = myJoint;
            }
        }

        void InitializeJointsListIfNeeded()
        {
            // Resize joints list as needed
            if (m_Joints.Count != m_DeepPoseSolverData.Config.Skeleton.Count)
            {
                m_Joints = new List<Joint>();
                for (var i = 0; i < m_DeepPoseSolverData.Config.Skeleton.Count; i++)
                    m_Joints.Add(new Joint { Transform = null, Stiff = false });
            }
        }

        bool HasValidSkeleton()
        {
            return m_DeepPoseSolverData.Config != null && m_DeepPoseSolverData.Config.Skeleton != null;
        }

        bool IsRetargetingValid()
        {
            return HasValidSkeleton()
                && SkeletonUtils.IsValidNonNullHumanAvatar(m_DeepPoseSolverData.Config.Avatar)
                && m_Animator != null
                && SkeletonUtils.IsValidNonNullHumanAvatar(m_Animator.avatar);
        }

        public void SetupAllEffectors()
        {
            var config = m_DeepPoseSolverData.Config;

            if (config == null || config.Skeleton == null)
                return;

            var effectorRoot = new GameObject("Effectors");
            effectorRoot.transform.SetParent(transform, false);

            m_DeepPoseSolverData.RemoveAllPositionalEffectors();
            m_DeepPoseSolverData.RemoveAllRotationalEffectors();
            m_DeepPoseSolverData.RemoveAllLookAtEffectors();

            for (var idx = 0; idx < config.Skeleton.Count; idx++)
            {
                var joint = config.Skeleton.FindJoint(idx);

                var isPositional = config.JointsWithPosition.Contains(joint.Index);
                var isRotational = config.JointsWithRotation.Contains(joint.Index);
                var isLookAt = config.JointsWithLookAt.Contains(joint.Index);

                if (isPositional || isRotational)
                {
                    var objectName = joint.Name + "_" + (isPositional ? "Pos" : "") + (isRotational ? "Rot" : "");
                    var effectorObject = new GameObject(objectName);
                    effectorObject.transform.SetParent(effectorRoot.transform, false);

                    if (isPositional)
                        m_DeepPoseSolverData.AddPositionalEffector(effectorObject.transform, joint.Index, 0f, 0f);
                    if (isRotational)
                        m_DeepPoseSolverData.AddRotationalEffector(effectorObject.transform, joint.Index, 0f, 0f);

                    var effectorDisplay = effectorObject.AddComponent<DeepPoseEffectorDisplay>();
                    effectorDisplay.Component = this;
                    effectorDisplay.AutoSnap = true;
                    effectorDisplay.FollowPosition = !isPositional;
                    effectorDisplay.FollowRotation = !isRotational;
                    effectorDisplay.Color = isRotational ? new Color(1f, 0f, 1f, 0.6f) : new Color(1f, 0.5f, 0f, 0.6f);
                }

                if (isLookAt)
                {
                    var effectorObject = new GameObject(joint.Name + "_LookAt");
                    effectorObject.transform.SetParent(effectorRoot.transform, false);

                    m_DeepPoseSolverData.AddLookAtEffector(effectorObject.transform, joint.Index, 0f, 0f);

                    var effectorDisplay = effectorObject.AddComponent<DeepPoseEffectorDisplay>();
                    effectorDisplay.Component = this;
                    effectorDisplay.AutoSnap = true;
                    effectorDisplay.FollowPosition = false;
                    effectorDisplay.FollowRotation = false;
                    effectorDisplay.Color = new Color(0f, 1.0f, 0.5f, 0.6f);
                }
            }
        }

        public void SnapAllEffectors()
        {
            foreach (var effector in m_DeepPoseSolverData.Positions)
            {
                SnapEffector(effector, true, false, false);
            }

            foreach (var effector in m_DeepPoseSolverData.Rotations)
            {
                SnapEffector(effector, false, true, false);
            }

            foreach (var effector in m_DeepPoseSolverData.LookAts)
            {
                SnapEffector(effector, false, false, true);
            }
        }

        void SnapEffector(Effector effector, bool snapPosition, bool snapRotation, bool snapLookAt)
        {
            if (effector.transform == null)
                return;

            if (effector.id < 0 || effector.id >= m_Joints.Count)
                return;

            var jointTransform = m_Joints[effector.id].Transform;
            if (jointTransform == null)
                return;

            if (snapPosition)
                effector.transform.position = jointTransform.position;

            if (snapRotation)
                effector.transform.rotation = jointTransform.rotation;

            if (snapLookAt)
            {
                var lookAtVector = GetLookAtDirection(effector.id);
                var direction = (jointTransform.rotation * effector.transform.localRotation) * lookAtVector;
                effector.transform.position = jointTransform.position + direction;
            }
        }

        public Vector3 GetLookAtDirection(int jointId)
        {
            if (!m_RetargetingEnabled || !IsRetargetingValid() || m_JointLookAtDirections == null || jointId >= m_JointLookAtDirections.Length)
                return DeepPoseSolver.DefaultLookAtDirection;

            return m_JointLookAtDirections[jointId];
        }

        public Effector GetPositionEffector(int solverListIndex)
        {
            return m_DeepPoseSolverData.Positions[solverListIndex];
        }

        public Effector GetLookAtEffector(int solverListIndex)
        {
            return m_DeepPoseSolverData.LookAts[solverListIndex];
        }
    }
}
