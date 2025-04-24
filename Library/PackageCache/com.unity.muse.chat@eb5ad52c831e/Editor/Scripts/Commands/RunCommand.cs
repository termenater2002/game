using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Components.ChatElements;

namespace Unity.Muse.Chat.Commands
{
    class RunCommand : ChatCommandHandler
    {
        public const string k_CommandName = "run";
        public override string Command => k_CommandName;
        public override bool UseContext => true;
        public override bool UseSmartContext => true;
        public override bool UseDependencies => true;
        public override bool UseProjectSummary => true;
        public override string PlaceHolderText => "Run a command";
        public override string Tooltip => "Running commands is experimental and may not be reliable or consistent. We recommend using it only for testing.";
        public override string Icon => "mui-icon-cmd-run";
        public override string Preprocess(string content) => RunCommandMessageUtils.HandleCommandMarkup(content);

#if !ENABLE_ASSISTANT_BETA_FEATURES
        public override bool ShowInList => false;
#endif

        public override CommandDisplayTemplate GetDisplayElement(string command)
        {
            if (command == "csx")
                return new ChatElementRunCommandBlock();

            if (command == "csx_execute")
                return new ChatElementRunExecutionBlock();

            return null;
        }
    }
}
