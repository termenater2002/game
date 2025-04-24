using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Unity.Muse.Chat
{
    internal static class CodeExportUtils
    {
        private static readonly Regex k_UsingsRegex = new(@"\s*using\s+([\w\.]+)\s*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex k_ClassRegex = new Regex(@"^.*?\s*class\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly IList<string> k_UsingTemp = new List<string>();
        private static readonly IList<string> k_ContentTemp = new List<string>();

        public static string Format(string source, string defaultClassName = "MuseCodeExport")
        {
            var output = new StringBuilder();
            output.Append(string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString()));

            k_UsingTemp.Clear();
            k_UsingTemp.Add("System");
            k_UsingTemp.Add("UnityEditor");
            k_UsingTemp.Add("UnityEngine");

            bool hasClass = false;
            bool isBehaviorClass = false;

            // Anlyze the code block
            k_ContentTemp.Clear();
            string[] lines = source.Split("\n");
            int indent = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var usingMatch = k_UsingsRegex.Match(line);
                if (usingMatch.Success)
                {
                    // Filter out usings, they have to be outside of the class
                    string usingValue = usingMatch.Groups[1].Value;
                    if (!k_UsingTemp.Contains(usingValue))
                    {
                        k_UsingTemp.Add(usingValue);
                    }

                    continue;
                }

                if (k_ClassRegex.IsMatch(line))
                {
                    hasClass = true;
                }

                if (line.IndexOf(": MonoBehaviour", StringComparison.Ordinal) > 0 || line.IndexOf("GetComponent<", StringComparison.Ordinal) > 0)
                {
                    isBehaviorClass = true;
                }

                k_ContentTemp.Add(line);
            }

            // Now re-construct the code with the result of the above analysis
            for (var i = 0; i < k_UsingTemp.Count; i++)
            {
                output.AppendLine($"using {k_UsingTemp[i]};");
            }

            if (!hasClass)
            {
                indent++;
                string classInherit = isBehaviorClass ? " : MonoBehaviour" : "";
                output.AppendLine($"class {defaultClassName}{classInherit}");
                output.AppendLine("{");
            }

            string codeIndent = new string(' ', indent * 4);
            for (var i = 0; i < k_ContentTemp.Count; i++)
            {
                output.Append(codeIndent);
                output.AppendLine(k_ContentTemp[i]);
            }

            if (!hasClass)
            {
                output.AppendLine("}");
            }

            return output.ToString();
        }

        public static string AddDisclaimer(string source)
        {
            var output = new StringBuilder();
            output.Append(string.Format(MuseChatConstants.DisclaimerText, DateTime.Now.ToShortDateString()));
            output.Append(source);
            return output.ToString();
        }
    }
}
