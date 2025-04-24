using System.Reflection;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.FunctionCalling
{
    class CachedFunction
    {
        public MethodInfo Method;
        public FunctionDefinition FunctionDefinition;
    }
}
