using System;
using JetBrains.Annotations;

namespace Unity.Muse.Common.Editor.Integration
{
    /// <summary>
    ///     Marks a static method returning some context for Muse Chat as a <see cref="string"/>.
    ///     Each method parameter must have a <see cref="ParameterAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    class ContextProviderAttribute : Attribute
    {
        /// <summary>
        ///     A description of the piece of context returned by the method.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Marks a static method returning some context for Muse Chat as a <see cref="string"/>.
        /// </summary>
        /// <param name="description">
        ///     A description of the piece of context returned by the method.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public ContextProviderAttribute([NotNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(nameof(description),
                    $"Cannot be empty");

            Description = description;
        }
    }
}
