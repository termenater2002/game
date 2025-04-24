using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
    class TrainingRoundsContent : ExVisualElement
    {
        Text m_TitleText;
        StyleData m_StyleData;
        EventBus m_EventBus;
        TrainingRoundsView m_TrainingRoundsView;

        public TrainingRoundsContent()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingRoundsContentStyleSheet));
            name = "StyleTrainerRoundsContent";

            CreateHeaderUI();
            CreateRoundsUI();
            UpdateUI();
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<TrainingSetDataSourceChangedEvent>(OnTrainingSetDataSourceChanged);
        }

        void OnTrainingSetDataSourceChanged(TrainingSetDataSourceChangedEvent arg0)
        {
            m_StyleData = arg0?.styleData;
            UpdateUI();
        }

        void CreateHeaderUI()
        {
            var headerContainer = new ExVisualElement();
            headerContainer.AddToClassList("styletrainer-trainingrounds-header-container");

            var backButton = new Button();
            backButton.AddToClassList("styletrainer-trainingrounds-back-button");
            backButton.title = StringConstants.trainingBackButton;
            backButton.clicked += OnBackButtonClicked;

            var titleContainer = new ExVisualElement();
            titleContainer.AddToClassList("styletrainer-trainingrounds-title-container");

            m_TitleText = new Text();
            m_TitleText.text = StringConstants.trainingSelectRoundText;
            m_TitleText.AddToClassList("styletrainer-trainingrounds-title");

            titleContainer.Add(m_TitleText);
            headerContainer.Add(backButton);
            headerContainer.Add(titleContainer);

            Add(headerContainer);
        }

        void OnBackButtonClicked()
        {
            m_EventBus.SendEvent(new SeeTrainedStyleEvent());
        }

        void CreateRoundsUI()
        {
            var roundsContainer = new ExVisualElement();
            m_TrainingRoundsView = new TrainingRoundsView();
            roundsContainer.Add(m_TrainingRoundsView);
            Add(roundsContainer);
        }

        void UpdateUI()
        {
            m_TrainingRoundsView?.SetEventBus(m_EventBus);
            m_TrainingRoundsView?.SetStyleData(m_StyleData);
        }
    }
}
