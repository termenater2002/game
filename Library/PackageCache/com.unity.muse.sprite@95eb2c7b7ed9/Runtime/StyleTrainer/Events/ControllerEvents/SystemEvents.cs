using Unity.Muse.Sprite.Common.Events;

namespace Unity.Muse.StyleTrainer.Events.TrainingControllerEvents
{
    class SystemEvents : BaseEvent<SystemEvents>
    {
        public enum ESystemState
        {
            Dispose,
            Modified,
            RequestSave,
            CloseWindow
        }

        public ESystemState state;
    }
}