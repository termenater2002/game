using UnityEngine;
using Unity.Muse.AppUI.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common.Account;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{

#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TakesUI : UITemplateContainer, IUITemplate
    {
        TakesUIModel m_Model;
        const string k_TakesUIUssClassName = "deeppose-takes";
        const string k_TakesGeneration = "takes-t2m";
        const string k_SectionName = "takes-t2m-section";
        const string k_PromptFieldName = "takes-t2m-input-field";
        const string k_GenerateButtonName = "takes-t2m-button-generate";
        const string k_TakesSliderName = "takes-t2m-slider";
        const string k_DurationSliderName = "takes-t2m-duration";
        const string k_ModelDropdownName = "takes-t2m-model";
        const string k_OptionalGeneratorsName = "takes-optional-generators";

        readonly ITimelineBakerTextToMotion.Model[] m_InferenceModelsType = { ITimelineBakerTextToMotion.Model.V1, ITimelineBakerTextToMotion.Model.V2 };
        readonly string[] m_InferenceModels = { "V1", "V2" };

        VisualElement m_Section;
        TextArea m_PromptField;
        Button m_GenerateButton;
        TouchSliderInt m_TakesSlider;
        TouchSliderFloat m_DurationSlider;
        Dropdown m_ModelDropdown;
        LibraryUI m_Library;
        
        bool m_LastKeyWasReturn;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<TakesUI, UxmlTraits> { }
#endif

        public TakesUI()
            : base(k_TakesUIUssClassName) { }

        public void FindComponents()
        {
            m_Section = this.Q<VisualElement>(k_SectionName);
            m_PromptField = this.Q<TextArea>(k_PromptFieldName);
            m_GenerateButton = this.Q<Button>(k_GenerateButtonName);
            m_GenerateButton.AddToClassList("deeppose-important-button");
            m_TakesSlider = this.Q<TouchSliderInt>(k_TakesSliderName);
            m_DurationSlider = this.Q<TouchSliderFloat>(k_DurationSliderName);
            m_ModelDropdown = this.Q<Dropdown>(k_ModelDropdownName);

            var generations = this.Q<VisualElement>(k_TakesGeneration);
            generations.AddManipulator(new SessionStatusTracker());
#if UNITY_MUSE_ANIMATE_ENABLE_V2M
            this.Q<Accordion>(k_OptionalGeneratorsName).style.display = DisplayStyle.Flex;
#endif
        }

        public void RegisterComponents()
        {
            m_PromptField.RegisterCallback<ChangingEvent<string>>(OnPromptChanged, TrickleDown.TrickleDown);
            m_PromptField.RegisterCallback<FocusInEvent>(OnPromptFocusIn);
            m_PromptField.RegisterCallback<FocusOutEvent>(OnPromptFocusOut);
            m_PromptField.RegisterCallback<KeyDownEvent>(OnPromptKeyDown, TrickleDown.TrickleDown);

            m_TakesSlider.RegisterCallback<ChangeEvent<int>>(OnTakesSliderChanged);
            m_DurationSlider.RegisterCallback<ChangeEvent<float>>(OnDurationSliderChanged);
            m_GenerateButton.RegisterCallback<ClickEvent>(OnGenerateClicked);
            m_ModelDropdown.RegisterValueChangedCallback(OnModelDropdownChanged);
        }

        public void UnregisterComponents()
        {
            m_PromptField.UnregisterCallback<ChangingEvent<string>>(OnPromptChanged);
            m_PromptField.UnregisterCallback<FocusInEvent>(OnPromptFocusIn);
            m_PromptField.UnregisterCallback<FocusOutEvent>(OnPromptFocusOut);
            m_PromptField.UnregisterCallback<KeyDownEvent>(OnPromptKeyDown);

            m_TakesSlider.UnregisterCallback<ChangeEvent<int>>(OnTakesSliderChanged);
            m_DurationSlider.UnregisterCallback<ChangeEvent<float>>(OnDurationSliderChanged);
            m_GenerateButton.UnregisterCallback<ClickEvent>(OnGenerateClicked);
            m_ModelDropdown.UnregisterValueChangedCallback(OnModelDropdownChanged);
        }

        public void SetModel(TakesUIModel model)
        {
            Log("SetModel(" + model + ")");
            UnregisterModel();
            m_Model = model;
            RegisterModel();
            Update();
            UpdateDropdown();
        }

        void UpdateDropdown()
        {
            if (m_Model == null)
                return;
            
            // Update model dropdown
            m_ModelDropdown.bindItem = (item, i) => item.label = m_InferenceModels[i];
            m_ModelDropdown.sourceItems = m_InferenceModelsType;
            m_ModelDropdown.SetValueWithoutNotify( new []{ Array.IndexOf(m_InferenceModelsType, m_Model.InferenceModel) });
        }
        
        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged += OnModelChangedProperty;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnChanged -= OnModelChangedProperty;
            m_Model = null;
        }

        public void Update()
        {
            if (m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            UpdateVisibility();
            UpdateBusy();
            UpdatePrompt();
            UpdateDuration();
        }

        public void FocusPrompt()
        {
            m_PromptField.Focus();
        }

        void UpdateVisibility()
        {
            style.display = m_Model.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void UpdateBusy()
        {
            UpdateGenerateButton();
        }

        void UpdatePrompt()
        {
            m_PromptField.value = m_Model.Prompt;
            UpdateGenerateButton();
        }

        void UpdateSeed()
        {
            // No editable seed yet
        }

        void UpdateTakesAmount()
        {
            m_TakesSlider.value = m_Model.TakesAmount;
        }

        void UpdateGenerateButton()
        {
            m_GenerateButton.SetEnabled(!m_Model.IsBusy && !string.IsNullOrEmpty(m_Model.Prompt));
        }

        void UpdateDuration()
        {
            m_DurationSlider.style.display = GetModelType() == ITimelineBakerTextToMotion.Model.V2 ? DisplayStyle.Flex : DisplayStyle.None;
            m_DurationSlider.value = m_Model.Duration;
        }

        ITimelineBakerTextToMotion.Model GetModelType() => m_InferenceModelsType[m_ModelDropdown.value.First()];

        void OnModelSubmitPrompt()
        {
            m_PromptField.Blur();
            m_Model?.RequestGenerate();
        }

        void OnModelFocusPrompt()
        {
            FocusPrompt();
        }

        void OnModelChangedPrompt(string prompt)
        {
            UpdatePrompt();
        }

        void OnModelChangedProperty(TakesUIModel.Property property)
        {
            switch (property)
            {
                case TakesUIModel.Property.Visibility:
                    UpdateVisibility();
                    break;

                case TakesUIModel.Property.Prompt:
                    UpdatePrompt();
                    break;

                case TakesUIModel.Property.Seed:
                    UpdateSeed();
                    break;

                case TakesUIModel.Property.TakesAmount:
                    UpdateTakesAmount();
                    break;

                case TakesUIModel.Property.Duration:
                    UpdateDuration();
                    break;

                case TakesUIModel.Property.IsWriting:
                    break;

                case TakesUIModel.Property.IsBakingTextToMotion:
                    UpdateBusy();
                    break;
            }
        }

        void OnPromptChanged(ChangingEvent<string> evt)
        {
            if (m_Model == null)
                return;

            m_Model.Prompt = evt.newValue;

#if UNITY_2023_2_OR_NEWER
            evt.StopPropagation();
            focusController.IgnoreEvent(evt);
#else
            evt.PreventDefault();
#endif
        }

        void OnTakesSliderChanged(ChangeEvent<int> evt)
        {
            if (m_Model == null)
                return;

            m_Model.TakesAmount = evt.newValue;

#if UNITY_2023_2_OR_NEWER
            evt.StopPropagation();
            focusController.IgnoreEvent(evt);
#else
            evt.PreventDefault();
#endif
        }

        void OnDurationSliderChanged(ChangeEvent<float> evt)
        {
            if (m_Model == null)
                return;

            m_Model.Duration = evt.newValue;

#if UNITY_2023_2_OR_NEWER
            evt.StopPropagation();
            focusController.IgnoreEvent(evt);
#else
            evt.PreventDefault();
#endif
        }

        void OnModelDropdownChanged(ChangeEvent<IEnumerable<int>> changeEvent)
        {
            if (m_Model == null)
                return;

            m_Model.InferenceModel = m_InferenceModelsType[changeEvent.newValue.First()];
            UpdateDuration();
        }

        void OnPromptFocusOut(FocusOutEvent evt)
        {
            if (m_Model == null)
                return;

            m_Model.IsWriting = false;
        }

        void OnPromptFocusIn(FocusInEvent evt)
        {
            if (m_Model == null)
                return;

            m_Model.IsWriting = true;
        }

        void OnPromptKeyDown(KeyDownEvent evt)
        {
            // This logic is suggested by Mickael Bonfill to trap the return key event so we submit the prompt instead
            // of inserting a line break.
            // During a key press we receive 2 keydown events, one with the keycode and one without.
            // UITK is weird like this. This is why we should use manipulators whenever possible since they abstract
            // away crazy event handling logic.
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                m_LastKeyWasReturn = true;
                return;
            }

            if (evt.keyCode is KeyCode.None && m_LastKeyWasReturn)
            {
#if UNITY_2023_2_OR_NEWER
                evt.StopPropagation();
                focusController.IgnoreEvent(evt);
#else
                evt.PreventDefault();
#endif
                if (!string.IsNullOrWhiteSpace(m_Model.Prompt))
                {
                    m_Model?.RequestGenerate();
                }
            }

            m_LastKeyWasReturn = false;
        }

        void OnGenerateClicked(ClickEvent evt)
        {
            m_Model?.RequestGenerate();
        }

        // [Section] Debugging

        void Log(string msg)
        {
            if (!ApplicationConstants.DebugTakesUI)
                return;

            Debug.Log(GetType().Name + " -> " + msg);
        }

        
    }
}
