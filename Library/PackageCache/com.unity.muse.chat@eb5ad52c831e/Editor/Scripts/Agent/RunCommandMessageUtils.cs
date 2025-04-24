using System.Linq;
using System.Text.RegularExpressions;

namespace Unity.Muse.Chat
{
    class RunCommandMessageUtils
    {
        public static string HandleCommandMarkup(string message)
        {
            if (string.IsNullOrEmpty(message) || !ContainsRunCommandIndicator(message))
            {
                return message;
            }

            return ProcessRunCommand(message);
        }

        static bool ContainsRunCommandIndicator(string message)
        {
            var agentActionIndicators = new[] { "IRunCommand", "CommandScript", "ExecutionResult", "PreviewBuilder" };

            return agentActionIndicators.Any(message.Contains);
        }

        static string ProcessRunCommand(string message)
        {
            // First, check for and handle existing csx code block
            var csxBlockMatch = Regex.Match(message, @"^(.*?)```csx(.*?)```(.*)$", RegexOptions.Singleline);
            if (csxBlockMatch.Success)
            {
                return HandleExistingCsxBlock(csxBlockMatch);
            }

            // If no existing csx block, standardize the markup
            var standardizedMessage = StandardizeCodeMarkup(message);

            // Ensure the message is wrapped in a csx code block
            if (!RunCommandInterpreter.k_CsxMarkupRegex.IsMatch(standardizedMessage))
            {
                standardizedMessage = $"```csx\n{standardizedMessage}\n```";
            }

            return standardizedMessage;
        }

        static string HandleExistingCsxBlock(Match csxBlockMatch)
        {
            var beforeCsx = RemoveBackticksIfNoCodeBlock(csxBlockMatch.Groups[1].Value);
            var csxContent = csxBlockMatch.Groups[2].Value;

            // Text after the code block is removed
            return $"{beforeCsx}```csx{csxContent}```";
        }

        static string RemoveBackticksIfNoCodeBlock(string text)
        {
            return Regex.IsMatch(text, @"```(.*?)```", RegexOptions.Singleline)
                ? text
                : text.Replace("`", "");
        }

        static string StandardizeCodeMarkup(string message)
        {
            // Replace all backticks with triple backticks
            var standardized = Regex.Replace(message, @"(`{1,})", "```");

            // Replace csharp markup with csx markup
            return standardized.Replace("```csharp", "```csx");
        }
    }
}
