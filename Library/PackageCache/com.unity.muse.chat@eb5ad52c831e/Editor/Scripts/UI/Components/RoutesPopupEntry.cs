using Unity.Muse.Chat.Commands;
using Unity.Muse.Chat.UI.Components;
using Unity.Muse.Chat.UI.Utils;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    internal class RoutesPopupEntry : ManagedTemplate
    {
        const string k_RouteChipHoveredClass = "mui-route-chip-hovered";

        Label m_LabelElement;
        Label m_DescriptionElement;

        public RoutesPopupEntry(ChatCommandHandler command)
            : base(MuseChatConstants.UIModulePath)
        {
            Command = command;
        }

        public ChatCommandHandler Command { get; }

        protected override void InitializeView(TemplateContainer view)
        {
            m_LabelElement = view.Q<Label>("commandItemText");
            m_LabelElement.text = Command.Label;

            m_DescriptionElement = view.Q<Label>("commandItemDescription");
            m_DescriptionElement.text = Command.PlaceHolderText;
        }

        public void SetHovered(bool hovered)
        {
            if (hovered)
            {
                AddToClassList(k_RouteChipHoveredClass);
            }
            else
            {
                RemoveFromClassList(k_RouteChipHoveredClass);
            }
        }
    }
}
