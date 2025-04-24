using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    interface ISmartContextManager
    {
        Task<SmartContextResponse> PostSmartContextAsync(
            string prompt,
            EditorContextReport editorContext,
            List<FunctionDefinition> catalog,
            string conversationId,
            CancellationToken ct = default);
    }
}
