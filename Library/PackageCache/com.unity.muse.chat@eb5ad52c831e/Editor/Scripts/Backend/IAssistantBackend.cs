using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat
{
    interface IAssistantBackend
    {
        bool SessionStatusTrackingEnabled { get; }
        Task<IEnumerable<MuseConversationInfo>> ConversationRefresh(CancellationToken ct = default);
        Task<MuseConversation> ConversationLoad(MuseConversationId conversationId, CancellationToken ct = default);
        Task ConversationFavoriteToggle(MuseConversationId conversationId, bool isFavorite, CancellationToken ct = default);
        Task<MuseConversationId> ConversationCreate(CancellationToken ct = default);
        Task ConversationRename(MuseConversationId conversationId, string newName, CancellationToken ct = default);
        Task ConversationSetAutoTitle(MuseConversationId id, CancellationToken ct = default);
        Task ConversationDelete(MuseConversationInfo conversation, CancellationToken ct = default);
        Task ConversationDeleteFragment(MuseConversationId conversationId, string fragment, CancellationToken ct = default);

        Task<IEnumerable<MuseChatInspiration>> InspirationRefresh(CancellationToken ct = default);
        Task InspirationUpdate(MuseChatInspiration inspiration, CancellationToken ct = default);
        Task InspirationDelete(MuseChatInspiration inspiration, CancellationToken ct = default);

        Task SendFeedback(MuseConversationId conversationId, MessageFeedback feedback, CancellationToken ct = default);

        Task<bool> CheckEntitlement(CancellationToken ct = default);

        Task<SmartContextResponse> SendSmartContext(MuseConversationId conversationId, string prompt, EditorContextReport context, CancellationToken ct = default);

        Task<MuseChatStreamHandler> SendPrompt(MuseConversationId conversationId, string prompt, EditorContextReport context, string command, List<MuseChatContextEntry> selectionContext, CancellationToken ct = default);

        Task<object> RepairCode(MuseConversationId conversationId, int messageIndex, string errorToRepair, string scriptToRepair, ScriptType scriptType, CancellationToken ct = default);

        /// <summary>
        /// Returns version support info that can used to check if the version of the server the client wants to
        /// communicate with is supported. Returns null if the version support info could not be retrieved.
        /// </summary>
        /// <param name="version">Server version the client wants to hit expressed as the url name. Example: v1</param>
        Task<List<VersionSupportInfo>> GetVersionSupportInfo(string version, CancellationToken ct = default);

        Task<object> RepairCompletion(MuseConversationId conversationId, int messageIndex, string errorToRepair, string itemToRepair, ProductEnum product, CancellationToken ct = default);
    }
}
