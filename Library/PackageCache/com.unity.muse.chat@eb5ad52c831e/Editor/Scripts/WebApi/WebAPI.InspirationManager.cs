using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class InspirationManager : IInspirationManager
        {
            readonly IMuseChatBackendApi k_Api;
            readonly IOrganizationIdProvider k_OrganizationIdProvider;

            public InspirationManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public async Task<IEnumerable<Inspiration>> GetInspirations(
                CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var response = await k_Api
                    .GetMuseInspirationV1Builder()
                    .SetLimit(MuseChatConstants.MaxInspirationCount)
                    .BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                return response.Data?.GetList();
            }

            public async Task<Inspiration> AddInspiration(
                Inspiration inspiration,
                CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var response = await k_Api
                    .PostMuseInspirationV1Builder(inspiration)
                    .BuildAndSendAsync(ct);

                if (response == null)
                    throw new WebApiException("Inspiration Add Exception", null);

                return response.Data?.GetInspiration();
            }

            public Task<Inspiration> UpdateInspiration(
                Inspiration inspiration,
                CancellationToken ct = default)
            {
                if (!k_OrganizationIdProvider.GetOrganizationId(out _))
                    throw new("Organization id not found");

                throw new NotImplementedException(
                    "Requires API Updates to avoid Migrating different enums (ModeEnum) with different implementations");
                /*var tsc = api.PutMuseInspirationUsingInspirationIdV1Async(
                    inspiration.Id,
                    new UpdateInspirationRequest(inspiration.Mode, inspiration.Value, inspiration.Description),
                    m_CurrentInspirationCancellationTokenSource.Token
                );

                loop.Register(RequestTick);

                void RequestTick()
                {
                    // If request is in progress, conversations are empty
                    if (tsc is {IsCompleted: false})
                        return;

                    loop.Unregister(RequestTick);

                    if (tsc.IsCompletedSuccessfully)
                    {
                        ApiResponse<ResponsePutMuseInspirationUsingInspirationIdV1> res = tsc.Result;
                        if (res == null)
                        {
                            onError?.Invoke(new WebAPIException("Inspiration Update Exception", res));
                            return;
                        }

                        onComplete?.Invoke(res.Data.GetInspiration());
                    }
                    else
                        onError?.Invoke(GetExceptionFromTask(tsc));
                }*/
            }

            public async Task DeleteInspiration(
                Inspiration inspiration,
                CancellationToken ct = default)
            {
                var res = await k_Api
                    .DeleteMuseInspirationUsingInspirationIdV1Builder(inspiration.Id)
                    .BuildAndSendAsync(ct);

                if (res == null)
                    throw new WebApiException("Inspiration Update Exception", null);
            }
        }
    }
}
