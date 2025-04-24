namespace Unity.Muse.Chat
{
    internal struct MuseConversationInfo
    {
        public MuseConversationId Id;
        public string Title;
        public long LastMessageTimestamp;
        public bool IsContextual;
        public bool IsFavorite;
    }
}
