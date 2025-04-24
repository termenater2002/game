using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.CheckPointModelEvents;
using Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents;
using Unity.Muse.StyleTrainer.Events.SampleOutputUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
    class SampleOutputView : ExVisualElement, IStyleModelInfoTabView
    {
        StyleData m_StyleData;

        VisualElement m_UntrainedViewContainer;
        SampleOutputTrainedView m_TrainedViewContainer;

        SampleOutputListView m_ListView;
        Text m_PromptsHintText;
        Text m_HeaderPromptTitleText;
        EventBus m_EventBus;
        float m_ThumbnailSize = StyleModelInfoEditor.initialThumbnailSliderValue;
        public Action<int> OnDeleteClickedCallback;
        bool m_CanModify;



        const string k_HiddenClassList = "styletrainer-sampleoutputview__hidden";

        public SampleOutputView()
        {
            name = "SampleSetViewV2";
            AddToClassList("styletrainer-sampleoutputview");
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.sampleOutputViewStyleSheet));

            m_UntrainedViewContainer = CreateUntrainedViewContainer();
            m_UntrainedViewContainer.AddToClassList("styletrainer-sampleoutputview__untrained-container");
            m_UntrainedViewContainer.AddToClassList(k_HiddenClassList);
            Add(m_UntrainedViewContainer);

            m_TrainedViewContainer = CreateTrainedViewContainer();
            m_TrainedViewContainer.AddToClassList("styletrainer-sampleoutputview__trained-container");
            m_UntrainedViewContainer.AddToClassList(k_HiddenClassList);
            Add(m_TrainedViewContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        ExVisualElement CreateUntrainedViewContainer()
        {
            var hintContainer = new ExVisualElement();
            hintContainer.AddToClassList("styletrainer-sampleoutputview-prompts-hinttext");
            m_PromptsHintText = new Text(StringConstants.promptsHintText);
            var hintTextWithLineHeight = m_PromptsHintText.text.Insert(0, "<line-height=120%>");
            m_PromptsHintText.text = hintTextWithLineHeight;
            hintContainer.Add(m_PromptsHintText);

            var promptsContainer = new ExVisualElement();
            promptsContainer.AddToClassList("styletrainer-sampleoutputview-container");

            var headerPromptTitleText = new Text("Practice Prompts");
            headerPromptTitleText.AddToClassList("styletrainer-sampleoutputview-container__title");
            promptsContainer.Add(headerPromptTitleText);

            m_ListView = new SampleOutputListView();
            promptsContainer.Add(m_ListView);

            var untrainedViewContainer = new ExVisualElement();
            untrainedViewContainer.Add(hintContainer);
            untrainedViewContainer.Add(promptsContainer);
            return untrainedViewContainer;
        }

        SampleOutputTrainedView CreateTrainedViewContainer()
        {
            return new SampleOutputTrainedView();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            m_ListView.OnDeleteClickedCallback -= OnSampleOutputDeleteClicked;
            m_ListView.OnFavoriteToggleChangedCallback -= OnFavouriteToggleChanged;
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            m_ListView.OnDeleteClickedCallback += OnSampleOutputDeleteClicked;
            m_ListView.OnFavoriteToggleChangedCallback += OnFavouriteToggleChanged;
        }

        void OnFavouriteToggleChanged(bool favourite, CheckPointData checkPoint)
        {
            m_EventBus.SendEvent(new SetFavouriteCheckPointEvent
            {
                checkPointGUID = favourite ? checkPoint.guid : Guid.Empty.ToString(),
                styleData = m_StyleData
            });
        }

        void OnSampleOutputDeleteClicked(int obj)
        {
            m_EventBus.SendEvent(new DeleteSampleOutputEvent
            {
                styleData = m_StyleData,
                deleteIndex = obj
            });
        }

        void ShowView()
        {
            var itemCount = m_StyleData?.sampleOutputPrompts?.Count;
            if (itemCount == 0)
            {
                m_ListView.style.display = DisplayStyle.None;
            }
            else
            {
                m_ListView.style.display= DisplayStyle.Flex;
                m_ListView.SetStyleData(m_StyleData);
                m_TrainedViewContainer.SetStyleData(m_StyleData);
            }

            UpdateView();
        }

        void UpdateView()
        {
            if (m_StyleData != null)
            {
                m_UntrainedViewContainer.EnableInClassList(k_HiddenClassList, m_StyleData.state == EState.Loaded);
                m_TrainedViewContainer.EnableInClassList(k_HiddenClassList, m_StyleData.state != EState.Loaded);
            }
        }

        public VisualElement GetView()
        {
            return this;
        }

        public void SetEventBus(EventBus evtBus)
        {
            m_EventBus = evtBus;
            m_EventBus.RegisterEvent<ThumbnailSizeChangedEvent>(OnThumbnailSizeChanged);
            m_EventBus.RegisterEvent<SampleOutputDataSourceChangedEvent>(OnSampleOutputSourceChanged);
            m_EventBus.RegisterEvent<CheckPointSourceDataChangedEvent>(OnCheckPointSourceDataChanged);
            m_EventBus.RegisterEvent<CheckPointDataChangedEvent>(OnCheckPointDataChanged);
            m_EventBus.RegisterEvent<FavouriteCheckPointChangeEvent>(OnFavouriteToggleChanged);
            m_TrainedViewContainer.SetEventBus(m_EventBus);
        }

        void OnFavouriteToggleChanged(FavouriteCheckPointChangeEvent arg0)
        {
            m_ListView.UpdateFavouriteCheckpoint();
        }

        void OnCheckPointDataChanged(CheckPointDataChangedEvent arg0)
        {
            if (m_StyleData?.guid == arg0.styleData.guid)
            {
                m_ListView.CheckPointDataChanged(arg0.checkPointData);
            }
        }

        void OnCheckPointSourceDataChanged(CheckPointSourceDataChangedEvent arg0)
        {
            if (m_StyleData?.guid == arg0.styleData.guid)
            {
                m_ListView.CheckPointSourceDataChanged();
            }
        }

        public void CanModify(bool canModify)
        {
            m_CanModify = canModify;
            UpdateView();
            m_ListView.CanModify(canModify);
        }

        void OnSampleOutputSourceChanged(SampleOutputDataSourceChangedEvent arg0)
        {
            if (m_StyleData != null)
                m_StyleData.OnDataChanged -= OnStyleDataChanged;
            m_StyleData = arg0.styleData;

            // Todo unregister on clean up
            m_StyleData.OnDataChanged += OnStyleDataChanged;
            ShowView();
        }

        void OnStyleDataChanged(StyleData styleData)
        {
            if (m_StyleData != null)
                m_StyleData.OnDataChanged -= OnStyleDataChanged;
            m_StyleData = styleData;

            // Todo unregister on clean up
            m_StyleData.OnDataChanged += OnStyleDataChanged;
            ShowView();
        }

        void OnThumbnailSizeChanged(ThumbnailSizeChangedEvent arg0)
        {
            m_ThumbnailSize = arg0.thumbnailSize;
            m_ListView.SetRowSize(m_ThumbnailSize);
        }

        public void OnViewActivated(float thumbNailSize)
        {
            m_ThumbnailSize = thumbNailSize;
            m_ListView.SetRowSize(m_ThumbnailSize);
        }

        public void SelectItems(IList<int> indices)
        {
            m_ListView.SelectItems(indices);
        }
    }
}