using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class SmartContextManager : ISmartContextManager
        {
            readonly IMuseChatBackendApi k_Api;
            readonly IOrganizationIdProvider k_OrganizationIdProvider;

            public SmartContextManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public async Task<SmartContextResponse> PostSmartContextAsync(
                string prompt,
                EditorContextReport editorContext,
                List<FunctionDefinition> catalog,
                string conversationId,
                CancellationToken ct = default)
            {
                if (!k_OrganizationIdProvider.GetOrganizationId(out var organizationId))
                    return null;

                var request = new SmartContextRequest(organizationId, prompt)
                {
                    ConversationId = conversationId,
                    JsonCatalog = catalog,
                    EditorContext = new(editorContext)
                };

                return await k_Api.PostSmartContextV1Builder(request).BuildAndSendAsync(ct);
            }
        }
    }
}
