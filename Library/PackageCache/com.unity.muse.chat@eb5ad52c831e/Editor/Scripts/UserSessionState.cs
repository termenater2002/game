using UnityEditor;
using UnityEngine;
using Unity.Muse.Chat.Commands;

namespace Unity.Muse.Chat
{
    internal class UserSessionState : ScriptableSingleton<UserSessionState>
    {
        const string k_HistoryOpen = "MuseChatUserSession_HistoryOpen";
        const string k_LastActiveConversationId = "MuseChatUserSession_LastActiveConversationId";
        const string k_DefaultCommandMode = AskCommand.k_CommandName;

        [SerializeField]
        public string SelectedCommandMode = k_DefaultCommandMode;

        public bool HasActiveSession => IsHistoryOpen || LastActiveConversationId != null;

        public ServerCompatibility.CompatibilityStatus CompatibilityStatus
            = ServerCompatibility.CompatibilityStatus.Undetermined;

        public bool IsHistoryOpen
        {
            get => SessionState.GetBool(k_HistoryOpen, false);
            set => SessionState.SetBool(k_HistoryOpen, value);
        }

        public string LastActiveConversationId
        {
            get => SessionState.GetString(k_LastActiveConversationId, null);
            set => SessionState.SetString(k_LastActiveConversationId, value);
        }

        /// <summary>
        /// Clears data that should survive domain reload.
        /// </summary>
        public void Clear()
        {
            IsHistoryOpen = false;
            LastActiveConversationId = null;
        }
    }
}
