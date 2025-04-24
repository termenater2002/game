using System.Text.RegularExpressions;

namespace Unity.Muse.Editor.Markup
{
    /// <summary>
    /// Utility class to help with reformatting text appearing in the chat.
    /// </summary>
    internal class MarkupUtil
    {
        private static readonly Regex s_CarriageReturnPattern = new Regex("(?<!\\\\)(\\\\r|\\\\n)");

        public static string QuoteCarriageReturn(string text)
        {
            return s_CarriageReturnPattern.Replace(text, "<noparse>\\$1</noparse>");
        }
    }
}
