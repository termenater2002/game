namespace Unity.Muse.Chat.Serialization
{
    interface ISerializationOverrideProvider
    {
        ISerializationOverride Find(string declaringType, string field);
    }
}
