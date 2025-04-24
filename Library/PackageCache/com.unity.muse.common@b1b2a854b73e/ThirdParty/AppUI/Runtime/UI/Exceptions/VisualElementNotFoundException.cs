using System;
using System.Runtime.Serialization;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Exception thrown when a Visual Element hasn't been found.
    /// </summary>
    [Serializable]
    internal sealed class VisualElementNotFoundException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">THe exception message.</param>
        public VisualElementNotFoundException(string message) : base(message) { }

        VisualElementNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
