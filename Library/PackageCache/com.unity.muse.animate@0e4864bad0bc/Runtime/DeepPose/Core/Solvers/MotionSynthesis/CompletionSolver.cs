using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class CompletionSolver : BaseCompletionSolver
    {
        public bool IsSolving => m_IsSolving;
        public float SolveProgress => m_SolverData.AsyncEvaluationProgress;

        protected override void SolveSequence()
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_SolverData.Evaluate(m_Backend);
            CaptureOutputSequence();
        }
    }
}
