using System;
using Unity.Muse.Chat.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Utils
{
    /// <summary>
    /// Helper class to keep track of and dismiss popup style visual elements
    /// Can also auto-align the popup to an anchor control
    /// </summary>
    class PopupTracker : IDisposable
    {
        readonly ManagedTemplate m_Root;
        readonly VisualElement m_PopupButtonElement;
        readonly VisualElement m_Anchor;
        readonly bool m_AutoAlign;

        int m_YOffset = 0;
        Vector2Int m_AlignOffset = new(0, 17);
        Vector2Int m_MinMargin = new(8, 8);

        public PopupTracker(
            ManagedTemplate root,
            VisualElement popupButtonElement,
            int yOffset,
            VisualElement anchorElement = null,
            bool autoAlignToAnchor = true
        ) {
            m_Root = root;
            m_PopupButtonElement = popupButtonElement;
            m_YOffset = yOffset;
            m_Root.RegisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
            m_Root.panel.visualTree.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            m_Root.panel.visualTree.RegisterCallback<GeometryChangedEvent>(OnTreeGeometryChanged);
            m_Anchor = anchorElement;
            if (m_Anchor != null)
            {
                m_Anchor.RegisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
                m_Anchor.parent.RegisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
            }

            m_AutoAlign = autoAlignToAnchor;
            if (m_AutoAlign)
            {
                RealignPopup();
            }
        }

        public event Action Dismiss;

        public Vector2Int AlignOffset
        {
            get => m_AlignOffset;
            set
            {
                if (m_AlignOffset != value)
                {
                    m_AlignOffset = value;
                    RealignPopup();
                }
            }
        }

        public Vector2Int MinMargin
        {
            get => m_MinMargin;
            set
            {
                if (m_MinMargin != value)
                {
                    m_MinMargin = value;
                    RealignPopup();
                }
            }
        }

        public void Dispose()
        {
            m_Root.UnregisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
            m_Root.panel.visualTree.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            m_Root.panel.visualTree.UnregisterCallback<GeometryChangedEvent>(OnTreeGeometryChanged);

            if (m_Anchor != null)
            {
                m_Anchor.UnregisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
                m_Anchor.parent.UnregisterCallback<GeometryChangedEvent>(OnAlignGeometryChanged);
            }
        }

        bool CheckDismissalFromClick(VisualElement clickedElement)
        {
            if (clickedElement == null)
            {
                return true;
            }

            if (m_Anchor != null && clickedElement.FindCommonAncestor(m_Anchor) == m_Anchor)
            {
                return false;
            }

            if (clickedElement.FindCommonAncestor(m_Root) == m_Root)
            {
                return false;
            }

            return true;
        }

        void OnTreeDown(PointerDownEvent evt)
        {
            if (!CheckDismissalFromClick(evt.target as VisualElement))
            {
                return;
            }

            if (evt.target == m_PopupButtonElement)
            {
                return;
            }

            Dismiss?.Invoke();
        }

        void OnTreeGeometryChanged(GeometryChangedEvent evt)
        {
            Dismiss?.Invoke();
        }

        void OnAlignGeometryChanged(GeometryChangedEvent evt)
        {
            RealignPopup();
        }

        public void RealignPopup()
        {
            if (m_Anchor == null)
            {
                return;
            }

            var popupBounds = m_Root.worldBound;
            if (popupBounds.width == 0 || popupBounds.height == 0)
            {
                return;
            }

            //var worldBounds = m_Root.parent.worldBound;
            var anchorBounds = m_Anchor.worldBound;
            m_Root.style.left = Math.Max(m_MinMargin.x, anchorBounds.xMax - anchorBounds.width + m_AlignOffset.x);
            m_Root.style.top = Math.Max(m_MinMargin.y, anchorBounds.yMax - popupBounds.height - m_YOffset);
        }
    }
}
