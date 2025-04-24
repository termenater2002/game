using System;
using System.Collections.Generic;
using System.Linq;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;


namespace Unity.DeepPose.Components
{
    class MotionCompletionComponent : MonoBehaviour
    {
        public bool IsRunningAsyncSolve => m_MotionSolver?.IsSolving ?? false;
        public float AsyncSolveProgress => m_MotionSolver?.SolveProgress ?? 0f;

        [Serializable]
        public struct Joint
        {
            public Transform Transform;

            public Joint(Transform transform)
            {
                Transform = transform;
            }
        }

        [SerializeField]
        CompletionConfiguration m_Config;

        [SerializeField]
        List<Joint> m_Joints = new ();

        CompletionSolver m_MotionSolver;

        public void SetFrameTimes(List<int> inputTimeIndices, int inputReferenceIdx, List<int> targetTimeIndices)
        {
            m_MotionSolver.SetInputAndTargetFrameTimes(inputTimeIndices, targetTimeIndices);
            m_MotionSolver.SetInputReferenceFrame(inputReferenceIdx);
        }

        public void SetInputFrameTimes(List<int> timeIndices, int referenceIdx)
        {
            m_MotionSolver.SetInputFrameTimes(timeIndices);
            m_MotionSolver.SetInputReferenceFrame(referenceIdx);
        }

        public void SetTargetFrameTimes(List<int> timeIndices)
        {
            m_MotionSolver.SetTargetFrameTimes(timeIndices);
        }

        public void SetFrameTimes(int[] inputTimeIndices, int inputReferenceIdx, int[] targetTimeIndices)
        {
            m_MotionSolver.SetInputAndTargetFrameTimes(inputTimeIndices, targetTimeIndices);
            m_MotionSolver.SetInputReferenceFrame(inputReferenceIdx);
        }

        public void SetInputFrameTimes(int[] timeIndices, int referenceIdx)
        {
            m_MotionSolver.SetInputFrameTimes(timeIndices);
            m_MotionSolver.SetInputReferenceFrame(referenceIdx);
        }

        public void SetTargetFrameTimes(int[] timeIndices)
        {
            m_MotionSolver.SetTargetFrameTimes(timeIndices);
        }

        public void CapturePose(int inputFrameIdx)
        {
            m_MotionSolver.CaptureInputFramePose(inputFrameIdx);
        }

        public void Solve()
        {
            m_MotionSolver.Solve();
        }

        public void StartAsyncSolve(int syncProgressEachNthLayer = 0)
        {
            m_MotionSolver.StartAsyncSolve(-1, syncProgressEachNthLayer);
        }

        public void StopAsyncSolve()
        {
            m_MotionSolver.StopAsyncSolve();
        }

        public bool StepAsyncSolve()
        {
            return m_MotionSolver.StepAsyncSolve();
        }

        public void Apply(int frame)
        {
            m_MotionSolver.Solve(frame);
        }

        void OnEnable()
        {
            InitializeSolver();
        }

        void OnDisable()
        {
            DisposeSolver();
        }

        void InitializeSolver()
        {
            DisposeSolver();

            Assert.IsNotNull(m_Config, "You must set a configuration");
            Assert.IsNotNull(m_Config.Skeleton, "Configuration has no skeleton");

            var solverSettings = new CompletionSolverSettings();
            solverSettings.Config = m_Config;
            solverSettings.Joints = m_Joints.Select(x => x.Transform).ToList();

            m_MotionSolver = new CompletionSolver();
            m_MotionSolver.Initialize(solverSettings);
        }

        void DisposeSolver()
        {
            StopAsyncSolve();
            
            if (m_MotionSolver == null)
                return;

            m_MotionSolver.Dispose();
            m_MotionSolver = null;
        }

        public void FindJoints()
        {
            if (!HasValidSkeleton())
                return;

            InitializeJointsListIfNeeded();

            // Find root of skeleton (or some parent of the root)
            var skeleton = m_Config.Skeleton;
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

        bool HasValidSkeleton()
        {
            return m_Config != null && m_Config.Skeleton != null;
        }

        void InitializeJointsListIfNeeded()
        {
            // Resize joints list as needed
            if (m_Joints.Count != m_Config.Skeleton.Count)
            {
                m_Joints = new List<Joint>();
                for (var i = 0; i < m_Config.Skeleton.Count; i++)
                    m_Joints.Add(new Joint(null));
            }
        }
    }
}
