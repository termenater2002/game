using UnityEngine;

namespace Unity.Muse.Animate
{
    static class Vector3Utils
    {
        public static bool NearlyEquals(this Vector3 v1, Vector3 v2, float epsilon)
        {
            return v1.x.NearlyEquals(v2.x, epsilon)
                && v1.y.NearlyEquals(v2.y, epsilon)
                && v1.z.NearlyEquals(v2.z, epsilon);
        }
    }
}
