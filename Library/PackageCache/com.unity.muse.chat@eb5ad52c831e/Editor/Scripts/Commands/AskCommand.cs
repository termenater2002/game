using Unity.Muse.Chat.UI.Components;

namespace Unity.Muse.Chat.Commands
{
    class AskCommand : ChatCommandHandler
    {
        public const string k_CommandName = "ask";
        public override string Command => k_CommandName;
        public override bool ShowInList => false;
        public override bool UseContext => true;
        public override bool UseSmartContext => true;
        public override bool UseDependencies => true;
        public override bool UseProjectSummary => true;
        public override string PlaceHolderText => "Ask Muse";

        public override CommandDisplayTemplate GetDisplayElement(string command) => null;
    }
}
