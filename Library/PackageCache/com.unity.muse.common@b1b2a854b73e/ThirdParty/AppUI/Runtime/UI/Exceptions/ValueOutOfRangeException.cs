using System;
using System.Runtime.Serialization;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Exception thrown when a Visual Element hasn't been found.
    /// </summary>
    [Serializable]
    internal sealed class ValueOutOfRangeException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="varName">The name of the variable with an out of range value.</param>
        /// <param name="value">The current value of the variable.</param>
        public ValueOutOfRangeException(string varName, object value = null) : base($"The variable {varName} has an out of range value: {value}") { }

        ValueOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
