using System;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerProjectEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleModelInfoEditor : ExVisualElement
    {
        public const int sampleOutputTab = 0;
        public const int trainingSetTab = 1;
        public const float initialThumbnailSliderValue = 1f;

        ExVisualElement m_StyleModelEditorContainer;
        ExVisualElement m_StyleModelInfoEditorContent;
        ExVisualElement m_StyleModelRoundsContainer;
        ExVisualElement m_StyleModelFooter;
        TrainingRoundsContent m_TrainingRoundsContent;
        EventBus m_EventBus;
        ScrollView m_ScrollViewMainContainer;
        VisualElement m_SplashScreen;
        Text m_SplashScreenText;
        IStyleModelEditorContent m_EditorContent;
        StyleData m_StyleData;

        Button m_DeleteStyleButton;
        Button m_TrainStyleButton;
        Button m_TrainingStyleButton;
        Button m_PublishStyleButton;
        Button m_UnpublishStyleButton;
        Button m_SelectFavoriteStyleButton;
        const string k_ClassHiddenButton = "styletrainer-stylemodelinfoeditor__buttons-container";

        static readonly CustomStyleProperty<string> k_MarginInfoEditorContainerProperty =
            new ("--margin-stylemodelinfoeditor-container");

        internal static StyleModelInfoEditor CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.styleModelInfoEditorTemplate);
            var ve = (StyleModelInfoEditor)visualTree.CloneTree().Q("StyleModelInfoEditor");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.styleModelInfoEditorStyleSheet));
            ve.BindElements();
            return ve;
        }

        void BindElements()
        {
            name = "StyleModelInfoEditor";

            m_StyleModelFooter = this.Q<ExVisualElement>(className: "styletrainer-stylemodelinfoeditor__footer");
            m_StyleModelEditorContainer = this.Q<ExVisualElement>("StyleModelEditorContainer");
            m_StyleModelEditorContainer.RegisterCallback<CustomStyleResolvedEvent>(OnEditorContainerStyleResolved);
            m_StyleModelInfoEditorContent = m_StyleModelEditorContainer.Q<ExVisualElement>("StyleModelInfoEditorContent");
            var contentEditor = StyleModelInfoEditorContent.CreateFromUxml();
            m_EditorContent = contentEditor;
            m_StyleModelInfoEditorContent.Add(contentEditor);

            m_DeleteStyleButton = this.Q<Button>("DeleteStyleButton");
            m_DeleteStyleButton.clicked += OnDeleteStyleClicked;

            m_TrainStyleButton = this.Q<Button>("TrainStyleButton");
            m_TrainStyleButton.clicked += OnTrainStyleClicked;
            m_TrainStyleButton.AddToClassList(k_ClassHiddenButton);

            m_TrainingStyleButton = this.Q<Button>("TrainingStyleButton");
            m_TrainingStyleButton.AddToClassList(k_ClassHiddenButton);

            m_PublishStyleButton = this.Q<Button>("PublishStyleButton");
            m_PublishStyleButton.clicked += OnPublishStyleClicked;
            m_PublishStyleButton.AddToClassList(k_ClassHiddenButton);

            m_UnpublishStyleButton = this.Q<Button>("UnpublishStyleButton");
            m_UnpublishStyleButton.clicked += OnUnpublishStyleClicked;
            m_UnpublishStyleButton.AddToClassList(k_ClassHiddenButton);

            m_SelectFavoriteStyleButton = this.Q<Button>("SelectFavoriteStyleButton");
            m_SelectFavoriteStyleButton.clicked += OnSelectFavoriteStyleClicked;
            m_SelectFavoriteStyleButton.AddToClassList(k_ClassHiddenButton);

            m_StyleModelRoundsContainer = new ExVisualElement
            {
                name = "StyleTrainerRounds"
            };
            m_StyleModelEditorContainer.Add(m_StyleModelRoundsContainer);
            m_TrainingRoundsContent = new TrainingRoundsContent();
            m_StyleModelRoundsContainer.Add(m_TrainingRoundsContent);

            m_ScrollViewMainContainer = this.Q<ScrollView>();
            m_SplashScreen = this.Q<VisualElement>("StyleModelEditorSplashScreen");
            m_SplashScreenText = this.Q<Text>("HintText");
            UpdateMainScreen();
        }

        void OnEditorContainerStyleResolved(CustomStyleResolvedEvent evt)
        {
            const int anchorBorderWidth = 1;
            evt.customStyle.TryGetValue(k_MarginInfoEditorContainerProperty, out var margin);
            if (int.TryParse(margin.Replace("px", ""), out var marginInt))
            {
                // Add 1px to the margin to account for the border of the anchor
                // When trying to get the border width of the anchor resolved style, width was NaN
                if (m_StyleModelEditorContainer != null)
                {
                    m_StyleModelEditorContainer.style.marginLeft = marginInt + anchorBorderWidth;
                }

                if (m_StyleModelFooter != null)
                {
                    m_StyleModelFooter.style.marginLeft = marginInt + anchorBorderWidth;
                }
            }
        }

        void OnStyleModelListSelectionChanged(StyleModelListSelectionChangedEvent arg0)
        {
            if (m_StyleData != null)
            {
                m_StyleData.OnStateChanged -= OnStyleDataStateChange;
                m_StyleData.OnDataChanged -= OnStyleDataChanged;
            }

            m_StyleData = arg0.styleData;
            if (m_StyleData != null)
            {
                m_StyleData.OnStateChanged += OnStyleDataStateChange;
                m_StyleData.OnDataChanged += OnStyleDataChanged;
            }

            if (m_StyleData is not null)
            {
                m_EventBus.SendEvent(new TrainingSetDataSourceChangedEvent
                {
                    styleData = m_StyleData,
                    trainingSetData = m_StyleData.trainingSetData
                });
                m_EventBus.SendEvent(new SampleOutputDataSourceChangedEvent
                {
                    styleData = m_StyleData,
                    sampleOutput = m_StyleData.sampleOutputPrompts
                });
            }

            UpdateMainScreen();
        }

        void OnStyleDataStateChange(StyleData styleData)
        {
            UpdateMainScreen();
        }

        void OnStyleDataChanged(StyleData styleData)
        {
            UpdateTrainStyleButton();
        }

        void UpdateMainScreen()
        {
            m_TrainStyleButton.EnableInClassList(k_ClassHiddenButton, m_StyleData == null || m_StyleData.state != EState.New);
            m_TrainingStyleButton.AddToClassList(k_ClassHiddenButton);
            m_SelectFavoriteStyleButton.AddToClassList(k_ClassHiddenButton);
            m_PublishStyleButton.AddToClassList(k_ClassHiddenButton);
            m_UnpublishStyleButton.AddToClassList(k_ClassHiddenButton);
            m_ScrollViewMainContainer.EnableInClassList("styletrainer-stylemodelinfoeditor__maincontainer-no-style-selected", m_StyleData == null);

            if (m_StyleData is not null)
            {
                m_SplashScreen.style.display = DisplayStyle.None;
                m_StyleModelEditorContainer.style.display = m_StyleData.state == EState.Loading ? DisplayStyle.None : DisplayStyle.Flex;

                if (m_StyleData.state == EState.Loaded)
                {
                    m_PublishStyleButton.EnableInClassList(k_ClassHiddenButton, m_StyleData.visible);
                    m_UnpublishStyleButton.EnableInClassList(k_ClassHiddenButton, !m_StyleData.visible);
                    m_PublishStyleButton.SetEnabled(true);
                    m_UnpublishStyleButton.SetEnabled(true);
                }

                if (m_StyleData.state == EState.Training)
                {
                    m_TrainingStyleButton.RemoveFromClassList(k_ClassHiddenButton);
                    m_TrainingStyleButton.SetEnabled(false);
                }

                UpdateTrainStyleButton();
            }
            else
            {
                m_SplashScreen.style.display = DisplayStyle.Flex;
                m_StyleModelEditorContainer.style.display = DisplayStyle.None;
            }

            m_StyleModelInfoEditorContent.style.display = DisplayStyle.Flex;
            m_StyleModelRoundsContainer.style.display = DisplayStyle.None;
            m_DeleteStyleButton.SetEnabled(m_StyleData != null);

            m_EditorContent.UpdateView();
        }

        void UpdateTrainStyleButton()
        {
            if (!m_TrainStyleButton.ClassListContains(k_ClassHiddenButton))
            {
                var enableTrainStyleButton = true;
                var validateTask = new ValidateTrainingParametersTask(m_StyleData, m_EventBus);
                var hasValidNameAndDescription = validateTask.HasValidNameAndDescription(null, false);
                var hasEmptyPrompts = validateTask.HasEmptyPrompts(null, false);
                var hasDuplicatePrompts = validateTask.HasDuplicatePrompts(null, false);
                var isTrainingSetImagesCountBelowMinSize = validateTask.IsTrainingSetImagesCountBelowMinSize(null, false);

                var trainStyleButtonTooltip = string.Empty;
                if (!hasValidNameAndDescription)
                {
                    trainStyleButtonTooltip += "Style name and description cannot be empty. ";
                }
                if (isTrainingSetImagesCountBelowMinSize)
                {
                    trainStyleButtonTooltip += $"Need at least {StyleTrainerConfig.config.minTrainingSetSize} training images. ";
                }
                if (hasEmptyPrompts)
                {
                    trainStyleButtonTooltip += $"Need at least {StyleTrainerConfig.config.minSampleSetSize} practice prompts. ";
                }
                if (hasDuplicatePrompts)
                {
                    trainStyleButtonTooltip += "Cannot have duplicate prompts. ";
                }

                m_TrainStyleButton.tooltip = "";
                if (!string.IsNullOrWhiteSpace(trainStyleButtonTooltip))
                {
                    enableTrainStyleButton = false;
                    m_TrainStyleButton.SetTooltipTemplate(new Text(trainStyleButtonTooltip));
                    m_TrainStyleButton.preferredTooltipPlacementOverride = new OptionalEnum<PopoverPlacement>(PopoverPlacement.Left);
                }
                else
                {
                    m_TrainStyleButton.SetTooltipTemplate(null);
                }

                m_TrainStyleButton.SetEnabled(enableTrainStyleButton);
            }
        }

        void OnAddClicked()
        {
            m_EditorContent.OnAddClicked();
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EditorContent.SetEventBus(eventBus);
            m_TrainingRoundsContent.SetEventBus(eventBus);
            m_EventBus.RegisterEvent<StyleModelListSelectionChangedEvent>(OnStyleModelListSelectionChanged);
            m_EventBus.RegisterEvent<StyleVisibilityButtonClickedEvent>(OnStyleVisibilityChanged);
            m_EventBus.RegisterEvent<ChooseRoundsButtonClickEvent>(OnChooseRoundsButtonClicked);
            m_EventBus.RegisterEvent<SeeTrainedStyleEvent>(OnSeeTrainedStyle);
            m_EventBus.RegisterEvent<StyleModelSourceChangedEvent>(OnStyleModelSourceChanged);
        }

        void OnStyleModelSourceChanged(StyleModelSourceChangedEvent arg0)
        {
            if(arg0.styleModels == null || arg0.styleModels.Count == 0)
            {
                m_SplashScreenText.text = StringConstants.createNewStyleText;
            }
            else
            {
                m_SplashScreenText.text = StringConstants.createOrSelectStyleText;
            }
        }

        void OnChooseRoundsButtonClicked(ChooseRoundsButtonClickEvent arg0)
        {
            m_StyleModelInfoEditorContent.style.display = DisplayStyle.None;
            m_StyleModelRoundsContainer.style.display = DisplayStyle.Flex;
        }

        void OnSeeTrainedStyle(SeeTrainedStyleEvent arg0)
        {
            if (m_StyleData?.state == EState.Loaded)
            {
                m_StyleModelInfoEditorContent.style.display = DisplayStyle.Flex;
                m_StyleModelRoundsContainer.style.display = DisplayStyle.None;
            }
        }

        void OnDeleteStyleClicked()
        {
            m_EventBus.SendEvent(new StyleDeleteButtonClickedEvent
            {
                styleData = m_StyleData
            });
        }

        void OnTrainStyleClicked()
        {
            m_EventBus.SendEvent(new GenerateButtonClickEvent());
        }

        void OnSelectFavoriteStyleClicked()
        {
        }

        void OnUnpublishStyleClicked()
        {
            var e = new StyleVisibilityButtonClickedEvent
            {
                styleData = m_StyleData,
                visible = false
            };
            m_EventBus.SendEvent(e);
        }

        void OnPublishStyleClicked()
        {
            var e = new StyleVisibilityButtonClickedEvent
            {
                styleData = m_StyleData,
                visible = true
            };
            m_EventBus.SendEvent(e);
        }

        void OnStyleVisibilityChanged(StyleVisibilityButtonClickedEvent evt)
        {
            m_PublishStyleButton.EnableInClassList(k_ClassHiddenButton, evt.visible);
            m_UnpublishStyleButton.EnableInClassList(k_ClassHiddenButton, !evt.visible);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleModelInfoEditor, UxmlTraits> { }
#endif
    }

    interface IStyleModelEditorContent
    {
        void SetEventBus(EventBus eventBus);
        void OnAddClicked();
        void UpdateView();
    }
}