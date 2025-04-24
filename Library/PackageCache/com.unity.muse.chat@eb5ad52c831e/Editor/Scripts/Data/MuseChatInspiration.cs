using System;

namespace Unity.Muse.Chat
{
    [Serializable]
    internal struct MuseChatInspiration
    {
        public MuseInspirationId Id;
        public string Command;
        public string Description;
        public string Value;
    }
}
