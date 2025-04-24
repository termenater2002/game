using System.Collections.Generic;
using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.StyleTrainerProjectEvents
{
    class StyleModelSourceChangedEvent : BaseEvent<StyleModelSourceChangedEvent>
    {
        public int selectedIndex;
        public IReadOnlyList<StyleData> styleModels;
    }
}