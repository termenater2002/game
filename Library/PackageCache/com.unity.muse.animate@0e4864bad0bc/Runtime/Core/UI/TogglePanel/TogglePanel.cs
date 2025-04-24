using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    abstract partial class TogglePanel : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-toggle-panel";
        const string k_BackgroundUssClassName = "deeppose-toggle-panel-background";
        const string k_QuietUssClassName = "--quiet";
        const string k_UssClassNameQuiet = k_UssClassName + k_QuietUssClassName;
        const string k_BackgroundUssClassNameQuiet = k_BackgroundUssClassName + k_QuietUssClassName;
        
        public TogglePanelMenu Menu => m_Menu;
        
        public int NbPages => m_Pages.NbPages;
        public int NbPagesUsed => m_NbPagesUsed;

        public int SelectedPageIndex => m_Model?.SelectedPageIndex ?? -1;

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool reverseOrder
        {
            get => m_ReverseOrder;
            set
            {
                m_ReverseOrder = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool alignRight
        {
            get => m_AlignRight;
            set
            {
                m_AlignRight = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool expand
        {
            get => m_Expand;
            set
            {
                m_Expand = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool selectDefaultPage
        {
            get => m_SelectDefaultPage;
            set
            {
                m_SelectDefaultPage = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool quiet
        {
            get => m_Quiet;
            set
            {
                m_Quiet = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool alignBottom
        {
            get => m_AlignBottom;
            set
            {
                m_AlignBottom = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int contentWidth
        {
            get => m_ContentWidth;
            set
            {
                m_ContentWidth = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool hidePageTitle
        {
            get => m_HidePageTitle;
            set
            {
                m_HidePageTitle = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool hideMenuWhenNoPage
        {
            get => m_HideMenuWhenNoPage;
            set
            {
                m_HideMenuWhenNoPage = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool hideMenuIfPageShown
        {
            get => m_HideMenuIfPageShown;
            set
            {
                m_HideMenuIfPageShown = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool hidePageIfEmpty
        {
            get => m_HidePageIfEmpty;
            set
            {
                m_HidePageIfEmpty = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool hideMenuIfNoPageShown
        {
            get => m_HideMenuIfNoPageShown;
            set
            {
                m_HideMenuIfNoPageShown = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string leftButtonIcon
        {
            get => m_LeftButtonIcon;
            set
            {
                m_LeftButtonIcon = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string rightButtonIcon
        {
            get => m_RightButtonIcon;
            set
            {
                m_RightButtonIcon = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string leftButtonTooltip
        {
            get => m_LeftButtonTooltip;
            set
            {
                m_LeftButtonTooltip = value;
                Update();
            }
        }

#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string rightButtonTooltip
        {
            get => m_RightButtonTooltip;
            set
            {
                m_RightButtonTooltip = value;
                Update();
            }
        }

        TogglePanelPages m_Pages;
        TogglePanelMenu m_Menu;
        VisualElement m_Background;

        int m_SelectedPageIndex;
        int m_NbPagesUsed;
        
        bool m_ReverseOrder;
        bool m_AlignRight;
        bool m_Expand;
        bool m_SelectDefaultPage = true;
        bool m_Quiet;
        bool m_AlignBottom;
        int m_ContentWidth = 260;
        
        bool m_HidePageTitle;
        bool m_HideMenuWhenNoPage;
        bool m_HideMenuIfPageShown = true;
        bool m_HidePageIfEmpty = true;
        bool m_HideMenuIfNoPageShown;

        string m_LeftButtonIcon;
        string m_RightButtonIcon;
        string m_LeftButtonTooltip;
        string m_RightButtonTooltip;
        
        TogglePanelUIModel m_Model;

        protected TogglePanel()
            : base("") { }

        protected TogglePanel(string styleName)
            : base(styleName) { }

        public void InitComponents()
        {
            AddToClassList(k_UssClassName);
            pickingMode = PickingMode.Ignore;

            m_Background = new VisualElement();
            m_Menu = new TogglePanelMenu(this);
            m_Pages = new TogglePanelPages(this);

            Add(m_Background);
            Add(m_Menu);
            Add(m_Pages);
        }

        public void RegisterComponents()
        {
            m_Menu.OnButtonClicked += OnMenuButtonClicked;
        }

        public void UnregisterComponents()
        {
            m_Menu.OnButtonClicked -= OnMenuButtonClicked;
        }

        protected void SetModel(TogglePanelUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChanged;
            m_Model.OnPanelAdded += OnPanelAdded;
            m_Model.OnPanelRemoved += OnPanelRemoved;
            m_Model.OnSelectedPageIndexChanged += OnModelSelectedPageIndexChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnChanged -= OnModelChanged;
            m_Model.OnPanelAdded -= OnPanelAdded;
            m_Model.OnPanelRemoved -= OnPanelRemoved;
            m_Model.OnSelectedPageIndexChanged -= OnModelSelectedPageIndexChanged;
        }

        public void Update()
        {
            if (m_Model == null)
                return;
            
            // Reset the style
            RemoveFromClassList(k_UssClassName);
            RemoveFromClassList(k_UssClassNameQuiet);

            m_Background.RemoveFromClassList(k_BackgroundUssClassName);
            m_Background.RemoveFromClassList(k_BackgroundUssClassNameQuiet);

            // Quiet vs. Base style
            AddToClassList(quiet ? k_UssClassNameQuiet : k_UssClassName);
            m_Background.AddToClassList(quiet ? k_BackgroundUssClassNameQuiet : k_BackgroundUssClassName);

            // Menu to the right or left
            style.flexDirection = alignRight ? FlexDirection.RowReverse : FlexDirection.Row;

            // Does the panel itself expands to fill the whole column or does it only matches the minimum required space
            if (expand)
            {
                style.flexGrow = 1;
                style.height = new StyleLength(Length.Percent(100));
                style.alignItems = Align.Stretch;
            }
            else
            {
                style.flexGrow = 0;
                style.height = new StyleLength(StyleKeyword.Auto);
                style.alignItems = Align.FlexEnd;
            }
            
            // Order list items starting from top or bottom
            style.justifyContent = reverseOrder ? Justify.FlexEnd : Justify.FlexStart;

            // Update the style of the sub-objects
            m_Pages.Update();
            m_Menu.Update();
            
            if (m_Model == null)
                return;

            // Update the visibility
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (!IsAttachedToPanel)
                return;
        }

        void AttachToPage(int index, VisualElement visualElement)
        {
            var wasEmpty = !m_Pages.IsPageUsed(index);
            m_Pages.AttachToPage(index, visualElement);

            if (wasEmpty)
            {
                m_NbPagesUsed++;
                PagesUsedChanged(index, true);
            }

            if (selectDefaultPage && (index == 0 || SelectedPageIndex == -1))
            {
                SelectPage(index);
            }
        }

        void RemoveFromPage(int index, VisualElement visualElement)
        {
            var wasUsed = m_Pages.IsPageUsed(index);
            m_Pages.DetachFromPage(index, visualElement);
            var isNowEmpty = !m_Pages.IsPageUsed(index);

            if (wasUsed && isNowEmpty)
            {
                m_NbPagesUsed--;
                PagesUsedChanged(index, false);
            }
        }

        void PagesUsedChanged(int index, bool used)
        {
            if (SelectedPageIndex != index)
                return;
            
            // If the page isn't used anymore (empty) and empty pages are not allowed,
            // then unselect the page.
            
            if (!used && hidePageIfEmpty)
            {
                SelectPage(-1);
            }
        }

        void OnModelSelectedPageIndexChanged()
        {
            Update();
        }

        void OnModelChanged()
        {
            Update();
        }

        void OnPanelRemoved(int page, VisualElement element)
        {
            RemoveFromPage(page, element);
        }

        void OnPanelAdded(int page, VisualElement element)
        {
            AttachToPage(page, element);
        }

        void OnMenuButtonClicked(TogglePanelMenu menu, TogglePanelMenuButton button, int index)
        {
            if (SelectedPageIndex == index)
            {
                SelectPage(-1);
            }
            else
            {
                SelectPage(index);
            }
        }

        protected void SelectPage(int index)
        {
            if (m_Model == null)
                return;
            
            m_Model.SelectedPageIndex = index;
        }

        protected void AddPage(string title, string icon)
        {
            m_Pages.AddPage(title, icon);
        }

        protected void AddButton(string icon, string tooltip)
        {
            m_Menu.AddButton(icon, tooltip);
        }

        public bool IsPageUsed(int index)
        {
            return m_Pages.IsPageUsed(index);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_SelectDefaultPage = new()
            {
                name = "select-default-page",
                defaultValue = true,
            };

            readonly UxmlBoolAttributeDescription m_Quiet = new()
            {
                name = "quiet",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_AlignBottom = new()
            {
                name = "align-bottom",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_AlignRight = new()
            {
                name = "align-right",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Expand = new()
            {
                name = "expand",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_HidePageTitle = new()
            {
                name = "hide-page-title",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_HidePageIfEmpty = new()
            {
                name = "hide-page-if-empty",
                defaultValue = true,
            };

            readonly UxmlBoolAttributeDescription m_HideMenuWhenNoPage = new()
            {
                name = "hide-menu-when-no-page",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_HideMenuIfPageShown = new()
            {
                name = "hide-menu-if-page-shown",
                defaultValue = true,
            };

            readonly UxmlBoolAttributeDescription m_HideMenuIfNoPageShown = new()
            {
                name = "hide-menu-if-no-page-shown",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_ReverseOrder = new()
            {
                name = "reverse-order",
                defaultValue = false,
            };

            readonly UxmlStringAttributeDescription m_LeftButtonIcon = new()
            {
                name = "left-button-icon",
                defaultValue = "",
            };

            readonly UxmlStringAttributeDescription m_RightButtonIcon = new()
            {
                name = "right-button-icon",
                defaultValue = "",
            };

            readonly UxmlStringAttributeDescription m_LeftButtonTooltip = new()
            {
                name = "left-button-tooltip",
                defaultValue = "",
            };

            readonly UxmlStringAttributeDescription m_RightButtonTooltip = new()
            {
                name = "right-button-tooltip",
                defaultValue = "",
            };
            
            readonly UxmlIntAttributeDescription m_ContentWidth = new()
            {
                name = "content-width",
                defaultValue = 260,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (TogglePanel)ve;

                el.selectDefaultPage = m_SelectDefaultPage.GetValueFromBag(bag, cc);
                el.quiet = m_Quiet.GetValueFromBag(bag, cc);
                el.alignRight = m_AlignRight.GetValueFromBag(bag, cc);
                el.expand = m_Expand.GetValueFromBag(bag, cc);
                el.alignBottom = m_AlignBottom.GetValueFromBag(bag, cc);
                el.hidePageTitle = m_HidePageTitle.GetValueFromBag(bag, cc);
                el.hidePageIfEmpty = m_HidePageIfEmpty.GetValueFromBag(bag, cc);
                el.hideMenuWhenNoPage = m_HideMenuWhenNoPage.GetValueFromBag(bag, cc);
                el.hideMenuIfPageShown = m_HideMenuIfPageShown.GetValueFromBag(bag, cc);
                el.hideMenuIfNoPageShown = m_HideMenuIfNoPageShown.GetValueFromBag(bag, cc);
                el.reverseOrder = m_ReverseOrder.GetValueFromBag(bag, cc);
                el.rightButtonIcon = m_RightButtonIcon.GetValueFromBag(bag, cc);
                el.rightButtonTooltip = m_RightButtonTooltip.GetValueFromBag(bag, cc);
                el.leftButtonIcon = m_LeftButtonIcon.GetValueFromBag(bag, cc);
                el.leftButtonTooltip = m_LeftButtonTooltip.GetValueFromBag(bag, cc);
                el.contentWidth = m_ContentWidth.GetValueFromBag(bag, cc);
                el.Update();
            }
        }
#endif
        public virtual void ClickedLeft() { }

        public virtual void ClickedRight() { }
    }
}
