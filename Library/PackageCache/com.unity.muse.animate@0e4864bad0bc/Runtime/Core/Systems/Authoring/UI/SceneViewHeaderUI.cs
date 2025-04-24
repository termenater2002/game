using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SceneViewHeaderUI : UITemplateContainer, IUITemplate
    {
        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                m_IsVisible = value;
                Update();
            }
        }

        public bool IsDisabled
        {
            get => m_IsDisabled;
            set
            {
                m_IsDisabled = value;
                Update();
            }
        }

        public VisualElement ToolbarsContainer { get; set; }
        public Text TitleText { get; set; }
        public VisualElement TitleSection { get; set; }
        bool m_IsVisible = true;
        bool m_IsDisabled;


#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SceneViewHeaderUI, UxmlTraits> { }
#endif

        public SceneViewHeaderUI()
            : base("deeppose-scene-view-header") { }

        public void FindComponents()
        {
            ToolbarsContainer = this.Q<VisualElement>("toolbars");
            TitleSection = this.Q<VisualElement>("title");
            TitleText = this.Q<Text>();
            
            // TODO: Establish paradigm for title / subtitle
            TitleText.style.display = DisplayStyle.None;
            TitleSection.style.display = DisplayStyle.None;
        }

        public void RegisterComponents()
        {
            
        }

        public void UnregisterComponents()
        {
            
        }

        public void Update()
        {
            if (!m_IsVisible)
            {
                style.display = DisplayStyle.None;
                return;
            }

            style.display = DisplayStyle.Flex;
        }
    }
}
