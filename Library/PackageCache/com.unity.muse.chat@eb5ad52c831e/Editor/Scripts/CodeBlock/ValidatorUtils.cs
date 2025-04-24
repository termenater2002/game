using System.Linq;
using System.Text.RegularExpressions;

namespace Unity.Muse.Chat
{
    internal class ValidatorUtils
    {
        public static string HandleValidatorMarkup(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            return StandardizeCodeMarkup(message);
        }

        static string StandardizeCodeMarkup(string message)
        {
            // Replace all backticks with triple backticks
            var standardized = Regex.Replace(message, @"(`{1,})", "```");

            // Replace csharp markup with validate-csharp markup
            return standardized.Replace("```csharp", "```validate-csharp");
        }
    }
}
