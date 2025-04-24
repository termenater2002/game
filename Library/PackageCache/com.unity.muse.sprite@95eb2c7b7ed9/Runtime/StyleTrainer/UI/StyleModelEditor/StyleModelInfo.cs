using System;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleTrainerMainUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = Unity.Muse.AppUI.UI.TextField;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleModelInfo : ExVisualElement
    {
        Text m_StatusLabel;
        Text m_DescriptionTextCount;
        TextField m_Name;
        TextArea m_Description;
        PreviewImage m_PreviewImage;
        EventBus m_EventBus;
        StyleData m_StyleData;
        CircularProgress m_TrainingIcon;
        Icon m_ErrorIcon;
        const float k_OriginalDescriptionTextAreaHeight = 75f;

        public StyleModelInfo()
        {
            RegisterCallback<AttachToPanelEvent>(AttachToPanel);
        }

        void AttachToPanel(AttachToPanelEvent evt)
        {
            UnregisterCallback<AttachToPanelEvent>(AttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(DetachFromPanel);

            m_Name = this.Q<TextField>("StyleModelInfoDetailsName");
            m_Name.RegisterValueChangedCallback(OnNameChanged);
            m_Name.RegisterValueChangingCallback(OnNameChanging);
            m_Description = this.Q<TextArea>("StyleModelInfoDetailsDescription");
            m_Description.placeholder = StringConstants.styleDescriptionPlaceholder;
            m_Description.RegisterValueChangedCallback(OnDescriptionChanged);
            m_Description.RegisterValueChangingCallback(OnDescriptionChanging);

            m_DescriptionTextCount = this.Q<Text>("DescriptionTextCount");

            m_TrainingIcon = this.Q<CircularProgress>("TrainingIcon");
            m_ErrorIcon = this.Q<Icon>("ErrorIcon");
            m_StatusLabel = this.Q<Text>("StatusLabel");
            m_PreviewImage = this.Q<PreviewImage>("styletrainer-preview-image");
            SetPreviewImage(null);
        }

        void OnNameChanging(ChangingEvent<string> evt)
        {
            if (evt.newValue.Length > StyleData.maxNameLength)
            {
                m_Name.SetValueWithoutNotify(evt.previousValue);
            }

            m_EventBus.SendEvent(new ChangeStyleNameEvent{newStyleName = evt.newValue});
        }

        void OnNameChanged(ChangeEvent<string> evt)
        {
            if (!string.IsNullOrWhiteSpace(evt.newValue))
            {
                m_StyleData.title = evt.newValue;
            }
            else
            {
                m_Name.SetValueWithoutNotify(m_StyleData.title);
            }

            m_EventBus.SendEvent(new ChangeStyleNameEvent{newStyleName = m_StyleData.title});
        }

        void OnDescriptionChanging(ChangingEvent<string> evt)
        {
            if (evt.newValue.Length > StyleData.maxDescriptionLength)
            {
                m_Description.SetValueWithoutNotify(evt.previousValue);
            }

            m_DescriptionTextCount.text = $"{m_Description.value.Length}/{StyleData.maxDescriptionLength}";
            m_StyleData.description = evt.newValue;
            m_Description.tooltip = m_Description.value;
        }

        void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            m_StyleData.description = evt.newValue;
            m_Description.tooltip = m_Description.value;
        }

        void UpdateInfoUI()
        {
            SetPreviewImage(null);
            UpdateStatusTextAndIcon();
            m_Name.SetEnabled(m_StyleData?.state == EState.New && !Utilities.ValidStringGUID(m_StyleData.guid));
            m_Description.SetEnabled(m_StyleData?.state == EState.New && !Utilities.ValidStringGUID(m_StyleData.guid));
            m_Name.SetValueWithoutNotify(m_StyleData?.title);
            m_Description.SetValueWithoutNotify(m_StyleData?.description);
            m_Description.tooltip = m_Description.value;
            m_DescriptionTextCount.text = $"{m_Description.value.Length}/{StyleData.maxDescriptionLength}";
            m_Description.style.height = k_OriginalDescriptionTextAreaHeight;
        }

        void UpdateStatusTextAndIcon()
        {
            m_TrainingIcon.style.display = DisplayStyle.None;
            m_ErrorIcon.style.display = DisplayStyle.None;

            switch (m_StyleData.state)
            {
                case EState.Loaded:
                    m_StatusLabel.style.display = DisplayStyle.None;
                    break;
                case EState.Error:
                    m_StatusLabel.style.display = DisplayStyle.Flex;
                    m_StatusLabel.text = StringConstants.styleError;
                    m_ErrorIcon.style.display = DisplayStyle.Flex;
                    break;
                case EState.Training:
                case EState.Loading:
                    m_StatusLabel.style.display = DisplayStyle.Flex;
                    m_StatusLabel.text = StringConstants.styleTraining;
                    m_TrainingIcon.style.display = DisplayStyle.Flex;
                    break;
                case EState.New:
                case EState.Initial:
                    m_StatusLabel.style.display = DisplayStyle.None;
                    m_StatusLabel.text = StringConstants.styleNotTrained;
                    break;
            }
        }

        void SetPreviewImage(ImageArtifact imageArtifact)
        {
            m_PreviewImage.SetArtifact(imageArtifact);
        }

        void DetachFromPanel(DetachFromPanelEvent evt)
        {
            RegisterCallback<AttachToPanelEvent>(AttachToPanel);
            UnregisterCallback<DetachFromPanelEvent>(DetachFromPanel);
            m_Name.UnregisterValueChangedCallback(OnNameChanged);
            m_Name.UnregisterValueChangingCallback(OnNameChanging);
            m_Description.UnregisterValueChangedCallback(OnDescriptionChanged);
            m_Description.UnregisterValueChangingCallback(OnDescriptionChanging);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleModelInfo, UxmlTraits> { }
#endif

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            m_EventBus.RegisterEvent<StyleModelListSelectionChangedEvent>(OnStyleModelListSelectionChanged);
            m_EventBus.RegisterEvent<StyleTrainingEvent>(OnStyleTrainingEvent);
            m_EventBus.RegisterEvent<FavoritePreviewSampleOutputEvent>(OnFavoriteSampleChanged);
        }

        void OnFavoriteSampleChanged(FavoritePreviewSampleOutputEvent arg0)
        {
            if (m_StyleData?.state == EState.Loaded)
            {
                SetPreviewImage(arg0?.favoriteSampleOutputData?.imageArtifact);
            }
        }

        void OnStyleTrainingEvent(StyleTrainingEvent arg0)
        {
            if (arg0.styleData.guid == m_StyleData.guid)
            {
                UpdateInfoUI();
            }
        }

        void OnStyleModelListSelectionChanged(StyleModelListSelectionChangedEvent arg0)
        {
            SetPreviewImage(null);
            if (arg0.styleData is not null && m_StyleData != arg0.styleData)
            {
                if (m_StyleData != null)
                    m_StyleData.OnStateChanged -= OnStyleStateChanged;
                m_StyleData = arg0.styleData;
                m_StyleData.OnStateChanged += OnStyleStateChanged;
                LoadStyle();
            }
        }

        void OnStyleStateChanged(StyleData obj)
        {
            if (obj.state is EState.Initial or EState.Loaded)
                LoadStyle();
        }

        void LoadStyle()
        {
            m_EventBus.SendEvent(new ShowLoadingScreenEvent
            {
                description = "Loading Style...",
                show = false
            });
            m_StyleData.GetArtifact(OnGetArtifactDone, true);
        }

        void OnGetArtifactDone(StyleData obj)
        {
            if (obj == m_StyleData)
            {
                m_EventBus.SendEvent(new ShowLoadingScreenEvent
                {
                    show = false
                });
                UpdateInfoUI();
                m_EventBus.SendEvent(new SampleOutputDataSourceChangedEvent
                {
                    styleData = m_StyleData
                });
                m_EventBus.SendEvent(new TrainingSetDataSourceChangedEvent()
                {
                    styleData = m_StyleData
                });
            }
        }
    }
}