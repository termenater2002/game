using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.Context.SmartContext;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Plugins;
using Unity.Muse.Chat.UI;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Chat
{
    internal partial class Assistant : ScriptableSingleton<Assistant>
    {
        internal const string k_UserRole = "user";
        internal const string k_AssistantRole = "assistant";
        internal const string k_SystemRole = "system";

        static float s_LastRefreshTokenTime;

        readonly Queue<MuseChatUpdateData> k_Updates = new();

        readonly List<MuseChatStreamHandler> k_MessageUpdaters = new();
        public int MessageUpdatersCount => k_MessageUpdaters.Count;

        SmartContextToolbox m_SmartContextToolbox;
        PluginToolbox m_PluginToolbox;

        IAssistantBackend m_Backend;

        bool m_IsEntitledToBeta;

        public event Action<MuseChatUpdateData> OnDataChanged;

#pragma warning disable CS0067 // Event is never used
        public event Action<string, bool> OnConnectionChanged;
#pragma warning restore CS0067

#if MUSE_INTERNAL
        internal event Action<TimeSpan, SmartContextResponse> OnSmartContextCallDone;
        internal event Action<TimeSpan, FunctionCall> OnSmartContextExtracted;
        internal event Action<MuseConversation> OnFinalResponseReceived;
        internal bool IsProcessingConversations => k_MessageUpdaters.Count > 0;
        internal bool SkipChatCall = false; // Used for benchmarking to skip the actual chat call and only call smart context.
#endif

        /// <summary>
        /// Agent that can executes actions in the project
        /// </summary>
        public RunCommandInterpreter Agent { get; } = new();

        /// <summary>
        /// Validator for generated script files
        /// </summary>
        public CodeBlockValidator CodeBlockValidator { get; } = new();

        public bool SessionStatusTrackingEnabled => m_Backend == null || m_Backend.SessionStatusTrackingEnabled;

        void OnEnable()
        {
            MuseChatState.InitializeState();

            if (m_Backend == null)
            {
                InitializeDriver(new AssistantWebBackend());
            }
        }

        public void InitializeDriver(IAssistantBackend backend)
        {
            m_Backend = backend;
            ServerCompatibility.SetBackend(backend);
        }

        public MuseMessage AddInternalMessage(string text, string role = null, bool musing = true, bool sendUpdate = true, string author = null)
        {
            var message = new MuseMessage
            {
                Author = author,
                Id = MuseMessageId.GetNextInternalId(m_ActiveConversation.Id),
                IsComplete = true,
                Content = text,
                Role = role,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            if (sendUpdate)
            {
                ExecuteUpdateImmediate(new MuseChatUpdateData
                {
                    Type = MuseChatUpdateType.NewMessage,
                    Message = message,
                    IsMusing = musing
                });
            }

            m_ActiveConversation.Messages.Add(message);
            return message;
        }

        MuseMessage AddIncompleteMessage(string text, string role = null, bool musing = true, bool sendUpdate = true)
        {
            var message = new MuseMessage
            {
                Id = MuseMessageId.GetNextIncompleteId(m_ActiveConversation.Id),
                IsComplete = false,
                Content = text,
                Role = role,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            if (sendUpdate)
            {
                ExecuteUpdateImmediate(new MuseChatUpdateData
                {
                    Type = MuseChatUpdateType.NewMessage,
                    Message = message,
                    IsMusing = musing
                });
            }

            m_ActiveConversation.Messages.Add(message);
            return message;
        }

        /// <summary>
        /// Refreshes the access token if we receive any "unauthorized" errors.
        /// </summary>
        /// <param name="errorCode">The error code received from server</param>
        /// <param name="errorText">The error text received from server, will be overwritten for "unauthorized" errors</param>
        private static bool CheckForInvalidAccessToken(IApiResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && response.ErrorText.Contains("unauthorized"))
            {
                // Editor access token can expire after a long time, we need to force a refresh
                if (Time.realtimeSinceStartup - s_LastRefreshTokenTime > 1f)
                {
                    UnityConnectUtils.ClearAccessToken();
                    CloudProjectSettings.RefreshAccessToken(_ => { });

                    s_LastRefreshTokenTime = Time.realtimeSinceStartup;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if there are message updaters with an internal conversation id.
        /// </summary>
        /// <returns>True if there is an updater with an internal ID.</returns>
        bool HasInternalIdUpdaters()
        {
            for (var i = 0; i < k_MessageUpdaters.Count; i++)
            {
                var updater = k_MessageUpdaters[i];
                if (!new MuseConversationId(updater.ConversationId).IsValid)
                {
                    return true;
                }
            }

            return false;
        }

        public void ViewInitialized()
        {
            _ = ViewInitializedAsync();
        }

        public async Task ViewInitializedAsync(CancellationToken ct = default)
        {
            string lastConvId = UserSessionState.instance.LastActiveConversationId;
            if (!string.IsNullOrEmpty(lastConvId))
            {
                var conversation =
                    await m_Backend.ConversationLoad(new MuseConversationId(lastConvId), ct);
                PushConversation(conversation);
            }

            await RefreshConversationsAsync(ct);
            await RefreshInspirations(ct);

            var @checked = await m_Backend.CheckEntitlement(ct);
            OnEntitlementCheckComplete(@checked);
        }

        public void SendFeedback(MessageFeedback feedback)
        {
            if (m_ActiveConversation == null)
            {
                throw new InvalidOperationException("Feedback send on null active conversation!");
            }

            m_Backend.SendFeedback(m_ActiveConversation.Id, feedback);
        }

        void ExecuteUpdateImmediate(MuseChatUpdateData entry)
        {
            k_Updates.Enqueue(entry);
            ProcessQueuedUpdates();
        }

        void ProcessQueuedUpdates()
        {
            while (k_Updates.Count > 0)
            {
                OnDataChanged?.Invoke(k_Updates.Dequeue());
            }
        }

        /// <summary>
        /// Analyzes the <see cref="IApiResponse"/> for errors and returns true if error is found
        /// </summary>
        /// <param name="response">The IApiResponse to analyze</param>
        /// <returns>True if error is detected</returns>
        bool TryHandleApiResponseAsError(IApiResponse response)
        {
            int statusCode = (int)response.StatusCode;

            if(statusCode > 299)
            {
                if(CheckForInvalidAccessToken(response))
                    return true;

                MuseChatView.ShowNotification("Request failed, please try again",
                    PopNotificationIconType.Error);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Analyzes the <see cref="IApiResponse"/> for errors and returns true if error is found
        /// </summary>
        /// <param name="response">The IApiResponse to analyze</param>
        /// <param name="message">Populates the message with error info if an error is detected</param>
        /// <returns>True if error is detected</returns>
        bool TryHandleApiResponseAsError(IApiResponse response, ref MuseMessage message)
        {
            if(TryHandleApiResponseAsError(response))
            {
                message.ErrorCode = (int)response.StatusCode;
                message.ErrorText = response.ErrorText;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Analyzes the <see cref="Exception"/> for errors and returns true if error is found
        /// </summary>
        /// <param name="e">The Exception to analyze</param>
        /// <param name="message">Populates the message with error info if an error is detected</param>
        /// <returns>True if error is detected</returns>
        void HandleExceptionAsMuseChatUpdate(Exception e)
        {
            InternalLog.LogException(e);

            if(m_ActiveConversation == null)
                return;

            MuseMessage message = new()
            {
                IsComplete = true,
                Role = k_AssistantRole,
                Id = MuseMessageId.GetNextIncompleteId(m_ActiveConversation.Id),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            switch (e)
            {
                case InvalidOperationException ioe:
                    message.ErrorText = $"Configuration Error: {ioe.Message}";
                    break;
                case ConnectionException ce:
                    message.ErrorCode = (int)HttpStatusCode.ServiceUnavailable;

                    if (ce.Result == UnityWebRequest.Result.ConnectionError)
                        message.ErrorText = "Connection Error: " + ce.Error;
                    else if (ce.Result == UnityWebRequest.Result.ProtocolError)
                        message.ErrorText = "Protocol Error: " + ce.Error;
                    else if (ce.Result == UnityWebRequest.Result.DataProcessingError)
                        message.ErrorText = "Data Processing Error: " + ce.Error;

                    break;
                case UnexpectedResponseException ure:
                    message.ErrorCode = (int)HttpStatusCode.InternalServerError;
                    message.ErrorText = $"Unexpected Response: {ure.Message}";
                    break;
                default:
                    message.ErrorCode = (int)HttpStatusCode.InternalServerError;
                    message.ErrorText = $"Unexpected Response: {e.Message}";
                    break;
            }

            ExecuteUpdateImmediate(new MuseChatUpdateData()
            {
                Type = MuseChatUpdateType.NewMessage, Message = message, IsMusing = false
            });
        }

        void OnEntitlementCheckComplete(bool isEntitled)
        {
            m_IsEntitledToBeta = isEntitled;

            InternalLog.Log($"Check Beta Entitlement: {m_IsEntitledToBeta}");
        }
    }
}
