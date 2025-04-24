using System;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Specifies the type of library item (the enum value) written to the JSON file.
    /// </summary>
    class SerializableLibraryItemAttribute : Attribute
    {
        internal LibraryItemType Type { get; }

        internal SerializableLibraryItemAttribute(LibraryItemType type)
        {
            Type = type;
        }
    }
}
