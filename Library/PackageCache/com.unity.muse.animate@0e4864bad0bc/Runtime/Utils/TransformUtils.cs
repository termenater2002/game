using UnityEngine;

namespace Unity.Muse.Animate
{
    static class TransformUtils
    {
        public static float GetRelativeDepth(this Transform reference, Vector3 worldPoint)
        {
            var heading = worldPoint - reference.position;
            var depth = Vector3.Dot(reference.forward, heading);
            return depth;
        }
    }
}
