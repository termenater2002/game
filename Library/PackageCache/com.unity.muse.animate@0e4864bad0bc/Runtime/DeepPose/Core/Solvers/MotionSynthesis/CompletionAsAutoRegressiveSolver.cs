using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    // Uses same inputs and backbones as one-shot transformer models, but runs them in auto-regressive fashion
    class CompletionAsAutoRegressiveSolver : BaseCompletionSolver
    {
        List<int> m_FixedTargetFrameIndices = new ();
        List<int> m_TargetFrameIndices = new ();

        
        protected override void SolveSequence()
        {
            Assert.IsTrue(m_SolverData.IsCreated, "Solver data was not created");
            m_TargetFrameIndices.Clear();
            m_FixedTargetFrameIndices.Clear();
            for (var i = 0; i < m_SolverData.NumTargetFrames; i++)
            {
                var targetIdx = m_SolverData.GetTargetFrameTime(i);
                m_FixedTargetFrameIndices.Add(targetIdx);
                m_TargetFrameIndices.Add(targetIdx);
            }

            //Initital solve. TODO redundant with first loop
            m_SolverData.Evaluate(m_Backend);

            // Right spot?
            if (m_Sequence.Length > m_SolverData.NumOutputFrames)
                m_Sequence.Clear();
            m_Sequence.Resize(m_SolverData.NumOutputFrames);

            // Copy original inputs
            for (var i = 0; i < m_Sequence.Length; i++)
            {
                if (!m_TargetFrameIndices.Contains(i))
                {
                    var frame = m_Sequence.GetFrame(i);
                    CaptureSolvedFrame(frame, i);
                }
            }

            // TODO: optimize for GC allocs
            foreach (var currentTargetIndex in m_FixedTargetFrameIndices)
            {
                m_SolverData.Evaluate(m_Backend);

                // Update outputs
                var frame = m_Sequence.GetFrame(currentTargetIndex);
                // Overwrite current frame only
                CaptureSolvedFrame(frame, currentTargetIndex);

                // Update inputs
                if (m_NumTargetFrames > 1)
                {
                    // Add input
                    m_NumInputFrames++;
                    SetInputFrameTime(m_NumInputFrames - 1, currentTargetIndex);
                    SetInputPosition(m_NumInputFrames - 1, 0, m_Sequence.GetFrame(currentTargetIndex).GlobalPositions[0]);
                    for (var jointIndex = 0; jointIndex < m_Sequence.JointsCount; jointIndex++)
                    {
                        var rotation = m_Sequence.GetFrame(currentTargetIndex).LocalRotations[jointIndex];
                        SetInputRotation(m_NumInputFrames - 1, jointIndex, rotation);
                    }

                    // Remove target
                    m_NumTargetFrames--;
                    m_TargetFrameIndices.Remove(currentTargetIndex);
                    SetTargetFrameTimes(m_TargetFrameIndices);
                }
            }
        }

        public List<SkeletonAnimationFrame> Solve(List<SkeletonAnimationFrame> allFrames,
            List<SkeletonAnimationFrame> inputFrames, List<int> inputFrameIndices, List<int> targetFrameIndices)
        {
            var fixedTargetIndices = new List<int>(targetFrameIndices);

            // Note: this happens in-place : allFrames is overwritten on targetFrameIndices.
            // Note: allows for unordered input frames : assumes the backbone is agnostic to input frame ordering
            // Note: Auto-regressive doesn't work in past-prediction context : first frame must be known / input
            foreach (var currentTargetIndex in fixedTargetIndices)
            {
                var currentInputReferenceIndex = inputFrameIndices.FindIndex(0, inputFrameIndices.Count, x => x == (currentTargetIndex - 1));
                Assert.IsTrue(currentInputReferenceIndex != -1, $"Input frame index for frame {currentTargetIndex - 1} not found!");

                var solverData = new CompletionSolverData(
                    inputFrameIndices.Count,
                    targetFrameIndices.Count,
                    m_NumJoints,
                    m_NumContacts,
                    m_TransposeOrtho6D,
                    m_EndJoints
                );

                solverData.SetFrames(
                    inputFrames,
                    inputFrameIndices,
                    targetFrameIndices,
                    currentInputReferenceIndex);

                solverData.Evaluate(m_Backend);

                var predictedPositions = solverData.GetPredictedFramePositions(currentTargetIndex);
                var predictedRotations = solverData.GetPredictedFrameRotations(currentTargetIndex);
                var predictedContacts = solverData.GetPredictedFrameContacts(currentTargetIndex);

                var sourceFrame = allFrames[currentTargetIndex];
                var predictedFrame = CreateFrame(sourceFrame.Frame, sourceFrame.Time, predictedPositions[0], predictedRotations, predictedContacts);
                allFrames[currentTargetIndex] = predictedFrame;

                targetFrameIndices.Remove(currentTargetIndex);
                inputFrameIndices.Add(currentTargetIndex);
                inputFrames.Add(predictedFrame);

                solverData.Dispose();
            }

            return allFrames;
        }

        SkeletonAnimationFrame CreateFrame(int frame, float time, Vector3 rootPosition, NativeSlice<Quaternion> rotations, NativeSlice<float> contacts)
        {
            var feetContacts = float4.zero;
            if (contacts.Length == 4)
            {
                feetContacts = new float4(contacts[0], contacts[1], contacts[2], contacts[3]);
            }

            var newFrame = new SkeletonAnimationFrame(frame, time, rootPosition, rotations.ToList(), null, null, feetContacts);
            return newFrame;
        }
    }
}
