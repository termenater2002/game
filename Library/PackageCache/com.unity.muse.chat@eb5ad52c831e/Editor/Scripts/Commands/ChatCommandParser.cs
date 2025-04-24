using System.Text.RegularExpressions;

namespace Unity.Muse.Chat.Commands
{
    internal static class ChatCommandParser
    {
        private static readonly Regex s_CommandPattern = new Regex(@"^\/(\w+)\s+(.*)");

        internal static bool IsCommand(string text)
        {
            return text.StartsWith('/');
        }

        public static (string command, string arguments) Parse(string input)
        {
            var match = s_CommandPattern.Match(input);

            if (match.Success)
            {
                var cmdText = match.Groups[1].Value.ToLower();
                var argumentText = match.Groups[2].Value;


                if (ChatCommands.TryGetCommandHandler(cmdText, out var handler))
                    return (cmdText, argumentText);

                // If command is unknown default, to Ask
                return (AskCommand.k_CommandName, argumentText);
            }

            return (AskCommand.k_CommandName, input);
        }
    }
}
