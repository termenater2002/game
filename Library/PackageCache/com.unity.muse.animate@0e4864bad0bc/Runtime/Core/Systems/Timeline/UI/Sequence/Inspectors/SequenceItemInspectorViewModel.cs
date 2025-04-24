namespace Unity.Muse.Animate
{
    class SequenceItemInspectorViewModel<T> : InspectorViewModel
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

        protected SequenceItemInspectorViewModel(InspectorsPanelViewModel inspectorsPanel)
            : base(inspectorsPanel, InspectorsPanelUtils.PageType.SelectedSequencerItem)
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
