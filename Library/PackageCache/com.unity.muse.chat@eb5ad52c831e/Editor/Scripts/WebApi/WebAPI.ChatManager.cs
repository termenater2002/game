using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Commands;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class ChatManager : IChatManager
        {
            readonly IMuseChatBackendApi k_Api;
            readonly IOrganizationIdProvider k_OrganizationIdProvider;

            public ChatManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public MuseChatStreamHandler BuildChatStream(
                string prompt,
                string conversationID = "",
                EditorContextReport context = null,
                string chatCommand = AskCommand.k_CommandName,
                Dictionary<string, string> extraBody = null,
                List<SelectedContextMetadataItems> selectionContext = null)
            {
                if (!k_OrganizationIdProvider.GetOrganizationId(out string organizationId))
                    throw new("No valid organization found.");

                if (!ChatCommands.TryGetCommandHandler(chatCommand, out var handler))
                    throw new Exception($"Invalid chat command: {chatCommand} specified!");

                object options;
                var handlerRoute = ChatCommands.GetServerRoute(handler);

                switch (handlerRoute)
                {
#if ENABLE_ASSISTANT_BETA_FEATURES
                case ChatCommands.ServerRoute.action:
                    options = new ActionRequest(organizationId, prompt, true)
                    {
                        Context = context == null ? null : new BackendApi.Model.Context(context),
                        ConversationId =
 string.IsNullOrWhiteSpace(conversationID) ? null : conversationID,
                        StreamResponse = false,
                        DependencyInformation = UnityDataUtils.GetPackageMap(),
                        ProjectSummary = UnityDataUtils.GetProjectSettingSummary(),
                        UnityVersions = k_UnityVersionField.ToList(),
                        SelectedContextMetadata = selectionContext,
                        Tags = new List<string>(new[] { UnityDataUtils.GetProjectId() }),
                        Debug = false,
                        ExtraBody = extraBody
                    };
                    break;
                case ChatCommands.ServerRoute.codeGen:
                    options = new CodeGenRequest(organizationId, prompt, true)
                    {
                        Context = context == null ? null : new BackendApi.Model.Context(context),
                        ConversationId =
 string.IsNullOrWhiteSpace(conversationID) ? null : conversationID,
                        DependencyInformation = UnityDataUtils.GetPackageMap(),
                        ProjectSummary = UnityDataUtils.GetProjectSettingSummary(),
                        UnityVersions = k_UnityVersionField.ToList(),
                        SelectedContextMetadata = selectionContext,
                        Tags = new List<string>(new[] { UnityDataUtils.GetProjectId() }),
                        Debug = false,
                        ExtraBody = extraBody
                    };
                    break;
                case ChatCommands.ServerRoute.completion:
                    options = new ContextualCompletionRequest(organizationId, prompt, true)
                    {
                        Context = context == null ? null : new BackendApi.Model.Context(context),
                        SelectedContextMetadata = selectionContext,
                        StreamResponse = false,
                        ConversationId = string.IsNullOrWhiteSpace(conversationID) ? null : conversationID,
                        Tags = new List<string>(new[] {UnityDataUtils.GetProjectId() }),
                        Product = Enum.Parse<ProductEnum>(handler.Command, true),
                        ExtraBody = extraBody
                    };
                    break;
#endif
                    default:
                        options = new ChatRequest(organizationId, prompt, true)
                        {
                            Context =
                                context == null ? null : new BackendApi.Model.Context(context),
                            ConversationId =
                                string.IsNullOrWhiteSpace(conversationID)
                                    ? null
                                    : conversationID,
                            DependencyInformation = UnityDataUtils.GetPackageMap(),
                            ProjectSummary = UnityDataUtils.GetProjectSettingSummary(),
                            UnityVersions = k_UnityVersionField.ToList(),
                            SelectedContextMetadata = selectionContext,
                            Tags = new List<string>(new[] {UnityDataUtils.GetProjectId()}),
                            ExtraBody = new Dictionary<string, object>
                            {
                                {"enable_plugins", true},
                                {"muse_guard", true},
                                {
                                    "mediation_system_prompt",
                                    string.IsNullOrWhiteSpace(
                                        MuseChatConstants.MediationPrompt)
                                        ? null
                                        : MuseChatConstants.MediationPrompt
                                },
                                {"skip_planning", MuseChatConstants.SkipPlanning}
                            }
                        };
                        break;
                }

                MuseChatStreamHandler.MuseChatStreamRequestDelegate request = handlerRoute switch
                {
#if ENABLE_ASSISTANT_BETA_FEATURES
                ChatCommands.ServerRoute.action => k_Api.PostMuseAgentActionV1Builder(options as ActionRequest).Build().SendAsync,
                ChatCommands.ServerRoute.codeGen => k_Api.PostMuseAgentCodegenV1Builder(options as CodeGenRequest).Build().SendAsync,
                ChatCommands.ServerRoute.completion => k_Api.PostMuseCompletionV1Builder(options as ContextualCompletionRequest).Build().SendAsync,
#endif
                    _ => k_Api.PostMuseChatV1Builder(options as ChatRequest).Build().SendAsync,
                };

                MuseChatStreamHandler updateHandler = new(conversationID, request);
                return updateHandler;
            }
        }
    }
}
