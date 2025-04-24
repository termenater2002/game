using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class PoseMetrics
    {
        /// <summary>
        /// Get an error (distance) metric between 0 and 1 between two poses.
        /// </summary>
        /// <param name="pose1">The first pose.</param>
        /// <param name="pose2">The second pose.</param>
        /// <param name="jointMask">The joints to be evaluated. If <c>null</c> all joints are considered.</param>
        /// <param name="minAngle">The threshold angle difference (in degrees).</param>
        /// <param name="maxAngle">The angle difference (in degrees) that saturates the error measure.</param>
        /// <param name="positionScale">The scaling to apply to position differences such that they are comparable to an
        /// angle (in degrees)</param>
        /// <returns>An error metric between 0 and 1.</returns>
        /// <remarks>
        /// This error metric is based on the maximum difference between the two poses in terms of rotation and position.
        /// </remarks>
        public static float GetPoseErrorMetric(in ArmatureStaticPoseModel pose1,
            in ArmatureStaticPoseModel pose2,
            JointMask jointMask = null,
            float minAngle = 30f,
            float maxAngle = 90f,
            float positionScale = 50f)
        {
            Assert.AreEqual(pose1.NumJoints, pose2.NumJoints);
            Assert.AreEqual(pose1.Type, pose2.Type);
            
            var maxDiff = 0f;
            var hasMask = jointMask != null;
            
            Assert.IsFalse(hasMask && jointMask.Count != pose1.NumJoints,
                "The mask must be the same size as the number of joints.");
            
            
            // maxDiff = Mathf.Max(maxDiff, positionDiff);
            var positionWeight = hasMask ? jointMask.PositionWeights[0] : 1f;
            var positionDiff = Vector3.Distance(pose1.GetPosition(0), pose2.GetPosition(0)) * 
                positionScale * positionWeight;
            
            maxDiff = Mathf.Max(maxDiff, positionDiff);
            for (int i = 0; i < pose1.NumJoints; i++)
            {
                var weight = hasMask ? jointMask.RotationWeights[i] : 1f;
                var rotationDiff = weight * Quaternion.Angle(pose1.GetRotation(i), pose2.GetRotation(i));
                    
                maxDiff = Mathf.Max(maxDiff, rotationDiff);
            }

            return Mathf.InverseLerp(minAngle, maxAngle, maxDiff);
        }
    }
}
