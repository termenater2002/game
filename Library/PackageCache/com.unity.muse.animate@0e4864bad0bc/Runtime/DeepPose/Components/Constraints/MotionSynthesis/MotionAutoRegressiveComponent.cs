using System;
using System.Collections.Generic;
using System.Linq;
using Unity.DeepPose.Core;
using UnityEngine;
using UnityEngine.Assertions;


namespace Unity.DeepPose.Components
{
    class MotionAutoRegressiveComponent : MonoBehaviour
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
        AutoregressiveConfiguration m_Config;

        [SerializeField]
        List<Joint> m_Joints = new ();

        AutoregressiveSolver m_MotionSolver;
        bool m_IsSolved;
        int m_LastFrameSolved;

        public void Solve(int lastFrameIdx)
        {
            m_MotionSolver.Solve(lastFrameIdx);
            m_IsSolved = true;
            m_LastFrameSolved = lastFrameIdx;
        }

        public void StartAsyncSolve(int lastFrameIdx, int syncProgressEachNthLayer = 0)
        {
            m_MotionSolver.StartAsyncSolve(lastFrameIdx, -1, syncProgressEachNthLayer);
            m_IsSolved = false;
            m_LastFrameSolved = lastFrameIdx;
        }

        public void StopAsyncSolve()
        {
            m_MotionSolver.StopAsyncSolve();
        }

        public bool StepAsyncSolve()
        {
            m_IsSolved = m_MotionSolver.StepAsyncSolve();
            return m_IsSolved;
        }

        public void Apply(int frame)
        {
            Assert.IsTrue(m_IsSolved, "Solver must solve first");
            m_MotionSolver.ApplySolvedPoseToJoints(frame);
        }

        public void ClearInputFrames()
        {
            m_MotionSolver.SetInputLength(0);
        }

        public void RemoveAllInputsBefore(int lastPastFrameIdx, bool inclusive = false)
        {
            m_MotionSolver.RemoveAllInputsBefore(lastPastFrameIdx, inclusive);
        }

        public void RemoveAllInputsAfter(int frameIdx, bool inclusive = false)
        {
            m_MotionSolver.RemoveAllInputsAfter(frameIdx, inclusive);
        }

        public void CaptureInputPose(int frameTimeIdx, bool setAsReferenceFrame = false)
        {
            var frameIdx = m_MotionSolver.GetOrInsertFrame(frameTimeIdx);
            m_MotionSolver.CaptureInputFrame(frameIdx);

            if (setAsReferenceFrame)
                m_MotionSolver.SetInputReferenceFrame(frameIdx);
        }

        void OnEnable()
        {
            InitializeSolver();
        }

        void OnDisable()
        {
            DisposeSolver();
            m_IsSolved = false;
        }

        void InitializeSolver()
        {
            DisposeSolver();

            Assert.IsNotNull(m_Config, "You must set a configuration");
            Assert.IsNotNull(m_Config.Skeleton, "Configuration has no skeleton");

            var solverSettings = new AutoregressiveSolverSettings();
            solverSettings.Config = m_Config;
            solverSettings.Joints = m_Joints.Select(x => x.Transform).ToList();

            m_MotionSolver = new AutoregressiveSolver();
            m_MotionSolver.Initialize(solverSettings);
        }

        void DisposeSolver()
        {
            m_MotionSolver?.Dispose();
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
        
        public void SetInputTolerance(float tolerance)
        {
            for (var i = 0; i < m_Config.Skeleton.Count; i++)
            {
                m_MotionSolver.SetInputTolerance(i, tolerance);
            }
            
        }
    }
}
