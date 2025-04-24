using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.StyleTrainer
{
    partial class TrainingItemGridItem : ExVisualElement
    {
        ActionButton m_DeleteButton;
        int m_ItemIndex;
        public Action<int> OnDeleteClicked;
        PreviewImage m_PreviewImage;
        StyleData m_StyleData;
        EventBus m_EventBus;

        public TrainingItemGridItem(StyleData styleData)
        {
            m_StyleData = styleData;

            name = "TrainingItemGridItem";
            AddToClassList("styletrainer-trainingitemgriditem");
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingItemGridItemStyleSheet));

            m_PreviewImage = new PreviewImage
            {
             name = "PreviewImage"
            };
            m_PreviewImage.AddToClassList("styletrainer-trainingitemgriditem__previewimage");
            Add(m_PreviewImage);
            m_DeleteButton = new ActionButton
            {
             name = "DeleteButton",
             icon = "delete"
            };
            m_DeleteButton.AddToClassList("styletrainer-trainingitemgriditem__deletebutton");
            m_DeleteButton.focusable = false;
            Add(m_DeleteButton);
            focusable = true;
            tabIndex = 10;
            BindElements();
        }

        void BindElements()
        {
            m_DeleteButton.clicked += OnDeleteButtonClicked;
            m_PreviewImage.image = Utilities.placeHolderTexture;
#if UNITY_WEBGL && !UNITY_EDITOR
            m_DeleteButton.AddToClassList("delete-button-webgl");
#endif
        }

        public int itemIndex
        {
            set => m_ItemIndex = value;
        }

        void OnDeleteButtonClicked()
        {
            OnDeleteClicked?.Invoke(m_ItemIndex);
        }

        public void SetPreviewImage(ImageArtifact imageArtifact)
        {
            m_PreviewImage.SetArtifact(imageArtifact);
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
        }

        public void CanModify(bool canModify)
        {
            m_DeleteButton.SetEnabled(canModify);
        }

        public void CreatePlusButton()
        {
            if (!CanModify())
            {
                return;
            }

            AddToClassList("styletrainer-trainingitemgriditem__plus-button");
            m_PreviewImage.image = null;
            m_DeleteButton.SetEnabled(false);
            m_DeleteButton.clicked -= OnDeleteButtonClicked;
            m_DeleteButton.style.display = DisplayStyle.None;

            var plusIconButton = new Button();
            plusIconButton.AddToClassList("styletrainer-trainingitemgriditem__plus-button__container");
            plusIconButton.clicked += () =>
            {
                m_EventBus.SendEvent(new AddImagesToTrainingSetEvent());
            };

            var plusIcon = new Icon{ iconName = "plus" };
            plusIconButton.Add(plusIcon);
            plusIconButton.focusable = false;

            m_PreviewImage.Add(plusIconButton);
        }

        bool CanModify()
        {
            return !Utilities.ValidStringGUID(m_StyleData?.trainingSetData[0].guid) && m_StyleData?.state != EState.Training;
        }
    }
}