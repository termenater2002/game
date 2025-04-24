using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class UITemplateContainer : VisualElement, IUITemplate
    {
        public bool IsAttachedToPanel => m_IsAttachedToPanel;
        
        bool m_IsAttachedToPanel;
        string m_StyleName;

        public string StyleName => m_StyleName;
        
        static UITemplatesRegistry Registry => Locator.Get<UITemplatesRegistry>();

        protected UITemplateContainer(string styleName)
        {
            m_StyleName = styleName;
            Initialize();
        }

        void Initialize()
        {
            CreateFromRegistry();
            
            ((IUITemplate)this).InitComponents();
            ((IUITemplate)this).FindComponents();
            
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }
        
        void CreateFromRegistry()
        {
            var assetName = GetType().Name;
            
            if (Registry.TryGetUITemplate(assetName, out var definition))
            {
                definition.Uxml.CloneTree(this);
                if (definition.StyleSheet != null)
                    styleSheets.Add(definition.StyleSheet);
            }
            else
            {
                throw new KeyNotFoundException($"Could not locate UI Template named: {assetName}");
            }
            
            if(!string.IsNullOrEmpty(m_StyleName))
                AddToClassList(m_StyleName);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            m_IsAttachedToPanel = true;
            
            ((IUITemplate)this).RegisterComponents();
            ((IUITemplate)this).Update();
        }
        
        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            m_IsAttachedToPanel = false;
            
            ((IUITemplate)this).UnregisterComponents();
        }
    }
}
