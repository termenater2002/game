using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TimelineView : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-timeline";

        TimelineViewModel m_Model;

        SequenceView m_SequenceView;
        PlaybackView m_PlaybackView;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<TimelineView, UxmlTraits> { }
#endif

        public TimelineView()
            : base(k_UssClassName) { }

        public void FindComponents()
        {
            m_SequenceView = this.Q<SequenceView>();
            m_PlaybackView = this.Q<PlaybackView>();
        }

        public void SetModel(TimelineViewModel model)
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

            m_SequenceView.SetModel(m_Model.SequenceViewModel);
            m_PlaybackView.SetModel(m_Model.PlaybackViewModel);

            m_Model.OnChanged += OnModelChanged;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChanged;
            m_Model = null;
        }

        public void Update()
        {
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            if (m_Model == null)
            {
                style.display = DisplayStyle.None;
                return;
            }

            if (!IsAttachedToPanel)
            {
                style.display = DisplayStyle.None;
                return;
            }
            
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnModelChanged(TimelineViewModel.Property property)
        {
            if (property != TimelineViewModel.Property.Visibility)
                return;

            UpdateVisibility();
        }
    }
}
