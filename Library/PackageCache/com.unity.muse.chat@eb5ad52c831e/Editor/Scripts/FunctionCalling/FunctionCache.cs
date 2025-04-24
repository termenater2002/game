using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.FunctionCalling
{
    class FunctionCache
    {
        readonly List<CachedFunction> m_Functions = new();

        public List<FunctionDefinition> AllFunctionDefinitions =>
            m_Functions
                .Select(function => function.FunctionDefinition)
                .ToList();

        public FunctionCache(IFunctionSource contextSource)
        {
            // Build list of available tool methods:
            m_Functions.Clear();
            m_Functions.AddRange(contextSource?.GetFunctions() ?? Array.Empty<CachedFunction>());
        }

        public IEnumerable<CachedFunction> GetFunctionsByTags(params string[] tags)
            => m_Functions?
                .Where(info => info != null && info.FunctionDefinition != null && info.FunctionDefinition.Tags != null)
                .Where(info => info.FunctionDefinition.Tags.Intersect(tags).Any())
               ?? new List<CachedFunction>();

        public IEnumerable<CachedFunction> GetAllFunctions() => m_Functions;
    }
}
