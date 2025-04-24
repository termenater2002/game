using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class ServerCompatibilityProvider : IServerCompatibilityProvider
        {
            readonly IMuseChatBackendApi k_Api;

            public ServerCompatibilityProvider(IMuseChatBackendApi api) => k_Api = api;

            public async Task<ApiResponse<List<VersionSupportInfo>>> GetServerCompatibility(
                string version,
                CancellationToken ct = default) =>
                await k_Api.GetVersionsBuilder().BuildAndSendAsync(ct);
        }
    }
}
