using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class FeedbackManager : IFeedbackManager
        {
            readonly IMuseChatBackendApi k_Api;
            readonly IOrganizationIdProvider k_OrganizationIdProvider;

            public FeedbackManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public async Task SendFeedback(
                string text,
                string conversationID,
                string conversationFragmentId,
                Sentiment sentiment,
                Category feedbackType,
                CancellationToken ct = default)
            {
                if (!k_OrganizationIdProvider.GetOrganizationId(out string organizationId))
                {
                    return;
                }

#pragma warning disable CS0162 // Unreachable code detected
                if (MuseChatConstants.DebugMode)
                {
                    return;
                }
#pragma warning restore CS0162 // Unreachable code detected

                var request = new Feedback(
                    feedbackType, conversationFragmentId, conversationID, text, organizationId,
                    sentiment);

                var error = await k_Api.PostMuseFeedbackV1Builder(request).BuildAndSendAsync(ct);

                if (error != null)
                    throw new WebApiException("", error);
            }
        }
    }
}
