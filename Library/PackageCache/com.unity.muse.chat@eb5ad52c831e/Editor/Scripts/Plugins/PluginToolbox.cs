using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Muse.Chat.FunctionCalling;
using UnityEngine;

namespace Unity.Muse.Chat.Plugins
{
    internal partial class PluginToolbox : FunctionToolbox
    {
        public PluginToolbox(FunctionCache functionCache) : base(functionCache, "plugin")
        {
        }

        /// <summary>
        /// Executes a plugin tool by name with the given arguments.
        /// </summary>
        /// <param name="name">Name of the tool function.</param>
        /// <param name="args">Arguments to pass to the tool function.</param>
        public bool TryRunToolByName(string name, string[] args)
        {
            try
            {
                if (TryGetSelectorAndConvertArgs(name, args, out var plugin, out var convertedArgs))
                {
                    plugin.Method.Invoke(null, convertedArgs);
                    return true;
                }
            }
            catch (Exception e)
            {
                InternalLog.LogException(e);
            }

            return false;
        }

    }
}
