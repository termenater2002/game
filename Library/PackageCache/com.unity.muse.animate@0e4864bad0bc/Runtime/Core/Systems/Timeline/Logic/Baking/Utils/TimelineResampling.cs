using System.Buffers;
using UnityEngine;

namespace Unity.Muse.Animate
{
    static class TimelineResampling
    {
        /// <summary>
        /// Resample the poses in the timeline to the target frame rate. This will overwrite the existing poses in the timeline.
        /// </summary>
        /// <param name="timeline">The timeline.</param>
        /// <param name="fromFps">The current frame rate of the timeline.</param>
        /// <param name="toFps">The desired frame rate of the timeline.</param>
        public static void Resample(this BakedTimelineModel timeline, float fromFps, float toFps)
        {
            using var entityIDs = TempHashSet<EntityID>.Allocate();
            timeline.GetAllEntities(entityIDs.Set);
            
            var totalTime = timeline.FramesCount / fromFps;
            var numResampledFrames = Mathf.RoundToInt(totalTime * toFps);
            using var resampledSequences = TempDictionary<EntityID, BakedArmaturePoseModel[]>.Allocate();

            // Get the resampled (interpolated) poses at the target frame rate
            for (var targetIndex = 0; targetIndex < numResampledFrames; ++targetIndex)
            {
                var time = targetIndex / toFps;
                var sourceIdxFloat = time * fromFps;
                var sourceIdx = Mathf.FloorToInt(sourceIdxFloat);
                var fraction = sourceIdxFloat - sourceIdx;
                
                var sourceFrame1 = timeline.GetFrame(sourceIdx);
                var sourceFrame2 = timeline.GetFrame(sourceIdx + 1 > timeline.FramesCount - 1
                    ? timeline.FramesCount - 1
                    : sourceIdx + 1);

                foreach (var entityID in entityIDs)
                {
                    if (!sourceFrame1.TryGetModel(entityID, out var sourcePose1))
                    {
                        continue;
                    }
                    
                    if (!sourceFrame2.TryGetModel(entityID, out var sourcePose2))
                    {
                        continue;
                    }

                    if (!resampledSequences.TryGetValue(entityID, out var resampledSequence))
                    {
                        resampledSequences.Add(entityID, ArrayPool<BakedArmaturePoseModel>.Shared.Rent(numResampledFrames));
                        resampledSequence = resampledSequences[entityID];
                    }
                    resampledSequence[targetIndex] = new BakedArmaturePoseModel(sourcePose1.NumJoints, 0);
                    resampledSequence[targetIndex].LocalPose.Interpolate(sourcePose1.LocalPose, sourcePose2.LocalPose, fraction);
                }
            }
            
            // Set the resampled poses into the timeline (overwriting the original poses)
            timeline.FramesCount = numResampledFrames;
            for (var frameIndex = 0; frameIndex < numResampledFrames; ++frameIndex)
            {
                var frame = timeline.GetFrame(frameIndex);
                foreach (var entityID in entityIDs)
                {
                    var poseSequence = resampledSequences[entityID];
                    
                    var numJoints = poseSequence[frameIndex].NumJoints;
                    if (!frame.TryGetModel(entityID, out var pose))
                    {
                        frame.AddEntity(entityID, numJoints, 0);
                        frame.TryGetModel(entityID, out pose);
                    }

                    poseSequence[frameIndex].CopyTo(pose);
                }
            }

            foreach (var (_, tempSequence) in resampledSequences)
            {
                ArrayPool<BakedArmaturePoseModel>.Shared.Return(tempSequence);
            }
        }
    }
}
