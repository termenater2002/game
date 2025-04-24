using System;

namespace Unity.Muse.Chat.UI.Utils
{
    interface IMuseChatHost
    {
        Action FocusLost { get; set; }
    }
}
