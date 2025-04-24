using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class TogglePanelPage : VisualElement
    {
        const string k_QuietSuffix = "--quiet";
        const string k_PageUssClassName = "deeppose-toggle-panel-page";
        const string k_PageUssClassNameQuiet = k_PageUssClassName + k_QuietSuffix;
        
        ToggleLogic ToggleLogic { get;  }
        public bool IsUsed => childCount > 0;
        public string Title => m_Title;

        int m_Index;
        string m_Title;
        string m_Icon;
        
        readonly TogglePanelPages m_Pages;

        internal TogglePanelPage(TogglePanelPages pages, int index, string title, string icon)
        {
            m_Pages = pages;
            m_Index = index;
            m_Title = title;
            m_Icon = icon;
            //verticalPageSize = 200f;
            pickingMode = PickingMode.Ignore;
            
            RegisterCallback<WheelEvent>(OnScrollWheel);
            ToggleLogic = new ToggleLogic(this, 0f, 0f);
            ToggleLogic.OnChanged += OnPageToggleChanged;

            UpdateStyle();
        }

        void UpdateStyle()
        {
            if (m_Pages == null) 
                return;
            
            RemoveFromClassList(k_PageUssClassNameQuiet);
            RemoveFromClassList(k_PageUssClassName);
            
            AddToClassList(m_Pages.Quiet?k_PageUssClassNameQuiet:k_PageUssClassName);

            style.height = !m_Pages.Expand ? new StyleLength(StyleKeyword.Auto) : new StyleLength(Length.Percent(100));
        }
        
        void OnScrollWheel(WheelEvent evt)
        {
            // TODO: Remove / test
            // Code I want to remember, it's a fix I was using to make the scrollwheel fast enough on scroll fields.
            // Could also cause problems with nested scroll views.
            // I need to test some things out before I remove or keep this.
            
            /*
            if (evt.delta.y > 0)
            {
                verticalScroller.ScrollPageDown((250f * Mathf.Abs(evt.delta.y)) / verticalPageSize);
            }
            else
            {
                verticalScroller.ScrollPageUp((250f * Mathf.Abs(evt.delta.y)) / verticalPageSize);
            }
            
            evt.StopImmediatePropagation();
            */
        }

        static void OnPageToggleChanged(ToggleLogic logic)
        {
            logic.Element.style.display = logic.ShownRatio == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            logic.Element.style.opacity = logic.ShownRatio;
        }

        public void Show()
        {
            ToggleLogic.Show();
            UpdateStyle();
        }
            
        public void Hide()
        {
            ToggleLogic.Hide();
        }
    }
}
