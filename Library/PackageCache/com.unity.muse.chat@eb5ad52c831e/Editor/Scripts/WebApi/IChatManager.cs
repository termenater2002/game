using System;
using System.Collections.Generic;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Commands;

namespace Unity.Muse.Chat.WebApi
{
    interface IChatManager
    {
        /// <summary>
        /// Builds a <see cref="MuseChatStreamHandler"/> for a chat prompt. This will not send the request immediately,
        /// but can be used to register to events that will occur during the streaming process.
        /// </summary>
        /// <param name="prompt">The chat prompt to send</param>
        /// <param name="conversationID">The conversationID, if this is an ongoing conversation</param>
        /// <param name="context">The context EditorContextModel</param>
        /// <param name="chatCommand">The type of command </param>
        /// <param name="extraBody">Extra body parameters to forward to the server</param>
        /// <param name="selectionContext">Context to add to the new prompt.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Throws if a valid organization is not found</exception>
        MuseChatStreamHandler BuildChatStream(
            string prompt,
            string conversationID = "",
            EditorContextReport context = null,
            string chatCommand = AskCommand.k_CommandName,
            Dictionary<string, string> extraBody = null,
            List<SelectedContextMetadataItems> selectionContext = null);
    }
}
