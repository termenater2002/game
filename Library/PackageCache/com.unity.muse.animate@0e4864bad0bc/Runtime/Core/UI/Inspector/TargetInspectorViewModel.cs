namespace Unity.Muse.Animate
{
    class TargetInspectorViewModel<T> : InspectorViewModel
    {
        public virtual T Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                NotifyTargetChange();
            }
        }

        public bool HasTarget => m_Target != null;

        protected T m_Target;

        protected TargetInspectorViewModel(InspectorsPanelViewModel inspectorsPanel, InspectorsPanelUtils.PageType pageType)
            : base(inspectorsPanel, pageType)
        {
            
        }

        public override void Close()
        {
            Target = default(T);
            base.Close();
        }

        protected void NotifyTargetChange()
        {
            NotifyChanged();
        }
    }
}
