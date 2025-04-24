using System;
using System.Collections.Generic;

namespace Unity.Muse.Chat
{
    internal partial class Assistant
    {
        readonly List<MuseConversationInfo> k_History = new();

        /// <summary>
        /// Indicates that the history has changed
        /// </summary>
        public event Action OnConversationHistoryChanged;

        internal List<MuseConversationInfo> History => k_History;

        void OnConversationHistoryReceived(IEnumerable<MuseConversationInfo> historyData)
        {
            k_History.Clear();
            k_History.AddRange(historyData);

            OnConversationHistoryChanged?.Invoke();
        }
    }
}
