using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Unity.DeepPose.Core
{
    static class SkeletonAnimationFrameUtils
    {
        public static SkeletonAnimationFrame ToSkeletonAnimationFrame(this PoseFrame frame, int frameIdx, float frameRate, float timeOffset = 0f)
        {
            var time = timeOffset + frameIdx * (1f / frameRate);
            var newFrame = new SkeletonAnimationFrame(frameIdx, time, frame.GlobalPositions[0], frame.LocalRotations.ToList(),
                frame.GlobalRotations.ToList(), frame.GlobalPositions.ToList(), float4.zero);
            return newFrame;
        }

        public static List<SkeletonAnimationFrame> ToSkeletonAnimationFrames(this PoseSequence sequence, float frameRate, float timeOffset = 0f, int firstFrame = 0, int lastFrame = -1)
        {
            if (lastFrame < 0)
                lastFrame += sequence.Length;

            var frames = new List<SkeletonAnimationFrame>();
            for (var frameIdx = firstFrame; frameIdx <= lastFrame; frameIdx++)
            {
                var frame = sequence.GetFrame(frameIdx);
                var newFrame = frame.ToSkeletonAnimationFrame(frameIdx, frameRate, timeOffset);
                frames.Add(newFrame);
            }

            return frames;
        }

        public static PoseFrame ToPoseFrame(this SkeletonAnimationFrame frame, bool includeContacts)
        {
            var newFrame = new PoseFrame(frame.JointRotations.Count, includeContacts ? 4 : 0, 0);

            newFrame.GlobalPositions[0] = frame.RootPosition;
            for (var jointIdx = 0; jointIdx < newFrame.JointsCount; jointIdx++)
            {
                newFrame.LocalRotations[jointIdx] = frame.JointRotations[jointIdx];

                if (frame.JointPositionsGlobal != null)
                    newFrame.GlobalPositions[jointIdx] = frame.JointPositionsGlobal[jointIdx];

                if (frame.JointRotationsGlobal != null)
                    newFrame.GlobalRotations[jointIdx] = frame.JointRotationsGlobal[jointIdx];
            }

            if (includeContacts)
            {
                newFrame.Contacts[0] = frame.FootContacts.x;
                newFrame.Contacts[1] = frame.FootContacts.y;
                newFrame.Contacts[2] = frame.FootContacts.z;
                newFrame.Contacts[3] = frame.FootContacts.w;
            }

            return newFrame;
        }

        public static PoseSequence ToPoseSequence(this IEnumerable<SkeletonAnimationFrame> frames, int[] contactJointIndices)
        {
            var hasContacts = contactJointIndices != null && contactJointIndices.Length > 0;
            var sequence = new PoseSequence(frames.First().JointRotations.Count, contactJointIndices);
            foreach (var frame in frames)
            {
                var newFrame = frame.ToPoseFrame(hasContacts);
                sequence.AddFrame(newFrame);
            }

            return sequence;
        }
    }
}
