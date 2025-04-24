using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Muse.Animate
{
    static class TimelineUtils
    {
        public delegate float PoseDistanceFunc(ArmatureStaticPoseModel a, ArmatureStaticPoseModel b);

        readonly struct PoseArgs
        {
            public readonly int NumJoints;
            public readonly ArmatureStaticPoseData.PoseType PoseType;
            public PoseArgs(int numJoints, ArmatureStaticPoseData.PoseType poseType)
            {
                NumJoints = numJoints;
                PoseType = poseType;
            }
        }

        public delegate void PoseInterpolationFunc(int targetIndex,
            ArmatureStaticPoseModel startPose,
            int startIndex,
            ArmatureStaticPoseModel endPose,
            int endIndex,
            ArmatureStaticPoseModel result);

        public delegate ValueTask PoseInterpolationAsyncFunc(int targetIndex,
            ArmatureStaticPoseModel startPose,
            int startIndex,
            ArmatureStaticPoseModel endPose,
            int endIndex,
            ArmatureStaticPoseModel result);

        static TimelineUtils()
        {
            TempObject<ArmatureStaticPoseModel, PoseArgs>.Register(
                args => new ArmatureStaticPoseModel(args.NumJoints, args.PoseType));
        }

        /// <summary>
        /// A data structure that holds a pose and its index in the timeline. Used internally to work with
        /// <see cref="KeyReduction"/>.
        /// </summary>
        readonly struct PoseAtIndex
        {
            public readonly ArmatureStaticPoseModel Pose;
            public readonly int Index;

            public PoseAtIndex(ArmatureStaticPoseModel pose, int index)
            {
                Pose = pose;
                Index = index;
            }

            public void Deconstruct(out ArmatureStaticPoseModel pose, out int index)
            {
                pose = Pose;
                index = Index;
            }
        }

        /// <summary>
        /// Use the Ramer-Douglas-Peucker algorithm to get a subset of salient keys from the dense timeline of poses.
        /// </summary>
        /// <param name="timelineModel">The timeline to get keyframes from.</param>
        /// <param name="entityID">The relevant entity in the timeline.</param>
        /// <param name="interpolatePose">A delegate to compute an interpolated pose.</param>
        /// <param name="poseDistance">A delegate to compute a distance metric between two poses.</param>
        /// <param name="tolerance">The minimum distance required for a pose to be considered a key frame.</param>
        /// <param name="keyIndicesOut">A list of indices that are good candidates for keyframes.</param>
        public static void ExtractKeyIndicesFromBakedTimeline(this BakedTimelineModel timelineModel,
            EntityID entityID,
            PoseInterpolationFunc interpolatePose,
            PoseDistanceFunc poseDistance,
            float tolerance,
            List<int> keyIndicesOut)
        {
            var numFrames = timelineModel.FramesCount;
            var bakedPoses = ArrayPool<PoseAtIndex>.Shared.Rent(numFrames);

            for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
            {
                var bakedFrame = timelineModel.GetFrame(frameIndex);
                if (!bakedFrame.TryGetModel(entityID, out var pose)) continue;

                bakedPoses[frameIndex] = new PoseAtIndex(pose.LocalPose, frameIndex);
            }

            KeyReduction.Reduce<PoseAtIndex>(bakedPoses,
                0,
                numFrames - 1,
                (pose, start, end) => GetInterpolationError(pose, start, end, interpolatePose, poseDistance),
                tolerance,
                keyIndicesOut);
            
            ArrayPool<PoseAtIndex>.Shared.Return(bakedPoses);
        }

        /// <summary>
        /// An async version of <see cref="ExtractKeyIndicesFromBakedTimeline"/>.
        /// </summary>
        public static async Task ExtractKeyIndicesFromBakedTimelineAsync(this BakedTimelineModel timelineModel,
            EntityID entityID,
            PoseInterpolationAsyncFunc interpolateAsync,
            PoseDistanceFunc poseDistance,
            float tolerance,
            ConcurrentBag<int> keyIndicesOut,
            CancellationToken cancellationToken = default)
        {
            var numFrames = timelineModel.FramesCount;
            var bakedPoses = ArrayPool<PoseAtIndex>.Shared.Rent(numFrames);

            for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
            {
                var bakedFrame = timelineModel.GetFrame(frameIndex);
                if (!bakedFrame.TryGetModel(entityID, out var pose)) continue;
                bakedPoses[frameIndex] = new PoseAtIndex(pose.LocalPose, frameIndex);
            }

            await KeyReduction.ReduceAsync<PoseAtIndex>(bakedPoses.AsMemory(0, numFrames),
                0,
                numFrames - 1,
                async (pose, start, end) => await GetInterpolationErrorAsync(pose, start, end, interpolateAsync, poseDistance),
                tolerance, 
                keyIndicesOut, 
                cancellationToken);
            ArrayPool<PoseAtIndex>.Shared.Return(bakedPoses);
        }

        static float GetInterpolationError(in PoseAtIndex pose,
            in PoseAtIndex start,
            in PoseAtIndex end,
            PoseInterpolationFunc interpolationFunc,
            PoseDistanceFunc distanceFunc)
        {
            var (startPose, startIndex) = start;
            var (endPose, endIndex) = end;
            var (current, currentIndex) = pose;
            if (currentIndex == startIndex || currentIndex == endIndex)
            {
                return 0f;
            }

            using var tempInterpolated =
                TempObject<ArmatureStaticPoseModel, PoseArgs>.Get(new PoseArgs(startPose.NumJoints, startPose.Type));
            
            interpolationFunc(currentIndex, startPose, startIndex, endPose, endIndex, tempInterpolated);
            return distanceFunc(tempInterpolated, current);
        }

        static async ValueTask<float> GetInterpolationErrorAsync(PoseAtIndex pose,
            PoseAtIndex start,
            PoseAtIndex end,
            PoseInterpolationAsyncFunc interpolateAsync,
            PoseDistanceFunc distanceFunc)
        {
            var (startPose, startIndex) = start;
            var (endPose, endIndex) = end;
            var (current, currentIndex) = pose;
            if (currentIndex == startIndex || currentIndex == endIndex)
            {
                return 0f;
            }

            using var tempInterpolated =
                TempObject<ArmatureStaticPoseModel, PoseArgs>.Get(new PoseArgs(startPose.NumJoints, startPose.Type));
            
            await interpolateAsync(currentIndex, startPose, startIndex, endPose, endIndex, tempInterpolated);
            return distanceFunc(tempInterpolated, current);
        }
    }
}
