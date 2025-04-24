using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account.UI
{
    class VisualElementUtility
    {
        public static Vector2 GetScreenPosition(Rect windowPosition, VisualElement element)
        {
            // Get the position of the element relative to its root
            Vector2 localPosition = element.worldBound.position;
            Vector2 screenPosition = new Vector2(localPosition.x + windowPosition.x, localPosition.y + windowPosition.y);
            return screenPosition;
        }
    }
}