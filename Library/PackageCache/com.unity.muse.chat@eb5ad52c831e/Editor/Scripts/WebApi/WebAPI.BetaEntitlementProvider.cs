using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class BetaEntitlementProvider : IBetaEntitlementProvider
        {
            readonly IMuseChatBackendApi k_Api;

            public BetaEntitlementProvider(IMuseChatBackendApi api) => k_Api = api;

            public async Task<bool> CheckBetaEntitlement(CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var response =
                    await k_Api.GetMuseBetaCheckEntitlementV1Builder().BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                return response.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
