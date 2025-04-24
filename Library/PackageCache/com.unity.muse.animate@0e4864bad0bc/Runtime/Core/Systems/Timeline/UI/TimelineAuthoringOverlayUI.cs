using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TimelineAuthoringOverlayUI : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-timeline-overlay";
        const string k_ExportButtonName = "timeline-overlay-button-export";

        ActionButton m_ExportButton;
        TimelineAuthoringOverlayUIModel m_Model;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<TimelineAuthoringOverlayUI, UxmlTraits> { }
#endif

        public TimelineAuthoringOverlayUI()
            : base(k_UssClassName) { }

        void IUITemplate.FindComponents()
        {
            m_ExportButton = this.Q<ActionButton>(k_ExportButtonName);
        }

        public void SetModel(TimelineAuthoringOverlayUIModel model)
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

            m_Model.OnChanged += OnModelChangedProperty;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChangedProperty;
        }

        void OnModelChangedProperty(TimelineAuthoringOverlayUIModel.Property property)
        {
            Update();
        }

        void Update()
        {
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            parent.style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void RegisterComponents()
        {
            m_ExportButton.clicked += OnExportButtonClicked;
        }
        
        public void UnregisterComponents()
        {
            m_ExportButton.clicked -= OnExportButtonClicked;
        }

        void OnExportButtonClicked()
        {
            m_Model.RequestExport();
        }
    }
}