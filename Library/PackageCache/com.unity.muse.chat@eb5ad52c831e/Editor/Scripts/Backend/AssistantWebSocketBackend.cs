using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.WebSocketApi.Temporary;
using Unity.Muse.Chat.WebSocketApi.Workflows;
using UnityEngine;

namespace Unity.Muse.Chat
{
    class AssistantWebSocketBackend : IAssistantBackend
    {
        IAssistantBackend m_Fallback;

        Dictionary<MuseConversationId, ChatWorkflow> m_chatWorkflows = new();


        public bool SessionStatusTrackingEnabled => m_Fallback.SessionStatusTrackingEnabled;

        public EditorContextReport HackSelectedContext { get; set; }
        public string HackPrompt { get; set; }

        public async Task HackyPerformChatRequestV1(MuseConversationId conversationId)
        {
            ChatWorkflow workflow = m_chatWorkflows[conversationId];
            workflow.SetupPrompt(HackPrompt, HackSelectedContext);

            await workflow.SendChatRequest();
        }

        public AssistantWebSocketBackend(IAssistantBackend fallback)
        {
            m_Fallback = fallback;
        }

        public Task<IEnumerable<MuseConversationInfo>> ConversationRefresh(CancellationToken ct = default)
            => m_Fallback.ConversationRefresh(ct);

        public Task<MuseConversation> ConversationLoad(MuseConversationId conversationId, CancellationToken ct = default)
            => m_Fallback.ConversationLoad(conversationId, ct);

        public Task ConversationFavoriteToggle(MuseConversationId conversationId, bool isFavorite, CancellationToken ct = default)
            => m_Fallback.ConversationFavoriteToggle(conversationId, isFavorite, ct);

        public Task ConversationRename(MuseConversationId conversationId, string newName, CancellationToken ct = default)
            => m_Fallback.ConversationRename(conversationId, newName, ct);

        public Task ConversationSetAutoTitle(MuseConversationId id, CancellationToken ct = default)
            => m_Fallback.ConversationSetAutoTitle(id, ct);

        public Task ConversationDelete(MuseConversationInfo conversation, CancellationToken ct = default)
            => m_Fallback.ConversationDelete(conversation, ct);

        public Task ConversationDeleteFragment(MuseConversationId conversationId, string fragment, CancellationToken ct = default)
            => m_Fallback.ConversationDeleteFragment(conversationId, fragment, ct);

        public Task<IEnumerable<MuseChatInspiration>> InspirationRefresh(CancellationToken ct = default)
            => m_Fallback.InspirationRefresh(ct);

        public Task InspirationUpdate(MuseChatInspiration inspiration, CancellationToken ct = default)
            => m_Fallback.InspirationUpdate(inspiration, ct);

        public Task InspirationDelete(MuseChatInspiration inspiration, CancellationToken ct = default)
            => m_Fallback.InspirationDelete(inspiration, ct);

        public Task SendFeedback(MuseConversationId conversationId, MessageFeedback feedback, CancellationToken ct = default)
            => m_Fallback.SendFeedback(conversationId, feedback, ct);

        public Task<bool> CheckEntitlement(CancellationToken ct = default)
            => m_Fallback.CheckEntitlement(ct);

        public async Task<MuseConversationId> ConversationCreate(CancellationToken ct = default)
        {
            Debug.Log(nameof(ConversationCreate));
            ChatWorkflow workflow = new();
            workflow.SetupPrompt(HackPrompt, HackSelectedContext);

            await workflow.Start();
            string conversationId = await workflow.GetConversationId();

            MuseConversationId id =  new MuseConversationId(conversationId);
            m_chatWorkflows[id] = workflow;
            return id;
        }

        public Task<SmartContextResponse> SendSmartContext(MuseConversationId conversationId, string prompt,
            EditorContextReport context, CancellationToken ct = default)
        {
            Debug.Log(nameof(SendSmartContext));

            // No smart context to call for now
            return Task.FromResult(new SmartContextResponse(new List<FunctionCall>()));
        }

        public Task<MuseChatStreamHandler> SendPrompt(MuseConversationId conversationId, string prompt, EditorContextReport context,
            string command, List<MuseChatContextEntry> selectionContext, CancellationToken ct = default)
        {
            // This is the biggest issue atm. Needs to be made a bit more generic
            return Task.FromResult(new MuseChatStreamHandler(
                conversationId.Value,
                async (token, callbacks) =>
                {
                    WebSocketUnityWebRequestWrapper requestWrapper = new();
                    callbacks.OnAfterRequestSend(requestWrapper);

                    ChatWorkflow workflow = m_chatWorkflows[conversationId];

                    workflow.OnChatIds.Subscribe(HandleIds);
                    void HandleIds(ChatIds s)
                    {
                        requestWrapper.ResponseHeaders = new();
                        requestWrapper.ResponseHeaders["x-muse-conversation-id"] = workflow.ConversationId;
                        requestWrapper.ResponseHeaders["x-muse-response-conversation-fragment-id"] = s.AgentResponseConversationFragmentId;
                        requestWrapper.ResponseHeaders["x-muse-user-prompt-conversation-fragment-id"] = s.UserPromptConversationFragmentId;
                        requestWrapper.ResponseHeaders["x-muse-message-author"] = s.MessageAuthor;
                        workflow.OnChatIds.Unsubscribe(HandleIds);
                    }

                    workflow.OnChatFragment.Subscribe(HandleFragment);
                    void HandleFragment(string s)
                    {
                        Debug.Log("STREAM: Fragment");
                        requestWrapper.Text += s;
                    }

                    bool isLastFragment = false;
                    workflow.OnChatFinalFragment.Subscribe(s =>
                    {
                        Debug.Log("STREAM: Last Fragment");
                        isLastFragment = true;
                    });

                    // Here we wait for the OnChatFinalFragment event from the workflow, once true, we can indicate that
                    // the response has been completely recieved
                    while (!isLastFragment)
                        await Task.Yield();

                    callbacks.OnAfterResponseReceived(requestWrapper);
                    return new ApiResponse<object>(HttpStatusCode.Accepted, requestWrapper.Text);
                }));
        }
        public Task<object> RepairCompletion(MuseConversationId conversationId, int messageIndex, string errorToRepair, string itemToRepair, ProductEnum product, CancellationToken ct = default)
            => m_Fallback.RepairCompletion(conversationId, messageIndex, errorToRepair, itemToRepair, product, ct);

        public Task<object> RepairCode(MuseConversationId conversationId, int messageIndex, string errorToRepair, string scriptToRepair,
            ScriptType scriptType, CancellationToken ct = default)
            => m_Fallback.RepairCode(conversationId, messageIndex, errorToRepair, scriptToRepair, scriptType, ct);

        public Task<List<VersionSupportInfo>> GetVersionSupportInfo(string version, CancellationToken ct = default)
            => m_Fallback.GetVersionSupportInfo(version, ct);
    }
}
