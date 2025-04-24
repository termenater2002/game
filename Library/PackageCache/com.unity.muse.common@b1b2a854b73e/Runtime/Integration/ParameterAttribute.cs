using System;
using JetBrains.Annotations;

namespace Unity.Muse.Common.Editor.Integration
{
    /// <summary>
    ///     Marks a parameter of a method decorated with a <see cref="ContextProviderAttribute"/>
    ///     attribute with a description of its purpose.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    class ParameterAttribute : Attribute
    {
        /// <summary>
        ///     Description of the argument marked by this attribute.
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParameterAttribute"/> type.
        /// </summary>
        /// <param name="description">
        ///     Description of the argument marked by this attribute.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public ParameterAttribute([NotNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(nameof(description),
                    $"Cannot be empty");

            Description = description;
        }
    }
}
