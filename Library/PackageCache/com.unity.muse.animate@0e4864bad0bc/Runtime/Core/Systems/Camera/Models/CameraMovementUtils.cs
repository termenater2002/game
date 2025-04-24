using UnityEngine;

namespace Unity.Muse.Animate
{
    static class CameraMovementUtils
    {
        // Screen size the mouse movements are balanced with.
        // Note: This is temporary and will be replaced by using viewport-space ratios (0-1)
        // TODO: Use viewport-space ratios instead
        static readonly Vector2 k_ReferenceScreenSize = new(1920f, 1080f);

        public static Vector2 ViewRatio => new Vector2(k_ReferenceScreenSize.x / Screen.width, k_ReferenceScreenSize.y / Screen.height);
    }
}
