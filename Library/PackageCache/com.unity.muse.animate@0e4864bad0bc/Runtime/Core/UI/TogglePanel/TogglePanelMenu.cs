using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class TogglePanelMenu : VisualElement
    {
        const string k_QuietSuffix = "--quiet";
        const string k_MenuUssClassName = "deeppose-toggle-panel-menu";
        const string k_MenuUssClassNameQuiet = "deeppose-toggle-panel-menu" + k_QuietSuffix;

        const string k_ButtonUssClassName = "deeppose-toggle-panel-menu-button";
        const string k_ButtonActiveSuffix = "--active";
        const string k_ButtonSingleSuffix = "--single";
        const string k_ButtonFirstSuffix = "--first";
        const string k_ButtonMiddleSuffix = "--middle";
        const string k_ButtonLastSuffix = "--last";

        const string k_ButtonActiveUssClassName = k_ButtonUssClassName + k_ButtonActiveSuffix;
        const string k_ButtonSingleUssClassName = k_ButtonUssClassName + k_ButtonSingleSuffix;
        const string k_ButtonFirstUssClassName = k_ButtonUssClassName + k_ButtonFirstSuffix;
        const string k_ButtonMiddleUssClassName = k_ButtonUssClassName + k_ButtonMiddleSuffix;
        const string k_ButtonLastUssClassName = k_ButtonUssClassName + k_ButtonLastSuffix;

        const string k_ButtonActiveUssClassNameQuiet = k_ButtonUssClassName + k_QuietSuffix + k_ButtonActiveSuffix;
        const string k_ButtonSingleUssClassNameQuiet = k_ButtonUssClassName + k_QuietSuffix + k_ButtonSingleSuffix;
        const string k_ButtonFirstUssClassNameQuiet = k_ButtonUssClassName + k_QuietSuffix + k_ButtonFirstSuffix;
        const string k_ButtonMiddleUssClassNameQuiet = k_ButtonUssClassName + k_QuietSuffix + k_ButtonMiddleSuffix;
        const string k_ButtonLastUssClassNameQuiet = k_ButtonUssClassName + k_QuietSuffix + k_ButtonLastSuffix;

        public delegate void ButtonClicked(TogglePanelMenu menu, TogglePanelMenuButton button, int index);

        public event ButtonClicked OnButtonClicked;

        enum MenuButtonPositionType
        {
            Single,
            First,
            Middle,
            Last
        }

        readonly List<TogglePanelMenuButton> m_Buttons;
        readonly TogglePanel m_Panel;

        public TogglePanelMenu(TogglePanel panel)
        {
            m_Panel = panel;
            m_Buttons = new List<TogglePanelMenuButton>();
            pickingMode = PickingMode.Ignore;
        }

        public TogglePanelMenuButton AddButton(string icon, string toolTip)
        {
            var index = m_Buttons.Count;
            var button = new TogglePanelMenuButton(index, icon, toolTip);
            button.clicked += () => ClickedIndex(index);
            Add(button);
            m_Buttons.Add(button);
            return button;
        }

        public TogglePanelMenuButton GetButton(int index)
        {
            return m_Buttons[index];
        }

        public void Update()
        {
            // Reset Style
            RemoveFromClassList(k_MenuUssClassName);
            RemoveFromClassList(k_MenuUssClassNameQuiet);

            AddToClassList(m_Panel.quiet ? k_MenuUssClassNameQuiet : k_MenuUssClassName);

            // Hide / Show entire menu
            style.display = TestIfShowingMenu() ? DisplayStyle.Flex : DisplayStyle.None;

            // Is the panel expanding
            if (m_Panel.expand)
            {
                // Fill the whole column
                style.flexGrow = 1;
                style.height = new StyleLength(Length.Percent(100));
            }
            else
            {
                // Expand to fit inner content
                style.flexGrow = 1;
                style.height = new StyleLength(StyleKeyword.Auto);
            }

            // Aligned to top or bottom
            if (m_Panel.alignBottom)
            {
                style.alignItems = Align.FlexEnd;
            }
            else
            {
                style.alignItems = Align.FlexStart;
            }

            // Is the first item on top or at bottom
            if (m_Panel.reverseOrder)
            {
                style.flexDirection = FlexDirection.ColumnReverse;
            }
            else
            {
                style.flexDirection = FlexDirection.Column;
            }

            // Update the buttons
            UpdateButtons();
        }

        void UpdateButtons() 
        {
            using var indexes = TempList<int>.Allocate();

            for (var i = 0; i < m_Buttons.Count; i++)
            {
                if (IsUsed(i) || !m_Panel.hidePageIfEmpty)
                {
                    indexes.Add(i);
                }
            }

            switch (indexes.Count)
            {
                case 0:
                    return;
                case 1:
                    SetButtonStyle(0, MenuButtonPositionType.Single);
                    return;
            }

            var position = 0;

            foreach (var index in indexes)
            {
                if (position == 0)
                {
                    SetButtonStyle(index, m_Panel.reverseOrder ? MenuButtonPositionType.Last : MenuButtonPositionType.First);
                }
                else if (position == indexes.Count - 1)
                {
                    SetButtonStyle(index, m_Panel.reverseOrder ? MenuButtonPositionType.First : MenuButtonPositionType.Last);
                }
                else
                {
                    SetButtonStyle(index, MenuButtonPositionType.Middle);
                }
                
                position ++;
            }
        }

        bool TestIfShowingMenu()
        {
            if (m_Panel.hideMenuIfNoPageShown)
            {
                if (m_Panel.SelectedPageIndex == -1)
                    return false;
            }

            if (m_Panel.hideMenuWhenNoPage)
            {
                if (m_Panel.hidePageIfEmpty && m_Panel.NbPagesUsed == 0)
                    return false;
                
                if (!m_Panel.hidePageIfEmpty && m_Panel.NbPages == 0)
                    return false;
            }
            
            if (m_Panel.hideMenuIfPageShown)
            {
                if (m_Panel.SelectedPageIndex != -1)
                    return false;
            }

            return true;
        }

        void ClickedIndex(int index)
        {
            OnButtonClicked?.Invoke(this, m_Buttons[index], index);
        }

        void SetButtonStyle(int index, MenuButtonPositionType positionInMenu)
        {
            SetButtonStyle(m_Buttons[index], positionInMenu, IsActive(index), IsUsed(index));
        }

        void SetButtonStyle(VisualElement button, MenuButtonPositionType positionInMenu, bool isActive, bool isUsed)
        {
            string styleToEnable = positionInMenu switch
            {
                MenuButtonPositionType.Single => m_Panel.quiet ? k_ButtonSingleUssClassNameQuiet : k_ButtonSingleUssClassName,
                MenuButtonPositionType.First => m_Panel.quiet ? k_ButtonFirstUssClassNameQuiet : k_ButtonFirstUssClassName,
                MenuButtonPositionType.Middle => m_Panel.quiet ? k_ButtonMiddleUssClassNameQuiet : k_ButtonMiddleUssClassName,
                MenuButtonPositionType.Last => m_Panel.quiet ? k_ButtonLastUssClassNameQuiet : k_ButtonLastUssClassName,
                _ => throw new ArgumentOutOfRangeException(nameof(positionInMenu), positionInMenu, null)
            };

            // Regular Position-Based Styles
            EnableOrDisable(button, k_ButtonSingleUssClassName, styleToEnable);
            EnableOrDisable(button, k_ButtonFirstUssClassName, styleToEnable);
            EnableOrDisable(button, k_ButtonMiddleUssClassName, styleToEnable);
            EnableOrDisable(button, k_ButtonLastUssClassName, styleToEnable);

            // Quiet Position-Based Styles
            EnableOrDisable(button, k_ButtonSingleUssClassNameQuiet, styleToEnable);
            EnableOrDisable(button, k_ButtonFirstUssClassNameQuiet, styleToEnable);
            EnableOrDisable(button, k_ButtonMiddleUssClassNameQuiet, styleToEnable);
            EnableOrDisable(button, k_ButtonLastUssClassNameQuiet, styleToEnable);

            // Regular/Quiet Active Style
            var activeStyleName = m_Panel.quiet ? k_ButtonActiveUssClassNameQuiet : k_ButtonActiveUssClassName;
            EnableOrDisable(button, k_ButtonActiveUssClassNameQuiet, activeStyleName);
            EnableOrDisable(button, k_ButtonActiveUssClassName, activeStyleName);

            button.EnableInClassList(m_Panel.quiet ? k_ButtonActiveUssClassNameQuiet : k_ButtonActiveUssClassName, isActive);

            // Hide / Show button
            // Depending on if there is a page with content in it attached to the button
            button.style.display = isUsed || !m_Panel.hidePageIfEmpty ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static void EnableOrDisable(VisualElement button, string style, string styleToEnable)
        {
            button.EnableInClassList(style, style.Equals(styleToEnable));
        }

        bool IsActive(int index)
        {
            return m_Panel.SelectedPageIndex == index;
        }

        bool IsUsed(int index)
        {
            return m_Panel.IsPageUsed(index);
        }
    }
}
