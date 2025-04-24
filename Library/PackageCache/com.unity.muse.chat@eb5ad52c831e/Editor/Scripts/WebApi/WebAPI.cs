using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Commands;
using UnityEditor;

namespace Unity.Muse.Chat.WebApi
{
    static class ConfigurationUtility
    {
        public static void SetAccessToken([NotNull] this Configuration @this, string accessToken)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            @this.DefaultHeaders["Authorization"] = $"Bearer {accessToken}";
        }
    }

    partial class WebAPI
    {
        static readonly string[] k_UnityVersionField;

        static WebAPI()
        {
            k_UnityVersionField = new[]
            {
                UnityDataUtils.GetProjectVersion(UnityDataUtils.VersionDetail.Revision)
            };
        }

        internal static Configuration GetDefaultConfig()
        {
            Configuration config = new() {BasePath = MuseChatEnvironment.instance.ApiUrl};
            config.SetAccessToken(CloudProjectSettings.accessToken);
            return config;
        }

        IBetaEntitlementProvider m_BetaEntitlementProvider;
        IConversationManager m_ConversationManager;
        IRepairManager m_RepairManager;
        IFeedbackManager m_FeedbackManager;
        IInspirationManager m_InspirationManager;
        IServerCompatibilityProvider m_ServerCompatibilityProvider;
        ISmartContextManager m_SmartContextManager;
        IOrganizationIdProvider m_OrganizationIdProvider;
        IChatManager m_ChatManager;

        public WebAPI() : this(config: GetDefaultConfig())
        { }

        // ReSharper disable once MemberCanBePrivate.Global
        internal WebAPI(
            Configuration config = null,
            IMuseChatBackendApi api = null,
            IOrganizationIdProvider organizationIdProvider = null,
            IBetaEntitlementProvider betaEntitlementProvider = null,
            IConversationManager conversationManager = null,
            IRepairManager repairManager = null,
            IInspirationManager inspirationManager = null,
            IServerCompatibilityProvider serverCompatibilityProvider = null,
            ISmartContextManager smartContextManager = null,
            IChatManager chatManager = null,
            IFeedbackManager feedbackManager = null)
        {
            config ??= GetDefaultConfig();
            api ??= new MuseChatBackendApi(config);

            organizationIdProvider ??= new OrganizationIdProvider();

            betaEntitlementProvider ??= new BetaEntitlementProvider(api);
            conversationManager ??= new ConversationManager(api, organizationIdProvider);
            repairManager ??= new RepairManager(api, organizationIdProvider);
            inspirationManager ??= new InspirationManager(api, organizationIdProvider);
            serverCompatibilityProvider ??= new ServerCompatibilityProvider(api);
            smartContextManager ??= new SmartContextManager(api, organizationIdProvider);
            chatManager ??= new ChatManager(api, organizationIdProvider);
            feedbackManager ??= new FeedbackManager(api, organizationIdProvider);

            Init(
                betaEntitlementProvider, conversationManager, repairManager, inspirationManager,
                serverCompatibilityProvider, smartContextManager, chatManager, feedbackManager);
        }

        void Init(
            IBetaEntitlementProvider betaProvider,
            IConversationManager conversationManager,
            IRepairManager repairManager,
            IInspirationManager inspirationManager,
            IServerCompatibilityProvider serverCompatibilityManager,
            ISmartContextManager smartContextManager,
            IChatManager chatManager,
            IFeedbackManager feedbackManager)
        {
            m_BetaEntitlementProvider = betaProvider;
            m_ConversationManager = conversationManager;
            m_RepairManager = repairManager;
            m_InspirationManager = inspirationManager;
            m_ServerCompatibilityProvider = serverCompatibilityManager;
            m_SmartContextManager = smartContextManager;
            m_ChatManager = chatManager;
            m_FeedbackManager = feedbackManager;
        }

        public Task<bool> CheckBetaEntitlement(CancellationToken ct = default) =>
            m_BetaEntitlementProvider.CheckBetaEntitlement(ct);

        public MuseChatStreamHandler BuildChatStream(
            string prompt,
            string conversationID = "",
            EditorContextReport context = null,
            string chatCommand = AskCommand.k_CommandName,
            Dictionary<string, string> extraBody = null,
            List<SelectedContextMetadataItems> selectionContext = null) =>
            m_ChatManager.BuildChatStream(
                prompt, conversationID, context, chatCommand, extraBody, selectionContext);

        public Task<IEnumerable<Inspiration>> GetInspirations(CancellationToken ct = default) =>
            m_InspirationManager.GetInspirations(ct);

        public Task<Inspiration> AddInspiration(
            Inspiration inspiration,
            CancellationToken ct = default) =>
            m_InspirationManager.AddInspiration(inspiration, ct);

        public Task<Inspiration> UpdateInspiration(
            Inspiration inspiration,
            CancellationToken ct = default) =>
            m_InspirationManager.UpdateInspiration(inspiration, ct);

        public Task DeleteInspiration(Inspiration inspiration, CancellationToken ct = default) =>
            m_InspirationManager.DeleteInspiration(inspiration, ct);

        public Task<SmartContextResponse> PostSmartContextAsync(
            string prompt,
            EditorContextReport editorContext,
            List<FunctionDefinition> catalog,
            string conversationId,
            CancellationToken ct = default) =>
            m_SmartContextManager.PostSmartContextAsync(
                prompt, editorContext, catalog, conversationId, ct);

        public Task<ApiResponse<List<VersionSupportInfo>>> GetServerCompatibility(
            string version,
            CancellationToken ct = default) =>
            m_ServerCompatibilityProvider.GetServerCompatibility(version, ct);

        public Task SendFeedback(
            string text,
            string conversationID,
            string conversationFragmentId,
            Sentiment sentiment,
            Category feedbackType,
            CancellationToken ct = default) =>
            m_FeedbackManager.SendFeedback(
                text, conversationID, conversationFragmentId, sentiment, feedbackType, ct);

        public Task<IEnumerable<ConversationInfo>> GetConversations(
            CancellationToken ct = default) =>
            m_ConversationManager.GetConversations(ct);

        public Task<ClientConversation> GetConversation(
            string id,
            CancellationToken ct = default) =>
            m_ConversationManager.GetConversation(id, ct);

        public Task DeleteConversation(string id, CancellationToken ct = default) =>
            m_ConversationManager.DeleteConversation(id, ct);

        public Task<Conversation> PostConversation(
            List<FunctionDefinition> functions,
            CancellationToken ct = default) =>
            m_ConversationManager.PostConversation(functions, ct);

        public Task<string> GetConversationTitle(string id, CancellationToken ct = default) =>
            m_ConversationManager.GetConversationTitle(id, ct);

        public Task SetConversationFavoriteState(
            string id,
            bool favoriteState,
            CancellationToken ct = default) =>
            m_ConversationManager.SetConversationFavoriteState(id, favoriteState, ct);

        public Task RenameConversation(string id, string name, CancellationToken ct = default) =>
            m_ConversationManager.RenameConversation(id, name, ct);

        public Task DeleteConversationFragment(
            string conversationId,
            string fragmentId,
            CancellationToken ct = default) =>
            m_ConversationManager.DeleteConversationFragment(conversationId, fragmentId, ct);

        public Task<string> CodeRepair(
            int messageIndex,
            string errorToRepair,
            string scriptToRepair,
            CancellationToken ct = default,
            string conversationID = null,
            ScriptType scriptType = ScriptType.AgentAction,
            Dictionary<string, string> extraBody = null,
            string userPrompt = null) =>
            m_RepairManager.CodeRepair(
                messageIndex, errorToRepair, scriptToRepair, ct, conversationID, scriptType,
                extraBody, userPrompt);

        public Task<Object> CompletionRepair(
            int messageIndex,
            string errorToRepair,
            string itemToRepair,
            CancellationToken ct = default,
            string conversationID = null,
            ProductEnum product = ProductEnum.AiAssistant,
            Dictionary<string, string> extraBody = null,
            string userPrompt = null) =>
            m_RepairManager.CompletionRepair(
                messageIndex, errorToRepair, itemToRepair, ct, conversationID, product,
                extraBody, userPrompt);
    }
}
