using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using SliderInt = Unity.Muse.AppUI.UI.SliderInt;
using Toggle = Unity.Muse.AppUI.UI.Toggle;

namespace Unity.Muse.Animate
{
    
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class MotionToTimelineUI : UITemplateContainer, IUITemplate
    {
        const string k_PlaybackSectionName = "mtt-playback-section";
        const string k_PlaybackName = "mtt-playback";
        const string k_TimelineName = "mtt-timeline";
        const string k_MainName = "mtt-main";

        MotionToTimelineUIModel m_Model;
        BakedTimelinePlaybackUI m_PlaybackUI;
        TimelineView m_TimelineUI;

        VisualElement m_Main;
        VisualElement k_PlaybackSection;


#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<MotionToTimelineUI, UxmlTraits> { }
#endif

        public MotionToTimelineUI()
            : base("deeppose-mtt") { }

        public void FindComponents()
        {
            pickingMode = PickingMode.Ignore;
            
            // Motion To Timeline
            m_PlaybackUI = this.Q<BakedTimelinePlaybackUI>(k_PlaybackName);
            m_TimelineUI = this.Q<TimelineView>(k_TimelineName);
            k_PlaybackSection = this.Q<VisualElement>(k_PlaybackSectionName);

            // Motion to Keys UI
            m_Main = this.Q<VisualElement>(k_MainName);
            m_Main.pickingMode = PickingMode.Ignore;
        }

        public void SetModel(MotionToTimelineUIModel model)
        {
            UnregisterModel();
            m_Model = model;
            m_PlaybackUI.SetModel(m_Model.PlaybackUIModel);
            m_TimelineUI.SetModel(m_Model.TimelineUIModel);
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
            k_PlaybackSection.style.display = m_Model.PlaybackUIModel.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnModelChangedProperty(MotionToTimelineUIModel.Property property)
        {
            Update();
        }
    }
}
