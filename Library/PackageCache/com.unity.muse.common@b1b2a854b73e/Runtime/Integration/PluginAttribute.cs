using System;
using JetBrains.Annotations;

namespace Unity.Muse.Common.Editor.Integration
{
    /// <summary>
    ///     Marks a static method executing commands for Muse Chat.
    ///     Each method parameter must have a <see cref="ParameterAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    class PluginAttribute : Attribute
    {
        /// <summary>
        ///     A description of the effect applied by the method.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Marks a static method executing commands for Muse Chat.
        /// </summary>
        /// <param name="description">
        ///     A description of the effect applied by the method.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginAttribute([NotNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(nameof(description),
                    $"Cannot be empty");

            Description = description;
        }
    }
}
