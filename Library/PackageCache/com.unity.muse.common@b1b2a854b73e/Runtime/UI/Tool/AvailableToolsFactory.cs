using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Muse.Common
{
    internal static class AvailableToolsFactory
    {
        static Dictionary<string, List<Type>> s_AvailableTools = new();

        public static void RegisterTool<T>(string mode) where T: ICanvasTool, new()
        {
            s_AvailableTools ??= new();
            if (!s_AvailableTools.TryGetValue(mode, out var tools))
                s_AvailableTools[mode] = tools = new List<Type>();

            var t = typeof(T);
            if (!tools.Contains(t))
                tools.Add(t);
        }

        public static List<string> GetAvailableToolNames(Model model)
        {
            var names = new List<string>();
            
            if (!model || string.IsNullOrEmpty(model.CurrentMode))
                return names;
            
            s_AvailableTools.TryGetValue(model.CurrentMode, out var availableTools);
            
            if (availableTools != null)
            {
                names.AddRange(availableTools.Select(tool => tool.FullName));
            }
            
            return names;
        }

        public static List<ICanvasTool> GetAvailableTools(Model model)
        {
            var tools = new List<ICanvasTool>();

            if (!model || string.IsNullOrEmpty(model.CurrentMode))
                return tools;

            s_AvailableTools.TryGetValue(model.CurrentMode, out var availableTools);

            if (availableTools != null)
            {
                foreach (var instance in availableTools.Select(tool => (ICanvasTool)Activator.CreateInstance(tool)))
                {
                    instance.SetModel(model);
                    tools.Add(instance);
                }
            }

            return tools;
        }
    }
}
