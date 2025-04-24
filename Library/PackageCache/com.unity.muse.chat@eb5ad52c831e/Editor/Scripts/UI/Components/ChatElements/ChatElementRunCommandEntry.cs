using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Muse.Chat.UI.Utils;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Unity.Muse.Chat.UI.Components.ChatElements
{
    class ChatElementRunCommandEntry : ManagedTemplate
    {
        const string k_ParamTagStart = "<param>";
        const string k_ParamTagEnd = "</param>";

        VisualElement m_EntryRoot;
        VisualElement m_Description;
        Button m_PreviewAction;

        string m_PreviewText;
        AgentRunCommand m_AgentRunCommand;
        bool m_ItemizeAction;

        Action m_Action;

        public ChatElementRunCommandEntry(string previewText, AgentRunCommand agentRunCommand)  : base(MuseChatConstants.UIModulePath)
        {
            m_PreviewText = previewText;
            m_AgentRunCommand = agentRunCommand;
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_EntryRoot = view.Q<VisualElement>("entryRoot");
            m_Description = view.Q<VisualElement>("description");

            ItemizeItems();

            m_PreviewAction = view.SetupButton("action", OnActionClicked);
            m_PreviewAction.style.display = DisplayStyle.None;
        }

        void ItemizeItems()
        {
            var segments = Regex.Split(m_PreviewText, $"({k_ParamTagStart}.*?{k_ParamTagEnd})");
            foreach (var segment in segments)
            {
                if (segment.StartsWith(k_ParamTagStart) && segment.EndsWith(k_ParamTagEnd))
                {
                    string fieldName = segment.Substring(k_ParamTagStart.Length, segment.Length - k_ParamTagStart.Length - k_ParamTagEnd.Length);
                    var instanceType = m_AgentRunCommand.Instance.GetType();
                    var fieldInfo = instanceType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (fieldInfo != null)
                    {
                        var field = CreateFieldForType(fieldInfo, m_AgentRunCommand.Instance);
                        if (field != null)
                        {
                            field.AddToClassList("entry-label-item");
                            m_Description.Add(field);
                        }
                    }
                }
                else
                {
                    // Split the segment by spaces, but keep the spaces
                    string[] parts = Regex.Split(segment, @"(\s+)");
                    foreach (string part in parts)
                    {
                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            var label = new Label(part);
                            label.AddToClassList("entry-label-item");
                            m_Description.Add(label);
                        }
                    }
                }
            }
        }

        VisualElement CreateFieldForType(FieldInfo fieldInfo, object instance)
        {
            Type fieldType = fieldInfo.FieldType;
            object value = fieldInfo.GetValue(instance);

            if (fieldType == typeof(float))
            {
                var floatField = new FloatField { value = (float)value };
                floatField.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return floatField;
            }
            else if (fieldType == typeof(int))
            {
                var intField = new IntegerField { value = (int)value };
                intField.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return intField;
            }
            else if (fieldType == typeof(string))
            {
                var textField = new TextField { value = (string)value };
                textField.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return textField;
            }
            else if (fieldType == typeof(bool))
            {
                var toggle = new Toggle { value = (bool)value };
                toggle.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return toggle;
            }
            else if (fieldType == typeof(Vector3))
            {
                var vector3Field = new Vector3Field { value = (Vector3)value };
                vector3Field.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return vector3Field;
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                var objectField = new ObjectField { objectType = fieldType, value = (UnityEngine.Object)value };
                objectField.RegisterValueChangedCallback(evt =>
                {
                    fieldInfo.SetValue(instance, evt.newValue);
                });
                return objectField;
            }
            else
            {
                var label = new Label(value?.ToString() ?? "None");
                label.AddToClassList("entry-label-item");
                return label;
            }
        }


        public void RegisterAction(Action entryAction)
        {
            m_EntryRoot.AddToClassList("preview-entry-actionable");
            m_PreviewAction.style.display = DisplayStyle.Flex;

            m_Action = entryAction;
        }

        void OnActionClicked(PointerUpEvent evt)
        {
            m_Action?.Invoke();
        }
    }
}
