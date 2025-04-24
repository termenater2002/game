using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Muse.Chat.WebSocket.Model;
using UnityEngine;

namespace Unity.Muse.Chat.WebSocketApi.Models
{
    class ServerMessageJsonConverter : JsonConverter<IModel>
    {
        public override void WriteJson(JsonWriter writer, IModel value, JsonSerializer serializer)
        {
        }

        public override IModel ReadJson(
            JsonReader reader,
            Type objectType,
            IModel existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            Debug.Log($"Json object {jsonObject}");

            if (!jsonObject.TryGetValue("$type", out var typeToken))
                throw new JsonSerializationException("Unknown type");

            IModel message = typeToken.Value<string>() switch
            {
                CapabilitiesRequestV1.Type => new CapabilitiesRequestV1(),
                ChatInitializationV1.Type => new ChatInitializationV1(),
                ChatResponseFragmentV1.Type => new ChatResponseFragmentV1(),
                DiscussionInitializationV1.Type => new DiscussionInitializationV1(),
                FunctionCallRequestV1.Type => new FunctionCallRequestV1(),
                _ => throw new JsonSerializationException("Unknown type")
            };

            serializer.Populate(jsonObject.CreateReader(), message);

            return message;
        }
    }
}
