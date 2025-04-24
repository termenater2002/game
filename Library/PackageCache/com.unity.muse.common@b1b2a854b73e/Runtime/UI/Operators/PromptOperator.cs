using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Text = Unity.Muse.AppUI.UI.Text;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class PromptOperator : IOperator, ISerializationCallbackReceiver
    {
        public const int MinimumPromptLength = 1;
        public virtual string OperatorName => "PromptOperator";

        /// <summary>
        /// Human-readable label for the operator.
        /// </summary>
        public string Label => "Prompt";

        protected event Action OnDataUpdate;
        protected event Action OnIsSetChanged;

        [SerializeField]
        protected OperatorData m_OperatorData;

        protected TextArea m_PromptField;
        TextArea m_NegPromptField;
        protected bool m_LastKeyReturn;
        protected Model m_Model;

        protected const int k_PromptIndex = 0;
        protected const int k_NegPromptIndex = 1;
        protected bool m_PromptHasValue;

        public PromptOperator()
        {
            m_OperatorData = new OperatorData(OperatorName, "0.0.2", new[] { "", "" }, false);
        }

        public bool IsPromptValid()
        {
            return m_OperatorData.settings[k_PromptIndex].Length >= MinimumPromptLength;
        }

        public bool IsSavable()
        {
            return true;
        }

        public VisualElement GetCanvasView()
        {
            Debug.Log("PromptOperator.GetCanvasView()");
            return new VisualElement();
        }

        public virtual VisualElement GetOperatorView(Model model)
        {
            m_PromptField?.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            m_PromptField?.UnregisterValueChangingCallback(ValueChangedCallback);

            m_Model = model;
            var UI = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows | ExVisualElement.Passes.BackgroundColor
            };
            UI.AddToClassList("muse-node");
            UI.name = "prompt-node";
            var text = new Text
            {
                text = Label,
                pickingMode = PickingMode.Position,
                tooltip = TextContent.operatorPromptTooltip
            };
            text.AddToClassList("muse-node__title");
            text.AddToClassList("bottom-gap");
            UI.Add(text);
            m_PromptField = new TextArea()
            {
                name = "prompt-inputfield",
                placeholder = TextContent.promptPlaceholder,
                autoResize = true,
                maxLength = 1024
            };
            UI.Add(new TextAreaCount(m_PromptField));
            m_LastKeyReturn = false;

            var ticks = DateTime.Now.Ticks;
            m_PromptField.userData = ticks;
            m_PromptField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            m_PromptField.SetValueWithoutNotify(m_OperatorData.settings[k_PromptIndex]);
            model.SetCurrentPrompt(m_PromptField.value);

            m_PromptField.RegisterValueChangingCallback(ValueChangedCallback);
            m_PromptField.AddToClassList("bottom-gap");
            m_PromptField.AddToClassList("muse-prompt-text-area");
            UI.Add(m_PromptField);

            var negPromptText = new Text
            {
                text = "Negative Prompt",
                tooltip = TextContent.operatorNegativePromptTooltip,
                pickingMode = PickingMode.Position
            };
            negPromptText.AddToClassList("muse-node__title");
            negPromptText.AddToClassList("bottom-gap");
            UI.Add(negPromptText);

            m_NegPromptField = new TextArea
            {
                name = "neg-prompt-inputfield",
                autoResize = true,
                maxLength = 1024
            };

            UI.Add(new TextAreaCount(m_NegPromptField));

            var lastKeyReturn = false;
            m_NegPromptField.RegisterCallback((KeyDownEvent evt) =>
            {
                if ((evt.keyCode == KeyCode.Tab || evt.keyCode == KeyCode.None && evt.character == '\t') && !evt.shiftKey)
                {
                    evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
                    evt.PreventDefault();
#endif
                    if (evt.character != '\t')
                        m_NegPromptField.focusController.FocusNextInDirectionEx(m_NegPromptField, VisualElementFocusChangeDirection.right);
                    return;
                }

                if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                {
                    lastKeyReturn = true;
                    return;
                }

                if (evt.keyCode == KeyCode.None && lastKeyReturn)
                {
                    evt.StopPropagation();
#if !UNITY_2023_2_OR_NEWER
                    evt.PreventDefault();
#endif
                    m_OperatorData.settings[k_NegPromptIndex] = m_NegPromptField.value;
                    TryGenerate();
                }

                lastKeyReturn = false;
            }, TrickleDown.TrickleDown);

            m_NegPromptField.SetValueWithoutNotify(m_OperatorData.settings[k_NegPromptIndex]);
            m_NegPromptField.RegisterValueChangedCallback((evt) =>
            {
                m_OperatorData.settings[k_NegPromptIndex] = m_NegPromptField.value;
            });
            m_NegPromptField.AddToClassList("muse-prompt-text-area");
            UI.Add(m_NegPromptField);

            OnDataUpdate -= OnOnDataUpdate;
            OnDataUpdate += OnOnDataUpdate;

            return UI;
        }

        protected void OnOnDataUpdate()
        {
            if (m_OperatorData.settings[k_PromptIndex] != "")
            {
                m_PromptField.value = m_OperatorData.settings[k_PromptIndex];
                m_Model.SetCurrentPrompt(m_PromptField.value);
            }
        }

        protected void ValueChangedCallback(ChangingEvent<string> evt)
        {
            m_OperatorData.settings[k_PromptIndex] = m_PromptField.value;
            m_Model.SetCurrentPrompt(m_PromptField.value);
            HandleChangeIsSet();
        }

        protected void HandleChangeIsSet()
        {
            if (m_PromptHasValue != IsPromptValid())
            {
                m_PromptHasValue = IsPromptValid();
                OnIsSetChanged?.Invoke();
            }
        }

        protected void OnKeyDown(KeyDownEvent evt)
        {
            if ((evt.keyCode == KeyCode.Tab || evt.keyCode == KeyCode.None && evt.character == '\t') && !evt.shiftKey)
            {
                evt.StopImmediatePropagation();
#if !UNITY_2023_2_OR_NEWER
                evt.PreventDefault();
#endif
                if (evt.character != '\t') m_PromptField.focusController.FocusNextInDirectionEx(m_PromptField, VisualElementFocusChangeDirection.right);
                return;
            }

            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                m_LastKeyReturn = true;
                return;
            }

            if (evt.keyCode == KeyCode.None && m_LastKeyReturn)
            {
                evt.StopPropagation();
#if !UNITY_2023_2_OR_NEWER
                evt.PreventDefault();
#endif
                m_OperatorData.settings[k_PromptIndex] = m_PromptField.value;
                TryGenerate();
            }

            m_LastKeyReturn = false;
        }

        /// <summary>
        /// Gets the operator data.
        /// </summary>
        /// <returns>The operator data.</returns>
        public OperatorData GetOperatorData()
        {
            return m_OperatorData;
        }

        /// <summary>
        /// Sets the operator data.
        /// </summary>
        /// <param name="data">The data to use.</param>
        public void SetOperatorData(OperatorData data)
        {
            m_OperatorData.enabled = data.enabled;
            if (data.settings == null || data.settings.Length != m_OperatorData.settings.Length)
                return;
            m_OperatorData.settings = data.settings;

            OnDataUpdate?.Invoke();
        }

        void SetSettings(IReadOnlyList<string> settings)
        {
            m_OperatorData.settings[k_PromptIndex] = settings[k_PromptIndex];
            m_OperatorData.settings[k_NegPromptIndex] = settings[k_NegPromptIndex];
            OnDataUpdate?.Invoke();
        }

        string[] GetSettings()
        {
            return m_OperatorData.settings;
        }

        /// <summary>
        /// Gets the enabled state of the operator.
        /// </summary>
        /// <returns>The enabled state.</returns>
        public bool Enabled()
        {
            return m_OperatorData.enabled;
        }

        /// <summary>
        /// Sets the enabled state of the operator.
        /// </summary>
        /// <param name="enable">The new state to set.</param>
        public void Enable(bool enable)
        {
            m_OperatorData.enabled = enable;
        }

        public bool Hidden { get; set; }

        /// <summary>
        /// Clones the operator.
        /// </summary>
        /// <returns>The cloned operator.</returns>
        public virtual IOperator Clone()
        {
            var result = new PromptOperator();
            var operatorData = new OperatorData();
            operatorData.FromJson(GetOperatorData().ToJson());
            result.SetOperatorData(operatorData);
            return result;
        }

        /// <summary>
        /// Registers the operator to the model events.
        /// </summary>
        /// <param name="model"></param>
        public void RegisterToEvents(Model model) { }

        /// <summary>
        /// Unregisters the operator from the model events.
        /// </summary>
        /// <param name="model"></param>
        public void UnregisterFromEvents(Model model) { }

        /// <summary>
        /// Gets the prompt for this operator.
        /// </summary>
        /// <returns>The operator's prompt.</returns>
        public string GetPrompt()
        {
            return m_OperatorData.settings[k_PromptIndex];
        }

        /// <summary>
        /// Gets the negative prompt for this operator.
        /// </summary>
        /// <returns>The operator's negative prompt.</returns>
        public string GetNegativePrompt()
        {
            return m_OperatorData.settings[k_NegPromptIndex];
        }

        /// <summary>
        /// Sets the prompt text.
        /// </summary>
        /// <param name="promptText">Prompt text</param>
        public void SetPrompt(string promptText)
        {
            m_OperatorData.settings[k_PromptIndex] = promptText;
            if (m_PromptField != null)
                m_PromptField.value = promptText;

            if (!m_Model)
                return;

            m_Model.SetCurrentPrompt(promptText);
        }

        /// <summary>
        /// Sets the negative prompt text.
        /// </summary>
        /// <param name="negativePromptText">Prompt text</param>
        public void SetNegativePrompt(string negativePromptText)
        {
            m_OperatorData.settings[k_NegPromptIndex] = negativePromptText;
            if (m_NegPromptField != null)
                m_NegPromptField.value = negativePromptText;
        }

        /// <summary>
        /// Get the settings view for this operator.
        /// </summary>
        /// <param name="model">Current Model</param>
        /// <param name="isCustomSection">This VisualElement will override the whole operator section used by the generation settings</param>
        /// <param name="dismissAction">Action to call when the settings view is dismissed</param>
        /// <returns> UI for the operator. Set to Null if the operator should not be displayed in the settings view. Disable the returned VisualElement if you want it to be displayed but not usable.</returns>
        public VisualElement GetSettingsView(Model model, ref bool isCustomSection, Action dismissAction)
        {
            isCustomSection = true;
            var currentPromptOperator = model.CurrentOperators?.GetOperator<PromptOperator>();
            var container = new VisualElement();
            var prompt = GetPrompt();
            var negativePrompt = GetNegativePrompt();

            if (!string.IsNullOrEmpty(prompt))
            {
                container.Add(GenerationSettings.CreateView("Prompt", new Text { text = prompt }, () =>
                {
                    currentPromptOperator?.CopyPrompt(this);
                    dismissAction?.Invoke();
                }));
            }

            if (!string.IsNullOrEmpty(negativePrompt))
            {
                container.Add(GenerationSettings.CreateView("Negative Prompt", new Text { text = negativePrompt },
                    () =>
                    {
                        currentPromptOperator?.CopyNegativePrompt(this);
                        dismissAction?.Invoke();
                    }));
            }

            return container;
        }

        void CopyPrompt(PromptOperator op)
        {
            SetPrompt(op.GetPrompt());
        }

        void CopyNegativePrompt(PromptOperator op)
        {
            SetNegativePrompt(op.GetNegativePrompt());
        }

        void TryGenerate()
        {
            if (!m_Model)
                return;

            if (m_Model.GetData<GenerateButtonData>().isEnabled)
                m_Model.GenerateButtonClicked();
        }

        public void UpgradeVersion()
        {
            if (m_OperatorData.version == "0.0.1")
            {
                m_OperatorData = new OperatorData(OperatorName, "0.0.2", new[] { m_OperatorData.settings[0], "" }, m_OperatorData.enabled);
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            UpgradeVersion();
        }

        public void SetPromptFieldEnabled(bool enable)
        {
            m_PromptField?.SetEnabled(enable);
        }

        public void SetNegativePromptFieldEnabled(bool enable)
        {
            m_NegPromptField?.SetEnabled(enable);
        }
    }
}
