using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Utils;
using Unity.Muse.Chat.WebSocket.Model;
using Unity.Muse.Chat.WebSocketApi.Models;
using UnityEngine;

namespace Unity.Muse.Chat.WebSocketApi.Workflows
{
    class ChatIds
    {
        public string UserPromptConversationFragmentId { get; set; }
        public string AgentResponseConversationFragmentId { get; set; }
        public string MessageAuthor { get; set; }
    }

    // An event that caches values sent to it and plays them back to new subscribers
    class ChatWorkflow : IDisposable
    {
        public string Prompt { get; private set; }
        public EditorContextReport Context { get; private set; }

        const string k_Uri = "ws://localhost:8000/ws/assistant";

        public string ConversationId { get; private set; }

        Task m_ReceiveMessagesTask;
        Task m_ReceiveQueueProcessor;
        Queue<IModel> m_ReceivedMessageQueue = new();
        ServerMessageJsonConverter m_ServerMessageJsonConverter = new();

        ClientWebSocket m_WebSocket;

        public PlaybackEvent<ChatIds> OnChatIds { get; } = new();
        public PlaybackEvent<string> OnChatFragment { get; } = new();
        public PlaybackEvent<string> OnChatFinalFragment { get; } = new();

        // This is temporary. It exists to workaround some timing differences between REST and the WebSocket protocol
        public void SetupPrompt(string prompt, EditorContextReport context)
        {
            Prompt = prompt;
            Context = context;

            OnChatIds.ClearHistory();
            OnChatFragment.ClearHistory();
            OnChatFinalFragment.ClearHistory();
        }

        public async Task Start()
        {
            Uri uri = new(k_Uri);

            m_WebSocket = new();

            // Connect to the websocket
            await m_WebSocket.ConnectAsync(uri, default);
            Debug.Log($"Connected {nameof(ChatWorkflow)}");

            m_ReceiveMessagesTask = ReceiveMessages(m_WebSocket);
            m_ReceiveQueueProcessor = ProcessReceiveQueue();
        }

        // TODO: This is never stopped.
        public async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            while (true)
            {
                var bytes = new byte[1024];
                var result = await webSocket.ReceiveAsync(bytes, default);
                string res = Encoding.UTF8.GetString(bytes, 0, result.Count);

                try
                {
                    var model = JsonConvert.DeserializeObject<IModel>(res, m_ServerMessageJsonConverter);
                    m_ReceivedMessageQueue.Enqueue(model);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to deserialize: {res} ({e.Message})");
                }
            }
        }

        public async Task ProcessReceiveQueue()
        {
            while (true)
            {
                if (m_ReceivedMessageQueue.Count == 0)
                {
                    // TODO: Cancellation checks
                    await Task.Yield();
                    continue;
                }

                var message = m_ReceivedMessageQueue.Dequeue();

                Action action = message switch
                {
                    DiscussionInitializationV1 di => () => _ = HandleDiscussionInitializationV1(di),
                    CapabilitiesRequestV1 cr => () => _ = HandleCapabilitiesRequestV1(cr),
                    FunctionCallRequestV1 fcr => () => _ = HandleFunctionCallRequestV1(fcr),
                    ChatResponseFragmentV1 crf => () => HandleChatResponseFragmentV1(crf),
                    ChatInitializationV1 ci => () => _ = HandleChatInitializationV1(ci),
                    _ => () => Debug.LogError($"Unknown message type: {message.GetType()}")
                };

                action();
            }
        }

        public Task HandleFunctionCallRequestV1(FunctionCallRequestV1 message)
        {
            Debug.Log($"DID {nameof(HandleFunctionCallRequestV1)}");

            // Just grab the first one. The server has been hardcoded to do one function only.
            FunctionCall call = new FunctionCall(message.FunctionName, message.FunctionParameters.ToList());

            if (!MuseChatState.SmartContextToolbox.TryRunToolByName(message.FunctionName, message.FunctionParameters.ToArray(),
                    MuseChatConstants.PromptContextLimit, out IContextSelection result))
            {
                Debug.Log("Failed function call");
            }

            // TODO: This needs to be replaced by a call to function_call_response
            Debug.Log($"FUNCTION CALLED::::{result.Payload}");
            return Task.CompletedTask;
        }

