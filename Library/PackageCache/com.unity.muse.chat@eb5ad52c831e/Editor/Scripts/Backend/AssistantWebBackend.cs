using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.WebApi;
using Unity.Muse.Chat.BackendApi.Model;
using UnityEngine;

namespace Unity.Muse.Chat
{
    class AssistantWebBackend : IAssistantBackend
    {
        static readonly TimeSpan k_CancellationTimeout = TimeSpan.FromSeconds(30);

        static CancellationToken GetTimeoutToken(CancellationToken token) =>
            CancellationTokenSource.CreateLinkedTokenSource(
                    token, new CancellationTokenSource(k_CancellationTimeout).Token)
                .Token;
        /// <summary>
        /// The WebAPI implementation used to communicate with the Muse Backend.
        /// </summary>
        readonly WebAPI k_WebApi = new();

        public bool SessionStatusTrackingEnabled => true;



        /// <summary>
        /// Starts a request to refresh the list of conversations available. This is non-blocking.
        /// </summary>
        public async Task<IEnumerable<MuseConversationInfo>> ConversationRefresh(
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var timeout = GetTimeoutToken(ct);

            var infos = await k_WebApi.GetConversations(timeout);
            ct.ThrowIfCancellationRequested();

            var projectTag = UnityDataUtils.GetProjectId();

            return infos.Select(
                info => new MuseConversationInfo
                {
                    Id = new(info.ConversationId),
                    Title = info.Title,
                    LastMessageTimestamp = info.LastMessageTimestamp,
                    IsContextual = IsContextual(info),
                    IsFavorite = info.IsFavorite != null && info.IsFavorite.Value
                });

            bool IsContextual(ConversationInfo c)
            {
                var projectId = c.Tags.FirstOrDefault(
                    tag => tag.StartsWith(MuseChatConstants.ProjectIdTagPrefix));
                return projectId is null || projectId == projectTag;
            }
        }

        /// <summary>
        /// Starts a webrequest that attempts to load the conversation with <see cref="conversationId"/>.
        /// </summary>
        /// <param name="conversationId">If not null or empty function acts as noop.</param>
        public async Task<MuseConversation> ConversationLoad(
            MuseConversationId conversationId,
            CancellationToken ct = default)
        {
            if (!conversationId.IsValid)
                throw new("Invalid conversation ID");

            var conversations = await k_WebApi.GetConversation(conversationId.Value, ct);

            return ConvertConversation(conversations);
        }

        /// <summary>
        /// Starts a webrequest that attempts to rename change the favorite state of a conversation with <see cref="conversationId"/>.
        /// </summary>
        /// <param name="conversationId">If not null or empty function acts as noop.</param>
        /// <param name="isFavorite">New favorite state of the conversation</param>
        public async Task ConversationFavoriteToggle(
            MuseConversationId conversationId,
            bool isFavorite,
            CancellationToken ct = default)
        {
            if (!conversationId.IsValid)
                throw new("Invalid conversation ID");

            await k_WebApi.SetConversationFavoriteState(conversationId.Value, isFavorite, ct);
        }

        public async Task<MuseConversationId> ConversationCreate(CancellationToken ct = default)
        {
            var conversation = await k_WebApi.PostConversation(
                MuseChatState.FunctionCache.AllFunctionDefinitions, ct);
            return new(conversation.Id);
        }

        /// <summary>
        /// Starts a webrequest that attempts to rename a conversation with <see cref="conversationId"/>.
        /// </summary>
        /// <param name="conversationId">If not null or empty function acts as noop.</param>
        /// <param name="newName">New name of the conversation</param>
        public async Task ConversationRename(
            MuseConversationId conversationId,
            string newName,
            CancellationToken ct = default)
        {
            if (!conversationId.IsValid)
                throw new("Invalid conversation ID");

            await k_WebApi.RenameConversation(conversationId.Value, newName, ct);
        }

