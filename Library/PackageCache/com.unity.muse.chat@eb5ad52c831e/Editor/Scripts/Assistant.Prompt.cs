using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Commands;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        class PromptProcessState
        {
            public PromptProcessState(string prompt, MuseConversation conversation)
            {
                Prompt = prompt;
                Conversation = conversation;
            }

            public readonly string Prompt;
            public bool NeedsConversationTitle = false;
            public MuseConversation Conversation;
            public MuseMessage PlaceholderUserMessage;
        }

        internal enum PromptState
        {
            None,
            GatheringContext,
            Musing,
            Streaming,
            RepairCode
        }

        internal PromptState CurrentPromptState { get; private set; }

        public async void ProcessEditPrompt(string editedPrompt, MuseMessageId messageId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(editedPrompt))
            {
                return;
            }

            if (m_ActiveConversation == null)
            {
                return;
            }

            // Editing works by deleting the given prompt and all responses after it and then sending a new prompt.

            // Cancel any active operation to ensure no additional messages arrive while or after we're deleting:
            AbortPrompt();

            // Find the index of the message to edit:
            var editedMessageIndex = m_ActiveConversation.Messages.FindIndex(m => m.Id.FragmentId == messageId.FragmentId);

            if (editedMessageIndex < 0)
            {
                Debug.LogError("Cannot find message to edit in the current conversation!");
                return;
            }

            try
            {
                // Delete the edited message and any messages after it:
                for (int i = editedMessageIndex; i < m_ActiveConversation.Messages.Count; i++)
                {
                    var messageToDelete = m_ActiveConversation.Messages[i];
                    var messageIdToDelete = messageToDelete.Id;

                    // Remove message from local conversation:
                    m_ActiveConversation.Messages.RemoveAt(i--);

                    k_Updates.Enqueue(new MuseChatUpdateData
                    {
                        Type = MuseChatUpdateType.MessageDelete,
                        Message = messageToDelete
                    });

                    // Can't delete fragments we don't have the external id for.
                    // If we don't have the external id, we never received the
                    // response header with the id information, it was likely
                    // cancelled before the server receive it.
                    if (messageIdToDelete.Type == MuseMessageIdType.External)
                    {
                        await m_Backend.ConversationDeleteFragment(messageIdToDelete.ConversationId, messageIdToDelete.FragmentId, ct);
                    }
                }

                // Now post the given prompt as a new chat:
                await ProcessPrompt(editedPrompt, ct);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void AbortPrompt()
        {
            if (m_ActiveConversation == null)
            {
                return;
            }

            m_ContextCancelToken?.Cancel();

            // Cancel any active operation to ensure no additional messages arrive while or after we're deleting:
            MuseChatStreamHandler stream = GetStreamForConversation(m_ActiveConversation.Id);

            if(stream != null)
                stream.CancellationTokenSource.Cancel();
        }

        private async Task CreateNewConversationFromPrompt(PromptProcessState state, CancellationToken ct = default)
        {
            // If there is no active conversation, create a new one:
            state.NeedsConversationTitle = true;
            string conversationTitle = state.Prompt;
            if (conversationTitle.Length > k_MaxInternalConversationTitleLength)
            {
                conversationTitle = conversationTitle.Substring(0, k_MaxInternalConversationTitleLength) + "...";
            }

            state.Conversation = new MuseConversation
            {
                Title = conversationTitle,
                Id = MuseConversationId.GetNextInternalId(),
                StartTime = EditorApplication.timeSinceStartup
            };

            OnConversationHistoryChanged?.Invoke();

            // Clear old updates as this conversation has changed
            m_ActiveConversation = state.Conversation;
            k_Updates.Clear();

            state.PlaceholderUserMessage = AddInternalMessage(state.Prompt, role: k_UserRole, sendUpdate: true);
            AddIncompleteMessage(string.Empty, k_AssistantRole, sendUpdate: false);

            ExecuteUpdateImmediate(new MuseChatUpdateData
            {
                Type = MuseChatUpdateType.ConversationChange,
                IsMusing = true
            });

            try
            {
                m_ActiveConversation.Id = await m_Backend.ConversationCreate(ct);
            }
            catch (Exception e)
            {
                CurrentPromptState = PromptState.None;
                HandleExceptionAsMuseChatUpdate(e);
            }
        }

        void OnStreamComplete(PromptProcessState state, MuseChatStreamHandler updater)
        {
            k_MessageUpdaters.Remove(updater);

#if MUSE_INTERNAL
            OnFinalResponseReceived?.Invoke(state.Conversation);
#endif
        }

        private void OnStreamUpdate(PromptProcessState state, MuseChatStreamHandler updater)
        {
            if (IsUpdaterForActiveConversation(state))
                ProcessQueuedUpdates();
        }

        private void OnStreamData(PromptProcessState state, MuseChatStreamHandler updater, string data)
        {
            int messageIndex = state.Conversation.Messages.Count - 1;

            if (messageIndex < 0)
                return;

            MuseMessage currentResponse = state.Conversation.Messages[messageIndex];
            currentResponse.Content = data;
            currentResponse.IsComplete = false;

            // Only if this updater is for the active conversation, enqueue the update:
            if (IsUpdaterForActiveConversation(state))
            {
                state.Conversation.Messages[messageIndex] = currentResponse;
                k_Updates.Enqueue(new MuseChatUpdateData
                {
                    Type = MuseChatUpdateType.MessageUpdate, Message = currentResponse, IsMusing = true
                });

                // If there is a message in the update, get out of the musing state to hide the musing element:
                if (data.Length > 0)
                {
                    CurrentPromptState = PromptState.Streaming;
                }
            }
        }

        void OnStreamConversationId(PromptProcessState state, MuseChatStreamHandler.ConversationIds ids)
        {
            // if the Conversation has a valid id don't do this
            if (!state.Conversation.Id.IsValid)
            {
                var newId = new MuseConversationId(ids.ConversationId);

                state.NeedsConversationTitle = true;
                if (state.Conversation.Id == newId)
                    return;

                // Change ID of the conversation and all its current messages
                state.Conversation.Id = newId;
                for (var i = 0; i < state.Conversation.Messages.Count; i++)
                {
                    var message = state.Conversation.Messages[i];
                    message.Id = new MuseMessageId(state.Conversation.Id, message.Id.FragmentId, message.Id.Type);
                    state.Conversation.Messages[i] = message;
                }
            }

            if (!string.IsNullOrEmpty(ids.UserFragmentId))
            {
                int userMessageIndex = state.Conversation.Messages.Count - 2;

                if (userMessageIndex >= 0)
                {
                    // Change the request ID to the external server id:
                    var currentUserMessage = state.Conversation.Messages[userMessageIndex];
                    var userMessageId =
                        new MuseMessageId(state.Conversation.Id, ids.UserFragmentId, MuseMessageIdType.External);

                    // Enqueue id update only if the id has changed and this updater is for active conversation
                    if (IsUpdaterForActiveConversation(state) && userMessageId != currentUserMessage.Id)
                    {
                        k_Updates.Enqueue(new MuseChatUpdateData
                        {
                            Type = MuseChatUpdateType.MessageIdChange,
                            Message = currentUserMessage,
                            NewMessageId = userMessageId
                        });
                    }

                    currentUserMessage.Id = userMessageId;
                    state.Conversation.Messages[userMessageIndex] = currentUserMessage;
                }
            }

            if (!string.IsNullOrEmpty(ids.AssistantFragmentId))
            {
                int assistantMessageIndex = state.Conversation.Messages.Count - 1;

                if (assistantMessageIndex < 0)
                    return;

                // Change the request ID to the external server id:
                var currentAssistantMessage = state.Conversation.Messages[assistantMessageIndex];
                var assistantMessageId =
                    new MuseMessageId(state.Conversation.Id, ids.AssistantFragmentId, MuseMessageIdType.External);

                // Enqueue id update only if the id has changed and this updater is for active conversation
                if (IsUpdaterForActiveConversation(state) && assistantMessageId != currentAssistantMessage.Id)
                {
                    k_Updates.Enqueue(new MuseChatUpdateData
                    {
                        Type = MuseChatUpdateType.MessageIdChange,
                        Message = currentAssistantMessage,
                        NewMessageId = assistantMessageId
                    });
                }

                currentAssistantMessage.Id = assistantMessageId;
                state.Conversation.Messages[assistantMessageIndex] = currentAssistantMessage;
            }

            if (!string.IsNullOrEmpty(ids.AssistantAuthor))
            {
                int assistantMessageIndex = state.Conversation.Messages.Count - 1;

                if (assistantMessageIndex < 0)
                    return;

                // Change the message author to the assistant:
                var currentAssistantMessage = state.Conversation.Messages[assistantMessageIndex];

                currentAssistantMessage.Author = ids.AssistantAuthor;
                state.Conversation.Messages[assistantMessageIndex] = currentAssistantMessage;
            }

            _ = RefreshConversationsAsync();
        }

        public async Task ProcessPrompt(string prompt, CancellationToken ct = default)
        {
            // Check if the prompt contains a command
            var command = UserSessionState.instance.SelectedCommandMode;
            if (ChatCommandParser.IsCommand(prompt))
                (command, prompt) = ChatCommandParser.Parse(prompt);

            CurrentPromptState = PromptState.GatheringContext;

            // processing a prompt either acts on the current active conversation or requires a new one to be created.
            PromptProcessState promptState = new PromptProcessState(prompt, m_ActiveConversation);

            // TODO: Refactor so this is not necessary for Orchestration to work
            await OrchestrationPrepareProcessPrompt(promptState, ct);

            // Create a thread if needed
            // PATCH NOTES: After a domain reload, if the m_ActiveConversation is null FOR SOME REASON it is no longer
            // null and is instead an empty version of a MuseConversation
            // {Id = null, Title = null, Messages = new List}. This needed patching quickly, but I didn't manage to
            // find the root cause of the issue. This check at least catches the problem
            if (promptState.Conversation == null || promptState.Conversation.Messages.Count == 0)
            {
                await CreateNewConversationFromPrompt(promptState, ct);
            }
            else
            {
                // Reset start time for next progress bar:
                promptState.Conversation.StartTime = EditorApplication.timeSinceStartup;
                promptState.PlaceholderUserMessage = AddInternalMessage(prompt, role: k_UserRole, sendUpdate: true);
                AddIncompleteMessage(string.Empty, k_AssistantRole, sendUpdate: false);
            }

            try
            {
                m_ContextCancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var context = await GetContextModel(promptState.Conversation.Id, MuseChatConstants.PromptContextLimit - prompt.Length, prompt, m_ContextCancelToken.Token);

                if (m_ContextCancelToken.IsCancellationRequested)
                {
                    CurrentPromptState = PromptState.None;
                    return;
                }

#if MUSE_INTERNAL
                if (SkipChatCall)
                {
                    CurrentPromptState = PromptState.None;
                    OnDataChanged?.Invoke(new MuseChatUpdateData
                    {
                        IsMusing = false,
                        Type = MuseChatUpdateType.MessageDelete
                    });
                    return;
                }
#endif

                CurrentPromptState = PromptState.Musing;

                var selectionContext = BuildPromptSelectionContext(k_ObjectAttachments, k_ConsoleAttachments);
                var stream = await m_Backend.SendPrompt(promptState.Conversation.Id, prompt, context, command, selectionContext, ct);

                // To avoid having to re-fetch we copy the context back into the placeholder internal message
                promptState.PlaceholderUserMessage.Context = selectionContext.ToArray();
                ExecuteUpdateImmediate(new MuseChatUpdateData
                {
                    Type = MuseChatUpdateType.MessageUpdate, Message = promptState.PlaceholderUserMessage, IsMusing = true
                });

                stream.OnData += (x, y) => OnStreamData(promptState, x, y);
                stream.OnUpdate += x => OnStreamUpdate(promptState, x);
                stream.OnComplete += x =>
                {
                    OnStreamComplete(promptState, x);
                    stream.PollRequestForIds();
                };
                stream.OnConversationIds += x => OnStreamConversationId(promptState, x);

                k_MessageUpdaters.Add(stream);
                var response = await stream.SendAsync();

                // At this point the stream has completed. Post the final message and stop musing.
                if (IsUpdaterForActiveConversation(promptState))
                {
                    MuseMessage message = m_ActiveConversation.Messages[^1];

                    // If the response is NOT an error, we need to update the message with the response content
                    // Otherwise we ignore response content. We still need to populate the IsComplete and Timestamp
                    // fields and enqueue the message for update even when an error occurs.
                    if (!TryHandleApiResponseAsError(response, ref message))
                    {
                        message.Content = ExtractContent(response.Content);
                    }

                    message.IsComplete = true;
                    message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    message.MessageIndex = m_ActiveConversation.Messages.Count - 1;

                    k_Updates.Enqueue(
                        new MuseChatUpdateData
                        {
                            Type = MuseChatUpdateType.MessageUpdate, Message = message, IsMusing = false
                        });

                    ProcessQueuedUpdates();
                }

                if (promptState.NeedsConversationTitle)
                {
                    await m_Backend.ConversationSetAutoTitle(promptState.Conversation.Id, ct);
                    await RefreshConversationsAsync(ct);
                }
            }
            catch (ConnectionException ce)
            {
                // TODO: Improve the reporting of request cancellation
                // Currently the only way to detect a request cancellation is to check the error message of
                // the ConnectionException. When an exception indicates cancellation, it should not be displayed
                // as an error message in the UI.
                if (ce.Error.StartsWith($"Request aborted"))
                {
                    if (IsUpdaterForActiveConversation(promptState))
                    {
                        MuseMessage currentMessage = m_ActiveConversation.Messages[^1];
                        currentMessage.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        currentMessage.IsComplete = true;

                        ExecuteUpdateImmediate(new MuseChatUpdateData()
                        {
                            Type = MuseChatUpdateType.MessageUpdate,
                            Message = currentMessage,
                            IsMusing = false,
                        });
                    }
                }
                else
                    HandleExceptionAsMuseChatUpdate(ce);
            }
            catch (Exception e)
            {
                if (m_ActiveConversation != null)
                    HandleExceptionAsMuseChatUpdate(e);
            }
        }

        bool IsUpdaterForActiveConversation(PromptProcessState state)
            => state.Conversation.Id.Value == m_ActiveConversation?.Id.Value;

        public List<MuseChatContextEntry> BuildPromptSelectionContext(List<UnityEngine.Object> objectAttachments, List<LogData> mConsoleAttachments)
        {
            var result = new List<MuseChatContextEntry>();

            for (var i = 0; i < objectAttachments.Count; i++)
            {
                var entry = objectAttachments[i].GetContextEntry();
                result.Add(entry);
            }

            for (var i = 0; i < mConsoleAttachments.Count; i++)
            {
                var entry = mConsoleAttachments[i];
                result.Add(new MuseChatContextEntry
                {
                    Value = entry.Message,
                    EntryType = MuseChatContextType.ConsoleMessage,
                    ValueType = entry.Type.ToString()
                });
            }

            return result;
        }

        string ExtractContent(object response)
        {
            var content = response as string;

            if (content == null)
            {
                var asJson = response as JContainer;
                while (asJson != null)
                {
                    var childResponse = asJson.SelectToken("response");
                    if (childResponse == null)
                        return "";

                    if (childResponse.Type == JTokenType.String)
                    {
                        content = childResponse.ToString();
                        break;
                    }
                    else
                        return "";
                }
            }
            return content;
        }
    }
}
