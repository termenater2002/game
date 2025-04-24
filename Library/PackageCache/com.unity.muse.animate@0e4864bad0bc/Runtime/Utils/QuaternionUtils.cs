using UnityEngine;

namespace Unity.Muse.Animate
{
    static class QuaternionUtils
    {
        public static bool NearlyEquals(this Quaternion q1, Quaternion q2, float epsilon)
        {
            return Mathf.Abs(Quaternion.Dot(q1.normalized, q2.normalized)) >= 1f - epsilon;
        }
    }
}
