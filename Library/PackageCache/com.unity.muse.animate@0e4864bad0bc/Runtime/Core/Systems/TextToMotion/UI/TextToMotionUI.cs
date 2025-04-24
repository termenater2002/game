using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
    
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TextToMotionUI: UITemplateContainer, IUITemplate
    {
        const string k_PlaybackName = "t2m-playback";
        
        TextToMotionUIModel m_Model;
        BakedTimelinePlaybackUI m_PlaybackUI;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<TextToMotionUI, UxmlTraits> { }
#endif

        public TextToMotionUI() : base("deeppose-t2m") { }

        void IUITemplate.FindComponents()
        {
            m_PlaybackUI = this.Q<BakedTimelinePlaybackUI>(k_PlaybackName);
        }

        public void SetModel(TextToMotionUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            m_PlaybackUI.SetModel(m_Model.PlaybackUI);
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnChanged += OnModelChangedProperty;
        }
        
        void UnregisterModel()
        {
            if (m_Model == null)
                return;
            
            m_Model.OnChanged -= OnModelChangedProperty;
        }

        public void Update()
        {
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;

            m_PlaybackUI.style.display = DisplayStyle.Flex;
        }
        
        void OnModelChangedProperty(TextToMotionUIModel.Property property)
        {
            Update();
        }
    }
}
