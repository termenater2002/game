using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat.UI.Utils
{
    static class ContextViewUtils
    {
        public static string GetObjectTooltip(Object obj)
        {
            var type = obj.GetType().ToString();
            var idx = type.LastIndexOf('.');

            if (idx != -1)
                type = type.Substring(idx + 1);

            return $"{obj.name} ({AddSpacesBeforeCapitals(type)})";
        }

        static string AddSpacesBeforeCapitals(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var newText = Regex.Replace(text, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
            return newText;
        }
    }
}
