using System;
using System.Diagnostics;

namespace Unity.Muse.Chat
{
    [DebuggerDisplay("{Type}:{ConversationId} #{FragmentId}")]
    internal struct MuseMessageId
    {
        private const string k_InternalIdPrefix = "INT_";
        private const string k_IncompletePrefix = "INC_";

        private static int k_NextInternalId = 1;
        private static int k_NextIncompleteId = 1;

        internal static readonly MuseMessageId Invalid = default;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public MuseMessageId(MuseConversationId conversationId, string fragmentId, MuseMessageIdType type)
        {
            ConversationId = conversationId;
            FragmentId = fragmentId;
            Type = type;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public readonly MuseConversationId ConversationId;
        public readonly string FragmentId;
        public readonly MuseMessageIdType Type;

        public static MuseMessageId GetNextInternalId(MuseConversationId conversationId)
        {
            return new MuseMessageId(conversationId, $"{k_InternalIdPrefix}{k_NextInternalId++}", MuseMessageIdType.Internal);
        }

        public static MuseMessageId GetNextIncompleteId(MuseConversationId conversationId)
        {
            return new MuseMessageId(conversationId, $"{k_IncompletePrefix}{k_NextIncompleteId++}", MuseMessageIdType.Incomplete);
        }

        public static bool operator ==(MuseMessageId value1, MuseMessageId value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(MuseMessageId value1, MuseMessageId value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return obj is MuseMessageId other && Equals(other);
        }

        public bool Equals(MuseMessageId other)
        {
            return Type == other.Type && ConversationId == other.ConversationId && FragmentId == other.FragmentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, ConversationId, FragmentId ?? string.Empty);
        }

        public override string ToString()
        {
            return $"{Type}:{ConversationId} #{FragmentId ?? string.Empty}";
        }
    }
}
