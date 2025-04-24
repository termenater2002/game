using UnityEditor;

namespace Unity.Muse.Chat.Serialization
{
    interface ISerializationOverride
    {
        string DeclaringType { get; }
        string Field { get; }
        object Override(SerializedProperty property);
    }
}
