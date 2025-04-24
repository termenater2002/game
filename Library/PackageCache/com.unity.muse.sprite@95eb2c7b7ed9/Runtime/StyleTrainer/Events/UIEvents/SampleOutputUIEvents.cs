using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.SampleOutputUIEvents
{
    class AddSampleOutputEvent : BaseEvent<AddSampleOutputEvent>
    {
        public StyleData styleData;
    }

    class DeleteSampleOutputEvent : BaseEvent<DeleteSampleOutputEvent>
    {
        public StyleData styleData;
        public int deleteIndex;
    }
}
