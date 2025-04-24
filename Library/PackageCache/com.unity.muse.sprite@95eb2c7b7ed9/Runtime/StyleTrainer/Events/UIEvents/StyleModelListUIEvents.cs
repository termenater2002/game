using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents
{
    class AddStyleButtonClickedEvent : BaseEvent<AddStyleButtonClickedEvent> { }

    class StyleModelListSelectionChangedEvent : BaseEvent<StyleModelListSelectionChangedEvent>
    {
        public StyleData styleData;
    }

    class StyleVisibilityButtonClickedEvent : BaseEvent<StyleVisibilityButtonClickedEvent>
    {
        public StyleData styleData;
        public bool visible;
    }

    class StyleDeleteButtonClickedEvent : BaseEvent<StyleDeleteButtonClickedEvent>
    {
        public StyleData styleData;
    }

    class StyleModelListCollapsedEvent : BaseEvent<StyleModelListCollapsedEvent>
    {
        public bool collapsed;
    }

    class SearchStyleEvent : BaseEvent<SearchStyleEvent>
    {
        public string search;
    }
}
