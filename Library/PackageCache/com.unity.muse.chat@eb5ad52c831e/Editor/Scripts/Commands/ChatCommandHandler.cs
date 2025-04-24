using Unity.Muse.Chat.UI.Components;

namespace Unity.Muse.Chat.Commands
{
    abstract class ChatCommandHandler
    {
        public abstract string Command { get; }
        public virtual string Label => $"/{Command}";
        public virtual string PopupLabel => $"{Label} [text]";
        public abstract string PlaceHolderText { get; }
        public virtual string Tooltip => string.Empty;
        public virtual string Icon => string.Empty;
        public virtual bool ShowInList => true;

        public abstract bool UseContext { get; }
        public abstract bool UseSmartContext { get; }
        public abstract bool UseDependencies { get; }
        public abstract bool UseProjectSummary { get; }

        public virtual string Preprocess(string content) => content;
        public abstract CommandDisplayTemplate GetDisplayElement(string command);

    }
}
