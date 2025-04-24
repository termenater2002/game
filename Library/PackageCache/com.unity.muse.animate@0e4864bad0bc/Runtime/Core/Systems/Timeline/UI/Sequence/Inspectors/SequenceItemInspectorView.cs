namespace Unity.Muse.Animate
{
    abstract class SequenceItemInspectorView<T> : InspectorView
    {
        protected new SequenceItemInspectorViewModel<T> Model => base.Model as SequenceItemInspectorViewModel<T>;
    }
}
