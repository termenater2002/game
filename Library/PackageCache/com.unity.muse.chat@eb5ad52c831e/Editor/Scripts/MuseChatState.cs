using System;
using System.Collections.Generic;
using Unity.Muse.Chat.Context.SmartContext;
using Unity.Muse.Chat.FunctionCalling;
using Unity.Muse.Chat.Plugins;

namespace Unity.Muse.Chat
{
    internal static class MuseChatState
    {
        public static FunctionCache FunctionCache { get; } = new(new AttributeBasedFunctionSource());
        public static SmartContextToolbox SmartContextToolbox { get; private set; }
        public static PluginToolbox PluginToolbox { get; private set; }

        public static void InitializeState()
        {
            PluginToolbox = new PluginToolbox(FunctionCache);
            SmartContextToolbox = new SmartContextToolbox(FunctionCache);
        }
    }
}
