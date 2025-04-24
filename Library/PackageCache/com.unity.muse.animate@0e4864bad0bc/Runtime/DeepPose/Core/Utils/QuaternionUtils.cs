using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class QuaternionUtils
    {
        public static void ToSwingTwist(this Quaternion q, Vector3 twistAxis, out Quaternion swing, out Quaternion twist)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
            var r = new Vector3(q.x, q.y, q.z);
            var p = Vector3.Project(r, twistAxis);
            twist = new Quaternion(p.x, p.y, p.z, q.w);
            twist.Normalize();
            swing = q * Quaternion.Inverse(twist);
        }

        public static float ToSmallerAngle(float angle)
        {
            while (angle > 360f)
            {
                angle -= 360f;
            }

            while (angle < 0f)
            {
                angle += 360f;
            }

            var otherAngle = 360f - angle;
            if (otherAngle < angle)
                angle = -otherAngle;

            return angle;
        }

        public static Vector3 QuaternionToAxisAngle(this Quaternion quat, bool inRadians = true)
        {
            quat.ToAngleAxis(out var angle, out var axis);
            if (inRadians)
                angle = Mathf.Deg2Rad * angle;

            return angle * axis;
        }

        public static Quaternion AxisAngleToQuaternion(this Vector3 axisAngle, bool inRadians = true)
        {
            var angle = axisAngle.magnitude;
            var axis = axisAngle / angle;
            if (inRadians)
                angle *= Mathf.Rad2Deg;

            var quat = Quaternion.AngleAxis(angle, axis);
            return quat;
        }
    }
}