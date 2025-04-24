using System;
using System.Collections.Generic;

namespace Unity.Muse.Chat
{
    [Serializable]
    internal class MuseConversation
    {
        public string Title;
        public MuseConversationId Id;
        public readonly List<MuseMessage> Messages = new();

        [NonSerialized]
        public double StartTime;
    }
}
