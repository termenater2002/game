using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    interface IConversationManager
    {
        /// <summary>
        ///     Starts a task to return conversations.
        /// </summary>
        Task<IEnumerable<ConversationInfo>> GetConversations(
            CancellationToken ct = default);

        /// <summary>
        /// Starts a task to return the data associated with a conversation
        /// </summary>
        Task<ClientConversation> GetConversation(
            [NotNull] string id,
            CancellationToken ct = default);

        /// <summary>
        /// Starts a task to delete the data associated with a conversation
        /// </summary>
        Task DeleteConversation([NotNull] string id, CancellationToken ct = default);

        /// <summary>
        /// Posts a conversation.
        /// </summary>
        Task<Conversation> PostConversation(
            [NotNull] List<FunctionDefinition> functions,
            CancellationToken ct = default);

        /// <summary>
        /// Starts a task to rename a conversation
        /// </summary>
        Task<string> GetConversationTitle([NotNull] string id, CancellationToken ct = default);

        Task SetConversationFavoriteState(
            [NotNull] string id,
            bool favoriteState,
            CancellationToken ct = default);

        Task RenameConversation(
            [NotNull] string id,
            [NotNull] string name,
            CancellationToken ct = default);

        Task DeleteConversationFragment(
            [NotNull] string conversationId,
            [NotNull] string fragmentId,
            CancellationToken ct = default);
    }
}
