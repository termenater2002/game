using System;
using System.Collections.Generic;
using Unity.Muse.Chat.UI.Components;

namespace Unity.Muse.Chat.UI
{
    static class SharedDisplayTemplates
    {
        static private Dictionary<Type, CommandDisplayTemplate> s_SharedTemplates = new Dictionary<Type, CommandDisplayTemplate>();

        static internal void Reset()
        {
            s_SharedTemplates.Clear();
        }

        static public T GetSharedTemplate<T>() where T : CommandDisplayTemplate, new()
        {
            if (!s_SharedTemplates.TryGetValue(typeof(T), out var template))
            {
                template = new T();
                s_SharedTemplates[typeof(T)] = template;
            }
            return template as T;
        }
    }
}
