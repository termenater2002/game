namespace Unity.Muse.Sprite.Common.Events
{
    internal interface IEvent
    {
        /// <summary>
        /// Temporary solution for sending queue events
        /// </summary>
        /// <param name="evtBus"></param>
        void SendEvent(EventBus evtBus);
    }

    internal abstract class BaseEvent : IEvent
    {
        public abstract void SendEvent(EventBus evtBus);
    }

    internal class BaseEvent<T> : BaseEvent where T : BaseEvent
    {
        public override void SendEvent(EventBus evtBus)
        {
            evtBus.SendEvent(this as T);
        }
    }
}
