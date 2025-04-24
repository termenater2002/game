using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class SampleOutputPromptExample : ExVisualElement
    {
        PreviewImage m_PreviewImage;
        Text m_PromptText;
        ActionButton m_FavoriteButton;
        EventBus m_EventBus;

        const string k_StarFilledIconClass = "star-filled";
        const string k_StarIconClass = "star";
        CheckPointData m_CheckPointData;
        SampleOutputData m_SampleOutputData;

        public SampleOutputPromptExample()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputPromptExampleStyleSheet));
            AddToClassList("styletrainer-sampleoutputpromptexample-item");

            var container = new ExVisualElement();
            container.AddToClassList("styletrainer-sampleoutputpromptexample-container");

            m_FavoriteButton = new ActionButton()
            {
                icon = k_StarIconClass
            };
            m_FavoriteButton.AddToClassList("styletrainer-sampleoutputpromptexample-favoritebutton");
            m_FavoriteButton.clicked += OnFavoriteButtonClicked;

            m_PreviewImage = new PreviewImage();
            m_PreviewImage.AddToClassList("styletrainer-sampleoutputpromptexample-image");
            m_PreviewImage.Add(m_FavoriteButton);

            m_PromptText = new Text("Prompt example");
            m_PromptText.AddToClassList("styletrainer-sampleoutputpromptexample-prompt");

            var imageContainer = new ExVisualElement();
            imageContainer.AddToClassList("styletrainer-sampleoutputpromptexample-image-container");
            imageContainer.Add(m_PreviewImage);

            container.Add(imageContainer);
            container.Add(m_PromptText);

            Add(container);
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<FavoritePreviewSampleOutputEvent>(OnFavoriteSampleChanged);
        }

        void OnFavoriteButtonClicked()
        {
            m_EventBus.SendEvent(new FavoritePreviewSampleOutputEvent
            {
                checkPointData = m_CheckPointData,
                favoriteSampleOutputData = m_SampleOutputData
            });
        }

        void OnFavoriteSampleChanged(FavoritePreviewSampleOutputEvent arg0)
        {
            var isFavorite = arg0.favoriteSampleOutputData == m_SampleOutputData;

            m_FavoriteButton.EnableInClassList("is-favorite", isFavorite);
            m_FavoriteButton.icon = isFavorite ? k_StarFilledIconClass : k_StarIconClass;
        }

        public void SetData(CheckPointData checkpointData, SampleOutputData outputData)
        {
            m_CheckPointData = checkpointData;
            m_SampleOutputData = outputData;
            m_PreviewImage.SetArtifact(outputData.imageArtifact);
            m_PromptText.text = outputData.prompt;
        }
    }
}