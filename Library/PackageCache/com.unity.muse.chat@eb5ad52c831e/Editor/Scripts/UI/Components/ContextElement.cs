using System;
using Unity.Muse.Chat.UI.Utils;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Chat.UI.Components
{
    class ContextElement : ManagedTemplate, IContextReferenceVisualElement
    {
        const string k_TargetMissingClassName = "mui-context-element-target-missing";
        const string k_MissingObjectIconName = "mui-object-missing";
        const string k_MissingComponentIconName = "mui-component-missing";
        const string k_PrefabVariantClassName = "mui-context-prefab-variant";
        const string k_PrefabClassName = "mui-context-prefab";

        VisualElement m_Row;
        MuseChatImage m_Icon;
        Label m_Text;
        MuseChatImage m_IconMissingOverlay;
        Button m_RemoveButton;

        UnityEngine.Object m_CachedTargetObject;
        Component m_CachedTargetComponent;
        int m_LastTargetObjectNameHash;

        MuseChatContextEntry m_Context;
        bool m_VisualRegistryRegistered;
        bool m_CanRemove = true;
        bool m_ContextSet;

        Action<MuseChatContextEntry> m_OnRemoveCallback;

        public ContextElement() :
            base(MuseChatConstants.UIModulePath)
        {
        }

        protected override void InitializeView(TemplateContainer view)
        {
            m_Row = view.Q<VisualElement>("contextRow");
            m_Icon = view.SetupImage("contextIcon");
            m_Text = view.Q<Label>("contextText");
            m_Text.enableRichText = false;
            m_IconMissingOverlay = view.SetupImage("contextIconMissingOverlay", "star-close-s-trimmed");
            m_RemoveButton = view.SetupButton("removeButton", OnRemoveClicked);
            RegisterCallback<PointerUpEvent>(OnClick);

            RegisterAttachEvents(OnAttachToPanel, OnDetachFromPanel);
        }

        public void SetData(MuseChatContextEntry contextEntry, bool canRemove = false, Action<MuseChatContextEntry> onRemoveCallback = null, string extraStyle = null)
        {
            m_Context = contextEntry;
            m_ContextSet = true;
            if (!m_VisualRegistryRegistered)
            {
                RegisterContextVisualUpdate(true);
            }

            m_CanRemove = canRemove;
            m_RemoveButton.SetDisplay(canRemove);

            m_OnRemoveCallback = onRemoveCallback;

            if (!string.IsNullOrEmpty(extraStyle))
            {
                m_Row.AddToClassList(extraStyle);
            }

            RefreshContextCache();
            RefreshUI();
        }

        public void RefreshVisualElement(UnityEngine.Object activeTargetObject, Component activeTargetComponent)
        {
            // Note: we do not use `RefreshContextCache` here because it's too slow to do all the time for all elements
            //       instead we rely on the Context visual registry to do pre-check
            m_CachedTargetObject = activeTargetObject;
            m_CachedTargetComponent = activeTargetComponent;

            RefreshUI();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            RegisterContextVisualUpdate(false);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (!m_ContextSet)
            {
                // Too early, have to wait for SetData
                return;
            }

            RegisterContextVisualUpdate(true);
        }

        void RegisterContextVisualUpdate(bool register)
        {
            m_VisualRegistryRegistered = register;
            if (register)
            {
                ContextVisualElementRegistry.AddElement(m_Context, this);
            }
            else
            {
                ContextVisualElementRegistry.RemoveElement(this);
            }
        }

        void OnClick(PointerUpEvent evt)
        {
            m_Context.Activate();
        }

        void RefreshContextCache()
        {
            switch (m_Context.EntryType)
            {
                case MuseChatContextType.HierarchyObject:
                case MuseChatContextType.SceneObject:
                {
                    m_CachedTargetObject = m_Context.GetTargetObject();
                    break;
                }

                case MuseChatContextType.Component:
                {
                    m_CachedTargetObject = m_Context.GetTargetObject();
                    m_CachedTargetComponent = m_Context.GetComponent();
                    break;
                }

                default:
                {
                    return;
                }
            }
        }

        void ResetMissingState()
        {
            RemoveFromClassList(k_TargetMissingClassName);
            m_Row.RemoveFromClassList(k_TargetMissingClassName);
            m_Text.SetEnabled(true);
            m_IconMissingOverlay.SetDisplay(false);
        }

        void SetAsMissing()
        {
            AddToClassList(k_TargetMissingClassName);
            m_Row.AddToClassList(k_TargetMissingClassName);
            m_Text.SetEnabled(false);
            m_IconMissingOverlay.SetDisplay(true);
            m_Icon.SetIconByTypeString(m_Context.ValueType);
        }

        void RefreshUI()
        {
            if (!IsInitialized || !visible)
            {
                return;
            }

            ResetMissingState();
            m_Text.tooltip = null;

            switch (m_Context.EntryType)
            {
                case MuseChatContextType.ConsoleMessage:
                {
                    var logMode = Enum.Parse<LogDataType>(m_Context.ValueType);
                    m_Icon.SetIconClassName(ChatUIUtils.GetLogIconClassName(logMode));

                    if (string.IsNullOrEmpty(m_Context.Value))
                    {
                        m_Text.text = "Unknown";
                    } else
                    {
                        string[] lines = m_Context.Value.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                        if (lines.Length > 0)
                        {
                            m_Text.text = lines[0].Substring(0, Math.Min(20, lines[0].Length)) + "...";
                            m_Text.tooltip = $"Console {logMode}:\n{lines[0]}";
                        }
                    }

                    break;
                }

                case MuseChatContextType.SceneObject:
                {
                    if (m_CachedTargetObject == null)
                    {
                        SetAsMissing();
                        m_Icon.SetIconClassName(k_MissingObjectIconName);
                        m_Text.text = m_Context.DisplayValue;
                    }
                    else
                    {
                        if (m_CachedTargetObject is GameObject go)
                        {
                            m_Text.EnableInClassList(k_PrefabClassName, go.IsPrefabType());
                            m_Text.EnableInClassList(k_PrefabVariantClassName, go.IsPrefabVariant());
                        }

                        m_Icon.SetIconClassName(null);
                        m_Icon.SetTexture(m_CachedTargetObject.GetTextureForObject());
                        m_Text.text = m_CachedTargetObject.name;
                    }

                    break;
                }

                case MuseChatContextType.HierarchyObject:
                {
                    if (m_CachedTargetObject == null)
                    {
                        SetAsMissing();
                        m_Icon.SetIconClassName(k_MissingObjectIconName);
                        m_Text.text = m_Context.DisplayValue;
                    }
                    else
                    {
                        if (m_CachedTargetObject is GameObject go)
                        {
                            m_Text.EnableInClassList(k_PrefabClassName, go.IsPrefabType());
                            m_Text.EnableInClassList(k_PrefabVariantClassName, go.IsPrefabVariant());
                        }

                        m_Icon.SetIconClassName(null);
                        m_Icon.SetTexture(m_CachedTargetObject.GetTextureForObject());
                        m_Text.text = m_CachedTargetObject.name;
                        m_Text.tooltip = ContextViewUtils.GetObjectTooltip(m_CachedTargetObject);
                    }

                    //ShowTextAsPrefabInScene(MessageUtils.IsPrefabInScene(unityObj));
                    break;
                }

                case MuseChatContextType.Component:
                {
                    if (m_CachedTargetComponent == null)
                    {
                        SetAsMissing();
                        m_Icon.SetIconClassName(k_MissingComponentIconName);
                        m_Text.text = m_Context.DisplayValue;
                    }
                    else
                    {
                        m_Icon.SetIconClassName(null);
                        m_Icon.SetTexture(m_CachedTargetComponent.GetTextureForObjectType());
                        m_Text.text = m_CachedTargetComponent.GetType().Name;
                        m_Text.tooltip = m_CachedTargetComponent.name;
                    }

                    break;
                }

                default:
                {
                    throw new InvalidOperationException("Unhandled Context Type: " + m_Context.EntryType);
                }
            }
        }

        void OnRemoveClicked(PointerUpEvent evt)
        {
            m_OnRemoveCallback?.Invoke(m_Context);
        }
    }
}
