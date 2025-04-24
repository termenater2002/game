
namespace Unity.Muse.Animate
{
    class InspectorViewModel
    {
        public delegate void Changed();
        public event Changed OnChanged;
        
        public InspectorsPanelViewModel InspectorsPanel => m_InspectorsPanel;
        public InspectorsPanelUtils.PageType PageType => m_PageType;

        
        public bool IsVisible
        {
            get => m_IsVisible;
            internal set
            {
                if (value == m_IsVisible)
                    return;

                m_IsVisible = value;
                OnChanged?.Invoke();
            }
        }

        bool m_IsVisible;
        InspectorsPanelUtils.PageType m_PageType;
        readonly InspectorsPanelViewModel m_InspectorsPanel;

        public InspectorViewModel(InspectorsPanelViewModel inspectorsPanel, InspectorsPanelUtils.PageType pageType)
        {
            m_InspectorsPanel = inspectorsPanel;
            m_PageType = pageType;
        }

        public virtual void Close()
        {
            IsVisible = false;
        }
        
        protected void NotifyChanged()
        {
            OnChanged?.Invoke();
        }
    }
}
