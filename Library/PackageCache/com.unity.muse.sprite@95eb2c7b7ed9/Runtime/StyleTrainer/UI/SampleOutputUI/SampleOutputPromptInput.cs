using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class SampleOutputPromptInput : ExVisualElement
    {
        TextField m_Prompt;
        int m_ItemIndex;
        public Action<int, string> OnPromptChanged;
        public Action<int, string> OnPromptChanging;

        public int itemIndex
        {
            set => m_ItemIndex = value;
        }

        void BindElements()
        {
            m_Prompt = this.Q<TextField>("Prompt");
            m_Prompt.RegisterValueChangedCallback(OnPromptValueChanged);
            m_Prompt.RegisterValueChangingCallback(OnPromptValueChanging);
            SetPlaceholder("Eg. Cat with a hat");
        }

        void OnPromptValueChanged(ChangeEvent<string> evt)
        {
            OnPromptChanged?.Invoke(m_ItemIndex, evt.newValue);
        }

        void OnPromptValueChanging(ChangingEvent<string> evt)
        {
            OnPromptChanging?.Invoke(m_ItemIndex, evt.newValue);
        }

        public string prompt
        {
            set
            {
                m_Prompt.SetValueWithoutNotify(value);
            }
        }

        public void SetPlaceholder(string placeholder)
        {
            m_Prompt.placeholder = placeholder;
        }

        public void CanModify(bool canModify)
        {
            m_Prompt.SetEnabled(canModify);
            EnableInClassList("styletrainer-sampleoutputview-gridview-disable", !canModify);
        }

        public void FocusItem()
        {
            schedule.Execute(() =>
            {
                m_Prompt.contentContainer.Focus();
            });
        }

        internal static SampleOutputPromptInput CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.sampleOutputPromptInputTemplate);
            var ve = (SampleOutputPromptInput)visualTree.CloneTree().Q("SampleOutputPromptInput");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputPromptInputStyleSheet));
            ve.BindElements();
            return ve;
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<SampleOutputPromptInput, UxmlTraits> { }
#endif
    }
}