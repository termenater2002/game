using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat.Commands
{
    [InitializeOnLoad]
    static class ChatCommands
    {
        public enum ServerRoute
        {
            completion = 0,
            chat,
            action,
            codeGen
        }

        static Dictionary<string, ChatCommandHandler> s_CommandHandlers = new Dictionary<string, ChatCommandHandler>();
        static Dictionary<Type, ChatCommandHandler> s_CommandHandlersByType = new Dictionary<Type, ChatCommandHandler>();
        static List<string> s_Commands = new List<string>();

        static ChatCommands()
        {
            // Get all the ChatCommand attribute holders, put them in the map
            var typeList = TypeCache.GetTypesDerivedFrom<ChatCommandHandler>();

            // Throw errors for types not based on CommandHandler and duplicate commands
            foreach (var type in typeList)
            {
                if (!type.IsSubclassOf(typeof(ChatCommandHandler)))
                {
                    Debug.LogError($"{type.Name} does not inherit from ChatCommandHandler");
                    continue;
                }

                var instance = Activator.CreateInstance(type) as ChatCommandHandler;
                if (s_CommandHandlers.TryGetValue(instance.Command.ToLower(), out var oldInstance))
                {
                    Debug.LogError($"{type.Name} uses the command {instance.Command}, which is also used by {oldInstance.GetType().Name}");
                    continue;
                }

                s_CommandHandlers.Add(instance.Command.ToLower(), instance);
                s_CommandHandlersByType.Add(type, instance);
            }
            s_Commands = s_CommandHandlers.Keys.ToList();
        }

        static public bool TryGetCommandHandler(string command, out ChatCommandHandler commandHandler)
        {
            return s_CommandHandlers.TryGetValue(command, out commandHandler);
        }

        static public bool TryGetCommandHandler(Type type, out ChatCommandHandler commandHandler)
        {
            return s_CommandHandlersByType.TryGetValue(type, out commandHandler);
        }

        static public string CommandTypeToName(ChatCommandType type)
        {
            var typeString = AskCommand.k_CommandName;
            switch(type)
            {
                case ChatCommandType.Run:
                    return RunCommand.k_CommandName;
                case ChatCommandType.Code:
                    return CodeCommand.k_CommandName;
            }
            return typeString;
        }

        static public ServerRoute GetServerRoute(ChatCommandHandler handler)
        {
            var handlerType = handler.GetType();
            if (handlerType == typeof(AskCommand))
                return ServerRoute.chat;
            if (handlerType == typeof(RunCommand))
                return ServerRoute.action;
            if (handlerType == typeof(CodeCommand))
                return ServerRoute.codeGen;
            return ServerRoute.completion;
        }

        static public List<string> GetCommands()
        {
            return s_Commands;
        }
    }
}
