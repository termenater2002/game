using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    static class ScrollViewUtils
    {
        public static void ScrollToImmediate(this ScrollView scrollView, VisualElement item)
        {
            if (item == null || scrollView == null)
            {
                return;
            }

            int remainingIterations = 4;

            void TryScroll()
            {
                // If both layout and item have a size, then we can scroll immediate
                // otherwise, we need to listen to layout changes then scrollTo

                if (item.layout.width > 0 && scrollView.layout.width > 0)
                {
                    scrollView.ScrollTo(item);
                    return;
                }
                else if (remainingIterations <= 0)
                {
                    Debug.LogWarning("Too many layout iterations");

                    scrollView.ScrollTo(item);
                    return;
                }

                if (scrollView.layout.width > 0)
                {
                    item.RegisterCallback<GeometryChangedEvent, VisualElement>(OnGeometryChanged, item);
                }
                else
                {
                    scrollView.RegisterCallback<GeometryChangedEvent, VisualElement>(OnGeometryChanged, scrollView);
                }
            }

            void OnGeometryChanged(GeometryChangedEvent evt, VisualElement target)
            {
                target.UnregisterCallback<GeometryChangedEvent, VisualElement>(OnGeometryChanged);

                // Try scrolling after we received a geometry changed event from either the item or scrollView
                // the geometry still might be 0, so keep trying if so

                remainingIterations--;

                TryScroll();
            }

            TryScroll();
        }
    }
}
