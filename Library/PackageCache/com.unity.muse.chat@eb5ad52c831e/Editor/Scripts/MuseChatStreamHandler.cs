using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat
{
    /// <summary>
    /// Encapsulates the update handling of a chat request.
    /// </summary>
    sealed class MuseChatStreamHandler
    {
        public delegate Task<ApiResponse<object>> MuseChatStreamRequestDelegate(CancellationToken token,
            RequestInterceptionCallbacks callbacks);

        public enum State
        {
            /// <summary>
            /// This means the stream handler has been created but the stream has not started
            /// </summary>
            Waiting,
            /// <summary>
            /// This means that the stream is currently in progress
            /// </summary>
            InProgress,
            /// <summary>
            /// This means that the stream has finished streaming or an error has occurred
            /// </summary>
            Completed
        }

        public struct ConversationIds
        {
            public string ConversationId;
            public string AssistantFragmentId;
            public string AssistantAuthor;
            public string UserFragmentId;
        }

        readonly MuseChatStreamRequestDelegate k_Request;
        IUnityWebRequest m_UnityWebRequest;

        string _lastData = string.Empty;

        public event Action<MuseChatStreamHandler, string> OnData;
        public event Action<MuseChatStreamHandler> OnUpdate;
        public event Action<MuseChatStreamHandler> OnComplete;

        public event Action<ConversationIds> OnConversationIds;

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public State CurrentState { get; private set; } = State.Waiting;
        public TaskCompletionSource<ApiResponse<object>> TaskCompletionSource { get; private set; } = new();
        internal string ConversationId { get; }


        public MuseChatStreamHandler(string conversationId, MuseChatStreamRequestDelegate request)
        {
            ConversationId = conversationId;
            k_Request = request;
        }

        /// <summary>
        /// Schedule updating for this handler.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="options"></param>
        /// <param name="cancellationTokenSource"></param>
        internal async Task<ApiResponse<object>> SendAsync()
        {
            if (CurrentState != State.Waiting)
                throw new Exception("Cannot start a chat stream handler that is already started or completed.");

            CurrentState = State.InProgress;

            try
            {
                CancellationTokenSource = new();

                var result = await k_Request.Invoke(
                    CancellationTokenSource.Token,
                    new()
                    {
                        OnAfterRequestSend = request =>
                        {
                            m_UnityWebRequest = request;

                            EditorApplication.update -= ChatUpdate;
                            EditorApplication.update += ChatUpdate;

                            EditorApplication.update -= PollRequestForIds;
                            EditorApplication.update += PollRequestForIds;
                        },
                        OnAfterResponseReceived = _ => Stop()
                    });

                TaskCompletionSource.SetResult(result);
                return result;
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        /// <summary>
        /// Stops scheduled updates for this handler.
        /// </summary>
        internal void Stop()
        {
            if(CurrentState == State.Completed)
                return;

            EditorApplication.update -= ChatUpdate;
            EditorApplication.update -= PollRequestForIds;
            CurrentState = State.Completed;
            OnComplete?.Invoke(this);

        }

        bool CheckCancelledAndHandleCleanup()
        {
            bool cancelled = CancellationTokenSource.Token.IsCancellationRequested;

            if(cancelled)
                Stop();

            return CancellationTokenSource.Token.IsCancellationRequested;
        }

        /// <summary>
        /// Updates local data from server data.
        /// </summary>
        void ChatUpdate()
        {
            if (CheckCancelledAndHandleCleanup())
                return;

            string candidateUpdate = m_UnityWebRequest.downloadHandler.text;
            if (!string.IsNullOrEmpty(candidateUpdate))
            {
                if(_lastData != candidateUpdate)
                    OnData?.Invoke(this, candidateUpdate);

                _lastData = candidateUpdate;
            }

            OnUpdate?.Invoke(this);
        }

        // TODO: Make this happen for all ids at the same time
        internal void PollRequestForIds()
        {
            if (CheckCancelledAndHandleCleanup())
                return;

            if (m_UnityWebRequest.GetResponseHeaders() == null)
                return;

            string conversationId = m_UnityWebRequest.GetResponseHeader("x-muse-conversation-id");
            string assistantFragmentId = m_UnityWebRequest.GetResponseHeader("x-muse-response-conversation-fragment-id");

            // TODO: There is a bug on the server side where the user fragment id is not being reported
            string userFragmentId = m_UnityWebRequest.GetResponseHeader("x-muse-user-prompt-conversation-fragment-id");
            string assistantAuthor = m_UnityWebRequest.GetResponseHeader("x-muse-message-author");

            // checking conversationId is enough to know if all ids are returned. They are reported at the same time.
            if(string.IsNullOrEmpty(conversationId))
                return;

            ConversationIds conversationIds = new()
            {
                ConversationId = conversationId,
                AssistantFragmentId = assistantFragmentId,
                AssistantAuthor = assistantAuthor,
                UserFragmentId = userFragmentId
            };

            OnConversationIds?.Invoke(conversationIds);
            EditorApplication.update -= PollRequestForIds;
        }
    }
}
