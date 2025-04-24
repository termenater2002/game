using System;
using Unity.Muse.Common.Utils;
using UnityEngine;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    internal class ChatElementValidatedCodeBlock : ChatElementCodeBlock
    {
        const string k_OldFailMessage = "Something went wrong.\nThe generated script was not able to compile in your project.";
        const string k_NewFailMessage = "Something went wrong.\nThe generated script was not able to compile in your project. Try to correct any errors, or generate it again.";

        bool m_OldFailure = false;

        public ChatElementValidatedCodeBlock() : base()
        {
            SetResourceName("ChatElementCodeBlock");
        }

        public override void Display()
        {
            var content = ContentGroups[0];

            switch (content.State)
            {
                case DisplayState.Success:
                    UpdateTextWithCode();
                    m_SaveButton.SetEnabled(true);
                    m_CopyButton.SetEnabled(true);
                    break;
                case DisplayState.Fail:
                    FailCode(m_OldFailure ? k_OldFailMessage : k_NewFailMessage);
                    UpdateTextWithCode();
                    m_SaveButton.SetEnabled(false);
                    m_CopyButton.SetEnabled(true);
                    break;
            }
        }

        protected override bool ValidateInternal(int index, out string logs)
        {
            var contentGroup = ContentGroups[index];

            var valid = Assistant.instance.CodeBlockValidator.ValidateCode(contentGroup.Content, out var localRepairedCode, out logs);
            contentGroup.Content = localRepairedCode;
            return valid;
        }

        void FailCode(string message)
        {
            SetWarningText($"{message}\n{ContentGroups[0].Logs}");
        }

        protected override string FormatCodeForSave(string code, string className)
        {
            return CodeExportUtils.AddDisclaimer(code);
        }
    }
}
