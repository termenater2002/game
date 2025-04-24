using System;
using System.Collections.Generic;
using Unity.Muse.Chat.FunctionCalling;
using Unity.Muse.Chat.BackendApi;
using UnityEngine;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal partial class SmartContextToolbox : FunctionToolbox
    {
        public IEnumerable<CachedFunction> Tools => k_Tools.Values;

        internal static int SmartContextLimit { get; set; }

        /// <summary>
        ///     Create a toolbox.
        ///     The Toolbox will use mthods returned by the contextProviderSource to build a list of available tools.
        /// </summary>
        /// <param name="functionCache">Provides context methods</param>
        public SmartContextToolbox(FunctionCache functionCache)
            : base(functionCache, FunctionCallingUtilities.k_SmartContextTag)
        {
        }

        /// <summary>
        /// Executes a context retrieval tool by name with the given arguments.
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
