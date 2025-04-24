using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Linq;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.Common
{
    class GenerateOperatorUI : ExVisualElement
    {
        const string museCostSprite = "<sprite=\"MuseIconWhite\" name=\"MuseIconWhite\">";
        const string disabledMuseCostSprite = "<sprite=\"MuseIconWhiteDisabled\" name=\"MuseIconWhiteDisabled\">";
        protected Model m_CurrentModel;
        Button m_CurrentGenerateButton;
        VisualElement m_Content = new() { name = "muse-node-content" };
        protected Text m_NodeTitle;
        protected TouchSliderInt m_GenerationCountSlider;
        protected OperatorData m_OperatorData;
        protected VisualElement generateButtonContainer { get; set; }
        protected VisualElement generatorCountSliderContainer { get; set; }
        CircularProgress m_CircularProgress;
        bool m_IsLoading;

        public VisualElement GetContent() { return m_Content; }

        public GenerateOperatorUI()
        {}

        public void SetupUIBasics(Model model, OperatorData operatorData)
        {
            m_OperatorData = operatorData;
            m_CurrentModel = model;
            passMask = Passes.Clear | Passes.OutsetShadows | Passes.BackgroundColor;

            AddToClassList("muse-node");
            name = "generate-node";
            m_NodeTitle = new Text();
            m_NodeTitle.text = "Generation";
            m_NodeTitle.AddToClassList("muse-node__title");
            m_NodeTitle.AddToClassList("bottom-gap");
            Add(m_NodeTitle);
            Add(m_Content);
            SetupGeneratorModelSelector();
        }

        public virtual void SetupGeneratorModelSelector()
        {
            
        }

        public virtual void SetupGeneratorCountSlider(string generateType)
        {
            m_GenerationCountSlider = new TouchSliderInt { tooltip = TextContent.operatorGenerateNumberTooltip };
            m_GenerationCountSlider.name = "image-count-slider";
            m_GenerationCountSlider.AddToClassList("bottom-gap");
            m_GenerationCountSlider.label = generateType;
            m_GenerationCountSlider.lowValue = 1;
            m_GenerationCountSlider.highValue = 10;
            m_GenerationCountSlider.value = int.Parse(m_OperatorData.settings[1]);
            m_GenerationCountSlider.RegisterValueChangedCallback(evt =>
            {
                m_OperatorData.settings[1] = evt.newValue.ToString();
                m_CurrentModel.ResetCost();
            });
            
            var container = generatorCountSliderContainer ?? m_Content;
            container.Add(m_GenerationCountSlider);
        }

        public virtual void SetupGenerateButton()
        {
            m_CurrentGenerateButton = new Button();
            m_CurrentGenerateButton.name = "generate-button";
            ChangeGenerateButtonTextWithCost(m_CurrentModel.CostInMusePoints);
            m_CurrentGenerateButton.AddToClassList("muse-theme");

            m_CurrentGenerateButton.AddToClassList("muse-node__button");
            m_CurrentGenerateButton.Q(Button.titleContainerUssClassName).style.flexDirection = FlexDirection.Row;
            m_CurrentGenerateButton.Q(Button.titleContainerUssClassName).style.justifyContent = Justify.Center;
            m_CurrentGenerateButton.variant = ButtonVariant.Accent;

            m_CurrentGenerateButton.clicked += m_CurrentModel.GenerateButtonClicked;

            SetGenerateButtonEnabled(false);

            var container = generateButtonContainer ?? m_Content;
            container.Add(m_CurrentGenerateButton);
        }

        public void SetupLoading()
        {
            m_CircularProgress = new CircularProgress()
            {
                style =
                {
                    width = 16,
                    height = 16,
                    marginLeft = 5,
                    display = DisplayStyle.Flex,
                    alignSelf = Align.FlexEnd
                },
                tooltip = TextContent.mppCostCircularProgressTooltip,
                pickingMode = PickingMode.Position
            };
        }

        public void SubscribeToEvents(Action OnDataUpdate)
        {
            OnDataUpdate += () =>
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (m_OperatorData.settings[0] != "")
                {
                    dropdown.SetValueWithoutNotify(new[] {ModesFactory.GetModeIndexFromKey(m_OperatorData.settings[0])});
                }
#endif

                if (m_OperatorData.settings[1] != "")
                {
                    m_GenerationCountSlider.value = int.Parse(m_OperatorData.settings[1]);
                }
            };

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected void OnModeSelected(ModeStruct @struct)
        {
            var prefsKey = @struct.type + "_termsAccepted";
#if UNITY_EDITOR
            var alreadyAccepted = EditorPrefs.GetBool(prefsKey, false);
#else
            var alreadyAccepted = PlayerPrefs.GetInt(prefsKey, 0) == 1;
#endif
            if (!string.IsNullOrEmpty(@struct.eula_url) && !alreadyAccepted)
                AccountUtility.DisplayThirdPartyTerms(panel.visualTree.Q<Panel>(), @struct,
                    acceptedTerms =>
                    {
                        if (acceptedTerms)
                        {
#if UNITY_EDITOR
                            EditorPrefs.SetBool(prefsKey, true);
#else
                            PlayerPrefs.SetInt(prefsKey, 1);
#endif
                            m_CurrentModel.ModeChanged(ModesFactory.GetModeIndexFromKey(@struct.type));
                        }
                    });
            else
                m_CurrentModel.ModeChanged(ModesFactory.GetModeIndexFromKey(@struct.type));
        }

        internal void SetCount(int count)
        {
            m_GenerationCountSlider.value = count;
        }

        void SetGenerateButtonEnabled(bool value)
        {
            if (m_CurrentGenerateButton == null)
                return;
            
            m_CurrentGenerateButton.SetEnabled(value);
            
            var oldTitle = m_CurrentGenerateButton.title;
            if (oldTitle == null)
                return;
            m_CurrentGenerateButton.title = value ? oldTitle.Replace(disabledMuseCostSprite, museCostSprite)
                : oldTitle.Replace(museCostSprite, disabledMuseCostSprite);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_CurrentModel.GetData<GenerateButtonData>().OnModified -= OnToggleGenerateButton;
            m_CurrentModel.OnCostChanged -= ChangeGenerateButtonTextWithCost;
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_CurrentModel.GetData<GenerateButtonData>().OnModified += OnToggleGenerateButton;
            m_CurrentModel.OnCostChanged += ChangeGenerateButtonTextWithCost;
            m_CurrentModel.OnSupportCostSimulationChanged += OnSupportCostSimulationChanged;
        }

        void AddLoadingToGenerateButtonText()
        {
            m_CurrentGenerateButton.Q(Button.titleContainerUssClassName).Add(m_CircularProgress);
        }

        void RemoveLoadingFromGenerateButtonText()
        {
            var titleContainer = m_CurrentGenerateButton.Q(Button.titleContainerUssClassName);
            if (titleContainer.hierarchy.IndexOf(m_CircularProgress) > 0)
            {
                titleContainer.Remove(m_CircularProgress);
            }
        }

        void OnSupportCostSimulationChanged(bool _)
        {
            ChangeGenerateButtonTextWithCost(m_CurrentModel.CostInMusePoints);
        }

        void ChangeGenerateButtonTextWithCost(int? cost)
        {
            if (Model.shouldShowCost && m_CurrentModel.SupportCostSimulation)
            {
                if (cost != null)
                {
                    m_IsLoading = false;
                    OnToggleGenerateButton();
                    var iconString = m_CurrentGenerateButton.enabledSelf ? museCostSprite : disabledMuseCostSprite;
                    var spriteAndCostText = $" - {iconString} {cost}";
                    ChangeGenerateButtonText(spriteAndCostText);
                    RemoveLoadingFromGenerateButtonText();
                    return;
                }
                m_IsLoading = true;
                OnToggleGenerateButton();
                ChangeGenerateButtonText(" - ");
                AddLoadingToGenerateButtonText();
            }
            else
            {
                m_IsLoading = false;
                OnToggleGenerateButton();
                ChangeGenerateButtonText("");
                RemoveLoadingFromGenerateButtonText();
            }
        }

        protected virtual void ChangeGenerateButtonText(string text)
        {
            m_CurrentGenerateButton.title = $"{TextContent.mppGenerateButtonText}{text}";
        }

        void OnToggleGenerateButton()
        {
            if (m_CurrentGenerateButton != null)
            {
                var data = m_CurrentModel.GetData<GenerateButtonData>();
                SetGenerateButtonEnabled(data.isEnabled && !m_IsLoading);
                m_CurrentGenerateButton.tooltip = data.tooltip;
            }
        }
    }
}