        public async Task ConversationSetAutoTitle(
            MuseConversationId id,
            CancellationToken ct = default)
        {
            if (!id.IsValid)
                throw new("Invalid conversation ID");

            var suggestedTitle = await k_WebApi.GetConversationTitle(id.Value, ct);
            if (!string.IsNullOrEmpty(suggestedTitle))
            {
                await ConversationRename(id, suggestedTitle.Trim('"'), ct);
            }
        }

        /// <summary>
        /// Starts a webrequest that attempts to delete a conversation with <see cref="conversation"/>.
        /// </summary>
        /// <param name="conversation">If not null or empty function acts as noop.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        public async Task ConversationDelete(
            MuseConversationInfo conversation,
            CancellationToken ct = default)
        {
            if (!conversation.Id.IsValid)
                throw new("Invalid conversation ID");

            await k_WebApi.DeleteConversation(conversation.Id.Value, ct);
        }

        public async Task ConversationDeleteFragment(
            MuseConversationId conversationId,
            string fragment,
            CancellationToken ct = default)
        {
            if (!conversationId.IsValid)
                throw new("Invalid conversation ID");

            await k_WebApi.DeleteConversationFragment(conversationId.Value, fragment, ct);
        }

        /// <summary>
        /// Starts a request to refresh the list of conversations available. This is non-blocking.
        /// </summary>
        public async Task<IEnumerable<MuseChatInspiration>> InspirationRefresh(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var result = await k_WebApi.GetInspirations(ct);

            ct.ThrowIfCancellationRequested();

            return result.Select(inspiration => inspiration.ToInternal());
        }

        /// <summary>
        /// Starts a webrequest that attempts to add or update a inspiration.
        /// </summary>
        /// <param name="inspiration">the inspiration data to update.</param>
        public async Task InspirationUpdate(MuseChatInspiration inspiration, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var externalData = inspiration.ToExternal();
            if (!inspiration.Id.IsValid)
            {
                externalData.Id = null;
                _ = await k_WebApi.AddInspiration(externalData, ct);
            }
            else
            {
                _ = await k_WebApi.UpdateInspiration(externalData, ct);
            }
        }

        /// <summary>
        /// Starts a webrequest that attempts to delete an inspiration.
        /// </summary>
        /// <param name="inspiration">the inspiration data to delete.</param>
        public async Task InspirationDelete(
            MuseChatInspiration inspiration,
            CancellationToken ct = default)
        {
            var tsc = new TaskCompletionSource<bool>();

            var externalData = inspiration.ToExternal();
            if (!inspiration.Id.IsValid)
                throw new InvalidOperationException(
                    "Tried to delete non-existing Inspiration entry!");

            await k_WebApi.DeleteInspiration(externalData, ct);
        }

        public Task SendFeedback(
            MuseConversationId conversationId,
            MessageFeedback feedback,
            CancellationToken ct = default) =>
            k_WebApi.SendFeedback(
                feedback.Message, conversationId.Value, feedback.MessageId.FragmentId,
                feedback.Sentiment, feedback.Type, ct);

        public Task<bool> CheckEntitlement(CancellationToken ct = default) =>
            k_WebApi.CheckBetaEntitlement(ct);

public Task<MuseChatStreamHandler> SendPrompt(MuseConversationId conversationId, string prompt, EditorContextReport context, string command, List<MuseChatContextEntry> selectionContext, CancellationToken ct = default)

        {
            var stream = k_WebApi.BuildChatStream(
                prompt,
                conversationId.Value,
                context,
                command,
                selectionContext: ToExternalContext(selectionContext)
            );

            return Task.FromResult(stream);
        }

        public async Task<SmartContextResponse> SendSmartContext(MuseConversationId conversationId, string prompt, EditorContextReport context, CancellationToken ct = default)
        {
            var timeout = new CancellationTokenSource(k_CancellationTimeout);
            var merged = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

            return await k_WebApi.PostSmartContextAsync(prompt, context,
                MuseChatState.SmartContextToolbox.Tools.Select(c => c.FunctionDefinition).ToList(),
                conversationId.Value,
                merged.Token);
        }

