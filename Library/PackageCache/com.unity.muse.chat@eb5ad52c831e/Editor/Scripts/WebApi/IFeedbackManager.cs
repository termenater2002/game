using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
#pragma warning disable CS0162
    interface IFeedbackManager
    {
        Task SendFeedback(
            string text,
            string conversationID,
            string conversationFragmentId,
            Sentiment sentiment,
            Category feedbackType,
            CancellationToken ct = default);
    }
}
