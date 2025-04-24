using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class ConversationManager : IConversationManager
        {
            readonly IMuseChatBackendApi k_Api;
            readonly IOrganizationIdProvider k_OrganizationIdProvider;

            public ConversationManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public async Task<IEnumerable<ConversationInfo>> GetConversations(
                CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                var convosBuilder = k_Api
                    .GetMuseConversationV1Builder()
                    .SetLimit(MuseChatConstants.MaxConversationHistory);

                var response = await convosBuilder.BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                return response.Data;
            }

            public async Task<ClientConversation> GetConversation(
                string id,
                CancellationToken ct = default)
            {
                var response = await k_Api
                    .GetMuseConversationUsingConversationIdV1Builder(id)
                    .BuildAndSendAsync(ct);

                var data = response.Data;

                if (data.ActualInstance is ClientConversation conversation)
                    return conversation;

                throw new WebApiException("Get Conversation Error", data.ActualInstance);
            }

            public async Task DeleteConversation(string id, CancellationToken ct = default)
            {
                var response = await k_Api
                    .DeleteMuseConversationUsingConversationIdV1Builder(id)
                    .BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                var res = response.Data;

                if (res != null)
                    throw new WebApiException("Delete Conversation Error", res);
            }

            public async Task<Conversation> PostConversation(
                List<FunctionDefinition> functions,
                CancellationToken ct = default)
            {
                ct.ThrowIfCancellationRequested();

                if (!GetOrganizationID(out var organizationId))
                    throw new("Unable to get the organization ID");

                return await k_Api
                    .PostMuseConversationV1Builder(
                        new(organizationId) {FunctionCatalog = functions})
                    .BuildAndSendAsync(ct);
            }

            public async Task<string> GetConversationTitle(
                string id,
                CancellationToken ct = default)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentNullException(nameof(id));

                if (!GetOrganizationID(out var organizationId))
                    throw new("Unable to get the organization ID");

                var response = await k_Api
                    .GetMuseTopicUsingConversationIdV1Builder(id, organizationId)
                    .BuildAndSendAsync(ct);

                return response.Data;
            }

            public async Task SetConversationFavoriteState(
                string id,
                bool favoriteState,
                CancellationToken ct = default)
            {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id));

                var payload = new ConversationPatchRequest {IsFavorite = favoriteState};

                var response = await k_Api
                    .PatchMuseConversationUsingConversationIdV1Builder(id, payload)
                    .BuildAndSendAsync(ct);

                var error = response.Data;
                if (error != null)
                    throw new WebApiException(
                        "Conversation Favorite State change Exception", error);
            }

            public async Task RenameConversation(
                string id,
                string name,
                CancellationToken ct = default)
            {
                if (string.IsNullOrEmpty(id))
                    return;

                var payload = new ConversationPatchRequest {Title = name};

                var response = await k_Api
                    .PatchMuseConversationUsingConversationIdV1Builder(id, payload)
                    .BuildAndSendAsync(ct);

                var error = response.Data;
                if (error != null)
                    throw new WebApiException("Conversation Rename Exception", error);
            }

            public async Task DeleteConversationFragment(
                [NotNull] string conversationId,
                string fragmentId,
                CancellationToken ct = default)
            {
                if (string.IsNullOrEmpty(conversationId))
                    throw new ArgumentNullException(nameof(conversationId));

                if (fragmentId == null)
                    throw new ArgumentNullException(nameof(fragmentId));

                var response = await k_Api
                    .DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Builder(
                        conversationId, fragmentId)
                    .BuildAndSendAsync(ct);

                var error = response.Data;
                if (error != null)
                    throw new WebApiException("Conversation Fragment Delete Exception", error);
            }

            bool GetOrganizationID(out string organizationId) =>
                k_OrganizationIdProvider.GetOrganizationId(out organizationId);
        }
    }
}
