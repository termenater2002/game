using System.Collections.Generic;
using UnityEngine;

namespace Unity.Muse.Chat
{
    internal static class MuseChatSelection
    {
        [SerializeField]
        public static List<Object> ObjectSelection = new();

        [SerializeField]
        public static List<LogData> ConsoleSelection = new();
    }
}
