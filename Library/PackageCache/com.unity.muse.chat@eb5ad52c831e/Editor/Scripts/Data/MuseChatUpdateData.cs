using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Unity.Muse.DeveloperTools.Editor.Tests")]
namespace Unity.Muse.Chat
{
    internal struct MuseChatUpdateData
    {
        public MuseChatUpdateType Type;
        public MuseMessage Message;
        public bool IsMusing;
        public MuseMessageId NewMessageId;
    }
}
