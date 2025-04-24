using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unity.Muse.Animate
{
    class LibraryItemJsonConverter : JsonConverter<LibraryItemModel>
    {
        static Dictionary<LibraryItemType, Type> s_ItemTypeToType;

        static LibraryItemJsonConverter()
        {
            // Get all the types that inherit from TakeModel with the TakesSerializationType attribute.
            var itemTypes = ReflectionUtils.GetAllTypesWithAttribute<SerializableLibraryItemAttribute>(type =>
                typeof(LibraryItemModel).IsAssignableFrom(type));

            // Create a mapping from the TakeType enum to the type.
            s_ItemTypeToType = itemTypes.ToDictionary(type => type.GetCustomAttribute<SerializableLibraryItemAttribute>().Type);
        }

        public override void WriteJson(JsonWriter writer, LibraryItemModel value, JsonSerializer serializer)
        {
            var itemType = GetItemTypeFromAttribute(value);
            var serializedType = s_ItemTypeToType[itemType];
            
            // Serialize as the specified type
            serializer.Serialize(writer, value, serializedType);
        }
        
        static LibraryItemType GetItemTypeFromAttribute(LibraryItemModel libraryItemModel)
        {
            var type = libraryItemModel.GetType();
            var attribute = type.GetCustomAttribute<SerializableLibraryItemAttribute>();
            if (attribute == null)
            {
                throw new JsonSerializationException($"{type} is not serializable library item");
            }

            return attribute.Type;
        }

        public override LibraryItemModel ReadJson(JsonReader reader,
            Type objectType,
            LibraryItemModel existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            
            // Create an instance of the correct take type.
            if (hasExistingValue)
            {
                serializer.Populate(jsonObject.CreateReader(), existingValue);
                return existingValue;
            }

            var typeIdx = LibraryItemType.KeySequenceTake;
            
            // Legacy support
            if (jsonObject["m_LibraryItemType"]?.ToObject<LibraryItemType>() is {} takeType1)
            {
                typeIdx = takeType1;
            }
            else if(jsonObject["m_ItemType"]?.ToObject<LibraryItemType>() is {} takeType2)
            {
                typeIdx = takeType2;
            }
            else if(jsonObject["m_Type"]?.ToObject<LibraryItemType>() is {} takeType3)
            {
                typeIdx = takeType3;
            }
            else
            {
                DevLogger.LogWarning("Failed to load LibraryItemModel.ItemType, defaulting to LibraryItemModel.ItemType.KeySequenceTake.");
            }

            return jsonObject.ToObject(s_ItemTypeToType[typeIdx], serializer) as LibraryItemModel;
        }
    }
}
