using System;
using Unity.Muse.Chat.BackendApi.Model;
using Unity.Muse.Chat.Commands;

namespace Unity.Muse.Chat
{
    internal static class MuseChatDataUtils
    {
        public static MuseChatInspiration ToInternal(this Inspiration apiData)
        {
            var result = new MuseChatInspiration
            {
                Id = new MuseInspirationId(apiData.Id),
                Description = apiData.Description,
                Value = apiData.Value
            };

            var commandString = apiData.Mode.ToString().ToLower();
            if (ChatCommands.TryGetCommandHandler(commandString, out _))
            {
                result.Command = commandString;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return result;
        }

        public static Inspiration ToExternal(this MuseChatInspiration data)
        {
            Inspiration apiData = new Inspiration(Inspiration.ModeEnum.Ask, data.Value)
            {
                Id = data.Id.IsValid ? data.Id.Value : default,
                Description = data.Description
            };
            if (Enum.TryParse<Inspiration.ModeEnum>(data.Command, true, out var result))
            {
                apiData.Mode = result;
            }
            return apiData;
        }
    }
}
