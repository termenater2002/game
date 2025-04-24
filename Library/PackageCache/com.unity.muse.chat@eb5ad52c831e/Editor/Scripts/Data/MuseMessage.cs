using System;
using Unity.Muse.Chat.Commands;

namespace Unity.Muse.Chat
{
    [Serializable]
    internal struct MuseMessage
    {
        const string k_completionTag = "ai assistant ";

        public MuseMessageId Id;
        public string Author;
        public string Role;
        public string Content;
        public MuseChatContextEntry[] Context;
        public bool IsComplete;
        public int ErrorCode;
        public string ErrorText;
        public bool IsError => ErrorCode != 0;
        public long Timestamp;
        public int MessageIndex;

        public readonly string GetChatCommand()
        {
            if (Author == null)
                return "";

            var author = Author.ToLower();
            if (author.Contains("agent"))
                return RunCommand.k_CommandName;

            if (author.Contains("codegen"))
                return CodeCommand.k_CommandName;

            if (author.StartsWith(k_completionTag))
            {
                var versionTagLocation = author.IndexOf("(v");
                if (versionTagLocation != -1)
                {
                    author = author.Substring(k_completionTag.Length, versionTagLocation - k_completionTag.Length).Trim();
                    return author;
                }
            }

            return author;
        }

        public readonly ChatCommandHandler GetChatCommandHandler()
        {
            if (!ChatCommands.TryGetCommandHandler(GetChatCommand(), out var handler))
            {
                ChatCommands.TryGetCommandHandler(AskCommand.k_CommandName, out handler);
            }
            return handler;
        }
    }
}
