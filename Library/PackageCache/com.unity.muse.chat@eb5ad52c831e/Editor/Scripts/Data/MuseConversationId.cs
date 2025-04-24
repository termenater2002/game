using System.Diagnostics;

namespace Unity.Muse.Chat
{
    [DebuggerDisplay("{k_Value}")]
    internal struct MuseConversationId
    {
        private const string k_InternalIdPrefix = "INT_";
        private static int s_NextInternalId = 1;

        private bool m_IsInternal;

        private readonly string k_Value;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public MuseConversationId(string value)
        {
            k_Value = value;
            m_IsInternal = false;
        }

        public static MuseConversationId GetNextInternalId()
        {
            return new MuseConversationId($"{k_InternalIdPrefix}{s_NextInternalId++}") { m_IsInternal = true };
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Value => m_IsInternal ? null : k_Value;

        public static bool operator ==(MuseConversationId value1, MuseConversationId value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(MuseConversationId value1, MuseConversationId value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return obj is MuseConversationId other && Equals(other);
        }

        private bool Equals(MuseConversationId other)
        {
            return k_Value == other.k_Value && m_IsInternal == other.m_IsInternal;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(k_Value) ? 0 : k_Value.GetHashCode();
        }

        public override string ToString()
        {
            return k_Value ?? string.Empty;
        }

        public bool IsValid => !string.IsNullOrEmpty(k_Value) && !m_IsInternal;
    }
}
