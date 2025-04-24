using System.Text.RegularExpressions;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Components.ChatElements;

namespace Unity.Muse.Chat.Commands
{
    class CodeCommand : ChatCommandHandler
    {
        const string k_FenceName = "validate-csharp";
        public const string k_CommandName = "code";
        public override string Command => k_CommandName;
        public override bool UseContext => true;
        public override bool UseSmartContext => true;
        public override bool UseDependencies => true;
        public override bool UseProjectSummary => true;
        public override string PlaceHolderText => "Use a dedicated code generator";
        public override string Tooltip => "Generating code is experimental and may not be reliable or consistent. We recommend using it only for testing.";
        public override string Icon => "mui-icon-cmd-code";

#if !ENABLE_ASSISTANT_BETA_FEATURES
        public override bool ShowInList => false;
#endif

        public override string Preprocess(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            // Replace all backticks with triple backticks
            var standardized = Regex.Replace(content, @"(`{1,})", "```");

            // Replace csharp markup with validate-csharp markup
            return standardized.Replace("```csharp", "```validate-csharp");
        }
        public override CommandDisplayTemplate GetDisplayElement(string command)
        {
            if (command == k_FenceName)
                return new ChatElementValidatedCodeBlock();

            return null;
        }
    }
}