        public async Task HandleCapabilitiesRequestV1(CapabilitiesRequestV1 message)
        {
            List<FunctionDefinition> functions = MuseChatState
                .SmartContextToolbox
                .Tools
                .Select(c => c.FunctionDefinition).ToList();

            IEnumerable<CapabilitiesResponseV1.FunctionsObject> FunctionObjects = functions.Select(f =>
            {
                return new CapabilitiesResponseV1.FunctionsObject()
                {
                    FunctionTag = f.Tags,
                    FunctionName = f.Name,
                    FunctionNamespace = "UNKNOWN",
                    FunctionDescription = f.Description,
                    FunctionParameters = f.Parameters.Select(p => new CapabilitiesResponseV1.FunctionsObject.FunctionParametersObject()
                    {
                        Name = p.Name,
                        Type = p.Type,
                        Description = p.Description
                    }).ToList()
                };
            });

            string json = JsonConvert.SerializeObject(new CapabilitiesResponseV1()
            {
                Functions = FunctionObjects.ToList(),
                Outputs = new List<CapabilitiesResponseV1.OutputsObject>()
                {
                    new(){ OutputName = "I DON'T KNOW WHAT THIS IS"}
                }
            });

            await m_WebSocket.SendAsync(
                UTF8Encoding.UTF8.GetBytes(json).AsMemory(),
                WebSocketMessageType.Text,
                true,
                default
            );
        }

        public Task HandleChatInitializationV1(ChatInitializationV1 message)
        {
            OnChatIds?.Invoke(new ChatIds()
            {
                UserPromptConversationFragmentId = message.UserPromptConversationFragmentId,
                AgentResponseConversationFragmentId = message.AgentResponseConversationFragmentId,
                MessageAuthor = message.MessageAuthor
            });
            return Task.CompletedTask;
        }

        public async Task HandleDiscussionInitializationV1(DiscussionInitializationV1 message)
        {
            Debug.Log($"DID {nameof(HandleDiscussionInitializationV1)}");
            ConversationId = message.ConversationId;

            Debug.Log($"Conversation ID: {ConversationId}");
            await SendChatRequest();
        }

        public async Task SendChatRequest()
        {
            // This is super the issue
            string json = JsonConvert.SerializeObject(new ChatRequestV1
            {
                Prompt = Prompt,

                // TODO: Need to translate EditorContextReport to this
                SelectedContext = new List<ChatRequestV1.SelectedContextModel>()
            });

            await m_WebSocket.SendAsync(
                UTF8Encoding.UTF8.GetBytes(json).AsMemory(),
                WebSocketMessageType.Text,
                true,
                default
            );
        }

        public Task HandleChatResponseFragmentV1(ChatResponseFragmentV1 message)
        {
            Debug.Log($"DID {nameof(HandleChatResponseFragmentV1)}");

            OnChatFragment?.Invoke(message.Markdown);

            if (message.LastMessage)
                OnChatFinalFragment?.Invoke(message.Markdown);

            return Task.CompletedTask;
        }

        // TEMP to emulate REST workflows
        public async Task<string> GetConversationId()
        {
            while (ConversationId == null)
                await Task.Yield();

            return ConversationId;
        }

        public async Task<string> GetResponse()
        {
            // Just to give fake data
            return await GetConversationId();
        }

        public void Dispose()
        {
            m_ReceiveMessagesTask?.Dispose();
            m_ReceiveQueueProcessor?.Dispose();
            m_WebSocket?.Dispose();
        }
    }
}
