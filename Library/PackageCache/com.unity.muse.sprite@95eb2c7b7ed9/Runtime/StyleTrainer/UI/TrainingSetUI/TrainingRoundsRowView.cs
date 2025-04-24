using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class TrainingRoundsRowView : AccordionItem
    {
        CheckPointData m_CheckPointData;
        StyleData m_StyleData;
        EventBus m_EventBus;
        Radio m_Radio;
        int m_RoundNumber;

        public TrainingRoundsRowView(int roundNumber, CheckPointData checkPointData, StyleData styleData, EventBus eventBus)
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingRoundsViewStyleSheet));
            m_RoundNumber = roundNumber;
            m_CheckPointData = checkPointData;
            m_StyleData = styleData;
            m_EventBus = eventBus;

            CreateUI();
        }

        public CheckPointData GetCheckPointData()
        {
            return m_CheckPointData;
        }

        void CreateUI()
        {
            m_Radio = new Radio
            {
                emphasized = true
            };

            title = $"Round {m_RoundNumber}";

            var header = this.Q<ExVisualElement>(className: "appui-accordionitem__header");
            header.Insert(0, m_Radio);

            var imageContainer = new ExVisualElement();
            imageContainer.AddToClassList("styletrainer-trainingrounds-image-container");

            for (int i = 0; i < m_CheckPointData.validationImageData.Count; i++)
            {
                var smallContainer = new ExVisualElement();
                smallContainer.AddToClassList("styletrainer-trainingrounds-image-small-container");
                var paddingTop = Length.Percent(100f / m_CheckPointData.validationImageData.Count);
                smallContainer.style.paddingTop = paddingTop;
                var previewImage = new PreviewImage
                {
                    name = "StyleTrainingRoundPreviewImage"
                };
                previewImage.AddToClassList("styletrainer-trainingrounds-preview-image");

                previewImage.SetArtifact(m_CheckPointData.validationImageData[i].imageArtifact);

                smallContainer.Add(previewImage);
                imageContainer.Add(smallContainer);
            }

            Add(imageContainer);

            m_Radio.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    m_EventBus?.SendEvent(new SetFavouriteCheckPointEvent
                    {
                        checkPointGUID = m_CheckPointData.guid,
                        styleData = m_StyleData
                    });
                }
            });

            if (m_StyleData?.GetFavouriteOrLatestCheckPoint()?.guid == m_CheckPointData?.guid)
            {
                m_Radio.value = true;
                value = true;
            }

            m_EventBus.RegisterEvent<SetFavouriteCheckPointEvent>(OnSetFavouriteCheckPoint);
        }

        void OnSetFavouriteCheckPoint(SetFavouriteCheckPointEvent arg0)
        {
            m_Radio.value = arg0.checkPointGUID == m_CheckPointData.guid;
        }
    }
}
