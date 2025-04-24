using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    static class CompletionSolverUtils
    {
        // Note: this is provided for backward compatibility only, prefer using the solver API directly
        public static void SetFrames(this CompletionSolver solver,
            List<SkeletonAnimationFrame> inputFrames,
            List<int> inputFrameIndices,
            List<int> targetFrameIndices,
            int inputReferenceIndex)
        {
            Assert.AreEqual(inputFrames.Count, inputFrameIndices.Count);

            solver.NumInputFrames = inputFrameIndices.Count;
            solver.NumTargetFrames = targetFrameIndices.Count;

            // INPUT REFERENCE INDEX
            solver.SetInputReferenceFrame(inputReferenceIndex);

            // INPUT FRAMES
            for (var i = 0; i < inputFrames.Count; i++)
            {
                var frameTimeIdx = inputFrameIndices[i];
                var rootPosition = inputFrames[i].RootPosition;

                solver.SetInputFrameTime(i, frameTimeIdx);
                solver.SetInputPosition(i, 0, rootPosition);

                for (var jointIndex = 0; jointIndex < inputFrames[i].JointRotations.Count; jointIndex++)
                {
                    var rotation = inputFrames[i].JointRotations[jointIndex];
                    solver.SetInputRotation(i, jointIndex, rotation);
                }
            }

            // TARGET FRAMES
            for (var i = 0; i < targetFrameIndices.Count; i++)
            {
                var frameTimeIdx = targetFrameIndices[i];
                solver.SetTargetFrameTime(i, frameTimeIdx);
            }
        }

        // Note: this is provided for backward compatibility only, prefer using the solver API directly
        public static void SetFrames(this CompletionSolverData solverData,
            List<SkeletonAnimationFrame> inputFrames,
            List<int> inputFrameIndices,
            List<int> targetFrameIndices,
            int inputReferenceIndex)
        {
            Assert.AreEqual(inputFrames.Count, inputFrameIndices.Count);
            Assert.AreEqual(solverData.NumInputFrames, inputFrameIndices.Count);
            Assert.AreEqual(solverData.NumTargetFrames, targetFrameIndices.Count);

            // INPUT REFERENCE INDEX
            solverData.SetInputReferenceFrame(inputReferenceIndex);

            // INPUT FRAMES
            for (var i = 0; i < inputFrames.Count; i++)
            {
                var frameTimeIdx = inputFrameIndices[i];
                var rootPosition = inputFrames[i].RootPosition;

                solverData.SetInputFrameTime(i, frameTimeIdx);
                solverData.SetInputPosition(i, 0, rootPosition);

                for (var jointIndex = 0; jointIndex < inputFrames[i].JointRotations.Count; jointIndex++)
                {
                    var rotation = inputFrames[i].JointRotations[jointIndex];
                    solverData.SetInputRotation(i, jointIndex, rotation);
                }
            }

            // TARGET FRAMES
            for (var i = 0; i < targetFrameIndices.Count; i++)
            {
                var frameTimeIdx = targetFrameIndices[i];
                solverData.SetTargetFrameTime(i, frameTimeIdx);
            }
        }
    }
}
