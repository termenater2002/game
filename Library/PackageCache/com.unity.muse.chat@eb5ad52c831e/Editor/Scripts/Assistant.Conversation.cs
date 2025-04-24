using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        const int k_MaxInternalConversationTitleLength = 30;

        MuseConversation m_ActiveConversation;
        bool m_ConversationRefreshSuspended;

        /// <summary>
        /// Indicates the conversation title has changed
        /// </summary>
        public event Action<string> OnConversationTitleChanged;

        public void ClearForNewConversation()
        {
            m_ActiveConversation = null;
        }

        public MuseConversation GetActiveConversation()
        {
            return m_ActiveConversation;
        }

        public void SuspendConversationRefresh()
        {
            m_ConversationRefreshSuspended = true;
        }

        public void ResumeConversationRefresh()
        {
            m_ConversationRefreshSuspended = false;
        }

        public void RefreshConversations()
        {
            _ = RefreshConversationsAsync();
        }

        public async Task RefreshConversationsAsync(CancellationToken ct = default)
        {
            if (m_ConversationRefreshSuspended)
                return;

            var conversations = await m_Backend.ConversationRefresh(ct);
            OnConversationHistoryReceived(conversations);
        }

        public async Task ConversationLoad(MuseConversationId conversationId, CancellationToken ct = default)
        {
            var conversation = await m_Backend.ConversationLoad(conversationId, ct);
            PushConversation(conversation);
        }

        public void ConversationReload()
        {
            if (m_ActiveConversation == null)
            {
                return;
            }

            _ = ConversationLoad(m_ActiveConversation.Id);
        }

        public void ConversationFavoriteToggle(MuseConversationId conversationId, bool isFavorite)
        {
            m_Backend.ConversationFavoriteToggle(conversationId, isFavorite);
        }

        public async Task ConversationRename(MuseConversationId conversationId, string newName, CancellationToken ct = default)
        {
            if (!conversationId.IsValid)
            {
                return;
            }

            if (m_ActiveConversation != null && m_ActiveConversation.Id == conversationId
                && m_ActiveConversation.Title != newName)
            {
                m_ActiveConversation.Title = newName;

                OnConversationTitleChanged?.Invoke(newName);
            }

            await m_Backend.ConversationRename(conversationId, newName, ct);
            await RefreshConversationsAsync(ct);
        }

        public void ConversationDelete(MuseConversationInfo conversation)
        {
            _ = ConversationDeleteAsync(conversation);
        }

        public async Task ConversationDeleteAsync(MuseConversationInfo conversation, CancellationToken ct = default)
        {
            if (!conversation.Id.IsValid || !k_History.Contains(conversation))
            {
                return;
            }

            k_History.Remove(conversation);
            OnConversationHistoryChanged?.Invoke();

            await m_Backend.ConversationDelete(conversation, ct);
            await RefreshConversationsAsync(ct);

            // If this is the active conversation, reset active:
            if (conversation.Id == m_ActiveConversation?.Id)
            {
                ClearForNewConversation();
                OnDataChanged?.Invoke(new() { Type = MuseChatUpdateType.ConversationClear });
            }
        }

        /// <summary>
        /// Finds and returns the message updater for the given conversation ID.
        /// </summary>
        internal MuseChatStreamHandler GetStreamForConversation(MuseConversationId conversationId)
        {
            for (var i = 0; i < k_MessageUpdaters.Count; i++)
            {
                var updater = k_MessageUpdaters[i];
                if (updater.ConversationId == conversationId.Value)
                {
                    return updater;
                }
            }

            return null;
        }



        bool IsActiveConversationMusing()
        {
            if (HasInternalIdUpdaters())
            {
                // If there is an updater with an internal ID, we are musing, but can't be sure for which conversation,
                return true;
            }

            var stream = GetStreamForConversation(m_ActiveConversation.Id);
            if (stream == null)
            {
                // If there is no updater, we are not musing.
                return false;
            }

            if (stream.CurrentState == MuseChatStreamHandler.State.InProgress)
            {
                // If the message is streaming in set musing to true.
                return true;
            }

            return false;
        }

        void PushConversation(MuseConversation conversation)
        {
            m_ActiveConversation = conversation;

            OnDataChanged?.Invoke(new MuseChatUpdateData
            {
                IsMusing = IsActiveConversationMusing(),
                Type = MuseChatUpdateType.ConversationChange
            });
        }
    }
}
