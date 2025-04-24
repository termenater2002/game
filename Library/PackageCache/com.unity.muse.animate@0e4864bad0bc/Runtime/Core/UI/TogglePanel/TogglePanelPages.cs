using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class TogglePanelPages: VisualElement
    {
        const string k_QuietSuffix = "--quiet";
        const string k_PagesUssClassName = "deeppose-toggle-panel-pages";
        const string k_PagesUssClassNameQuiet = k_PagesUssClassName+k_QuietSuffix;
        const string k_PagesHeaderUssClassName = "deeppose-toggle-panel-pages-header";
        const string k_PagesHeaderUssClassNameQuiet = k_PagesHeaderUssClassName+k_QuietSuffix;

        readonly List<TogglePanelPage> m_Pages;
        public int NbPages => m_Pages.Count;

        TogglePanel m_Panel;

        public bool Quiet => m_Panel.quiet;
        public bool Expand => m_Panel.expand;

        readonly Text m_PageHeaderLabel;
        readonly VisualElement m_PageHeader;
        readonly IconButton m_ButtonLeft;
        readonly IconButton m_ButtonRight;

        public TogglePanelPages(TogglePanel panel)
        {
            m_Panel = panel;

            m_PageHeader = new VisualElement();

            m_ButtonLeft = new IconButton("", ClickedLeft);
            m_ButtonRight = new IconButton("", ClickedRight);
            m_ButtonLeft.size = Size.L;
            m_ButtonRight.size = Size.L;

            m_PageHeaderLabel = new Text();
            m_PageHeaderLabel.size = TextSize.M;
            m_PageHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            m_PageHeader.Add(m_ButtonLeft);
            m_PageHeader.Add(m_PageHeaderLabel);
            m_PageHeader.Add(m_ButtonRight);
            Add(m_PageHeader);

            Add(new SessionStatusNotifications());

            m_Pages = new List<TogglePanelPage>();
            pickingMode = PickingMode.Ignore;
        }

        public void Update()
        {
            // Style
            RemoveFromClassList(k_PagesUssClassNameQuiet);
            RemoveFromClassList(k_PagesUssClassName);
            AddToClassList(m_Panel.quiet?k_PagesUssClassNameQuiet:k_PagesUssClassName);
            // HACK: Add border radius with code instead of uss. This is a workaround for an assertion problem.
            // See note in TogglePanel.uss.
            var radius = m_Panel.quiet ? 6f : 0f;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;

            // Width
            style.width = m_Panel.contentWidth;

            // Visibility
            style.display = m_Panel.SelectedPageIndex == -1 ? DisplayStyle.None : DisplayStyle.Flex;

            // Header
            m_PageHeader.RemoveFromClassList(k_PagesHeaderUssClassNameQuiet);
            m_PageHeader.RemoveFromClassList(k_PagesHeaderUssClassName);
            m_PageHeader.AddToClassList(m_Panel.quiet?k_PagesHeaderUssClassNameQuiet:k_PagesHeaderUssClassName);

            // Header - Title
            m_PageHeaderLabel.style.flexGrow = 1;

            var showTitle = TestIfShowingTitle();

            m_PageHeaderLabel.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
            m_PageHeader.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
            m_PageHeaderLabel.text = showTitle ? m_Pages[m_Panel.SelectedPageIndex].Title : "";
            m_PageHeaderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Header - Left Button
            if (m_Panel.leftButtonIcon == "")
            {
                m_ButtonLeft.style.display = DisplayStyle.None;
                m_PageHeaderLabel.style.marginRight = 0;
            }
            else
            {
                m_ButtonLeft.style.display = DisplayStyle.Flex;
                m_ButtonLeft.icon = m_Panel.leftButtonIcon;
                m_ButtonLeft.quiet = true;

                // If no right button, add margin to the right of the header to fill the empty space
                if (m_Panel.rightButtonIcon == "")
                {
                    m_PageHeaderLabel.style.marginRight = 30;
                }
                else
                {
                    m_PageHeaderLabel.style.marginRight = 0;
                }
            }

            // Header - Right Button
            if (m_Panel.rightButtonIcon == "")
            {
                m_ButtonRight.style.display = DisplayStyle.None;
                m_PageHeaderLabel.style.marginLeft = 0;
            }
            else
            {
                m_ButtonRight.style.display = DisplayStyle.Flex;
                m_ButtonRight.icon = m_Panel.rightButtonIcon;
                m_ButtonRight.quiet = true;

                // If no left button, add margin to the left of the header to fill the empty space
                if (m_Panel.leftButtonIcon == "")
                {
                    m_PageHeaderLabel.style.marginLeft = 30;
                }
                else
                {
                    m_PageHeaderLabel.style.marginLeft = 0;
                }
            }

            // Expand
            if (m_Panel.expand)
            {
                // Fill the whole column
                style.flexGrow = 1;
                style.height = new StyleLength(Length.Percent(100));
            }
            else
            {
                // Expand to fit inner content
                style.flexGrow = 0;
                style.height = new StyleLength(StyleKeyword.Auto);
            }

            // Top or bottom
            if (m_Panel.alignBottom)
            {
                style.alignItems = Align.FlexEnd;
                style.justifyContent = Justify.FlexEnd;
            }
            else
            {
                style.alignItems = Align.FlexStart;
                style.justifyContent = Justify.FlexStart;
            }

            // First item on top or at bottom
            if (m_Panel.reverseOrder)
            {
                style.flexDirection = FlexDirection.ColumnReverse;
            }
            else
            {
                style.flexDirection = FlexDirection.Column;
            }

            // Update the pages last
            UpdatePages();
        }

        void UpdatePages()
        {
            for (int i = 0; i < m_Pages.Count; i++)
            {
                if (i != m_Panel.SelectedPageIndex)
                {
                    m_Pages[i].Hide();
                }
                else
                {
                    m_Pages[i].Show();
                }
            }
        }

        public void AddPage(string title, string icon)
        {
            var page = new TogglePanelPage(this, m_Pages.Count, title, icon);
            Add(page);
            m_Pages.Add(page);
        }

        public void AttachToPage(int index, VisualElement visualElement)
        {
            m_Pages[index].Add(visualElement);
        }

        public void DetachFromPage(int index, VisualElement visualElement)
        {
            // HACK: How the heck did this happen?
            if (visualElement.parent != m_Pages[index])
            {
                return;
            }

            m_Pages[index].Remove(visualElement);
        }

        public bool IsPageUsed(int page)
        {
            return m_Pages[page].IsUsed;
        }

        void ClickedLeft()
        {
            m_Panel.ClickedLeft();
        }

        void ClickedRight()
        {
            m_Panel.ClickedRight();
        }

        bool TestIfShowingTitle()
        {
            if (m_Panel.hidePageTitle)
                return false;

            if (m_Panel.SelectedPageIndex == -1)
                return false;

            return m_Pages[m_Panel.SelectedPageIndex].Title.Length != 0;
        }
    }
}
