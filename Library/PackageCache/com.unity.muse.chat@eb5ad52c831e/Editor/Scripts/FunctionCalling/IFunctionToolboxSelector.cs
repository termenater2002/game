using System.Collections.Generic;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.FunctionCalling
{
    internal interface IFunctionToolboxSelector
    {
        FunctionDefinition FunctionDefinition { get; }
        string Name { get; }
        List<ParameterDefinition> Parameters { get; }
    }
}
