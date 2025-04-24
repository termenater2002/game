using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Editor.Markup;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementPluginButton : ManagedTemplate
    {
        Button m_Button;
        Label m_Text;

        /// <summary>
        /// Create a new shared chat element
        /// </summary>
        public ChatElementPluginButton()
            : base(MuseChatConstants.UIModulePath)
        {
        }

        /// <summary>
        /// Set the data for this source element
        /// </summary>
        /// <param name="index">the index of the source</param>
        /// <param name="call">the source block defining the URL and title</param>
        public void SetData(PluginCall call)
        {
            PluginCall = call;
            RefreshDisplay();
        }

        public PluginCall PluginCall { get; private set; }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Text = view.Q<Label>("pluginButtonLabel");

            m_Button = view.SetupButton("pluginButton", OnSourceClicked);
            m_Button.style.width = new StyleLength(StyleKeyword.Auto);
        }

        void OnSourceClicked(PointerUpEvent evt)
        {
            if(!MuseChatState.PluginToolbox.TryRunToolByName(PluginCall.Function, PluginCall.Parameters))
                Debug.LogWarning($"Failed to call plugin {PluginCall.Label}");
        }

        void RefreshDisplay()
        {
            m_Text.text = PluginCall.Parameters != null && PluginCall.Parameters.Length > 0
                ? PluginCall.Parameters[0]
                : $"{PluginCall.Function}({ string.Join(", ", PluginCall.Parameters) })";

            m_Button.text = PluginCall.Label;
            m_Button.tooltip = PluginCall.Label;
        }
    }
}
