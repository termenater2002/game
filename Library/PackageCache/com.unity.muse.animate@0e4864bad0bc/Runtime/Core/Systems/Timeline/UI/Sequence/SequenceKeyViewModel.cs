namespace Unity.Muse.Animate
{
    class SequenceKeyViewModel : SequenceItemViewModel<TimelineModel.SequenceKey>
    {
        protected override void RegisterCallbacks()
        {
            base.RegisterCallbacks();
            Target.Key.OnChanged += OnKeyModelChanged;
        }
        
        protected override void UnregisterCallbacks()
        {
            Target.Key.OnChanged -= OnKeyModelChanged;
            base.UnregisterCallbacks();
        }
        
        void OnKeyModelChanged(KeyModel model, KeyModel.Property property)
        {
            if (property is KeyModel.Property.Thumbnail or KeyModel.Property.Type)
            {
                InvokeChanged();
            }
        }

        public SequenceKeyViewModel(TimelineModel.SequenceKey key)
            : base(key) { }
    }
}
