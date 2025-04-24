using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.DeepPose.Core
{
    static class PromptUtils
    {
        static readonly string[] k_ConnectingWords = { "and", "then", "next", "finally", "lastly", ".", "," };

        public static List<string> SplitPrompts(this string prompt)
        {
            // Split the prompt into phrases delineated by connecting words
            return prompt.Split(k_ConnectingWords, StringSplitOptions.RemoveEmptyEntries)
                .Select(phrase => phrase.Trim())
                .ToList();
        }
    }
}
