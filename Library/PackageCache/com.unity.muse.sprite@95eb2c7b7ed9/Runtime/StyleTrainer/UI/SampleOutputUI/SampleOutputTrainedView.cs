using System;
using System.Collections.Generic;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
    class SampleOutputTrainedView : ExVisualElement
    {
        StyleData m_StyleData;
        CheckPointData m_FavoriteCheckPointData;
        List<SampleOutputPromptExample> m_SampleOutputPromptExamples;
        EventBus m_EventBus;
        ExVisualElement m_ExamplesContainer;

        public SampleOutputTrainedView()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputTrainedViewStyleSheet));

            var titleContainer = new ExVisualElement();
            titleContainer.AddToClassList("styletrainer-sampleoutputtrainedview-title-container");

            var title = new Text(StringConstants.trainingSelectedTrainingRound);
            title.AddToClassList("styletrainer-sampleoutputtrainedview-examples-title");

            titleContainer.Add(title);

            var descriptionContainer = new ExVisualElement();
            descriptionContainer.AddToClassList("styletrainer-sampleoutputtrainedview-description-container");

            var description = new Text(StringConstants.trainingRoundDescription);
            description.AddToClassList("styletrainer-sampleoutputtrainedview-description-text");

            var chooseRoundsButton = new Button();
            chooseRoundsButton.title = StringConstants.trainingChooseRoundButton;
            chooseRoundsButton.clicked += OnChooseRoundClicked;

            descriptionContainer.Add(description);
            descriptionContainer.Add(chooseRoundsButton);

            m_ExamplesContainer = new ExVisualElement();
            m_ExamplesContainer.AddToClassList("styletrainer-sampleoutputtrainedview-examples-container");

            m_SampleOutputPromptExamples = new List<SampleOutputPromptExample>();

            Add(titleContainer);
            Add(descriptionContainer);
            Add(m_ExamplesContainer);
        }

        void OnChooseRoundClicked()
        {
            m_EventBus.SendEvent(new ChooseRoundsButtonClickEvent());
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
        }

        public void SetStyleData(StyleData styleData)
        {
            m_StyleData = styleData;
            if (m_StyleData != null)
            {
                m_FavoriteCheckPointData = m_StyleData.GetFavouriteOrLatestCheckPoint();
                SetupFavoriteImages();
            }
        }

        void SetupFavoriteImages()
        {
            if (m_FavoriteCheckPointData == null || m_FavoriteCheckPointData.state == EState.Error
                || m_FavoriteCheckPointData.validationImageData?.Count == 0)
                return;

            m_ExamplesContainer.Clear();
            m_SampleOutputPromptExamples.Clear();

            var validationData = m_FavoriteCheckPointData?.validationImageData;
            for (int i = 0; i < validationData?.Count; i++)
            {
                var outputData = validationData[i];

                var exampleItem = new SampleOutputPromptExample();
                exampleItem.SetData(m_FavoriteCheckPointData, outputData);
                exampleItem.SetEventBus(m_EventBus);

                m_SampleOutputPromptExamples.Add(exampleItem);
                m_ExamplesContainer.Add(exampleItem);

                if (i == validationData.Count - 1) // We want to remove the margin on the last item
                {
                    exampleItem.AddToClassList("styletrainer-sampleoutputpromptexample-item__no-margin");
                }
            }

            var favorite = m_FavoriteCheckPointData.GetFavoriteSampleOutputData();

            m_EventBus.SendEvent(new FavoritePreviewSampleOutputEvent
            {
                checkPointData = m_FavoriteCheckPointData,
                favoriteSampleOutputData = favorite
            });
        }
    }
}
