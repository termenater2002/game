using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Muse.Chat.BackendApi.Client;
using UnityEngine.Networking;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.BackendApi.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    internal interface IMuseChatBackendApi
    {


        /// <summary>
        /// Build request to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdBuilder(string conversationId, string fragmentId);


        /// <summary>
        /// Build request to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Builder(string conversationId, string fragmentId);


        /// <summary>
        /// Build request to call /muse/conversation/{conversation_id}
        /// </summary>
        public DeleteMuseConversationUsingConversationIdRequestBuilder DeleteMuseConversationUsingConversationIdBuilder(string conversationId);


        /// <summary>
        /// Build request to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public DeleteMuseConversationUsingConversationIdV1RequestBuilder DeleteMuseConversationUsingConversationIdV1Builder(string conversationId);


        /// <summary>
        /// Build request to call /muse/conversations/by-tags
        /// </summary>
        public DeleteMuseConversationsByTagsRequestBuilder DeleteMuseConversationsByTagsBuilder();


        /// <summary>
        /// Build request to call /v1/muse/conversations/by-tags
        /// </summary>
        public DeleteMuseConversationsByTagsV1RequestBuilder DeleteMuseConversationsByTagsV1Builder();


        /// <summary>
        /// Build request to call /muse/inspiration/{inspiration_id}
        /// </summary>
        public DeleteMuseInspirationUsingInspirationIdRequestBuilder DeleteMuseInspirationUsingInspirationIdBuilder(string inspirationId);


        /// <summary>
        /// Build request to call /v1/muse/inspiration/{inspiration_id}
        /// </summary>
        public DeleteMuseInspirationUsingInspirationIdV1RequestBuilder DeleteMuseInspirationUsingInspirationIdV1Builder(string inspirationId);


        /// <summary>
        /// Build request to call /health
        /// </summary>
        public GetHealthRequestBuilder GetHealthBuilder();


        /// <summary>
        /// Build request to call /healthz
        /// </summary>
        public GetHealthzRequestBuilder GetHealthzBuilder();


        /// <summary>
        /// Build request to call /muse/beta/check_entitlement
        /// </summary>
        public GetMuseBetaCheckEntitlementRequestBuilder GetMuseBetaCheckEntitlementBuilder();


        /// <summary>
        /// Build request to call /v1/muse/beta/check_entitlement
        /// </summary>
        public GetMuseBetaCheckEntitlementV1RequestBuilder GetMuseBetaCheckEntitlementV1Builder();


        /// <summary>
        /// Build request to call /muse/conversation
        /// </summary>
        public GetMuseConversationRequestBuilder GetMuseConversationBuilder();


        /// <summary>
        /// Build request to call /muse/conversation/{conversation_id}
        /// </summary>
        public GetMuseConversationUsingConversationIdRequestBuilder GetMuseConversationUsingConversationIdBuilder(string conversationId);


        /// <summary>
        /// Build request to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public GetMuseConversationUsingConversationIdV1RequestBuilder GetMuseConversationUsingConversationIdV1Builder(string conversationId);


        /// <summary>
        /// Build request to call /v1/muse/conversation
        /// </summary>
        public GetMuseConversationV1RequestBuilder GetMuseConversationV1Builder();


        /// <summary>
        /// Build request to call /muse/inspiration/
        /// </summary>
        public GetMuseInspirationRequestBuilder GetMuseInspirationBuilder();


        /// <summary>
        /// Build request to call /v1/muse/inspiration/
        /// </summary>
        public GetMuseInspirationV1RequestBuilder GetMuseInspirationV1Builder();


        /// <summary>
        /// Build request to call /muse/opt
        /// </summary>
        public GetMuseOptRequestBuilder GetMuseOptBuilder();


        /// <summary>
        /// Build request to call /v1/muse/opt
        /// </summary>
        public GetMuseOptV1RequestBuilder GetMuseOptV1Builder();


        /// <summary>
        /// Build request to call /muse/topic/{conversation_id}
        /// </summary>
        public GetMuseTopicUsingConversationIdRequestBuilder GetMuseTopicUsingConversationIdBuilder(string conversationId, string organizationId);


        /// <summary>
        /// Build request to call /v1/muse/topic/{conversation_id}
        /// </summary>
        public GetMuseTopicUsingConversationIdV1RequestBuilder GetMuseTopicUsingConversationIdV1Builder(string conversationId, string organizationId);


        /// <summary>
        /// Build request to call /versions/
        /// </summary>
        public GetVersionsRequestBuilder GetVersionsBuilder();


        /// <summary>
        /// Build request to call /health
        /// </summary>
        public HeadHealthRequestBuilder HeadHealthBuilder();


        /// <summary>
        /// Build request to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder PatchMuseConversationFragmentUsingConversationIdAndFragmentIdBuilder(string conversationId, string fragmentId, ConversationFragmentPatch requestBody);


        /// <summary>
        /// Build request to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Builder(string conversationId, string fragmentId, ConversationFragmentPatch requestBody);


        /// <summary>
        /// Build request to call /muse/conversation/{conversation_id}
        /// </summary>
        public PatchMuseConversationUsingConversationIdRequestBuilder PatchMuseConversationUsingConversationIdBuilder(string conversationId, ConversationPatchRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public PatchMuseConversationUsingConversationIdV1RequestBuilder PatchMuseConversationUsingConversationIdV1Builder(string conversationId, ConversationPatchRequest requestBody);


        /// <summary>
        /// Build request to call /muse/agent/action
        /// </summary>
        public PostMuseAgentActionRequestBuilder PostMuseAgentActionBuilder(ActionRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/agent/action
        /// </summary>
        public PostMuseAgentActionV1RequestBuilder PostMuseAgentActionV1Builder(ActionRequest requestBody);


        /// <summary>
        /// Build request to call /muse/agent/code_repair
        /// </summary>
        public PostMuseAgentCodeRepairRequestBuilder PostMuseAgentCodeRepairBuilder(ActionCodeRepairRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/agent/code_repair
        /// </summary>
        public PostMuseAgentCodeRepairV1RequestBuilder PostMuseAgentCodeRepairV1Builder(ActionCodeRepairRequest requestBody);


        /// <summary>
        /// Build request to call /muse/agent/codegen
        /// </summary>
        public PostMuseAgentCodegenRequestBuilder PostMuseAgentCodegenBuilder(CodeGenRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/agent/codegen
        /// </summary>
        public PostMuseAgentCodegenV1RequestBuilder PostMuseAgentCodegenV1Builder(CodeGenRequest requestBody);


        /// <summary>
        /// Build request to call /muse/chat
        /// </summary>
        public PostMuseChatRequestBuilder PostMuseChatBuilder(ChatRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/chat
        /// </summary>
        public PostMuseChatV1RequestBuilder PostMuseChatV1Builder(ChatRequest requestBody);


        /// <summary>
        /// Build request to call /muse/completion
        /// </summary>
        public PostMuseCompletionRequestBuilder PostMuseCompletionBuilder(ContextualCompletionRequest requestBody);


        /// <summary>
        /// Build request to call /muse/completion/repair
        /// </summary>
        public PostMuseCompletionRepairRequestBuilder PostMuseCompletionRepairBuilder(CompletionRepairRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/completion/repair
        /// </summary>
        public PostMuseCompletionRepairV1RequestBuilder PostMuseCompletionRepairV1Builder(CompletionRepairRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/completion
        /// </summary>
        public PostMuseCompletionV1RequestBuilder PostMuseCompletionV1Builder(ContextualCompletionRequest requestBody);


        /// <summary>
        /// Build request to call /muse/conversation
        /// </summary>
        public PostMuseConversationRequestBuilder PostMuseConversationBuilder(CreateConversationRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/conversation
        /// </summary>
        public PostMuseConversationV1RequestBuilder PostMuseConversationV1Builder(CreateConversationRequest requestBody);


        /// <summary>
        /// Build request to call /muse/feedback
        /// </summary>
        public PostMuseFeedbackRequestBuilder PostMuseFeedbackBuilder(Feedback requestBody);


        /// <summary>
        /// Build request to call /v1/muse/feedback
        /// </summary>
        public PostMuseFeedbackV1RequestBuilder PostMuseFeedbackV1Builder(Feedback requestBody);


        /// <summary>
        /// Build request to call /muse/inspiration/
        /// </summary>
        public PostMuseInspirationRequestBuilder PostMuseInspirationBuilder(Inspiration requestBody);


        /// <summary>
        /// Build request to call /v1/muse/inspiration/
        /// </summary>
        public PostMuseInspirationV1RequestBuilder PostMuseInspirationV1Builder(Inspiration requestBody);


        /// <summary>
        /// Build request to call /muse/opt
        /// </summary>
        public PostMuseOptRequestBuilder PostMuseOptBuilder(OptRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/opt
        /// </summary>
        public PostMuseOptV1RequestBuilder PostMuseOptV1Builder(OptRequest requestBody);


        /// <summary>
        /// Build request to call /smart-context
        /// </summary>
        public PostSmartContextRequestBuilder PostSmartContextBuilder(SmartContextRequest requestBody);


        /// <summary>
        /// Build request to call /v1/smart-context
        /// </summary>
        public PostSmartContextV1RequestBuilder PostSmartContextV1Builder(SmartContextRequest requestBody);


        /// <summary>
        /// Build request to call /muse/inspiration/{inspiration_id}
        /// </summary>
        public PutMuseInspirationUsingInspirationIdRequestBuilder PutMuseInspirationUsingInspirationIdBuilder(string inspirationId, UpdateInspirationRequest requestBody);


        /// <summary>
        /// Build request to call /v1/muse/inspiration/{inspiration_id}
        /// </summary>
        public PutMuseInspirationUsingInspirationIdV1RequestBuilder PutMuseInspirationUsingInspirationIdV1Builder(string inspirationId, UpdateInspirationRequest requestBody);

    }

    /// <summary>
    /// Used to build requests to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
    /// </summary>
    internal class DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string FragmentId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string fragmentId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            FragmentId = fragmentId;
        }

        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest Build() => new DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest : IDeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest
    {
        DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder m_Builder;

        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequest(DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdAsync(m_Builder.ConversationId, m_Builder.FragmentId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversation Fragment Delete conversation fragment by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     fragment_id (str): Conversation fragment ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="fragmentId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdAsync(string conversationId, string fragmentId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->DeleteMuseConversationFragmentUsingConversationIdAndFragmentId");

            // verify the required parameter 'fragmentId' is set
            if (fragmentId == null)
                throw new ApiException(400, "Missing required parameter 'fragmentId' when calling MuseChatBackendApi->DeleteMuseConversationFragmentUsingConversationIdAndFragmentId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fragment_id", ClientUtils.ParameterToString(fragmentId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/muse/conversation/{conversation_id}/fragment/{fragment_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
    /// </summary>
    internal class DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string FragmentId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string fragmentId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            FragmentId = fragmentId;
        }

        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request Build() => new DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request : IDeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request
    {
        DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder m_Builder;

        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request(DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Async(m_Builder.ConversationId, m_Builder.FragmentId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversation Fragment Delete conversation fragment by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     fragment_id (str): Conversation fragment ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="fragmentId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Async(string conversationId, string fragmentId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1");

            // verify the required parameter 'fragmentId' is set
            if (fragmentId == null)
                throw new ApiException(400, "Missing required parameter 'fragmentId' when calling MuseChatBackendApi->DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fragment_id", ClientUtils.ParameterToString(fragmentId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/v1/muse/conversation/{conversation_id}/fragment/{fragment_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation/{conversation_id}
    /// </summary>
    internal class DeleteMuseConversationUsingConversationIdRequestBuilder
    {
        internal readonly string ConversationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation/{conversation_id}
        /// </summary>
        public DeleteMuseConversationUsingConversationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
        }

        public DeleteMuseConversationUsingConversationIdRequest Build() => new DeleteMuseConversationUsingConversationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationUsingConversationIdRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationUsingConversationIdRequest : IDeleteMuseConversationUsingConversationIdRequest
    {
        DeleteMuseConversationUsingConversationIdRequestBuilder m_Builder;

        public DeleteMuseConversationUsingConversationIdRequest(DeleteMuseConversationUsingConversationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationUsingConversationIdAsync(m_Builder.ConversationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversation Delete conversation by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationUsingConversationIdAsync(string conversationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->DeleteMuseConversationUsingConversationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation/{conversation_id}
    /// </summary>
    internal class DeleteMuseConversationUsingConversationIdV1RequestBuilder
    {
        internal readonly string ConversationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public DeleteMuseConversationUsingConversationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
        }

        public DeleteMuseConversationUsingConversationIdV1Request Build() => new DeleteMuseConversationUsingConversationIdV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationUsingConversationIdV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationUsingConversationIdV1Request : IDeleteMuseConversationUsingConversationIdV1Request
    {
        DeleteMuseConversationUsingConversationIdV1RequestBuilder m_Builder;

        public DeleteMuseConversationUsingConversationIdV1Request(DeleteMuseConversationUsingConversationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationUsingConversationIdV1Async(m_Builder.ConversationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversation Delete conversation by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationUsingConversationIdV1Async(string conversationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->DeleteMuseConversationUsingConversationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/v1/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversations/by-tags
    /// </summary>
    internal class DeleteMuseConversationsByTagsRequestBuilder
    {
        internal List<string> Tags;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversations/by-tags
        /// </summary>
        public DeleteMuseConversationsByTagsRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public DeleteMuseConversationsByTagsRequestBuilder SetTags(List<string> value)
        {
            Tags = value;
            return this;
        }

        public DeleteMuseConversationsByTagsRequest Build() => new DeleteMuseConversationsByTagsRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationsByTagsRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationsByTagsRequest : IDeleteMuseConversationsByTagsRequest
    {
        DeleteMuseConversationsByTagsRequestBuilder m_Builder;

        public DeleteMuseConversationsByTagsRequest(DeleteMuseConversationsByTagsRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationsByTagsAsync(m_Builder.Tags, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversations By Tags Delete conversations by tags.  Args:     request (Request): FastAPI request object.     tags (list[str])): list of tags.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="tags">List of tags to delete conversations by. (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationsByTagsAsync(List<string> tags = default(List<string>), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (tags != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("multi", "tags", tags));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/muse/conversations/by-tags", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversations/by-tags
    /// </summary>
    internal class DeleteMuseConversationsByTagsV1RequestBuilder
    {
        internal List<string> Tags;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversations/by-tags
        /// </summary>
        public DeleteMuseConversationsByTagsV1RequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public DeleteMuseConversationsByTagsV1RequestBuilder SetTags(List<string> value)
        {
            Tags = value;
            return this;
        }

        public DeleteMuseConversationsByTagsV1Request Build() => new DeleteMuseConversationsByTagsV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseConversationsByTagsV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseConversationsByTagsV1Request : IDeleteMuseConversationsByTagsV1Request
    {
        DeleteMuseConversationsByTagsV1RequestBuilder m_Builder;

        public DeleteMuseConversationsByTagsV1Request(DeleteMuseConversationsByTagsV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseConversationsByTagsV1Async(m_Builder.Tags, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Conversations By Tags Delete conversations by tags.  Args:     request (Request): FastAPI request object.     tags (list[str])): list of tags.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="tags">List of tags to delete conversations by. (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> DeleteMuseConversationsByTagsV1Async(List<string> tags = default(List<string>), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (tags != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("multi", "tags", tags));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ErrorResponse>("/v1/muse/conversations/by-tags", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/inspiration/{inspiration_id}
    /// </summary>
    internal class DeleteMuseInspirationUsingInspirationIdRequestBuilder
    {
        internal readonly string InspirationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/inspiration/{inspiration_id}
        /// </summary>
        public DeleteMuseInspirationUsingInspirationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string inspirationId)
        {
            Configuration = config;
            Client = apiClient;


            InspirationId = inspirationId;
        }

        public DeleteMuseInspirationUsingInspirationIdRequest Build() => new DeleteMuseInspirationUsingInspirationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationId>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseInspirationUsingInspirationIdRequest
    {
        Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseInspirationUsingInspirationIdRequest : IDeleteMuseInspirationUsingInspirationIdRequest
    {
        DeleteMuseInspirationUsingInspirationIdRequestBuilder m_Builder;

        public DeleteMuseInspirationUsingInspirationIdRequest(DeleteMuseInspirationUsingInspirationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseInspirationUsingInspirationIdAsync(m_Builder.InspirationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Inspiration Delete an inspiration from the database.  Args:     request: FastAPI request object.     inspiration_id: The ID of the inspiration to delete.  Returns: Success message or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspirationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseDeleteMuseInspirationUsingInspirationId)</returns>
        [Obsolete]
         async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationId>> DeleteMuseInspirationUsingInspirationIdAsync(string inspirationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspirationId' is set
            if (inspirationId == null)
                throw new ApiException(400, "Missing required parameter 'inspirationId' when calling MuseChatBackendApi->DeleteMuseInspirationUsingInspirationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("inspiration_id", ClientUtils.ParameterToString(inspirationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ResponseDeleteMuseInspirationUsingInspirationId>("/muse/inspiration/{inspiration_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/inspiration/{inspiration_id}
    /// </summary>
    internal class DeleteMuseInspirationUsingInspirationIdV1RequestBuilder
    {
        internal readonly string InspirationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/inspiration/{inspiration_id}
        /// </summary>
        public DeleteMuseInspirationUsingInspirationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string inspirationId)
        {
            Configuration = config;
            Client = apiClient;


            InspirationId = inspirationId;
        }

        public DeleteMuseInspirationUsingInspirationIdV1Request Build() => new DeleteMuseInspirationUsingInspirationIdV1Request(this);


        public async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationIdV1>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IDeleteMuseInspirationUsingInspirationIdV1Request
    {
        Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class DeleteMuseInspirationUsingInspirationIdV1Request : IDeleteMuseInspirationUsingInspirationIdV1Request
    {
        DeleteMuseInspirationUsingInspirationIdV1RequestBuilder m_Builder;

        public DeleteMuseInspirationUsingInspirationIdV1Request(DeleteMuseInspirationUsingInspirationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await DeleteMuseInspirationUsingInspirationIdV1Async(m_Builder.InspirationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Delete Inspiration Delete an inspiration from the database.  Args:     request: FastAPI request object.     inspiration_id: The ID of the inspiration to delete.  Returns: Success message or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspirationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseDeleteMuseInspirationUsingInspirationIdV1)</returns>
         async Task<ApiResponse<ResponseDeleteMuseInspirationUsingInspirationIdV1>> DeleteMuseInspirationUsingInspirationIdV1Async(string inspirationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspirationId' is set
            if (inspirationId == null)
                throw new ApiException(400, "Missing required parameter 'inspirationId' when calling MuseChatBackendApi->DeleteMuseInspirationUsingInspirationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("inspiration_id", ClientUtils.ParameterToString(inspirationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.DeleteAsync<ResponseDeleteMuseInspirationUsingInspirationIdV1>("/v1/muse/inspiration/{inspiration_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /health
    /// </summary>
    internal class GetHealthRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /health
        /// </summary>
        public GetHealthRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetHealthRequest Build() => new GetHealthRequest(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetHealthRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetHealthRequest : IGetHealthRequest
    {
        GetHealthRequestBuilder m_Builder;

        public GetHealthRequest(GetHealthRequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetHealthAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Health
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> GetHealthAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Object>("/health", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /healthz
    /// </summary>
    internal class GetHealthzRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /healthz
        /// </summary>
        public GetHealthzRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetHealthzRequest Build() => new GetHealthzRequest(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetHealthzRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetHealthzRequest : IGetHealthzRequest
    {
        GetHealthzRequestBuilder m_Builder;

        public GetHealthzRequest(GetHealthzRequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetHealthzAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Healthz
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> GetHealthzAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Object>("/healthz", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/beta/check_entitlement
    /// </summary>
    internal class GetMuseBetaCheckEntitlementRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/beta/check_entitlement
        /// </summary>
        public GetMuseBetaCheckEntitlementRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseBetaCheckEntitlementRequest Build() => new GetMuseBetaCheckEntitlementRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseBetaCheckEntitlementRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseBetaCheckEntitlementRequest : IGetMuseBetaCheckEntitlementRequest
    {
        GetMuseBetaCheckEntitlementRequestBuilder m_Builder;

        public GetMuseBetaCheckEntitlementRequest(GetMuseBetaCheckEntitlementRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseBetaCheckEntitlementAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Check Entitlement Checks the user for beta entitlement.  Args:     request (Request): The Starlette request.     user_info (UserInfo): The UserInfo.     user_genesis_token (str): The genesis token.  Returns: 200 if user is entitled, 404 otherwise.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> GetMuseBetaCheckEntitlementAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Object>("/muse/beta/check_entitlement", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/beta/check_entitlement
    /// </summary>
    internal class GetMuseBetaCheckEntitlementV1RequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/beta/check_entitlement
        /// </summary>
        public GetMuseBetaCheckEntitlementV1RequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseBetaCheckEntitlementV1Request Build() => new GetMuseBetaCheckEntitlementV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseBetaCheckEntitlementV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseBetaCheckEntitlementV1Request : IGetMuseBetaCheckEntitlementV1Request
    {
        GetMuseBetaCheckEntitlementV1RequestBuilder m_Builder;

        public GetMuseBetaCheckEntitlementV1Request(GetMuseBetaCheckEntitlementV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseBetaCheckEntitlementV1Async(cancellationToken, callbacks);
        }

        /// <summary>
        /// Check Entitlement Checks the user for beta entitlement.  Args:     request (Request): The Starlette request.     user_info (UserInfo): The UserInfo.     user_genesis_token (str): The genesis token.  Returns: 200 if user is entitled, 404 otherwise.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> GetMuseBetaCheckEntitlementV1Async(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Object>("/v1/muse/beta/check_entitlement", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation
    /// </summary>
    internal class GetMuseConversationRequestBuilder
    {
        internal int? Limit;
        internal int? Skip;
        internal bool? SkipProjectTag;
        internal string Tags;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation
        /// </summary>
        public GetMuseConversationRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseConversationRequestBuilder SetLimit(int? value)
        {
            Limit = value;
            return this;
        }

        public GetMuseConversationRequestBuilder SetSkip(int? value)
        {
            Skip = value;
            return this;
        }

        public GetMuseConversationRequestBuilder SetSkipProjectTag(bool? value)
        {
            SkipProjectTag = value;
            return this;
        }

        public GetMuseConversationRequestBuilder SetTags(string value)
        {
            Tags = value;
            return this;
        }

        public GetMuseConversationRequest Build() => new GetMuseConversationRequest(this);

        [Obsolete]
        public async Task<ApiResponse<List<ConversationInfo>>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseConversationRequest
    {
        Task<ApiResponse<List<ConversationInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseConversationRequest : IGetMuseConversationRequest
    {
        GetMuseConversationRequestBuilder m_Builder;

        public GetMuseConversationRequest(GetMuseConversationRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<List<ConversationInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseConversationAsync(m_Builder.Limit, m_Builder.Skip, m_Builder.SkipProjectTag, m_Builder.Tags, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Conversations Get conversation summaries for user conversations.  Args:     request (Request): FastAPI request object.     user_info (UserInfo): User information extracted from bearer token.     tags (Optional[str], optional): Project ID to filter conversations by. Defaults to None.     skip_project_tag (bool, optional): Whether to skip conversations with a project tag.     limit (int, optional): Number of conversations to return. Defaults to 100.     skip (int, optional): Number of conversations to skip. Defaults to 0.  Returns:     list[ConversationInfo]: List of conversation summaries for user.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit"> (optional, default to 100)</param>
        /// <param name="skip"> (optional, default to 0)</param>
        /// <param name="skipProjectTag"> (optional)</param>
        /// <param name="tags"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (List&lt;ConversationInfo&gt;)</returns>
        [Obsolete]
         async Task<ApiResponse<List<ConversationInfo>>> GetMuseConversationAsync(int? limit = default(int?), int? skip = default(int?), bool? skipProjectTag = default(bool?), string tags = default(string), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (limit != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "limit", limit));
            }
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip", skip));
            }
            if (skipProjectTag != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip_project_tag", skipProjectTag));
            }
            if (tags != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "tags", tags));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<List<ConversationInfo>>("/muse/conversation", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation/{conversation_id}
    /// </summary>
    internal class GetMuseConversationUsingConversationIdRequestBuilder
    {
        internal readonly string ConversationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation/{conversation_id}
        /// </summary>
        public GetMuseConversationUsingConversationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
        }

        public GetMuseConversationUsingConversationIdRequest Build() => new GetMuseConversationUsingConversationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ResponseGetMuseConversationUsingConversationId>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseConversationUsingConversationIdRequest
    {
        Task<ApiResponse<ResponseGetMuseConversationUsingConversationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseConversationUsingConversationIdRequest : IGetMuseConversationUsingConversationIdRequest
    {
        GetMuseConversationUsingConversationIdRequestBuilder m_Builder;

        public GetMuseConversationUsingConversationIdRequest(GetMuseConversationUsingConversationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ResponseGetMuseConversationUsingConversationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseConversationUsingConversationIdAsync(m_Builder.ConversationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Conversation Get conversation by conversation ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     ClientConversation | JSONResponse:         ClientConversation corresponding to ID if it exists, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseGetMuseConversationUsingConversationId)</returns>
        [Obsolete]
         async Task<ApiResponse<ResponseGetMuseConversationUsingConversationId>> GetMuseConversationUsingConversationIdAsync(string conversationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->GetMuseConversationUsingConversationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<ResponseGetMuseConversationUsingConversationId>("/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation/{conversation_id}
    /// </summary>
    internal class GetMuseConversationUsingConversationIdV1RequestBuilder
    {
        internal readonly string ConversationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public GetMuseConversationUsingConversationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
        }

        public GetMuseConversationUsingConversationIdV1Request Build() => new GetMuseConversationUsingConversationIdV1Request(this);


        public async Task<ApiResponse<ResponseGetMuseConversationUsingConversationIdV1>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseConversationUsingConversationIdV1Request
    {
        Task<ApiResponse<ResponseGetMuseConversationUsingConversationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseConversationUsingConversationIdV1Request : IGetMuseConversationUsingConversationIdV1Request
    {
        GetMuseConversationUsingConversationIdV1RequestBuilder m_Builder;

        public GetMuseConversationUsingConversationIdV1Request(GetMuseConversationUsingConversationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ResponseGetMuseConversationUsingConversationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseConversationUsingConversationIdV1Async(m_Builder.ConversationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Conversation Get conversation by conversation ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     ClientConversation | JSONResponse:         ClientConversation corresponding to ID if it exists, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseGetMuseConversationUsingConversationIdV1)</returns>
         async Task<ApiResponse<ResponseGetMuseConversationUsingConversationIdV1>> GetMuseConversationUsingConversationIdV1Async(string conversationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->GetMuseConversationUsingConversationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<ResponseGetMuseConversationUsingConversationIdV1>("/v1/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation
    /// </summary>
    internal class GetMuseConversationV1RequestBuilder
    {
        internal int? Limit;
        internal int? Skip;
        internal bool? SkipProjectTag;
        internal string Tags;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation
        /// </summary>
        public GetMuseConversationV1RequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseConversationV1RequestBuilder SetLimit(int? value)
        {
            Limit = value;
            return this;
        }

        public GetMuseConversationV1RequestBuilder SetSkip(int? value)
        {
            Skip = value;
            return this;
        }

        public GetMuseConversationV1RequestBuilder SetSkipProjectTag(bool? value)
        {
            SkipProjectTag = value;
            return this;
        }

        public GetMuseConversationV1RequestBuilder SetTags(string value)
        {
            Tags = value;
            return this;
        }

        public GetMuseConversationV1Request Build() => new GetMuseConversationV1Request(this);


        public async Task<ApiResponse<List<ConversationInfo>>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseConversationV1Request
    {
        Task<ApiResponse<List<ConversationInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseConversationV1Request : IGetMuseConversationV1Request
    {
        GetMuseConversationV1RequestBuilder m_Builder;

        public GetMuseConversationV1Request(GetMuseConversationV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<List<ConversationInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseConversationV1Async(m_Builder.Limit, m_Builder.Skip, m_Builder.SkipProjectTag, m_Builder.Tags, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Conversations Get conversation summaries for user conversations.  Args:     request (Request): FastAPI request object.     user_info (UserInfo): User information extracted from bearer token.     tags (Optional[str], optional): Project ID to filter conversations by. Defaults to None.     skip_project_tag (bool, optional): Whether to skip conversations with a project tag.     limit (int, optional): Number of conversations to return. Defaults to 100.     skip (int, optional): Number of conversations to skip. Defaults to 0.  Returns:     list[ConversationInfo]: List of conversation summaries for user.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit"> (optional, default to 100)</param>
        /// <param name="skip"> (optional, default to 0)</param>
        /// <param name="skipProjectTag"> (optional)</param>
        /// <param name="tags"> (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (List&lt;ConversationInfo&gt;)</returns>
         async Task<ApiResponse<List<ConversationInfo>>> GetMuseConversationV1Async(int? limit = default(int?), int? skip = default(int?), bool? skipProjectTag = default(bool?), string tags = default(string), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (limit != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "limit", limit));
            }
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip", skip));
            }
            if (skipProjectTag != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip_project_tag", skipProjectTag));
            }
            if (tags != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "tags", tags));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<List<ConversationInfo>>("/v1/muse/conversation", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/inspiration/
    /// </summary>
    internal class GetMuseInspirationRequestBuilder
    {
        internal int? Limit;
        internal string Mode;
        internal int? Skip;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/inspiration/
        /// </summary>
        public GetMuseInspirationRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseInspirationRequestBuilder SetLimit(int? value)
        {
            Limit = value;
            return this;
        }

        public GetMuseInspirationRequestBuilder SetMode(string value)
        {
            Mode = value;
            return this;
        }

        public GetMuseInspirationRequestBuilder SetSkip(int? value)
        {
            Skip = value;
            return this;
        }

        public GetMuseInspirationRequest Build() => new GetMuseInspirationRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ResponseGetMuseInspiration>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseInspirationRequest
    {
        Task<ApiResponse<ResponseGetMuseInspiration>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseInspirationRequest : IGetMuseInspirationRequest
    {
        GetMuseInspirationRequestBuilder m_Builder;

        public GetMuseInspirationRequest(GetMuseInspirationRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ResponseGetMuseInspiration>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseInspirationAsync(m_Builder.Limit, m_Builder.Mode, m_Builder.Skip, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Inspirations Get inspirations from the database.  Args:     request: FastAPI request object.     mode: Filter inspirations by mode.     limit: Number of inspirations to return.     skip: Number of inspirations to skip.  Returns: List of inspirations or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit"> (optional, default to 100)</param>
        /// <param name="mode">Filter inspirations by mode (optional)</param>
        /// <param name="skip"> (optional, default to 0)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseGetMuseInspiration)</returns>
        [Obsolete]
         async Task<ApiResponse<ResponseGetMuseInspiration>> GetMuseInspirationAsync(int? limit = default(int?), string mode = default(string), int? skip = default(int?), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (limit != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "limit", limit));
            }
            if (mode != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "mode", mode));
            }
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip", skip));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<ResponseGetMuseInspiration>("/muse/inspiration/", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/inspiration/
    /// </summary>
    internal class GetMuseInspirationV1RequestBuilder
    {
        internal int? Limit;
        internal string Mode;
        internal int? Skip;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/inspiration/
        /// </summary>
        public GetMuseInspirationV1RequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseInspirationV1RequestBuilder SetLimit(int? value)
        {
            Limit = value;
            return this;
        }

        public GetMuseInspirationV1RequestBuilder SetMode(string value)
        {
            Mode = value;
            return this;
        }

        public GetMuseInspirationV1RequestBuilder SetSkip(int? value)
        {
            Skip = value;
            return this;
        }

        public GetMuseInspirationV1Request Build() => new GetMuseInspirationV1Request(this);


        public async Task<ApiResponse<ResponseGetMuseInspirationV1>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseInspirationV1Request
    {
        Task<ApiResponse<ResponseGetMuseInspirationV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseInspirationV1Request : IGetMuseInspirationV1Request
    {
        GetMuseInspirationV1RequestBuilder m_Builder;

        public GetMuseInspirationV1Request(GetMuseInspirationV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ResponseGetMuseInspirationV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseInspirationV1Async(m_Builder.Limit, m_Builder.Mode, m_Builder.Skip, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Inspirations Get inspirations from the database.  Args:     request: FastAPI request object.     mode: Filter inspirations by mode.     limit: Number of inspirations to return.     skip: Number of inspirations to skip.  Returns: List of inspirations or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="limit"> (optional, default to 100)</param>
        /// <param name="mode">Filter inspirations by mode (optional)</param>
        /// <param name="skip"> (optional, default to 0)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponseGetMuseInspirationV1)</returns>
         async Task<ApiResponse<ResponseGetMuseInspirationV1>> GetMuseInspirationV1Async(int? limit = default(int?), string mode = default(string), int? skip = default(int?), CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (limit != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "limit", limit));
            }
            if (mode != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "mode", mode));
            }
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "skip", skip));
            }

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<ResponseGetMuseInspirationV1>("/v1/muse/inspiration/", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/opt
    /// </summary>
    internal class GetMuseOptRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/opt
        /// </summary>
        public GetMuseOptRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseOptRequest Build() => new GetMuseOptRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Dictionary<string, OptDecision>>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseOptRequest
    {
        Task<ApiResponse<Dictionary<string, OptDecision>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseOptRequest : IGetMuseOptRequest
    {
        GetMuseOptRequestBuilder m_Builder;

        public GetMuseOptRequest(GetMuseOptRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Dictionary<string, OptDecision>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseOptAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Opt Get the current opt status of the requesting user.  Args:     request (Request): _description_     user_info (UserInfo, optional): _description_. Defaults to Depends(extract_user_info).  Returns:     dict[OptType, OptDecision]: Opt status of the user.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Dictionary&lt;string, OptDecision&gt;)</returns>
        [Obsolete]
         async Task<ApiResponse<Dictionary<string, OptDecision>>> GetMuseOptAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Dictionary<string, OptDecision>>("/muse/opt", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/opt
    /// </summary>
    internal class GetMuseOptV1RequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/opt
        /// </summary>
        public GetMuseOptV1RequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetMuseOptV1Request Build() => new GetMuseOptV1Request(this);


        public async Task<ApiResponse<Dictionary<string, OptDecision>>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseOptV1Request
    {
        Task<ApiResponse<Dictionary<string, OptDecision>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseOptV1Request : IGetMuseOptV1Request
    {
        GetMuseOptV1RequestBuilder m_Builder;

        public GetMuseOptV1Request(GetMuseOptV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Dictionary<string, OptDecision>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseOptV1Async(cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Opt Get the current opt status of the requesting user.  Args:     request (Request): _description_     user_info (UserInfo, optional): _description_. Defaults to Depends(extract_user_info).  Returns:     dict[OptType, OptDecision]: Opt status of the user.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Dictionary&lt;string, OptDecision&gt;)</returns>
         async Task<ApiResponse<Dictionary<string, OptDecision>>> GetMuseOptV1Async(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);


            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<Dictionary<string, OptDecision>>("/v1/muse/opt", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/topic/{conversation_id}
    /// </summary>
    internal class GetMuseTopicUsingConversationIdRequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string OrganizationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/topic/{conversation_id}
        /// </summary>
        public GetMuseTopicUsingConversationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string organizationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            OrganizationId = organizationId;
        }

        public GetMuseTopicUsingConversationIdRequest Build() => new GetMuseTopicUsingConversationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<string>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseTopicUsingConversationIdRequest
    {
        Task<ApiResponse<string>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseTopicUsingConversationIdRequest : IGetMuseTopicUsingConversationIdRequest
    {
        GetMuseTopicUsingConversationIdRequestBuilder m_Builder;

        public GetMuseTopicUsingConversationIdRequest(GetMuseTopicUsingConversationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<string>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseTopicUsingConversationIdAsync(m_Builder.ConversationId, m_Builder.OrganizationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Topic Get topic title for conversation.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     organization_id (str): Organization ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     str | JSONResponse:         Plain-text topic if conversation exists, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="organizationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (string)</returns>
        [Obsolete]
         async Task<ApiResponse<string>> GetMuseTopicUsingConversationIdAsync(string conversationId, string organizationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->GetMuseTopicUsingConversationId");

            // verify the required parameter 'organizationId' is set
            if (organizationId == null)
                throw new ApiException(400, "Missing required parameter 'organizationId' when calling MuseChatBackendApi->GetMuseTopicUsingConversationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "organization_id", organizationId));

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<string>("/muse/topic/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/topic/{conversation_id}
    /// </summary>
    internal class GetMuseTopicUsingConversationIdV1RequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string OrganizationId;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/topic/{conversation_id}
        /// </summary>
        public GetMuseTopicUsingConversationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string organizationId)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            OrganizationId = organizationId;
        }

        public GetMuseTopicUsingConversationIdV1Request Build() => new GetMuseTopicUsingConversationIdV1Request(this);


        public async Task<ApiResponse<string>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetMuseTopicUsingConversationIdV1Request
    {
        Task<ApiResponse<string>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetMuseTopicUsingConversationIdV1Request : IGetMuseTopicUsingConversationIdV1Request
    {
        GetMuseTopicUsingConversationIdV1RequestBuilder m_Builder;

        public GetMuseTopicUsingConversationIdV1Request(GetMuseTopicUsingConversationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<string>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetMuseTopicUsingConversationIdV1Async(m_Builder.ConversationId, m_Builder.OrganizationId, cancellationToken, callbacks);
        }

        /// <summary>
        /// Get Topic Get topic title for conversation.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     organization_id (str): Organization ID.     user_info (UserInfo): User information extracted from bearer token.  Returns:     str | JSONResponse:         Plain-text topic if conversation exists, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="organizationId"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (string)</returns>
         async Task<ApiResponse<string>> GetMuseTopicUsingConversationIdV1Async(string conversationId, string organizationId, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->GetMuseTopicUsingConversationIdV1");

            // verify the required parameter 'organizationId' is set
            if (organizationId == null)
                throw new ApiException(400, "Missing required parameter 'organizationId' when calling MuseChatBackendApi->GetMuseTopicUsingConversationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "organization_id", organizationId));

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.GetAsync<string>("/v1/muse/topic/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /versions/
    /// </summary>
    internal class GetVersionsRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /versions/
        /// </summary>
        public GetVersionsRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public GetVersionsRequest Build() => new GetVersionsRequest(this);


        public async Task<ApiResponse<List<VersionSupportInfo>>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IGetVersionsRequest
    {
        Task<ApiResponse<List<VersionSupportInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class GetVersionsRequest : IGetVersionsRequest
    {
        GetVersionsRequestBuilder m_Builder;

        public GetVersionsRequest(GetVersionsRequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<List<VersionSupportInfo>>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await GetVersionsAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Get supported route versions Before calling any routes provided by this backend, clients should check the supported route versions to ensure compatibility.  If the current route used is deprecated, a warning should be displayed to the user that they should consider upgrading.  If the current route is unsupported, the client should display an error message to the user that tool is non functional and they should upgrade.  Routes may need to be deprecated for a variety of reasons including:  * Security vulnerabilities * Performance improvements (both result performance, cost performance and latency performance) * Business model changes * New versions are released and we do not have the resources to maintain the many older versions  Full design doc here: https://docs.google.com/document/d/1vKRnsuiTgBXDdt82w9fTkwm6QTMts65OepqT3mIKzjE/edit?tab&#x3D;t.tj5ninp3m6j4
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (List&lt;VersionSupportInfo&gt;)</returns>
         async Task<ApiResponse<List<VersionSupportInfo>>> GetVersionsAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request
            var task = m_Builder.Client.GetAsync<List<VersionSupportInfo>>("/versions/", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /health
    /// </summary>
    internal class HeadHealthRequestBuilder
    {

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /health
        /// </summary>
        public HeadHealthRequestBuilder(IReadableConfiguration config, IClient apiClient)
        {
            Configuration = config;
            Client = apiClient;


        }

        public HeadHealthRequest Build() => new HeadHealthRequest(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IHeadHealthRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class HeadHealthRequest : IHeadHealthRequest
    {
        HeadHealthRequestBuilder m_Builder;

        public HeadHealthRequest(HeadHealthRequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await HeadHealthAsync(cancellationToken, callbacks);
        }

        /// <summary>
        /// Health Head
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> HeadHealthAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {

            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);



            // make the HTTP request
            var task = m_Builder.Client.HeadAsync<Object>("/health", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
    /// </summary>
    internal class PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string FragmentId;
        internal readonly ConversationFragmentPatch ConversationFragmentPatch;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string fragmentId, ConversationFragmentPatch requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            FragmentId = fragmentId;
            ConversationFragmentPatch = requestBody;
        }

        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest Build() => new PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest : IPatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest
    {
        PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder m_Builder;

        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequest(PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PatchMuseConversationFragmentUsingConversationIdAndFragmentIdAsync(m_Builder.ConversationId, m_Builder.FragmentId, m_Builder.ConversationFragmentPatch, cancellationToken, callbacks);
        }

        /// <summary>
        /// Patch Conversation Fragment Update conversation fragment by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     fragment_id (str): Conversation fragment ID.     body (ConversationPatchRequest): Patch request for changing conversation fragment.     user_info (UserInfo): User information extracted from bearer token.  Returns:     None | JSONResponse: None if successful, otherwise ErrorResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="fragmentId"></param>
        /// <param name="conversationFragmentPatch"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> PatchMuseConversationFragmentUsingConversationIdAndFragmentIdAsync(string conversationId, string fragmentId, ConversationFragmentPatch conversationFragmentPatch, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentId");

            // verify the required parameter 'fragmentId' is set
            if (fragmentId == null)
                throw new ApiException(400, "Missing required parameter 'fragmentId' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentId");

            // verify the required parameter 'conversationFragmentPatch' is set
            if (conversationFragmentPatch == null)
                throw new ApiException(400, "Missing required parameter 'conversationFragmentPatch' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fragment_id", ClientUtils.ParameterToString(fragmentId)); // path parameter
            localVarRequestOptions.Data = conversationFragmentPatch;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PatchAsync<ErrorResponse>("/muse/conversation/{conversation_id}/fragment/{fragment_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
    /// </summary>
    internal class PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly string FragmentId;
        internal readonly ConversationFragmentPatch ConversationFragmentPatch;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation/{conversation_id}/fragment/{fragment_id}
        /// </summary>
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, string fragmentId, ConversationFragmentPatch requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            FragmentId = fragmentId;
            ConversationFragmentPatch = requestBody;
        }

        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request Build() => new PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request : IPatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request
    {
        PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder m_Builder;

        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Request(PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Async(m_Builder.ConversationId, m_Builder.FragmentId, m_Builder.ConversationFragmentPatch, cancellationToken, callbacks);
        }

        /// <summary>
        /// Patch Conversation Fragment Update conversation fragment by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     fragment_id (str): Conversation fragment ID.     body (ConversationPatchRequest): Patch request for changing conversation fragment.     user_info (UserInfo): User information extracted from bearer token.  Returns:     None | JSONResponse: None if successful, otherwise ErrorResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="fragmentId"></param>
        /// <param name="conversationFragmentPatch"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Async(string conversationId, string fragmentId, ConversationFragmentPatch conversationFragmentPatch, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1");

            // verify the required parameter 'fragmentId' is set
            if (fragmentId == null)
                throw new ApiException(400, "Missing required parameter 'fragmentId' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1");

            // verify the required parameter 'conversationFragmentPatch' is set
            if (conversationFragmentPatch == null)
                throw new ApiException(400, "Missing required parameter 'conversationFragmentPatch' when calling MuseChatBackendApi->PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fragment_id", ClientUtils.ParameterToString(fragmentId)); // path parameter
            localVarRequestOptions.Data = conversationFragmentPatch;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PatchAsync<ErrorResponse>("/v1/muse/conversation/{conversation_id}/fragment/{fragment_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation/{conversation_id}
    /// </summary>
    internal class PatchMuseConversationUsingConversationIdRequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly ConversationPatchRequest ConversationPatchRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation/{conversation_id}
        /// </summary>
        public PatchMuseConversationUsingConversationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, ConversationPatchRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            ConversationPatchRequest = requestBody;
        }

        public PatchMuseConversationUsingConversationIdRequest Build() => new PatchMuseConversationUsingConversationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPatchMuseConversationUsingConversationIdRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PatchMuseConversationUsingConversationIdRequest : IPatchMuseConversationUsingConversationIdRequest
    {
        PatchMuseConversationUsingConversationIdRequestBuilder m_Builder;

        public PatchMuseConversationUsingConversationIdRequest(PatchMuseConversationUsingConversationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PatchMuseConversationUsingConversationIdAsync(m_Builder.ConversationId, m_Builder.ConversationPatchRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Patch Conversation Update conversation by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     body (ConversationPatchRequest): Patch request for changing conversation.     user_info (UserInfo): User information extracted from bearer token.  Returns:     None | JSONResponse: None if successful, otherwise ErrorResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="conversationPatchRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> PatchMuseConversationUsingConversationIdAsync(string conversationId, ConversationPatchRequest conversationPatchRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->PatchMuseConversationUsingConversationId");

            // verify the required parameter 'conversationPatchRequest' is set
            if (conversationPatchRequest == null)
                throw new ApiException(400, "Missing required parameter 'conversationPatchRequest' when calling MuseChatBackendApi->PatchMuseConversationUsingConversationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.Data = conversationPatchRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PatchAsync<ErrorResponse>("/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation/{conversation_id}
    /// </summary>
    internal class PatchMuseConversationUsingConversationIdV1RequestBuilder
    {
        internal readonly string ConversationId;
        internal readonly ConversationPatchRequest ConversationPatchRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation/{conversation_id}
        /// </summary>
        public PatchMuseConversationUsingConversationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string conversationId, ConversationPatchRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ConversationId = conversationId;
            ConversationPatchRequest = requestBody;
        }

        public PatchMuseConversationUsingConversationIdV1Request Build() => new PatchMuseConversationUsingConversationIdV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPatchMuseConversationUsingConversationIdV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PatchMuseConversationUsingConversationIdV1Request : IPatchMuseConversationUsingConversationIdV1Request
    {
        PatchMuseConversationUsingConversationIdV1RequestBuilder m_Builder;

        public PatchMuseConversationUsingConversationIdV1Request(PatchMuseConversationUsingConversationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PatchMuseConversationUsingConversationIdV1Async(m_Builder.ConversationId, m_Builder.ConversationPatchRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Patch Conversation Update conversation by ID.  Args:     request (Request): FastAPI request object.     conversation_id (str): Conversation ID.     body (ConversationPatchRequest): Patch request for changing conversation.     user_info (UserInfo): User information extracted from bearer token.  Returns:     None | JSONResponse: None if successful, otherwise ErrorResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="conversationId"></param>
        /// <param name="conversationPatchRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> PatchMuseConversationUsingConversationIdV1Async(string conversationId, ConversationPatchRequest conversationPatchRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'conversationId' is set
            if (conversationId == null)
                throw new ApiException(400, "Missing required parameter 'conversationId' when calling MuseChatBackendApi->PatchMuseConversationUsingConversationIdV1");

            // verify the required parameter 'conversationPatchRequest' is set
            if (conversationPatchRequest == null)
                throw new ApiException(400, "Missing required parameter 'conversationPatchRequest' when calling MuseChatBackendApi->PatchMuseConversationUsingConversationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("conversation_id", ClientUtils.ParameterToString(conversationId)); // path parameter
            localVarRequestOptions.Data = conversationPatchRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PatchAsync<ErrorResponse>("/v1/muse/conversation/{conversation_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/agent/action
    /// </summary>
    internal class PostMuseAgentActionRequestBuilder
    {
        internal readonly ActionRequest ActionRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/agent/action
        /// </summary>
        public PostMuseAgentActionRequestBuilder(IReadableConfiguration config, IClient apiClient, ActionRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ActionRequest = requestBody;
        }

        public PostMuseAgentActionRequest Build() => new PostMuseAgentActionRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentActionRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentActionRequest : IPostMuseAgentActionRequest
    {
        PostMuseAgentActionRequestBuilder m_Builder;

        public PostMuseAgentActionRequest(PostMuseAgentActionRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentActionAsync(m_Builder.ActionRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Action Agent action route for performing actions in the editor.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="actionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseAgentActionAsync(ActionRequest actionRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'actionRequest' is set
            if (actionRequest == null)
                throw new ApiException(400, "Missing required parameter 'actionRequest' when calling MuseChatBackendApi->PostMuseAgentAction");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = actionRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/agent/action", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/agent/action
    /// </summary>
    internal class PostMuseAgentActionV1RequestBuilder
    {
        internal readonly ActionRequest ActionRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/agent/action
        /// </summary>
        public PostMuseAgentActionV1RequestBuilder(IReadableConfiguration config, IClient apiClient, ActionRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ActionRequest = requestBody;
        }

        public PostMuseAgentActionV1Request Build() => new PostMuseAgentActionV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentActionV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentActionV1Request : IPostMuseAgentActionV1Request
    {
        PostMuseAgentActionV1RequestBuilder m_Builder;

        public PostMuseAgentActionV1Request(PostMuseAgentActionV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentActionV1Async(m_Builder.ActionRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Action Agent action route for performing actions in the editor.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="actionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseAgentActionV1Async(ActionRequest actionRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'actionRequest' is set
            if (actionRequest == null)
                throw new ApiException(400, "Missing required parameter 'actionRequest' when calling MuseChatBackendApi->PostMuseAgentActionV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = actionRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/agent/action", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/agent/code_repair
    /// </summary>
    internal class PostMuseAgentCodeRepairRequestBuilder
    {
        internal readonly ActionCodeRepairRequest ActionCodeRepairRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/agent/code_repair
        /// </summary>
        public PostMuseAgentCodeRepairRequestBuilder(IReadableConfiguration config, IClient apiClient, ActionCodeRepairRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ActionCodeRepairRequest = requestBody;
        }

        public PostMuseAgentCodeRepairRequest Build() => new PostMuseAgentCodeRepairRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentCodeRepairRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentCodeRepairRequest : IPostMuseAgentCodeRepairRequest
    {
        PostMuseAgentCodeRepairRequestBuilder m_Builder;

        public PostMuseAgentCodeRepairRequest(PostMuseAgentCodeRepairRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentCodeRepairAsync(m_Builder.ActionCodeRepairRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Action Code Repair Agent action code repairing route for repairing generated csharp scripts.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="actionCodeRepairRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseAgentCodeRepairAsync(ActionCodeRepairRequest actionCodeRepairRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'actionCodeRepairRequest' is set
            if (actionCodeRepairRequest == null)
                throw new ApiException(400, "Missing required parameter 'actionCodeRepairRequest' when calling MuseChatBackendApi->PostMuseAgentCodeRepair");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = actionCodeRepairRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/agent/code_repair", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/agent/code_repair
    /// </summary>
    internal class PostMuseAgentCodeRepairV1RequestBuilder
    {
        internal readonly ActionCodeRepairRequest ActionCodeRepairRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/agent/code_repair
        /// </summary>
        public PostMuseAgentCodeRepairV1RequestBuilder(IReadableConfiguration config, IClient apiClient, ActionCodeRepairRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ActionCodeRepairRequest = requestBody;
        }

        public PostMuseAgentCodeRepairV1Request Build() => new PostMuseAgentCodeRepairV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentCodeRepairV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentCodeRepairV1Request : IPostMuseAgentCodeRepairV1Request
    {
        PostMuseAgentCodeRepairV1RequestBuilder m_Builder;

        public PostMuseAgentCodeRepairV1Request(PostMuseAgentCodeRepairV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentCodeRepairV1Async(m_Builder.ActionCodeRepairRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Action Code Repair Agent action code repairing route for repairing generated csharp scripts.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="actionCodeRepairRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseAgentCodeRepairV1Async(ActionCodeRepairRequest actionCodeRepairRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'actionCodeRepairRequest' is set
            if (actionCodeRepairRequest == null)
                throw new ApiException(400, "Missing required parameter 'actionCodeRepairRequest' when calling MuseChatBackendApi->PostMuseAgentCodeRepairV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = actionCodeRepairRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/agent/code_repair", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/agent/codegen
    /// </summary>
    internal class PostMuseAgentCodegenRequestBuilder
    {
        internal readonly CodeGenRequest CodeGenRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/agent/codegen
        /// </summary>
        public PostMuseAgentCodegenRequestBuilder(IReadableConfiguration config, IClient apiClient, CodeGenRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CodeGenRequest = requestBody;
        }

        public PostMuseAgentCodegenRequest Build() => new PostMuseAgentCodegenRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentCodegenRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentCodegenRequest : IPostMuseAgentCodegenRequest
    {
        PostMuseAgentCodegenRequestBuilder m_Builder;

        public PostMuseAgentCodegenRequest(PostMuseAgentCodegenRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentCodegenAsync(m_Builder.CodeGenRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Codegen POC of CodeGen route.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="codeGenRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseAgentCodegenAsync(CodeGenRequest codeGenRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'codeGenRequest' is set
            if (codeGenRequest == null)
                throw new ApiException(400, "Missing required parameter 'codeGenRequest' when calling MuseChatBackendApi->PostMuseAgentCodegen");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = codeGenRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/agent/codegen", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/agent/codegen
    /// </summary>
    internal class PostMuseAgentCodegenV1RequestBuilder
    {
        internal readonly CodeGenRequest CodeGenRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/agent/codegen
        /// </summary>
        public PostMuseAgentCodegenV1RequestBuilder(IReadableConfiguration config, IClient apiClient, CodeGenRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CodeGenRequest = requestBody;
        }

        public PostMuseAgentCodegenV1Request Build() => new PostMuseAgentCodegenV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseAgentCodegenV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseAgentCodegenV1Request : IPostMuseAgentCodegenV1Request
    {
        PostMuseAgentCodegenV1RequestBuilder m_Builder;

        public PostMuseAgentCodegenV1Request(PostMuseAgentCodegenV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseAgentCodegenV1Async(m_Builder.CodeGenRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Codegen POC of CodeGen route.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="codeGenRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseAgentCodegenV1Async(CodeGenRequest codeGenRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'codeGenRequest' is set
            if (codeGenRequest == null)
                throw new ApiException(400, "Missing required parameter 'codeGenRequest' when calling MuseChatBackendApi->PostMuseAgentCodegenV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = codeGenRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/agent/codegen", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/chat
    /// </summary>
    internal class PostMuseChatRequestBuilder
    {
        internal readonly ChatRequest ChatRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/chat
        /// </summary>
        public PostMuseChatRequestBuilder(IReadableConfiguration config, IClient apiClient, ChatRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ChatRequest = requestBody;
        }

        public PostMuseChatRequest Build() => new PostMuseChatRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseChatRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseChatRequest : IPostMuseChatRequest
    {
        PostMuseChatRequestBuilder m_Builder;

        public PostMuseChatRequest(PostMuseChatRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseChatAsync(m_Builder.ChatRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Chat Chat with Muse.  Args:     request (Request): FastAPI request object.     body (ChatRequest): Chat request body.  Returns:     JSONResponse | ManagedStreamingResponse: Either JSONResponse or ManagedStreamingResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="chatRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseChatAsync(ChatRequest chatRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'chatRequest' is set
            if (chatRequest == null)
                throw new ApiException(400, "Missing required parameter 'chatRequest' when calling MuseChatBackendApi->PostMuseChat");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = chatRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/chat", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/chat
    /// </summary>
    internal class PostMuseChatV1RequestBuilder
    {
        internal readonly ChatRequest ChatRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/chat
        /// </summary>
        public PostMuseChatV1RequestBuilder(IReadableConfiguration config, IClient apiClient, ChatRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ChatRequest = requestBody;
        }

        public PostMuseChatV1Request Build() => new PostMuseChatV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseChatV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseChatV1Request : IPostMuseChatV1Request
    {
        PostMuseChatV1RequestBuilder m_Builder;

        public PostMuseChatV1Request(PostMuseChatV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseChatV1Async(m_Builder.ChatRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Chat Chat with Muse.  Args:     request (Request): FastAPI request object.     body (ChatRequest): Chat request body.  Returns:     JSONResponse | ManagedStreamingResponse: Either JSONResponse or ManagedStreamingResponse.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="chatRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseChatV1Async(ChatRequest chatRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'chatRequest' is set
            if (chatRequest == null)
                throw new ApiException(400, "Missing required parameter 'chatRequest' when calling MuseChatBackendApi->PostMuseChatV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = chatRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/chat", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/completion
    /// </summary>
    internal class PostMuseCompletionRequestBuilder
    {
        internal readonly ContextualCompletionRequest ContextualCompletionRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/completion
        /// </summary>
        public PostMuseCompletionRequestBuilder(IReadableConfiguration config, IClient apiClient, ContextualCompletionRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ContextualCompletionRequest = requestBody;
        }

        public PostMuseCompletionRequest Build() => new PostMuseCompletionRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseCompletionRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseCompletionRequest : IPostMuseCompletionRequest
    {
        PostMuseCompletionRequestBuilder m_Builder;

        public PostMuseCompletionRequest(PostMuseCompletionRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseCompletionAsync(m_Builder.ContextualCompletionRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Completion Endpoint Endpoint for handling completion requests.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="contextualCompletionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseCompletionAsync(ContextualCompletionRequest contextualCompletionRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'contextualCompletionRequest' is set
            if (contextualCompletionRequest == null)
                throw new ApiException(400, "Missing required parameter 'contextualCompletionRequest' when calling MuseChatBackendApi->PostMuseCompletion");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = contextualCompletionRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/completion", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/completion/repair
    /// </summary>
    internal class PostMuseCompletionRepairRequestBuilder
    {
        internal readonly CompletionRepairRequest CompletionRepairRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/completion/repair
        /// </summary>
        public PostMuseCompletionRepairRequestBuilder(IReadableConfiguration config, IClient apiClient, CompletionRepairRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CompletionRepairRequest = requestBody;
        }

        public PostMuseCompletionRepairRequest Build() => new PostMuseCompletionRepairRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseCompletionRepairRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseCompletionRepairRequest : IPostMuseCompletionRepairRequest
    {
        PostMuseCompletionRepairRequestBuilder m_Builder;

        public PostMuseCompletionRepairRequest(PostMuseCompletionRepairRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseCompletionRepairAsync(m_Builder.CompletionRepairRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Repair Endpoint Generic repair route for repairing generated content.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="completionRepairRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseCompletionRepairAsync(CompletionRepairRequest completionRepairRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'completionRepairRequest' is set
            if (completionRepairRequest == null)
                throw new ApiException(400, "Missing required parameter 'completionRepairRequest' when calling MuseChatBackendApi->PostMuseCompletionRepair");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = completionRepairRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/completion/repair", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/completion/repair
    /// </summary>
    internal class PostMuseCompletionRepairV1RequestBuilder
    {
        internal readonly CompletionRepairRequest CompletionRepairRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/completion/repair
        /// </summary>
        public PostMuseCompletionRepairV1RequestBuilder(IReadableConfiguration config, IClient apiClient, CompletionRepairRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CompletionRepairRequest = requestBody;
        }

        public PostMuseCompletionRepairV1Request Build() => new PostMuseCompletionRepairV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseCompletionRepairV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseCompletionRepairV1Request : IPostMuseCompletionRepairV1Request
    {
        PostMuseCompletionRepairV1RequestBuilder m_Builder;

        public PostMuseCompletionRepairV1Request(PostMuseCompletionRepairV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseCompletionRepairV1Async(m_Builder.CompletionRepairRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Repair Endpoint Generic repair route for repairing generated content.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="completionRepairRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseCompletionRepairV1Async(CompletionRepairRequest completionRepairRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'completionRepairRequest' is set
            if (completionRepairRequest == null)
                throw new ApiException(400, "Missing required parameter 'completionRepairRequest' when calling MuseChatBackendApi->PostMuseCompletionRepairV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = completionRepairRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/completion/repair", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/completion
    /// </summary>
    internal class PostMuseCompletionV1RequestBuilder
    {
        internal readonly ContextualCompletionRequest ContextualCompletionRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/completion
        /// </summary>
        public PostMuseCompletionV1RequestBuilder(IReadableConfiguration config, IClient apiClient, ContextualCompletionRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            ContextualCompletionRequest = requestBody;
        }

        public PostMuseCompletionV1Request Build() => new PostMuseCompletionV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseCompletionV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseCompletionV1Request : IPostMuseCompletionV1Request
    {
        PostMuseCompletionV1RequestBuilder m_Builder;

        public PostMuseCompletionV1Request(PostMuseCompletionV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseCompletionV1Async(m_Builder.ContextualCompletionRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Completion Endpoint Endpoint for handling completion requests.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="contextualCompletionRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseCompletionV1Async(ContextualCompletionRequest contextualCompletionRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'contextualCompletionRequest' is set
            if (contextualCompletionRequest == null)
                throw new ApiException(400, "Missing required parameter 'contextualCompletionRequest' when calling MuseChatBackendApi->PostMuseCompletionV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = contextualCompletionRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/completion", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/conversation
    /// </summary>
    internal class PostMuseConversationRequestBuilder
    {
        internal readonly CreateConversationRequest CreateConversationRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/conversation
        /// </summary>
        public PostMuseConversationRequestBuilder(IReadableConfiguration config, IClient apiClient, CreateConversationRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CreateConversationRequest = requestBody;
        }

        public PostMuseConversationRequest Build() => new PostMuseConversationRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Conversation>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseConversationRequest
    {
        Task<ApiResponse<Conversation>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseConversationRequest : IPostMuseConversationRequest
    {
        PostMuseConversationRequestBuilder m_Builder;

        public PostMuseConversationRequest(PostMuseConversationRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Conversation>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseConversationAsync(m_Builder.CreateConversationRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Create Conversation
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="createConversationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Conversation)</returns>
        [Obsolete]
         async Task<ApiResponse<Conversation>> PostMuseConversationAsync(CreateConversationRequest createConversationRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'createConversationRequest' is set
            if (createConversationRequest == null)
                throw new ApiException(400, "Missing required parameter 'createConversationRequest' when calling MuseChatBackendApi->PostMuseConversation");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = createConversationRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Conversation>("/muse/conversation", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/conversation
    /// </summary>
    internal class PostMuseConversationV1RequestBuilder
    {
        internal readonly CreateConversationRequest CreateConversationRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/conversation
        /// </summary>
        public PostMuseConversationV1RequestBuilder(IReadableConfiguration config, IClient apiClient, CreateConversationRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            CreateConversationRequest = requestBody;
        }

        public PostMuseConversationV1Request Build() => new PostMuseConversationV1Request(this);


        public async Task<ApiResponse<Conversation>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseConversationV1Request
    {
        Task<ApiResponse<Conversation>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseConversationV1Request : IPostMuseConversationV1Request
    {
        PostMuseConversationV1RequestBuilder m_Builder;

        public PostMuseConversationV1Request(PostMuseConversationV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Conversation>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseConversationV1Async(m_Builder.CreateConversationRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Create Conversation
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="createConversationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Conversation)</returns>
         async Task<ApiResponse<Conversation>> PostMuseConversationV1Async(CreateConversationRequest createConversationRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'createConversationRequest' is set
            if (createConversationRequest == null)
                throw new ApiException(400, "Missing required parameter 'createConversationRequest' when calling MuseChatBackendApi->PostMuseConversationV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = createConversationRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Conversation>("/v1/muse/conversation", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/feedback
    /// </summary>
    internal class PostMuseFeedbackRequestBuilder
    {
        internal readonly Feedback Feedback;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/feedback
        /// </summary>
        public PostMuseFeedbackRequestBuilder(IReadableConfiguration config, IClient apiClient, Feedback requestBody)
        {
            Configuration = config;
            Client = apiClient;


            Feedback = requestBody;
        }

        public PostMuseFeedbackRequest Build() => new PostMuseFeedbackRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseFeedbackRequest
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseFeedbackRequest : IPostMuseFeedbackRequest
    {
        PostMuseFeedbackRequestBuilder m_Builder;

        public PostMuseFeedbackRequest(PostMuseFeedbackRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseFeedbackAsync(m_Builder.Feedback, cancellationToken, callbacks);
        }

        /// <summary>
        /// Feedback Provide feedback.  Args:     request (Request): FastAPI request object.     body (Feedback): Feedback request body.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="feedback"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<ErrorResponse>> PostMuseFeedbackAsync(Feedback feedback, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'feedback' is set
            if (feedback == null)
                throw new ApiException(400, "Missing required parameter 'feedback' when calling MuseChatBackendApi->PostMuseFeedback");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = feedback;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<ErrorResponse>("/muse/feedback", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/feedback
    /// </summary>
    internal class PostMuseFeedbackV1RequestBuilder
    {
        internal readonly Feedback Feedback;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/feedback
        /// </summary>
        public PostMuseFeedbackV1RequestBuilder(IReadableConfiguration config, IClient apiClient, Feedback requestBody)
        {
            Configuration = config;
            Client = apiClient;


            Feedback = requestBody;
        }

        public PostMuseFeedbackV1Request Build() => new PostMuseFeedbackV1Request(this);


        public async Task<ApiResponse<ErrorResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseFeedbackV1Request
    {
        Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseFeedbackV1Request : IPostMuseFeedbackV1Request
    {
        PostMuseFeedbackV1RequestBuilder m_Builder;

        public PostMuseFeedbackV1Request(PostMuseFeedbackV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ErrorResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseFeedbackV1Async(m_Builder.Feedback, cancellationToken, callbacks);
        }

        /// <summary>
        /// Feedback Provide feedback.  Args:     request (Request): FastAPI request object.     body (Feedback): Feedback request body.     user_info (UserInfo): User information extracted from bearer token.  Returns:     Optional[JSONResponse]: Nothing if successful, otherwise JSONResponse with error.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="feedback"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ErrorResponse)</returns>
         async Task<ApiResponse<ErrorResponse>> PostMuseFeedbackV1Async(Feedback feedback, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'feedback' is set
            if (feedback == null)
                throw new ApiException(400, "Missing required parameter 'feedback' when calling MuseChatBackendApi->PostMuseFeedbackV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = feedback;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<ErrorResponse>("/v1/muse/feedback", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/inspiration/
    /// </summary>
    internal class PostMuseInspirationRequestBuilder
    {
        internal readonly Inspiration Inspiration;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/inspiration/
        /// </summary>
        public PostMuseInspirationRequestBuilder(IReadableConfiguration config, IClient apiClient, Inspiration requestBody)
        {
            Configuration = config;
            Client = apiClient;


            Inspiration = requestBody;
        }

        public PostMuseInspirationRequest Build() => new PostMuseInspirationRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ResponsePostMuseInspiration>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseInspirationRequest
    {
        Task<ApiResponse<ResponsePostMuseInspiration>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseInspirationRequest : IPostMuseInspirationRequest
    {
        PostMuseInspirationRequestBuilder m_Builder;

        public PostMuseInspirationRequest(PostMuseInspirationRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ResponsePostMuseInspiration>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseInspirationAsync(m_Builder.Inspiration, cancellationToken, callbacks);
        }

        /// <summary>
        /// Create Inspiration Create a new inspiration in the database.  Args:     request: FastAPI request object.     body: Inspiration object to create.  Returns: Created inspiration or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspiration"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponsePostMuseInspiration)</returns>
        [Obsolete]
         async Task<ApiResponse<ResponsePostMuseInspiration>> PostMuseInspirationAsync(Inspiration inspiration, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspiration' is set
            if (inspiration == null)
                throw new ApiException(400, "Missing required parameter 'inspiration' when calling MuseChatBackendApi->PostMuseInspiration");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = inspiration;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<ResponsePostMuseInspiration>("/muse/inspiration/", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/inspiration/
    /// </summary>
    internal class PostMuseInspirationV1RequestBuilder
    {
        internal readonly Inspiration Inspiration;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/inspiration/
        /// </summary>
        public PostMuseInspirationV1RequestBuilder(IReadableConfiguration config, IClient apiClient, Inspiration requestBody)
        {
            Configuration = config;
            Client = apiClient;


            Inspiration = requestBody;
        }

        public PostMuseInspirationV1Request Build() => new PostMuseInspirationV1Request(this);


        public async Task<ApiResponse<ResponsePostMuseInspirationV1>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseInspirationV1Request
    {
        Task<ApiResponse<ResponsePostMuseInspirationV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseInspirationV1Request : IPostMuseInspirationV1Request
    {
        PostMuseInspirationV1RequestBuilder m_Builder;

        public PostMuseInspirationV1Request(PostMuseInspirationV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ResponsePostMuseInspirationV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseInspirationV1Async(m_Builder.Inspiration, cancellationToken, callbacks);
        }

        /// <summary>
        /// Create Inspiration Create a new inspiration in the database.  Args:     request: FastAPI request object.     body: Inspiration object to create.  Returns: Created inspiration or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspiration"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponsePostMuseInspirationV1)</returns>
         async Task<ApiResponse<ResponsePostMuseInspirationV1>> PostMuseInspirationV1Async(Inspiration inspiration, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspiration' is set
            if (inspiration == null)
                throw new ApiException(400, "Missing required parameter 'inspiration' when calling MuseChatBackendApi->PostMuseInspirationV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = inspiration;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<ResponsePostMuseInspirationV1>("/v1/muse/inspiration/", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/opt
    /// </summary>
    internal class PostMuseOptRequestBuilder
    {
        internal readonly OptRequest OptRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/opt
        /// </summary>
        public PostMuseOptRequestBuilder(IReadableConfiguration config, IClient apiClient, OptRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            OptRequest = requestBody;
        }

        public PostMuseOptRequest Build() => new PostMuseOptRequest(this);

        [Obsolete]
        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseOptRequest
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseOptRequest : IPostMuseOptRequest
    {
        PostMuseOptRequestBuilder m_Builder;

        public PostMuseOptRequest(PostMuseOptRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseOptAsync(m_Builder.OptRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Opt Opt in or out of model training.  Notes:     This is ideally a temporary solution. :)  Args:     request (Request): _description_     body (OptRequest): _description_     user_info (UserInfo, optional): _description_. Defaults to Depends(extract_user_info).
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="optRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        [Obsolete]
         async Task<ApiResponse<Object>> PostMuseOptAsync(OptRequest optRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'optRequest' is set
            if (optRequest == null)
                throw new ApiException(400, "Missing required parameter 'optRequest' when calling MuseChatBackendApi->PostMuseOpt");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = optRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/muse/opt", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/opt
    /// </summary>
    internal class PostMuseOptV1RequestBuilder
    {
        internal readonly OptRequest OptRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/opt
        /// </summary>
        public PostMuseOptV1RequestBuilder(IReadableConfiguration config, IClient apiClient, OptRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            OptRequest = requestBody;
        }

        public PostMuseOptV1Request Build() => new PostMuseOptV1Request(this);


        public async Task<ApiResponse<Object>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostMuseOptV1Request
    {
        Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostMuseOptV1Request : IPostMuseOptV1Request
    {
        PostMuseOptV1RequestBuilder m_Builder;

        public PostMuseOptV1Request(PostMuseOptV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<Object>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostMuseOptV1Async(m_Builder.OptRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Opt Opt in or out of model training.  Notes:     This is ideally a temporary solution. :)  Args:     request (Request): _description_     body (OptRequest): _description_     user_info (UserInfo, optional): _description_. Defaults to Depends(extract_user_info).
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="optRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (Object)</returns>
         async Task<ApiResponse<Object>> PostMuseOptV1Async(OptRequest optRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'optRequest' is set
            if (optRequest == null)
                throw new ApiException(400, "Missing required parameter 'optRequest' when calling MuseChatBackendApi->PostMuseOptV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = optRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<Object>("/v1/muse/opt", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /smart-context
    /// </summary>
    internal class PostSmartContextRequestBuilder
    {
        internal readonly SmartContextRequest SmartContextRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /smart-context
        /// </summary>
        public PostSmartContextRequestBuilder(IReadableConfiguration config, IClient apiClient, SmartContextRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            SmartContextRequest = requestBody;
        }

        public PostSmartContextRequest Build() => new PostSmartContextRequest(this);

        [Obsolete]
        public async Task<ApiResponse<SmartContextResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostSmartContextRequest
    {
        Task<ApiResponse<SmartContextResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostSmartContextRequest : IPostSmartContextRequest
    {
        PostSmartContextRequestBuilder m_Builder;

        public PostSmartContextRequest(PostSmartContextRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<SmartContextResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostSmartContextAsync(m_Builder.SmartContextRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Smart Context Handle smart context requests.  Args:     request (Request): FastAPI request object.     body (SmartContextRequest): Smart context request body.  Returns:     SmartContextResponse | JSONResponse:         Either smart context response or JSON error message.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="smartContextRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (SmartContextResponse)</returns>
        [Obsolete]
         async Task<ApiResponse<SmartContextResponse>> PostSmartContextAsync(SmartContextRequest smartContextRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'smartContextRequest' is set
            if (smartContextRequest == null)
                throw new ApiException(400, "Missing required parameter 'smartContextRequest' when calling MuseChatBackendApi->PostSmartContext");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = smartContextRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<SmartContextResponse>("/smart-context", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/smart-context
    /// </summary>
    internal class PostSmartContextV1RequestBuilder
    {
        internal readonly SmartContextRequest SmartContextRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/smart-context
        /// </summary>
        public PostSmartContextV1RequestBuilder(IReadableConfiguration config, IClient apiClient, SmartContextRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            SmartContextRequest = requestBody;
        }

        public PostSmartContextV1Request Build() => new PostSmartContextV1Request(this);


        public async Task<ApiResponse<SmartContextResponse>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPostSmartContextV1Request
    {
        Task<ApiResponse<SmartContextResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PostSmartContextV1Request : IPostSmartContextV1Request
    {
        PostSmartContextV1RequestBuilder m_Builder;

        public PostSmartContextV1Request(PostSmartContextV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<SmartContextResponse>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PostSmartContextV1Async(m_Builder.SmartContextRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Smart Context Handle smart context requests.  Args:     request (Request): FastAPI request object.     body (SmartContextRequest): Smart context request body.  Returns:     SmartContextResponse | JSONResponse:         Either smart context response or JSON error message.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="smartContextRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (SmartContextResponse)</returns>
         async Task<ApiResponse<SmartContextResponse>> PostSmartContextV1Async(SmartContextRequest smartContextRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'smartContextRequest' is set
            if (smartContextRequest == null)
                throw new ApiException(400, "Missing required parameter 'smartContextRequest' when calling MuseChatBackendApi->PostSmartContextV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.Data = smartContextRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PostAsync<SmartContextResponse>("/v1/smart-context", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /muse/inspiration/{inspiration_id}
    /// </summary>
    internal class PutMuseInspirationUsingInspirationIdRequestBuilder
    {
        internal readonly string InspirationId;
        internal readonly UpdateInspirationRequest UpdateInspirationRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /muse/inspiration/{inspiration_id}
        /// </summary>
        public PutMuseInspirationUsingInspirationIdRequestBuilder(IReadableConfiguration config, IClient apiClient, string inspirationId, UpdateInspirationRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            InspirationId = inspirationId;
            UpdateInspirationRequest = requestBody;
        }

        public PutMuseInspirationUsingInspirationIdRequest Build() => new PutMuseInspirationUsingInspirationIdRequest(this);

        [Obsolete]
        public async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationId>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPutMuseInspirationUsingInspirationIdRequest
    {
        Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PutMuseInspirationUsingInspirationIdRequest : IPutMuseInspirationUsingInspirationIdRequest
    {
        PutMuseInspirationUsingInspirationIdRequestBuilder m_Builder;

        public PutMuseInspirationUsingInspirationIdRequest(PutMuseInspirationUsingInspirationIdRequestBuilder builder)
        {
            m_Builder = builder;
        }
        [Obsolete]
        public async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationId>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PutMuseInspirationUsingInspirationIdAsync(m_Builder.InspirationId, m_Builder.UpdateInspirationRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Update Inspiration Update an existing inspiration in the database.  Args:     request: FastAPI request object.     inspiration_id: ID of the inspiration to update.     body: Updated inspiration object.  Returns: Updated inspiration or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspirationId"></param>
        /// <param name="updateInspirationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponsePutMuseInspirationUsingInspirationId)</returns>
        [Obsolete]
         async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationId>> PutMuseInspirationUsingInspirationIdAsync(string inspirationId, UpdateInspirationRequest updateInspirationRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspirationId' is set
            if (inspirationId == null)
                throw new ApiException(400, "Missing required parameter 'inspirationId' when calling MuseChatBackendApi->PutMuseInspirationUsingInspirationId");

            // verify the required parameter 'updateInspirationRequest' is set
            if (updateInspirationRequest == null)
                throw new ApiException(400, "Missing required parameter 'updateInspirationRequest' when calling MuseChatBackendApi->PutMuseInspirationUsingInspirationId");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("inspiration_id", ClientUtils.ParameterToString(inspirationId)); // path parameter
            localVarRequestOptions.Data = updateInspirationRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PutAsync<ResponsePutMuseInspirationUsingInspirationId>("/muse/inspiration/{inspiration_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }
    /// <summary>
    /// Used to build requests to call /v1/muse/inspiration/{inspiration_id}
    /// </summary>
    internal class PutMuseInspirationUsingInspirationIdV1RequestBuilder
    {
        internal readonly string InspirationId;
        internal readonly UpdateInspirationRequest UpdateInspirationRequest;

        internal readonly IReadableConfiguration Configuration;
        internal readonly IClient Client;

        /// <summary>
        /// Create builder to call /v1/muse/inspiration/{inspiration_id}
        /// </summary>
        public PutMuseInspirationUsingInspirationIdV1RequestBuilder(IReadableConfiguration config, IClient apiClient, string inspirationId, UpdateInspirationRequest requestBody)
        {
            Configuration = config;
            Client = apiClient;


            InspirationId = inspirationId;
            UpdateInspirationRequest = requestBody;
        }

        public PutMuseInspirationUsingInspirationIdV1Request Build() => new PutMuseInspirationUsingInspirationIdV1Request(this);


        public async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationIdV1>> BuildAndSendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await Build().SendAsync(cancellationToken, callbacks);
        }
    }

    internal interface IPutMuseInspirationUsingInspirationIdV1Request
    {
        Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null);
    }

    internal class PutMuseInspirationUsingInspirationIdV1Request : IPutMuseInspirationUsingInspirationIdV1Request
    {
        PutMuseInspirationUsingInspirationIdV1RequestBuilder m_Builder;

        public PutMuseInspirationUsingInspirationIdV1Request(PutMuseInspirationUsingInspirationIdV1RequestBuilder builder)
        {
            m_Builder = builder;
        }

        public async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationIdV1>> SendAsync(CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            return await PutMuseInspirationUsingInspirationIdV1Async(m_Builder.InspirationId, m_Builder.UpdateInspirationRequest, cancellationToken, callbacks);
        }

        /// <summary>
        /// Update Inspiration Update an existing inspiration in the database.  Args:     request: FastAPI request object.     inspiration_id: ID of the inspiration to update.     body: Updated inspiration object.  Returns: Updated inspiration or error response.
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="inspirationId"></param>
        /// <param name="updateInspirationRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <param name="callbacks">Callbacks that allow access to UnityWebRequest mid request</param>
        /// <returns>Task of ApiResponse (ResponsePutMuseInspirationUsingInspirationIdV1)</returns>
         async Task<ApiResponse<ResponsePutMuseInspirationUsingInspirationIdV1>> PutMuseInspirationUsingInspirationIdV1Async(string inspirationId, UpdateInspirationRequest updateInspirationRequest, CancellationToken cancellationToken = default, RequestInterceptionCallbacks callbacks = null)
        {
            // verify the required parameter 'inspirationId' is set
            if (inspirationId == null)
                throw new ApiException(400, "Missing required parameter 'inspirationId' when calling MuseChatBackendApi->PutMuseInspirationUsingInspirationIdV1");

            // verify the required parameter 'updateInspirationRequest' is set
            if (updateInspirationRequest == null)
                throw new ApiException(400, "Missing required parameter 'updateInspirationRequest' when calling MuseChatBackendApi->PutMuseInspirationUsingInspirationIdV1");


            RequestOptions localVarRequestOptions = new RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            localVarRequestOptions.PathParameters.Add("inspiration_id", ClientUtils.ParameterToString(inspirationId)); // path parameter
            localVarRequestOptions.Data = updateInspirationRequest;

            // authentication (APIKeyHeader) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.HeaderParameters.Add("access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token"));
            // authentication (HTTPBearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + m_Builder.Configuration.AccessToken);
            // authentication (APIKeyQuery) required
            if (!string.IsNullOrEmpty(m_Builder.Configuration.GetApiKeyWithPrefix("access_token")))
                localVarRequestOptions.QueryParameters.Add(ClientUtils.ParameterToMultiMap("", "access_token", m_Builder.Configuration.GetApiKeyWithPrefix("access_token")));

            // make the HTTP request
            var task = m_Builder.Client.PutAsync<ResponsePutMuseInspirationUsingInspirationIdV1>("/v1/muse/inspiration/{inspiration_id}", localVarRequestOptions, m_Builder.Configuration, cancellationToken, callbacks);

#if UNITY_EDITOR || !UNITY_WEBGL
            var localVarResponse = await task.ConfigureAwait(false);
#else
            var localVarResponse = await task;
#endif

            return localVarResponse;
        }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    internal class MuseChatBackendApi : IDisposable, IMuseChatBackendApi
    {
        IReadableConfiguration m_Configuration;
        IClient m_Client;

        public IClient Client => m_Client;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuseChatBackendApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <returns></returns>
        public MuseChatBackendApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuseChatBackendApi"/> class.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public MuseChatBackendApi(string basePath)
        {
            m_Configuration = Unity.Muse.Chat.BackendApi.Client.Configuration.MergeConfigurations(
                GlobalConfiguration.Instance,
                new Configuration { BasePath = basePath }
            );
            m_Client = new ApiClient(m_Configuration.BasePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuseChatBackendApi"/> class using Configuration object.
        /// **IMPORTANT** This will also create an instance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHandler</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public MuseChatBackendApi(Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            m_Configuration = Unity.Muse.Chat.BackendApi.Client.Configuration.MergeConfigurations(
                GlobalConfiguration.Instance,
                configuration
            );
            m_Client = new ApiClient(m_Configuration.BasePath);
        }

        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdBuilder(string conversationId, string fragmentId) => new(m_Configuration, m_Client, conversationId, fragmentId);
        public DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder DeleteMuseConversationFragmentUsingConversationIdAndFragmentIdV1Builder(string conversationId, string fragmentId) => new(m_Configuration, m_Client, conversationId, fragmentId);
        public DeleteMuseConversationUsingConversationIdRequestBuilder DeleteMuseConversationUsingConversationIdBuilder(string conversationId) => new(m_Configuration, m_Client, conversationId);
        public DeleteMuseConversationUsingConversationIdV1RequestBuilder DeleteMuseConversationUsingConversationIdV1Builder(string conversationId) => new(m_Configuration, m_Client, conversationId);
        public DeleteMuseConversationsByTagsRequestBuilder DeleteMuseConversationsByTagsBuilder() => new(m_Configuration, m_Client);
        public DeleteMuseConversationsByTagsV1RequestBuilder DeleteMuseConversationsByTagsV1Builder() => new(m_Configuration, m_Client);
        public DeleteMuseInspirationUsingInspirationIdRequestBuilder DeleteMuseInspirationUsingInspirationIdBuilder(string inspirationId) => new(m_Configuration, m_Client, inspirationId);
        public DeleteMuseInspirationUsingInspirationIdV1RequestBuilder DeleteMuseInspirationUsingInspirationIdV1Builder(string inspirationId) => new(m_Configuration, m_Client, inspirationId);
        public GetHealthRequestBuilder GetHealthBuilder() => new(m_Configuration, m_Client);
        public GetHealthzRequestBuilder GetHealthzBuilder() => new(m_Configuration, m_Client);
        public GetMuseBetaCheckEntitlementRequestBuilder GetMuseBetaCheckEntitlementBuilder() => new(m_Configuration, m_Client);
        public GetMuseBetaCheckEntitlementV1RequestBuilder GetMuseBetaCheckEntitlementV1Builder() => new(m_Configuration, m_Client);
        public GetMuseConversationRequestBuilder GetMuseConversationBuilder() => new(m_Configuration, m_Client);
        public GetMuseConversationUsingConversationIdRequestBuilder GetMuseConversationUsingConversationIdBuilder(string conversationId) => new(m_Configuration, m_Client, conversationId);
        public GetMuseConversationUsingConversationIdV1RequestBuilder GetMuseConversationUsingConversationIdV1Builder(string conversationId) => new(m_Configuration, m_Client, conversationId);
        public GetMuseConversationV1RequestBuilder GetMuseConversationV1Builder() => new(m_Configuration, m_Client);
        public GetMuseInspirationRequestBuilder GetMuseInspirationBuilder() => new(m_Configuration, m_Client);
        public GetMuseInspirationV1RequestBuilder GetMuseInspirationV1Builder() => new(m_Configuration, m_Client);
        public GetMuseOptRequestBuilder GetMuseOptBuilder() => new(m_Configuration, m_Client);
        public GetMuseOptV1RequestBuilder GetMuseOptV1Builder() => new(m_Configuration, m_Client);
        public GetMuseTopicUsingConversationIdRequestBuilder GetMuseTopicUsingConversationIdBuilder(string conversationId, string organizationId) => new(m_Configuration, m_Client, conversationId, organizationId);
        public GetMuseTopicUsingConversationIdV1RequestBuilder GetMuseTopicUsingConversationIdV1Builder(string conversationId, string organizationId) => new(m_Configuration, m_Client, conversationId, organizationId);
        public GetVersionsRequestBuilder GetVersionsBuilder() => new(m_Configuration, m_Client);
        public HeadHealthRequestBuilder HeadHealthBuilder() => new(m_Configuration, m_Client);
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdRequestBuilder PatchMuseConversationFragmentUsingConversationIdAndFragmentIdBuilder(string conversationId, string fragmentId, ConversationFragmentPatch requestBody) => new(m_Configuration, m_Client, conversationId, fragmentId, requestBody);
        public PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1RequestBuilder PatchMuseConversationFragmentUsingConversationIdAndFragmentIdV1Builder(string conversationId, string fragmentId, ConversationFragmentPatch requestBody) => new(m_Configuration, m_Client, conversationId, fragmentId, requestBody);
        public PatchMuseConversationUsingConversationIdRequestBuilder PatchMuseConversationUsingConversationIdBuilder(string conversationId, ConversationPatchRequest requestBody) => new(m_Configuration, m_Client, conversationId, requestBody);
        public PatchMuseConversationUsingConversationIdV1RequestBuilder PatchMuseConversationUsingConversationIdV1Builder(string conversationId, ConversationPatchRequest requestBody) => new(m_Configuration, m_Client, conversationId, requestBody);
        public PostMuseAgentActionRequestBuilder PostMuseAgentActionBuilder(ActionRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseAgentActionV1RequestBuilder PostMuseAgentActionV1Builder(ActionRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseAgentCodeRepairRequestBuilder PostMuseAgentCodeRepairBuilder(ActionCodeRepairRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseAgentCodeRepairV1RequestBuilder PostMuseAgentCodeRepairV1Builder(ActionCodeRepairRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseAgentCodegenRequestBuilder PostMuseAgentCodegenBuilder(CodeGenRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseAgentCodegenV1RequestBuilder PostMuseAgentCodegenV1Builder(CodeGenRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseChatRequestBuilder PostMuseChatBuilder(ChatRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseChatV1RequestBuilder PostMuseChatV1Builder(ChatRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseCompletionRequestBuilder PostMuseCompletionBuilder(ContextualCompletionRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseCompletionRepairRequestBuilder PostMuseCompletionRepairBuilder(CompletionRepairRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseCompletionRepairV1RequestBuilder PostMuseCompletionRepairV1Builder(CompletionRepairRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseCompletionV1RequestBuilder PostMuseCompletionV1Builder(ContextualCompletionRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseConversationRequestBuilder PostMuseConversationBuilder(CreateConversationRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseConversationV1RequestBuilder PostMuseConversationV1Builder(CreateConversationRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseFeedbackRequestBuilder PostMuseFeedbackBuilder(Feedback requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseFeedbackV1RequestBuilder PostMuseFeedbackV1Builder(Feedback requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseInspirationRequestBuilder PostMuseInspirationBuilder(Inspiration requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseInspirationV1RequestBuilder PostMuseInspirationV1Builder(Inspiration requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseOptRequestBuilder PostMuseOptBuilder(OptRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostMuseOptV1RequestBuilder PostMuseOptV1Builder(OptRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostSmartContextRequestBuilder PostSmartContextBuilder(SmartContextRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PostSmartContextV1RequestBuilder PostSmartContextV1Builder(SmartContextRequest requestBody) => new(m_Configuration, m_Client, requestBody);
        public PutMuseInspirationUsingInspirationIdRequestBuilder PutMuseInspirationUsingInspirationIdBuilder(string inspirationId, UpdateInspirationRequest requestBody) => new(m_Configuration, m_Client, inspirationId, requestBody);
        public PutMuseInspirationUsingInspirationIdV1RequestBuilder PutMuseInspirationUsingInspirationIdV1Builder(string inspirationId, UpdateInspirationRequest requestBody) => new(m_Configuration, m_Client, inspirationId, requestBody);

        /// <summary>
        /// Disposes resources if they were created by us
        /// </summary>
        public void Dispose()
        {
            m_Client?.Dispose();
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return m_Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public IReadableConfiguration Configuration { get; set; }
    }
}

namespace Unity.Muse.Chat.BackendApi.Utilities
{
    internal interface IUnityWebRequest : IDisposable
    {
        // Basic properties
        string url { get; set; }
        string method { get; set; }
        string error { get; }
        bool isDone { get; }
        bool isNetworkError { get; }
        bool isHttpError { get; }
        long responseCode { get; }

        // Upload/Download properties
        float uploadProgress { get; }
        float downloadProgress { get; }
        ulong uploadedBytes { get; }
        ulong downloadedBytes { get; }

        // Handlers
        IUploadHandler uploadHandler { get; set; }
        IDownloadHandler downloadHandler { get; set; }
        ICertificateHandler certificateHandler { get; set; }

        // Configuration
        int timeout { get; set; }
        int redirectLimit { get; set; }
        bool useHttpContinue { get; set; }
        bool disposeDownloadHandlerOnDispose { get; set; }
        bool disposeUploadHandlerOnDispose { get; set; }

        // Methods
        void SetRequestHeader(string name, string value);
        string GetRequestHeader(string name);
        string GetResponseHeader(string name);
        Dictionary<string, string> GetResponseHeaders();

        UnityWebRequestAsyncOperation SendWebRequest();
        void Abort();
    }

    internal interface IDownloadHandler : IDisposable
    {
        byte[] data { get; }
        string text { get; }
        NativeArray<byte>.ReadOnly nativeData { get; }
        bool isDone { get; }
        string error { get; }
    }

    internal interface IUploadHandler : IDisposable
    {
        string contentType { get; set; }
        byte[] data { get; }
        float progress { get; }
    }

    internal interface ICertificateHandler : IDisposable { }

    internal class UnityWebRequestWrapper : IUnityWebRequest
    {
        private readonly UnityWebRequest _request;
        private IDownloadHandler _downloadHandlerWrapper;
        private IUploadHandler _uploadHandlerWrapper;
        private ICertificateHandler _certificateHandlerWrapper;

        public UnityWebRequestWrapper(UnityWebRequest request)
        {
            _request = request;
            WrapHandlers();
        }

        private void WrapHandlers()
        {
            if (_request.downloadHandler != null)
                _downloadHandlerWrapper = new DownloadHandlerWrapper(_request.downloadHandler);

            if (_request.uploadHandler != null)
                _uploadHandlerWrapper = new UploadHandlerWrapper(_request.uploadHandler);

            if (_request.certificateHandler != null)
                _certificateHandlerWrapper = new CertificateHandlerWrapper(_request.certificateHandler);
        }

        // Basic properties
        public string url
        {
            get => _request.url;
            set => _request.url = value;
        }

        public string method
        {
            get => _request.method;
            set => _request.method = value;
        }

        public string error => _request.error;
        public bool isDone => _request.isDone;

        [Obsolete("UnityWebRequest.isNetworkError is deprecated. Use (UnityWebRequest.result == UnityWebRequest.Result.ConnectionError) instead.")]
        public bool isNetworkError => _request.isNetworkError;

        [Obsolete("UnityWebRequest.isHttpError is deprecated. Use (UnityWebRequest.result == UnityWebRequest.Result.ProtocolError) instead.")]
        public bool isHttpError => _request.isHttpError;
        public long responseCode => (long)_request.responseCode;

        // Upload/Download properties
        public float uploadProgress => _request.uploadProgress;
        public float downloadProgress => _request.downloadProgress;
        public ulong uploadedBytes => _request.uploadedBytes;
        public ulong downloadedBytes => _request.downloadedBytes;

        // Handlers using interfaces
        public IUploadHandler uploadHandler
        {
            get => _uploadHandlerWrapper;
            set
            {
                if (value is UploadHandlerWrapper wrapper)
                {
                    _uploadHandlerWrapper = wrapper;
                    _request.uploadHandler = wrapper.UploadHandler;
                }
                else
                {
                    throw new System.ArgumentException("Upload handler must be of type UploadHandlerWrapper");
                }
            }
        }

        public IDownloadHandler downloadHandler
        {
            get => _downloadHandlerWrapper;
            set
            {
                if (value is DownloadHandlerWrapper wrapper)
                {
                    _downloadHandlerWrapper = wrapper;
                    _request.downloadHandler = wrapper.DownloadHandler;
                }
                else
                {
                    throw new System.ArgumentException("Download handler must be of type DownloadHandlerWrapper");
                }
            }
        }

        public ICertificateHandler certificateHandler
        {
            get => _certificateHandlerWrapper;
            set
            {
                if (value is CertificateHandlerWrapper wrapper)
                {
                    _certificateHandlerWrapper = wrapper;
                    _request.certificateHandler = wrapper.CertificateHandler;
                }
                else
                {
                    throw new System.ArgumentException("Certificate handler must be of type CertificateHandlerWrapper");
                }
            }
        }

        // Configuration
        public int timeout
        {
            get => _request.timeout;
            set => _request.timeout = value;
        }

        public int redirectLimit
        {
            get => _request.redirectLimit;
            set => _request.redirectLimit = value;
        }

        public bool useHttpContinue
        {
            get => _request.useHttpContinue;
            set => _request.useHttpContinue = value;
        }

        public bool disposeDownloadHandlerOnDispose
        {
            get => _request.disposeDownloadHandlerOnDispose;
            set => _request.disposeDownloadHandlerOnDispose = value;
        }

        public bool disposeUploadHandlerOnDispose
        {
            get => _request.disposeUploadHandlerOnDispose;
            set => _request.disposeUploadHandlerOnDispose = value;
        }

        // Methods
        public void SetRequestHeader(string name, string value)
        {
            _request.SetRequestHeader(name, value);
        }

        public string GetRequestHeader(string name)
        {
            return _request.GetRequestHeader(name);
        }

        public string GetResponseHeader(string name)
        {
            return _request.GetResponseHeader(name);
        }

        public Dictionary<string, string> GetResponseHeaders()
        {
            return _request.GetResponseHeaders();
        }

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return _request.SendWebRequest();
        }

        public void Abort()
        {
            _request.Abort();
        }

        public void Dispose()
        {
            _downloadHandlerWrapper?.Dispose();
            _uploadHandlerWrapper?.Dispose();
            _certificateHandlerWrapper?.Dispose();
            _request.Dispose();
        }
    }

    // Handler Wrappers
    internal class DownloadHandlerWrapper : IDownloadHandler
    {
        private readonly DownloadHandler _handler;
        public DownloadHandler DownloadHandler => _handler;

        public DownloadHandlerWrapper(DownloadHandler handler)
        {
            _handler = handler;
        }

        public byte[] data => _handler.data;
        public string text => _handler.text;

        public NativeArray<byte>.ReadOnly nativeData => _handler.nativeData;
        public bool isDone => _handler.isDone;

        public string error => _handler.error;

        public void Dispose()
        {
            _handler.Dispose();
        }
    }

    internal class UploadHandlerWrapper : IUploadHandler
    {
        private readonly UploadHandler _handler;
        public UploadHandler UploadHandler => _handler;

        public UploadHandlerWrapper(UploadHandler handler)
        {
            _handler = handler;
        }

        public string contentType
        {
            get => _handler.contentType;
            set => _handler.contentType = value;
        }
        public byte[] data => _handler.data;
        public float progress => _handler.progress;

        public void Dispose()
        {
            _handler.Dispose();
        }
    }

    internal class CertificateHandlerWrapper : ICertificateHandler
    {
        private readonly CertificateHandler _handler;
        public CertificateHandler CertificateHandler => _handler;

        public CertificateHandlerWrapper(CertificateHandler handler)
        {
            _handler = handler;
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
