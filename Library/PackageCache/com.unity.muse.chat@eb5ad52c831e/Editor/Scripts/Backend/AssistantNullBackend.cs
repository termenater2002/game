using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat
{
    class AssistantNullBackend : IAssistantBackend
    {
        public bool SessionStatusTrackingEnabled => false;

        public Task<IEnumerable<MuseConversationInfo>> ConversationRefresh(CancellationToken ct = default)
        {
            return Task.FromResult<IEnumerable<MuseConversationInfo>>(new MuseConversationInfo[]{});
        }

        public Task<MuseConversation> ConversationLoad(MuseConversationId conversationId, CancellationToken ct = default)
        {
            return Task.FromResult<MuseConversation>(null);
        }

        public Task ConversationFavoriteToggle(MuseConversationId conversationId, bool isFavorite, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }


        public Task<MuseConversationId> ConversationCreate(CancellationToken ct = default)
        {
            return Task.FromResult(MuseConversationId.GetNextInternalId());
        }

        public Task ConversationRename(MuseConversationId conversationId, string newName, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task ConversationSetAutoTitle(MuseConversationId id, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task ConversationDelete(MuseConversationInfo conversation, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task ConversationDeleteFragment(MuseConversationId conversationId, string fragment, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<MuseChatInspiration>> InspirationRefresh(CancellationToken ct = default)
        {
            return Task.FromResult<IEnumerable<MuseChatInspiration>>(new MuseChatInspiration[]{});
        }

        public Task InspirationUpdate(MuseChatInspiration inspiration, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task InspirationDelete(MuseChatInspiration inspiration, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task SendFeedback(MuseConversationId conversationId, MessageFeedback feedback, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool> CheckEntitlement(CancellationToken ct = default)
        {
            return Task.FromResult(false);
        }

        public Task<SmartContextResponse> SendSmartContext(MuseConversationId conversationId, string prompt, EditorContextReport context, CancellationToken ct = default)
        {
            return Task.FromResult(new SmartContextResponse(new()));
        }

        public Task<MuseChatStreamHandler> SendPrompt(MuseConversationId conversationId, string prompt, EditorContextReport context,
            string commandType, List<MuseChatContextEntry> selectionContext, CancellationToken ct = default)
        {
            return Task.FromResult<MuseChatStreamHandler>(null);
        }

        public Task<object> RepairCode(MuseConversationId conversationId, int messageIndex, string errorToRepair, string scriptToRepair,
            ScriptType scriptType, CancellationToken ct = default)
        {
            return Task.FromResult<object>(null);
        }

        public Task<object> RepairCompletion(MuseConversationId conversationId, int messageIndex, string errorToRepair, string itemToRepair,
            ProductEnum product, CancellationToken ct = default)
        {
            return null;
        }

        public Task<List<VersionSupportInfo>> GetVersionSupportInfo(string version, CancellationToken ct = default)
        {
            return Task.FromResult<List<VersionSupportInfo>>(null);
        }
    }
}
