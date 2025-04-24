using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    internal class RoutesPopup : ManagedTemplate
    {
        VisualElement m_Root;
        VisualElement m_RunItem;
        VisualElement m_CodeItem;
        VisualElement m_MatchThreeItem;
        const string k_FirstPopupItem = "mui-first-popup-item";
        const string k_LastPopupItem = "mui-last-popup-item";

        List<string> m_RouteLabels;

        public Action OnSelectionChanged;

        public RoutesPopup()
            : base(MuseChatConstants.UIModulePath)
        {
            Entries = new List<RoutesPopupEntry>();
        }

        public IList<RoutesPopupEntry> Entries { get; private set; }

        public bool DisplayRoutes(string routeFilter = "", bool initialCreation = false)
        {
            int firstIndex = 0;
            int lastIndex = -1;
            var commands = ChatCommands.GetCommands();

            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                ChatCommands.TryGetCommandHandler(command, out var commandHandler);

                if (initialCreation)
                {
                    var newPopupItem = new RoutesPopupEntry(commandHandler);
                    newPopupItem.Initialize();
                    m_Root.Add(newPopupItem);
                    Entries.Add(newPopupItem);
                    newPopupItem.name = commandHandler.Command;
                    newPopupItem.RegisterCallback<ClickEvent>(_ => ChangeRoute(commandHandler.Command));
                }

                var popupItem = m_Root.Q<VisualElement>(commandHandler.Command);
                popupItem.visible = commandHandler.Label.StartsWith(routeFilter) && commandHandler.ShowInList;
                popupItem.RemoveFromClassList(k_FirstPopupItem);
                popupItem.RemoveFromClassList(k_LastPopupItem);

                if (popupItem.visible)
                {
                    popupItem.style.display = DisplayStyle.Flex;
                    lastIndex += 1;
                }
                else
                {
                    popupItem.style.display = DisplayStyle.None;
                }
            }

            Entries[firstIndex].AddToClassList(k_FirstPopupItem);
            if (lastIndex >= 0)
            {
                Entries[lastIndex].AddToClassList(k_LastPopupItem);
            }

            return lastIndex > -1;
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Root = view.Q<VisualElement>("popupRoot");
            DisplayRoutes("", true);
        }

        void ChangeRoute (string command)
        {
            UserSessionState.instance.SelectedCommandMode = command;
            OnSelectionChanged.Invoke();
        }
    }
}
