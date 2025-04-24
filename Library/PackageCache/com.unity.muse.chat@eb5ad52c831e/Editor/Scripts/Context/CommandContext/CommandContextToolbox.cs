using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.FunctionCalling;
using UnityEngine;
using static Unity.Muse.Chat.Context.SmartContext.SmartContextToolbox;

namespace Unity.Muse.Chat.Commands
{
    class CommandContextToolbox : FunctionToolbox
    {
        public IEnumerable<CachedFunction> Tools => k_Tools.Values;

        private static string[] GetCommandTags()
        {
            // Get all chat commands
            var allCommands = ChatCommands.GetCommands();

            var tags = allCommands.Select(e => $"command_{e}").ToArray();

            return tags;
        }

        public CommandContextToolbox(FunctionCache functionCache) : base(functionCache, GetCommandTags())
        {

        }

        /// <summary>
        /// Executes a command context tool by name with the given arguments.
        /// </summary>
        /// <param name="name">Name of the tool function.</param>
        /// <param name="args">Arguments to pass to the tool function.</param>
        /// <param name="maxContextLength">Context character limit</param>
        /// <param name="output">Output from the tool function</param>
        public bool TryRunToolByName(string name, string[] args, int maxContextLength, out IContextSelection output)
        {
            SmartContextLimit = maxContextLength;
            try
            {
                if (TryGetSelectorAndConvertArgs(name, args, out var tool, out var convertedArgs))
                {
                    var result = (ExtractedContext)tool.Method.Invoke(null, convertedArgs);
                    output = result != null ? new ContextSelection(tool, result) : null;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            output = default;
            return false;
        }
    }
}
