using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        readonly HashSet<MuseMessageId> k_MessagesUnderRepair = new();

        public bool IsUnderRepair(MuseMessageId messageId)
        {
            return k_MessagesUnderRepair.Contains(messageId);
        }

        /// <summary>
        /// Returns whether or not the given messageId can be sent for repair.
        /// Only the most recent message in the active message can be repaired.
        /// </summary>
        /// <param name="messageId">The id of the message to be repaired</param>
        /// <returns>True if a repair call can be sent for the given message, false otherwise</returns>
        public bool ValidRepairTarget(MuseMessageId messageId)
        {
            // Messages can be repaired if they are the last message in the conversation
            return (m_ActiveConversation.Messages.FindIndex(match => match.Id == messageId) == (m_ActiveConversation.Messages.Count - 1));
        }

        /// <summary>
        /// Repair the script with the given error and script.
        /// </summary>
        public async Task<string> RepairScript(MuseMessageId messageId, int messageIndex, string errorToRepair, string scriptToRepair, ScriptType scriptType = ScriptType.AgentAction)
        {
            // Add the message to the list of scripts under repair so it doesn't get repaired twice
            k_MessagesUnderRepair.Add(messageId);

            // Call the repair route and invoke OnCodeRepairComplete event when the repair is done
            CurrentPromptState = PromptState.RepairCode;

            OnDataChanged?.Invoke(new MuseChatUpdateData
            {
                IsMusing = true,
                Type = MuseChatUpdateType.CodeRepair
            });

            var repairedMessage = await m_Backend.RepairCode(messageId.ConversationId, messageIndex, errorToRepair, scriptToRepair, scriptType);
            OnDataChanged?.Invoke(new MuseChatUpdateData
            {
                IsMusing = false,
                Type = MuseChatUpdateType.CodeRepair
            });

            k_MessagesUnderRepair.Remove(messageId);
            return repairedMessage as string;
        }

        /// <summary>
        /// Repair the completion results with the given errors
        /// </summary>
        internal async Task<string> RepairCompletion(MuseMessageId messageId, int messageIndex, string errorToRepair, string itemToRepair, ProductEnum product)
        {
            // Add the message to the list of scripts under repair so it doesn't get repaired twice
            k_MessagesUnderRepair.Add(messageId);

            // Call the repair route and invoke OnCodeRepairComplete event when the repair is done
            CurrentPromptState = PromptState.RepairCode;

            OnDataChanged?.Invoke(new MuseChatUpdateData
            {
                IsMusing = true,
                Type = MuseChatUpdateType.CodeRepair
            });

            var repairedMessage = await m_Backend.RepairCompletion(messageId.ConversationId, messageIndex, errorToRepair,  itemToRepair, product);
            OnDataChanged?.Invoke(new MuseChatUpdateData
            {
                IsMusing = false,
                Type = MuseChatUpdateType.CodeRepair
            });

            k_MessagesUnderRepair.Remove(messageId);
            return ExtractContent(repairedMessage);
        }
    }
}