        public async Task<object> RepairCode(MuseConversationId conversationId, int messageIndex, string errorToRepair, string scriptToRepair, ScriptType scriptType, CancellationToken ct = default)
        {
            var timeout = new CancellationTokenSource(k_CancellationTimeout);
            var merged = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

            return await k_WebApi.CodeRepair(conversationID: conversationId.Value,
                messageIndex: messageIndex,
                errorToRepair: errorToRepair,
                scriptToRepair: scriptToRepair,
                ct: merged.Token,
                scriptType: scriptType);
        }

        public async Task<object> RepairCompletion(MuseConversationId conversationId, int messageIndex, string errorToRepair, string itemToRepair, ProductEnum product, CancellationToken ct = default)
        {
            var timeout = new CancellationTokenSource(k_CancellationTimeout);
            var merged = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);

            return await k_WebApi.CompletionRepair(conversationID: conversationId.Value,
                messageIndex: messageIndex,
                errorToRepair: errorToRepair,
                itemToRepair: itemToRepair,
                ct: merged.Token,
                product: product);
        }

        public async Task<List<VersionSupportInfo>> GetVersionSupportInfo(string version, CancellationToken ct = default)
        {
            var response = await k_WebApi.GetServerCompatibility(version);

            if (response.StatusCode == HttpStatusCode.OK)
                return response.Data;
            else
                return null;
        }

        MuseConversation ConvertConversation(ClientConversation remoteConversation)
        {
            var conversationId = new MuseConversationId(remoteConversation.Id);
            MuseConversation localConversation = new()
            {
                Id = conversationId,
                Title = remoteConversation.Title
            };

            for (var i = 0; i < remoteConversation.History.Count; i++)
            {
                var fragment = remoteConversation.History[i];
                var message = new MuseMessage
                {
                    Id = new(conversationId, fragment.Id, MuseMessageIdType.External),
                    IsComplete = true,
                    Role = fragment.Role,
                    Author = fragment.Author,
                    Content = fragment.Content,
                    Timestamp = fragment.Timestamp,
                    Context = ConvertSelectionContextToInternal(fragment.SelectedContextMetadata),
                    MessageIndex = i
                };

                localConversation.Messages.Add(message);
            }

            return localConversation;
        }

        public static MuseChatContextEntry[] ConvertSelectionContextToInternal(List<SelectedContextMetadataItems> context)
        {
            if (context == null || context.Count == 0)
            {
                return Array.Empty<MuseChatContextEntry>();
            }

            var result = new MuseChatContextEntry[context.Count];
            for (var i = 0; i < context.Count; i++)
            {
                var entry = context[i];
                if (entry.EntryType == null)
                {
                    // Invalid entry
                    Debug.LogError("Invalid Selection Context Entry");
                    continue;
                }

                var entryType = (MuseChatContextType)entry.EntryType;
                switch (entryType)
                {
                    case MuseChatContextType.ConsoleMessage:
                    {
                        result[i] = new MuseChatContextEntry
                        {
                            EntryType = MuseChatContextType.ConsoleMessage,
                            Value = entry.Value,
                            ValueType = entry.ValueType
                        };

                        break;
                    }

                    default:
                    {
                        result[i] = new()
                        {
                            Value = entry.Value,
                            DisplayValue = entry.DisplayValue,
                            EntryType = entryType,
                            ValueType = entry.ValueType,
                            ValueIndex = entry.ValueIndex ?? 0
                        };

                        break;
                    }
                }
            }

            return result;
        }

        static List<SelectedContextMetadataItems> ToExternalContext(List<MuseChatContextEntry> internalContext)
        {
            if (internalContext == null || internalContext.Count == 0)
            {
                return null;
            }

            var result = new List<SelectedContextMetadataItems>();
            for (var i = 0; i < internalContext.Count; i++)
            {
                var entry = internalContext[i];
                result.Add(new SelectedContextMetadataItems
                {
                    DisplayValue = entry.DisplayValue,
                    EntryType = (int)entry.EntryType,
                    Value = entry.Value,
                    ValueIndex = entry.ValueIndex,
                    ValueType = entry.ValueType
                });
            }

            return result;
        }
    }
}
