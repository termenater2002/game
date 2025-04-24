using System;
using Unity.Muse.Chat.UI.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TextOverflow = UnityEngine.UIElements.TextOverflow;

namespace Unity.Muse.Chat.UI.Components
{
    class SelectionElement : AdaptiveListViewEntry
    {
        Label m_Text;
        MuseChatImage m_Icon;
        Button m_FindButton;
        VisualElement m_Checkmark;

        SelectionPopup.ListEntry m_Entry;

        public Action<SelectionElement> OnAddRemoveButtonClicked;
        bool m_IsSelected;
        bool m_IgnoreNextClick;

        readonly string k_PrefabInSceneStyleClass = "mui-chat-selection-prefab-text-color";

        protected override void InitializeView(TemplateContainer view)
        {
            m_Text = view.Q<Label>("selectionElementText");
            m_Text.enableRichText = false;
            m_Icon = view.SetupImage("selectionElementIcon");
            m_FindButton = view.SetupButton("selectionElementFindButton", OnFindClicked);
            m_Checkmark = view.Q<VisualElement>("mui-selection-element-checkmark");

            m_FindButton.visible = false;
            m_FindButton.focusable = false;

            m_Text.style.overflow = Overflow.Hidden;
            m_Text.style.whiteSpace = WhiteSpace.NoWrap;

            view.RegisterCallback<ClickEvent>(ToggleSelection);
            view.RegisterCallback<MouseEnterEvent>(MouseEntered);
            view.RegisterCallback<MouseLeaveEvent>(MouseLeft);
        }

        void MouseEntered(MouseEnterEvent evt)
        {
            if (!m_IsSelected)
                m_FindButton.visible = true;
        }

        void MouseLeft(MouseLeaveEvent evt)
        {
            if (!m_IsSelected)
                m_FindButton.visible = false;
        }

        void ToggleSelection(ClickEvent evt)
        {
            if (m_IgnoreNextClick)
            {
                m_IgnoreNextClick = false;
                return;
            }

            m_Entry.OnRowClick?.Invoke(this);
        }

        public override void SetData(int index, object data, bool isSelected = false)
        {
            base.SetData(index, data);

            m_Entry = data as SelectionPopup.ListEntry;

            if (m_Entry == null)
            {
                return;
            }

            if (m_Entry.LogData != null)
            {
                m_Icon.SetIconClassName(ChatUIUtils.GetLogIconClassName(m_Entry.LogData.Value.Type));
                SetText(m_Entry.LogData.Value.Message);

                string[] lines = m_Entry.LogData.Value.Message.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                if (lines.Length > 0)
                    m_Text.tooltip = $"Console {m_Entry.LogData.Value.Type}:\n{lines[0]}";

                m_Text.style.textOverflow = TextOverflow.Ellipsis;
            }
            else
            {
                var unityObj = m_Entry.Object;

                // Component or in hierarchy
                if (unityObj is Component || !AssetDatabase.Contains(unityObj))
                {
                    var texture = unityObj.GetTextureForObjectType();
                    SetIcon(texture);
                }
                // Prefab type
                else if (unityObj.IsPrefabType() && !AssetDatabase.Contains(unityObj))
                {
                    m_Icon.SetIconClassName("prefab");
                }
                // Assets
                else
                {
                    var texture = unityObj.GetTextureForObject();
                    SetIcon(texture);
                }

                SetText(unityObj.name);
                ShowTextAsPrefabInScene(unityObj.IsPrefabInScene());

                m_Text.tooltip = ContextViewUtils.GetObjectTooltip(unityObj);

                m_Text.style.textOverflow = TextOverflow.Clip;
            }

            SetSelected(m_Entry.IsSelected);
        }

        public void SetText(string text)
        {
            m_Text.text = text;
        }

        public void ShowTextAsPrefabInScene(bool isPrefab)
        {
            if (isPrefab)
                m_Text.AddToClassList(k_PrefabInSceneStyleClass);
            else
                m_Text.RemoveFromClassList(k_PrefabInSceneStyleClass);
        }

        public void SetIcon(Texture2D texture)
        {
            m_Icon.SetTexture(texture);
        }

        public void SetSelected(bool selected)
        {
            m_Checkmark.visible = selected;
        }

        void OnFindClicked(PointerUpEvent evt)
        {
            m_IgnoreNextClick = true;
            m_Entry.OnFindButtonClick?.Invoke(this);
        }
    }
}
