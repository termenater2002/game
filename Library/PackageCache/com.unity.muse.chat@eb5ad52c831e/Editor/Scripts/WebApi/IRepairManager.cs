using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    interface IRepairManager
    {
        Task<string> CodeRepair(
            int messageIndex,
            string errorToRepair,
            string scriptToRepair,
            CancellationToken ct = default,
            string conversationID = null,
            ScriptType scriptType = ScriptType.AgentAction,
            Dictionary<string, string> extraBody = null,
            string userPrompt = null);

        Task<object> CompletionRepair(
            int messageIndex,
            string errorToRepair,
            string itemToRepair,
            CancellationToken ct = default,
            string conversationID = null,
            ProductEnum product = ProductEnum.AiAssistant,
            Dictionary<string, string> extraBody = null,
            string userPrompt = null);
    }
}
