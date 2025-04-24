using Unity.Muse.Chat.UI.Components.ChatElements;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    class MuseConversationPanel : ManagedTemplate
    {
        VisualElement m_ConversationRoot;
        AdaptiveListView<MuseMessage, ChatElementWrapper> m_ConversationList;

        public MuseConversationPanel()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_ConversationRoot = view.Q<VisualElement>("conversationRoot");
            m_ConversationList = new AdaptiveListView<MuseMessage, ChatElementWrapper>
            {
                EnableDelayedElements = false,
                EnableVirtualization = false,
                EnableScrollLock = true
            };
            m_ConversationList.Initialize();
            m_ConversationRoot.Add(m_ConversationList);
        }

        public void Populate(MuseConversation conversation)
        {
            m_ConversationList.BeginUpdate();
            for (var i = 0; i < conversation.Messages.Count; i++)
            {
                m_ConversationList.AddData(conversation.Messages[i]);
            }

            m_ConversationList.EndUpdate();
        }

        public void ClearConversation()
        {
            m_ConversationList.ClearData();
        }

        public void UpdateData(MuseChatUpdateData data)
        {
            switch (data.Type)
            {
                case MuseChatUpdateType.MessageDelete:
                {
                    DeleteChatMessage(data.Message.Id);
                    break;
                }

                case MuseChatUpdateType.MessageIdChange:
                {
                    UpdateChatElementId(data.Message.Id, data.NewMessageId);
                    break;
                }

                case MuseChatUpdateType.NewMessage:
                {
                    UpdateOrChangeChatMessage(data.Message);
                    m_ConversationList.ScrollToEnd();
                    break;
                }

                case MuseChatUpdateType.MessageUpdate:
                {
                    bool messageHasContentUpdate = IsContentDifferent(data.Message);
                    UpdateOrChangeChatMessage(data.Message);
                    if (messageHasContentUpdate)
                    {
                        m_ConversationList.ScrollToEndIfNotLocked();
                    }

                    break;
                }
            }
        }

        bool IsContentDifferent(MuseMessage message)
        {
            if (TryGetChatMessageIndex(message.Id, out int existingMessageIndex))
            {
                if (message.Content != m_ConversationList.Data[existingMessageIndex].Content)
                {
                    return true;
                }
            }

            return false;
        }

        bool TryGetChatMessageIndex(MuseMessageId id, out int index)
        {
            for (var i = 0; i < m_ConversationList.Data.Count; i++)
            {
                if (m_ConversationList.Data[i].Id == id)
                {
                    index = i;
                    return true;
                }
            }

            index = default;
            return false;
        }

        void UpdateOrChangeChatMessage(MuseMessage message)
        {
            if (TryGetChatMessageIndex(message.Id, out int existingMessageIndex))
            {
                m_ConversationList.UpdateData(existingMessageIndex, message);
                return;
            }

            InternalLog.Log($"MSG_ADD: {message.Id} - {message.Content?.Length}");

            m_ConversationList.AddData(message);
        }

        void DeleteChatMessage(MuseMessageId messageId)
        {
            if (TryGetChatMessageIndex(messageId, out var messageIndex))
            {
                InternalLog.Log($"MSG_DEL: {messageIndex} - {messageId}");

                m_ConversationList.RemoveData(messageIndex);
            }
        }

        void UpdateChatElementId(MuseMessageId currentId, MuseMessageId newId)
        {
            if (currentId == newId)
            {
                return;
            }

            if(!TryGetChatMessageIndex(currentId, out var messageIndex))
            {
                // This is currently a side-effect of async reloading, not critical but worth changing at some point
                // We reload during a queued update in the driver / message handler, so we can't guarantee the message is still there
                InternalLog.LogWarning("Change Message ID called for non-existent message: " + currentId);

                return;
            }

            var messageData = m_ConversationList.Data[messageIndex];

            InternalLog.Log($"MSG_ID_CHANGE: {messageIndex} - {messageData.Id} -> {newId}");

            messageData.Id = newId;
            m_ConversationList.UpdateData(messageIndex, messageData);
        }
    }
}
