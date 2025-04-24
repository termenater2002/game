using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class SceneViewUI : UITemplateContainer, IUITemplate
    {
        const string k_SceneViewTitleLabelElementName = "scene-view-title-label";
        
        public SceneViewHeaderUI Header { get; private set; }
        public SceneViewPlayArea PlayArea { get; private set; }

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

        bool m_IsVisible = true;
        bool m_IsDisabled;
        AuthorContext m_AuthorContext;


#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SceneViewUI, UxmlTraits> { }
#endif

        public SceneViewUI()
            : base("deeppose-scene-view") { }

        public void SetContext(AuthorContext authorContext)
        {
            m_AuthorContext = authorContext;
            PlayArea.SetContext(authorContext);
        }
        
        public void FindComponents()
        {
            Header = this.Q<SceneViewHeaderUI>();
            PlayArea = this.Q<SceneViewPlayArea>();
        }

        public void RegisterComponents()
        {
            
        }

        public void UnregisterComponents()
        {
            
        }
        
        public void Update()
        {
            PlayArea?.Update();
            
            if (!m_IsVisible)
            {
                style.display = DisplayStyle.None;
                return;
            }

            style.display = DisplayStyle.Flex;
        }

        public void LateUpdate()
        {
            PlayArea.LateUpdate();
        }
        
        public void Render()
        {
            PlayArea.Render();
        }

        public new void Clear()
        {
            PlayArea.Clear();
            m_AuthorContext = null;
        }
    }
}
