using System.Collections.Generic;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;

namespace Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents
{
    class TrainingSetDataSourceChangedEvent : BaseEvent<TrainingSetDataSourceChangedEvent>
    {
        public IReadOnlyList<TrainingSetData> trainingSetData;
        public StyleData styleData;
    }
}