using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.SampleOutputModelEvents;
using Unity.Muse.StyleTrainer.Events.SampleOutputUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetUIEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleModelInfoEditorContent : ExVisualElement, IStyleModelEditorContent
    {
        struct TabData
        {
            public string tabName;
            public IStyleModelInfoTabView view;
            public Action<EventBus, StyleData> sendAddEvent;
            public Func<bool> CanModify;
        }

        readonly TabData[] m_TabData;
        StyleData m_StyleData;
        VisualElement m_TabContainer;
        StyleModelInfo m_StyleModelInfo;
        EventBus m_EventBus;
        float m_ThumbnailSize = StyleModelInfoEditor.initialThumbnailSliderValue;
        Action<bool> m_NotifyCanAddChanged;

        public StyleModelInfoEditorContent()
        {
            m_TabData = new[]
            {
                new TabData
                {
                    tabName = StringConstants.trainingSetTab,
                    view = CreateTrainingSetView(),
                    CanModify = () => !Utilities.ValidStringGUID(m_StyleData?.trainingSetData[0].guid) && m_StyleData?.state != EState.Training && m_StyleData?.state != EState.Error,
                    sendAddEvent = (evtBus, styleData) =>
                    {
#if UNITY_EDITOR
                        string lastFolderPath = Preferences.lastImportFolderPath;
                        if (!Directory.Exists(lastFolderPath))
                            lastFolderPath = Preferences.defaultImportFolderPath;

                        NativePlugin.OpenFilePanelAsync(
                            title: "Add training set", 
                            directory: lastFolderPath, 
                            filters: new[]
                        {
                            new ExtensionFilter("Image files", "png", "jpg", "jpeg"),
                        }, 
                            multiselect: true, 
                            cb: files =>
                            {
                                if (files == null || files.Length == 0)
                                    return;
                                
                                var textures = files
                                    .Select(file =>
                                {
                                    if (!string.IsNullOrEmpty(file))
                                    {
                                        Preferences.lastImportFolderPath = Path.GetDirectoryName(file);
                                        var texture = new Texture2D(2, 2);
                                        ImageConversion.LoadImage(texture, File.ReadAllBytes(file));
                                        texture.hideFlags = HideFlags.HideAndDontSave;
                                        texture.name = $"AddFileTexture-{file}";
                                        return texture;
                                    }
                                    return null;
                                }).Where(t => t).ToList();
                            
                                evtBus.SendEvent(new AddTrainingSetEvent
                                {
                                    styleData = styleData,
                                    textures = textures,
                                });
                        });
#endif
                    }
                },
                new TabData
                {
                    tabName = StringConstants.sampleOutputTab,
                    view = new SampleOutputView(),
                    CanModify = () => !Utilities.ValidStringGUID(m_StyleData?.guid) && m_StyleData?.state == EState.New,
                    sendAddEvent = (evtBus, styleData) =>
                    {
                        evtBus.SendEvent(new AddSampleOutputEvent
                        {
                            styleData = styleData
                        });
                    }
                }
            };
        }

        IStyleModelInfoTabView CreateTrainingSetView()
        {
            var t = TrainingSetView.CreateFromUxml();
            t.OnDragAndDrop = TrainingDataDragAndDrop;
            return t;
        }

        void OnAddImagesToTrainingSetClicked()
        {
            m_TabData[0].sendAddEvent(m_EventBus, m_StyleData);
        }

        void OnTabValueChange(ChangeEvent<int> evt)
        {
            UpdateView();
        }

        public void UpdateView()
        {
            for (var i = 0; i < m_TabData.Length; i++)
            {
                m_TabData[i].view.GetView().style.display = DisplayStyle.Flex;
                m_TabData[i].view.CanModify(m_TabData[i].CanModify());
                m_TabData[i].view.OnViewActivated(m_ThumbnailSize);
                m_NotifyCanAddChanged?.Invoke(m_TabData[i].CanModify());
            }
        }

        public void NotifyCanAddChanged(Action<bool> callback)
        {
            m_NotifyCanAddChanged = callback;
        }

        public void SetEventBus(EventBus eventBus)
        {
            m_EventBus = eventBus;
            for (var i = 0; i < m_TabData.Length; ++i)
                m_TabData[i].view.SetEventBus(eventBus);
            m_EventBus.RegisterEvent<StyleModelListSelectionChangedEvent>(OnStyleModelListSelectionChanged);
            m_EventBus.RegisterEvent<CheckPointSelectionChangeEvent>(OnCheckPointSelectionChanged);
            m_EventBus.RegisterEvent<GenerateButtonClickEvent>(OnGenerateButtonClicked);
            m_EventBus.RegisterEvent<StyleTrainingEvent>(OnStyleTrainingEvent);
            m_EventBus.RegisterEvent<RequestChangeTabEvent>(OnRequestChangeTabEvent);
            m_EventBus.RegisterEvent<ThumbnailSizeChangedEvent>(OnThumbnailSizeChangedEvent);
            m_EventBus.RegisterEvent<AddImagesToTrainingSetEvent>(OnAddImagesToTrainingSetEvent);

            m_StyleModelInfo.SetEventBus(eventBus);
        }

        void OnAddImagesToTrainingSetEvent(AddImagesToTrainingSetEvent arg0)
        {
            OnAddImagesToTrainingSetClicked();
        }

        void OnThumbnailSizeChangedEvent(ThumbnailSizeChangedEvent arg0)
        {
            m_ThumbnailSize = arg0.thumbnailSize;
            UpdateView();
        }

        void IStyleModelEditorContent.OnAddClicked()
        {
        }

        void OnRequestChangeTabEvent(RequestChangeTabEvent arg0)
        {
            if (arg0.tabIndex >= 0 && arg0.tabIndex < m_TabData.Length)
            {
                if (arg0.highlightIndices != null)
                    m_TabData[arg0.tabIndex].view.SelectItems(arg0.highlightIndices);
            }
        }

        void OnStyleTrainingEvent(StyleTrainingEvent arg0)
        {
            UpdateView();
        }

        void OnGenerateButtonClicked(GenerateButtonClickEvent arg0)
        {
            for (var i = 0; i < m_TabData.Length; i++)
                m_TabData[i].view.CanModify(false);
        }

        void OnCheckPointSelectionChanged(CheckPointSelectionChangeEvent arg0)
        {
            UpdateView();
        }

        void OnStyleModelListSelectionChanged(StyleModelListSelectionChangedEvent arg0)
        {
            if (m_StyleData != null)
                m_StyleData.OnStateChanged -= OnStyleDataStateChange;
            m_StyleData = arg0.styleData;
            if (m_StyleData != null)
                m_StyleData.OnStateChanged += OnStyleDataStateChange;
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

            UpdateView();
        }

        void OnStyleDataStateChange(StyleData styleData)
        {
            UpdateView();
        }

        internal static StyleModelInfoEditorContent CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.styleModelInfoEditorContentTemplate);
            var ve = (StyleModelInfoEditorContent)visualTree.CloneTree().Q("StyleModelInfoEditorContent");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.styleModelInfoEditorStyleSheet));
            ve.BindElements();
            return ve;
        }

        void TrainingDataDragAndDrop(IList<Texture2D> textures)
        {
            var textureArray = new Texture2D[textures.Count];
            textures.CopyTo(textureArray, 0);
            m_EventBus.SendEvent(new AddTrainingSetEvent
            {
                styleData = m_StyleData,
                textures = textureArray
            });
        }

        void BindElements()
        {
            name = "StyleModelInfoEditorContent";

            m_StyleModelInfo = this.Q<StyleModelInfo>();
            m_TabContainer = this.Q<VisualElement>("StyleTabContainer");
            var tabContent = m_TabContainer.Q<VisualElement>("TabContent");
            tabContent.Add(m_TabData[0].view.GetView());
            tabContent.Add(m_TabData[1].view.GetView());
            UpdateView();
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleModelInfoEditorContent, UxmlTraits> { }
#endif
    }

    interface IStyleModelInfoTabView
    {
        VisualElement GetView();
        void SetEventBus(EventBus evtBus);
        void CanModify(bool canModify);
        void OnViewActivated(float thumbNailSize);
        void SelectItems(IList<int> indices);
    }
}