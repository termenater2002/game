using UnityEngine;

namespace Unity.Muse.Animate
{
    static class DebugUtils
    {
        public static string GetHierarchyPath(Transform transform, string suffix = "")
        {
            if (transform.parent != null)
            {
                return GetHierarchyPath(transform.parent, $"{transform.gameObject.name} -> {suffix}");
            }
            else
            {
                return $"{transform.gameObject.name} -> {suffix}";
            }
        }
    }
}
