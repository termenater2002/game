using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Api;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class RepairManager : IRepairManager
        {
            readonly IOrganizationIdProvider k_OrganizationIdProvider;
            readonly IMuseChatBackendApi k_Api;

            public RepairManager(
                IMuseChatBackendApi api,
                IOrganizationIdProvider organizationIdProvider)
            {
                k_Api = api;
                k_OrganizationIdProvider = organizationIdProvider;
            }

            public async Task<string> CodeRepair(
                int messageIndex,
                string errorToRepair,
                string scriptToRepair,
                CancellationToken ct = default,
                string conversationID = null,
                ScriptType scriptType = ScriptType.AgentAction,
                Dictionary<string, string> extraBody = null,
                string userPrompt = null)
            {
                ct.ThrowIfCancellationRequested();

                if (!k_OrganizationIdProvider.GetOrganizationId(out var organizationId))
                    throw new("No valid organization found.");

                var requestData =
                    new ActionCodeRepairRequest(
                        errorToRepair, messageIndex, organizationId, scriptToRepair, true)
                    {
                        ConversationId =
                            string.IsNullOrWhiteSpace(conversationID) ? null : conversationID,
                        Tags = new(new[] {UnityDataUtils.GetProjectId()}),
                        Debug = false,
                        UserPrompt = userPrompt,
                        ScriptType = scriptType,
                        ExtraBody = extraBody
                    };

                var result = await k_Api
                    .PostMuseAgentCodeRepairV1Builder(requestData)
                    .BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                return result.Data as string;
            }

            public async Task<Object> CompletionRepair(
                int messageIndex,
                string errorToRepair,
                string itemToRepair,
                CancellationToken ct = default,
                string conversationID = null,
                ProductEnum product = ProductEnum.AiAssistant,
                Dictionary<string, string> extraBody = null,
                string userPrompt = null)
            {
                if (!k_OrganizationIdProvider.GetOrganizationId(out string organizationId))
                    throw new Exception("No valid organization found.");

                var requestData =
                    new CompletionRepairRequest(
                        errorToRepair, itemToRepair, messageIndex, organizationId, true)
                {
                    ConversationId = string.IsNullOrWhiteSpace(conversationID) ? null : conversationID,
                    Tags = new List<string>(new[] { UnityDataUtils.GetProjectId() }),
                    Product = product,
                    Debug = false,
                    UserPrompt = userPrompt,
                    ExtraBody = extraBody
                };
                var result = await k_Api
                    .PostMuseCompletionRepairV1Builder(requestData)
                    .BuildAndSendAsync(ct);

                ct.ThrowIfCancellationRequested();

                return result.Data;
            }
        }
    }
}
