using System.Collections.Generic;
using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.StyleModelEvents
{
    class FavouriteCheckPointChangeEvent : BaseEvent<FavouriteCheckPointChangeEvent>
    {
        public StyleData styleData;
    }

    class DuplicateButtonStateUpdateEvent : BaseEvent<DuplicateButtonStateUpdateEvent>
    {
        public bool state;
    }

    class GenerateButtonStateUpdateEvent : BaseEvent<GenerateButtonStateUpdateEvent>
    {
        public bool state;
    }

    class RequestChangeTabEvent : BaseEvent<RequestChangeTabEvent>
    {
        public int tabIndex;
        public IList<int> highlightIndices;
    }
}
