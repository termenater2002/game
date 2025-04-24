using System;
using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.CheckPointModelEvents
{
    class CheckPointSourceDataChangedEvent : BaseEvent<CheckPointSourceDataChangedEvent>
    {
        public StyleData styleData;
    }

    class CheckPointDataChangedEvent : BaseEvent<CheckPointDataChangedEvent>
    {
        public StyleData styleData;
        public CheckPointData checkPointData;
    }

    class RequestCheckPointStatusEvent : BaseEvent<RequestCheckPointStatusEvent>
    {
        public CheckPointData checkPointData;
        public Action<CheckPointData, string, bool> onDoneCallback;
    }
}