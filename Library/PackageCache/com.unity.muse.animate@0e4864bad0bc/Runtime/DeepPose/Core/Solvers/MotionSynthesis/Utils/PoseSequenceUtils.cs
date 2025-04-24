using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class PoseSequenceUtils
    {
        static Color[] s_Colors = new Color[1];

        public static void DrawMotionLines(this PoseSequence sequence, int centerFrame, float prevDistance, float nextDistance, Color color, float thickness)
        {
            s_Colors[0] = color;
            DrawMotionLines(sequence, centerFrame, prevDistance, nextDistance, s_Colors, thickness);
        }

        public static void DrawMotionLines(this PoseSequence sequence, int centerFrame, float prevDistance, float nextDistance, Color[] frameColors, float thickness)
        {
            if (sequence == null)
                return;

            var firstFrame = Mathf.RoundToInt(Mathf.Max(0f, centerFrame - prevDistance));
            var lastFrame = Mathf.RoundToInt(Mathf.Min(sequence.Length - 1f, centerFrame + nextDistance));

            for (var jointIdx = 0; jointIdx < sequence.JointsCount; jointIdx++)
            {
                var prevPoint = Vector3.zero;
                for (var frameIdx = firstFrame; frameIdx <= lastFrame; frameIdx++)
                {
                    var color = frameColors.Length == 1 ? frameColors[0] : frameColors[frameIdx];
                    var point = sequence.GetFrame(frameIdx).GlobalPositions[jointIdx];

                    if (frameIdx > firstFrame)
                        DrawMotionLine(prevPoint, point, frameIdx, centerFrame, prevDistance, nextDistance, color, thickness);

                    prevPoint = point;
                }
            }
        }

        static void DrawMotionLine(Vector3 from, Vector3 to, int frameIdx, int centerFrame, float prevDistance, float nextDistance, Color color, float thickness)
        {
            var alpha = GetAlpha(color.a, frameIdx, centerFrame, nextDistance, prevDistance);
            if (alpha <= 0f)
                return;

            var newColor = new Color(color.r, color.g, color.b, alpha);
            GizmoUtils.DrawLine(from, to, newColor, thickness);
        }

        static float GetAlpha(float alpha, int frameIdx, int centerFrame, float nextDistance, float prevDistance)
        {
            // Avoid division by 0
            nextDistance = Mathf.Max(nextDistance, 1e-6f);
            prevDistance = Mathf.Max(prevDistance, 1e-6f);

            var ratio = frameIdx < centerFrame ? (centerFrame - frameIdx) / prevDistance : (frameIdx - centerFrame) / nextDistance;
            ratio = Mathf.Clamp(1f - ratio, 0f, 1f);
            var newAlpha = ratio * alpha * 0.5f;
            return newAlpha;
        }

        public static void DrawOnionSkin(this PoseSequence sequence, Skeleton skeleton, int centerFrame, float prevDistance, float nextDistance, Color color, float thickness)
        {
            s_Colors[0] = color;
            DrawOnionSkin(sequence, skeleton, centerFrame, prevDistance, nextDistance, s_Colors, thickness);
        }

        public static void DrawOnionSkin(this PoseSequence sequence, Skeleton skeleton, int centerFrame, float prevDistance, float nextDistance, Color[] frameColors, float thickness)
        {
            if (sequence == null)
                return;

            var firstFrame = Mathf.RoundToInt(Mathf.Max(0f, centerFrame - prevDistance));
            var lastFrame = Mathf.RoundToInt(Mathf.Min(sequence.Length - 1f, centerFrame + nextDistance));

            for (var frameIdx = firstFrame; frameIdx <= lastFrame; frameIdx++)
            {
                var color = frameColors.Length == 1 ? frameColors[0] : frameColors[frameIdx];
                var alpha = GetAlpha(color.a, frameIdx, centerFrame, nextDistance, prevDistance);
                var frame = sequence.GetFrame(frameIdx);

                frame.DrawSkeleton(skeleton, new Color(color.r, color.g, color.b, alpha), thickness);
            }
        }

        public static void CenterXZ(this PoseSequence sequence, Vector3 offset = default, int frameIdx = 0)
        {
            if (sequence == null)
                return;

            var center = sequence.GetFrame(frameIdx).GlobalPositions[0];
            var rootOffset = offset - new Vector3(center.x, 0f, center.z);
            Translate(sequence, rootOffset);
        }

        public static void Translate(this PoseSequence sequence, Vector3 offset)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
            {
                var frame = sequence.GetFrame(i);
                frame.Translate(offset);
            }
        }

        public static void Rotate(this PoseSequence sequence, Quaternion rotation)
        {
            if (sequence == null)
                return;

            for (var i = 0; i < sequence.Length; i++)
            {
                var frame = sequence.GetFrame(i);
                frame.Rotate(rotation);
            }
        }

        public static void Interpolate(this PoseSequence sequence, int firstFrame = 0, int lastFrame = -1)
        {
            if (sequence == null)
                return;

            if (firstFrame < 0)
                firstFrame += sequence.Length;

            if (lastFrame < 0)
                lastFrame += sequence.Length;

            var startFrame = sequence.GetFrame(firstFrame);
            var endFrame = sequence.GetFrame(lastFrame);

            for (var frameIdx = firstFrame + 1; frameIdx < lastFrame; frameIdx++)
            {
                var ratio = (float)(frameIdx - firstFrame) / (float)(lastFrame - firstFrame);
                var frame = sequence.GetFrame(frameIdx);
                frame.Interpolate(startFrame, endFrame, ratio);
            }
        }

        public static void ApplyLocallyAndCaptureTransforms(this PoseSequence sequence, IList<Transform> jointTransforms)
        {
            for (var frameIdx = 0; frameIdx < sequence.Length; frameIdx++)
            {
                var frame = sequence.GetFrame(frameIdx);
                frame.ApplyLocally(jointTransforms);
                frame.CaptureTransforms(jointTransforms);
            }
        }
    }
}
