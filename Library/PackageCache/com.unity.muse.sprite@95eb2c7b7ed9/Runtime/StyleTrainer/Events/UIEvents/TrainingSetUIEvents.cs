using System.Collections.Generic;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;

namespace Unity.Muse.StyleTrainer.Events.TrainingSetUIEvents
{
    class AddTrainingSetEvent : BaseEvent<AddTrainingSetEvent>
    {
        public StyleData styleData;
        public IList<Texture2D> textures;
    }

    class DeleteTrainingSetEvent : BaseEvent<DeleteTrainingSetEvent>
    {
        public StyleData styleData;
        public IList<int> indices;
    }
}