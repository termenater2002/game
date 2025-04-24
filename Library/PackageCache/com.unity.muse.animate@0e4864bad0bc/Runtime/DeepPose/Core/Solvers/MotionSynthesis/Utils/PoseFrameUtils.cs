using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class PoseFrameUtils
    {
        public static void DrawSkeleton(this PoseFrame frame, Skeleton skeleton, Color color, float thickness)
        {
            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                var joint = skeleton.FindJoint(jointIdx);
                if (joint == null)
                    continue;

                var parentJoint = joint.Parent;
                if (parentJoint == null)
                    continue;

                var jointPosition = frame.GlobalPositions[joint.Index];
                var parentPosition = frame.GlobalPositions[parentJoint.Index];

                GizmoUtils.DrawLine(parentPosition, jointPosition, color, thickness);
            }
        }

        public static void DrawLocalAxes(this PoseFrame frame, Skeleton skeleton, float thickness, float length)
        {
            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                var jointPosition = frame.GlobalPositions[jointIdx];
                var jointRotation = frame.GlobalRotations[jointIdx];

                GizmoUtils.DrawLocalAxes(jointPosition, jointRotation, thickness, length);
            }
        }

        public static void DrawContacts(this PoseFrame frame, Skeleton skeleton, Color color, float size, int[] contactJointIndices)
        {
            for (var contactIdx = 0; contactIdx < frame.ContactsCount; contactIdx++)
            {
                var contactValue = frame.Contacts[contactIdx];
                var contactColor = new Color(color.r, color.g, color.b, color.a * contactValue);

                var jointIdx = contactJointIndices[contactIdx];

                var joint = skeleton.FindJoint(jointIdx);
                if (joint == null)
                    continue;

                var jointPosition = frame.GlobalPositions[joint.Index];
                GizmoUtils.DrawSphere(jointPosition, contactColor, size);
            }
        }

        public static void ApplyLocally(this PoseFrame frame, IList<Transform> jointTransforms)
        {
            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                try
                {
                    var jointTransform = jointTransforms[jointIdx];
                    if (jointIdx == 0)
                    {
                        jointTransform.position = frame.GlobalPositions[jointIdx];
                        jointTransform.rotation = frame.LocalRotations[jointIdx];
                    }
                    else
                    {
                        jointTransform.localRotation = frame.LocalRotations[jointIdx];
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    Debug.Log("Woops, idx was :" + jointIdx);
                }
            }
        }

        public static void CaptureTransforms(this PoseFrame frame, IList<Transform> jointTransforms)
        {
            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                var jointTransform = jointTransforms[jointIdx];
                frame.GlobalPositions[jointIdx] = jointTransform.position;
                frame.LocalRotations[jointIdx] = jointIdx == 0 ? jointTransform.rotation : jointTransform.localRotation;
                frame.GlobalRotations[jointIdx] = jointTransform.rotation;
            }
        }

        public static void ApplyLocallyAndCaptureTransforms(this PoseFrame frame, IList<Transform> jointTransforms)
        {
            ApplyLocally(frame, jointTransforms);
            CaptureTransforms(frame, jointTransforms);
        }

        public static void Translate(this PoseFrame frame, Vector3 offset)
        {
            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                frame.GlobalPositions[jointIdx] += offset;
            }
        }

        public static void Rotate(this PoseFrame frame, Quaternion rotation)
        {
            frame.LocalRotations[0] = rotation * frame.LocalRotations[0];

            for (var jointIdx = 0; jointIdx < frame.JointsCount; jointIdx++)
            {
                frame.GlobalRotations[jointIdx] = rotation * frame.GlobalRotations[jointIdx];
                frame.GlobalPositions[jointIdx] = rotation * frame.GlobalPositions[jointIdx];
            }
        }

        public static void Interpolate(this PoseFrame output, PoseFrame from, PoseFrame to, float t)
        {
            for (var jointIdx = 0; jointIdx < output.JointsCount; jointIdx++)
            {
                output.GlobalPositions[jointIdx] = Vector3.Lerp(from.GlobalPositions[jointIdx], to.GlobalPositions[jointIdx], t);
                output.GlobalRotations[jointIdx] = Quaternion.Slerp(from.GlobalRotations[jointIdx], to.GlobalRotations[jointIdx], t);
                output.LocalRotations[jointIdx] = Quaternion.Slerp(from.LocalRotations[jointIdx], to.LocalRotations[jointIdx], t);
            }
        }
    }
}
