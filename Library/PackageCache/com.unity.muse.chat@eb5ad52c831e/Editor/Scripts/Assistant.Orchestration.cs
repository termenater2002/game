using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEditor;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        async Task OrchestrationPrepareProcessPrompt(PromptProcessState promptState, CancellationToken ct = default)
        {
            CurrentPromptState = PromptState.GatheringContext;

            // This is temporary, but to get around some timing issues we need access to concrete class
            AssistantWebSocketBackend backend = m_Backend as AssistantWebSocketBackend;

            if (backend == null)
               return;

            // The new protocol requires that the context and the prompt be sent with the create conversation request.
            // This is incompatible with the old code because the smart context call occurs before the conversation is started.

            // ---
            // Get the selected context and prompt so that the ChatRequestV1 can start the agent
            // Initialize all context, if any context has changed, add it all
            var contextBuilder = new ContextBuilder();
            GetAttachedContextString(ref contextBuilder);
            EditorContextReport selectedContext = contextBuilder.BuildContext(int.MaxValue);

            // Post these to the AssistantWebSocketBackend so that CreateConversation works properly.
            backend.HackSelectedContext = selectedContext;
            backend.HackPrompt = promptState.Prompt;

            // The below condition is the condition used to decide whether to create a new conversation or not. The
            // websocket backend must send a ChatRequestV1 before the smart context call is made. This condition is
            // saying "If a new conversation is not going to be created, start a ChatRequestV1 now"
            if (promptState.Conversation != null && promptState.Conversation.Messages.Count > 0)
                await backend.HackyPerformChatRequestV1(promptState.Conversation.Id);
        }
    }
}
