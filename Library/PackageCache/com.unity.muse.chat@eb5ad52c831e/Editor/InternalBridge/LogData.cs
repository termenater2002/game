using System;
using UnityEngine;

namespace Unity.Muse.Chat
{
    /// <summary>
    /// Stores relevant data from console logs
    /// </summary>
    [Serializable]
    internal struct LogData : IEquatable<LogData>
    {
        [SerializeField]
        public string Message;

        [SerializeField]
        public string File;

        [SerializeField]
        public int Line;

        [SerializeField]
        public int Column;

        [SerializeField]
        public LogDataType Type;

        public bool Equals(LogData other)
        {
            return Message == other.Message && File == other.File && Line == other.Line && Column == other.Column && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is LogData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Message, File, Line, Column, (int)Type);
        }
    }
}
