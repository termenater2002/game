using System.Collections.Generic;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;

namespace Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents
{
    class SampleOutputDataSourceChangedEvent : BaseEvent<SampleOutputDataSourceChangedEvent>
    {
        public IReadOnlyList<string> sampleOutput;
        public StyleData styleData;
    }
}