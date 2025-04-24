using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetModelEvents;
using Unity.Muse.StyleTrainer.Events.TrainingSetUIEvents;
using Unity.Muse.StyleTrainer.Manipulator;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class TrainingSetView : ExVisualElement, IStyleModelInfoTabView
    {
        GridView m_GridView;
        Text m_TrainingImagesTextCount;
        Text m_TrainingImagesTitleText;
        VisualElement m_TrainingImagesHintContainer;
        StyleData m_StyleData;
        EventBus m_EventBus;
        int m_CountPerRow = 2;
        const float k_DefaultThumbnailSize = 92;
        VisualElement m_DragAndDropContainer;
        ExVisualElement m_TextDragDropContainer;
        SpriteTextureDropManipulator m_SpriteTextureDropManipulator;
        public Action<IList<Texture2D>> OnDragAndDrop;
        float m_ThumbnailSize = 1;
        public Action<int> OnDeleteClickedCallback;
        bool m_CanModify;
        VisualElement m_LoadingScreen;
        VisualElement m_GridViewContainer;
        string m_CurrentContext;
        const string k_PlusButtonGuid = "dummy";
        List<MuseShortcut> m_DeleteShortcuts;

        void OnDeleteClicked(int indexToDelete)
        {
            OnDeleteClicked(new[] { indexToDelete });
        }

        void OnDeleteClicked(IList<int> indicesToDelete)
        {
            m_EventBus.SendEvent(new DeleteTrainingSetEvent
            {
                styleData = m_StyleData,
                indices = indicesToDelete
            });
        }

        new TrainingData this[int index] => (TrainingData)m_GridView.itemsSource[index];

        void BindItem(VisualElement arg1, int arg2)
        {
            if (arg1 is TrainingItemGridItem trainingItem)
            {
                trainingItem.SetEventBus(m_EventBus);

                if (this[arg2].guid == k_PlusButtonGuid)
                {
                    trainingItem.CreatePlusButton();
                    return;
                }

                trainingItem.CanModify(m_CanModify);
                trainingItem.itemIndex = arg2-1;
                trainingItem.SetPreviewImage(this[arg2].imageArtifact);
                trainingItem.OnDeleteClicked += OnDeleteClicked;
            }
        }

        void UnbindItem(VisualElement arg1, int arg2)
        {
            if (arg1 is TrainingItemGridItem ve)
            {
                ve.OnDeleteClicked -= OnDeleteClicked;
            }
        }

        VisualElement MakeGridItem()
        {
            return new TrainingItemGridItem(m_StyleData);
        }

        public VisualElement GetView()
        {
            return this;
        }

        public void SetEventBus(EventBus evtBus)
        {
            m_EventBus = evtBus;
            m_EventBus.RegisterEvent<ThumbnailSizeChangedEvent>(OnThumbnailSizeChanged);
            m_EventBus.RegisterEvent<TrainingSetDataSourceChangedEvent>(OnTrainingSetDataSourceChanged);
            m_SpriteTextureDropManipulator = new SpriteTextureDropManipulator();
            this.AddManipulator(m_SpriteTextureDropManipulator);
            m_SpriteTextureDropManipulator.onDragStart += OnDragStart;
            m_SpriteTextureDropManipulator.onDragEnd += OnDragEnd;
            m_SpriteTextureDropManipulator.onDrop += OnDrop;
        }

        void OnTrainingSetDataSourceChanged(TrainingSetDataSourceChangedEvent arg0)
        {
            m_LoadingScreen.style.display = DisplayStyle.Flex;
            m_GridViewContainer.style.display = DisplayStyle.None;
            m_StyleData = arg0.styleData;
            if (m_StyleData.state == EState.Loading)
            {
                m_StyleData.OnStateChanged += OnStyleStateChange;
            }
            else
            {
                if (m_StyleData.trainingSetData != null)
                {
                    var context = m_StyleData.trainingSetData[0].guid;
                    m_CurrentContext = context;
                    m_StyleData.trainingSetData[0].GetArtifact(x =>
                        OnGetTrainingSetDone(context, x), true);
                }
            }
        }

        void OnStyleStateChange(StyleData obj)
        {
            obj.OnStateChanged -= OnStyleStateChange;
            if (obj == m_StyleData && m_StyleData.trainingSetData != null)
            {
                m_CurrentContext = m_StyleData.trainingSetData[0].guid;
                m_StyleData.trainingSetData[0].GetArtifact(x =>
                    OnGetTrainingSetDone(m_StyleData.trainingSetData[0].guid, x), true);
            }
        }

        void OnGetTrainingSetDone(string context, IList<TrainingData> obj)
        {
            if (m_CurrentContext == context)
            {
                m_LoadingScreen.style.display = DisplayStyle.None;
                m_GridViewContainer.style.display = DisplayStyle.Flex;
                // dispose off old ones
                for(int i = 0; i < m_GridView.itemsSource?.Count; ++i)
                {
                    if (m_GridView.itemsSource[i] is TrainingData td)
                    {
                        td.imageArtifact?.OnDispose();
                    }
                }

                var trainingDataList = new List<TrainingData>();
                foreach (var trainingData in obj)
                {
                    trainingDataList.Add(trainingData);
                }

                var dummies = trainingDataList.Where(t => t.guid == k_PlusButtonGuid);
                if(!dummies.Any() && m_CanModify)
                {
                    var plusButtonData = new TrainingData(EState.New, k_PlusButtonGuid);
                    trainingDataList.Insert(0, plusButtonData);
                }

                m_GridView.itemsSource = trainingDataList;

                UpdateUI();
                m_GridView.Refresh();
            }
        }

        public void OnViewActivated(float thumbNailSize)
        {
            m_ThumbnailSize = thumbNailSize;
            m_GridView.Refresh();
        }

        void OnDrop(IList<Texture2D> textures)
        {
            if (m_CanModify)
            {
                m_DragAndDropContainer.RemoveFromClassList("styletrainer-trainingsetview__draganddropcontainer_dragging");
                OnDragAndDrop?.Invoke(textures);
            }
        }

        void OnDragEnd()
        {
            if (m_CanModify) m_DragAndDropContainer.RemoveFromClassList("styletrainer-trainingsetview__draganddropcontainer_dragging");
        }

        void OnDragStart()
        {
            if (m_CanModify)
                m_DragAndDropContainer.AddToClassList("styletrainer-trainingsetview__draganddropcontainer_dragging");
        }

        void OnThumbnailSizeChanged(ThumbnailSizeChangedEvent arg0)
        {
            m_ThumbnailSize = arg0.thumbnailSize;
            RefreshThumbnailSize(arg0.thumbnailSize);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (panel == null || float.IsNaN(evt.newRect.width) || Mathf.Approximately(0, evt.newRect.width))
                return;

            RefreshThumbnailSize(m_ThumbnailSize);

            m_TextDragDropContainer.style.paddingLeft = m_GridView.resolvedStyle.height;
        }

        void RefreshThumbnailSize(float value)
        {
            if (float.IsNaN(m_GridViewContainer.resolvedStyle.width))
                return;

            var size = value * k_DefaultThumbnailSize;

            var borderWidth = m_GridViewContainer.resolvedStyle.borderLeftWidth
                + m_GridViewContainer.resolvedStyle.borderRightWidth;

            var marginWidth = m_GridView.resolvedStyle.marginLeft + m_GridView.resolvedStyle.marginRight;
            var width = m_GridViewContainer.resolvedStyle.width - borderWidth - marginWidth;

            var newCountPerRow = Mathf.FloorToInt(width / size);
            newCountPerRow = Mathf.Max(1, newCountPerRow);

            if (newCountPerRow != m_CountPerRow)
            {
                m_CountPerRow = newCountPerRow;
                m_GridView.columnCount = m_CountPerRow;
            }

            var itemHeight = Mathf.FloorToInt(width / m_CountPerRow);

            if (!Mathf.Approximately(itemHeight, m_GridView.itemHeight))
                m_GridView.itemHeight = itemHeight;

            var itemsSourceCount = m_GridView.itemsSource?.Count ?? 0;
            var numberOfRows = (itemsSourceCount + m_CountPerRow -1) / m_CountPerRow;
            numberOfRows = Math.Max(1, numberOfRows);

            var borderHeight = m_GridViewContainer.resolvedStyle.borderTopWidth
                + m_GridViewContainer.resolvedStyle.borderBottomWidth;

            var marginHeight = m_GridView.resolvedStyle.marginTop + m_GridView.resolvedStyle.marginBottom;
            var gridViewHeight = numberOfRows * itemHeight + marginHeight + borderHeight;

            m_GridViewContainer.style.height = gridViewHeight;

            m_GridView.style.height = gridViewHeight;
        }

        internal static TrainingSetView CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.trainingSetViewTemplate);
            var ve = (TrainingSetView)visualTree.CloneTree().Q("TrainingSetView");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingSetViewStyleSheet));
            ve.BindElements();
            return ve;
        }

        void BindElements()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.trainingSetViewStyleSheet));
            m_GridView = this.Q<GridView>("TrainingSetViewGridView");
            m_GridView.makeItem = MakeGridItem;
            m_GridView.bindItem = BindItem;
            m_GridView.unbindItem = UnbindItem;
            m_GridView.columnCount = m_CountPerRow;
            m_GridView.selectionType = SelectionType.Multiple;
            m_GridView.itemHeight = (int)k_DefaultThumbnailSize;
            m_GridView.scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            m_GridView.scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            var contextMenu = new ContextualMenuManipulator(OnGridViewContextClicked);
            contextMenu.target = m_GridView;

            m_DragAndDropContainer = this.Q<VisualElement>("DragAndDropContainer");
            m_DragAndDropContainer.pickingMode = PickingMode.Ignore;

            m_TextDragDropContainer = new ExVisualElement();
            m_TextDragDropContainer.pickingMode = PickingMode.Ignore;
            m_TextDragDropContainer.AddToClassList("styletrainer-trainingsetview-gridview__hint-text__container");

            var dragAndDropDescriptionText = new Text(StringConstants.dragAndDropDescription);
            dragAndDropDescriptionText.AddToClassList("styletrainer-trainingsetview-gridview__hint-text");
            dragAndDropDescriptionText.pickingMode = PickingMode.Ignore;
            m_TextDragDropContainer.Add(dragAndDropDescriptionText);

            m_DragAndDropContainer.Add(m_TextDragDropContainer);

            m_LoadingScreen = this.Q<VisualElement>("LoadingScreen");
            m_GridViewContainer = this.Q<VisualElement>("GridViewContainer");
            m_GridViewContainer.style.display = DisplayStyle.None;

            m_TrainingImagesTextCount = this.Q<Text>("TrainingImagesTextCount");
            m_TrainingImagesTitleText = this.Q<Text>("TrainingImagesTitleText");
            m_TrainingImagesHintContainer = this.Q<ExVisualElement>("TrainingImagesHintContainer");
            var hint = m_TrainingImagesHintContainer.Q<Text>();
            var hintTextWithLineHeight = hint.text.Insert(0, "<line-height=120%>");
            hint.text = hintTextWithLineHeight;

            UpdateUI();
            m_GridViewContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            this.AddManipulator(new MuseShortcutHandler());
            m_DeleteShortcuts = new List<MuseShortcut>();

            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                m_DeleteShortcuts.Add( new MuseShortcut("Delete Training Images", DeleteSelectedTrainingSetImages, KeyCode.Backspace, KeyModifier.Action, m_GridView){ requireFocus = true });
            }
            else
            {
                m_DeleteShortcuts.Add(new MuseShortcut("Delete Training Images", DeleteSelectedTrainingSetImages, KeyCode.Delete, source: m_GridView){ requireFocus = true });
            }

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt) => MuseShortcuts.AddShortcuts(m_DeleteShortcuts);
        void OnDetachFromPanel(DetachFromPanelEvent evt) => MuseShortcuts.RemoveShortcuts(m_DeleteShortcuts);

        void OnGridViewContextClicked(ContextualMenuPopulateEvent menuEvent)
        {
            menuEvent.menu.AppendAction("Delete", DeleteSelectedTrainingSetImages,
                 m_CanModify ? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
        }

        void DeleteSelectedTrainingSetImages(DropdownMenuAction menuItem)
        {
            DeleteSelectedTrainingSetImages();
        }

        void DeleteSelectedTrainingSetImages()
        {
            if (!m_GridView.selectedItems.Any() || !m_CanModify)
                return;

            var trainingDataIndices = m_GridView.selectedIndices.Select(index => index - 1).ToList();
            trainingDataIndices.Remove(-1); // remove the plus button index

            OnDeleteClicked(trainingDataIndices);
            m_GridView.SetSelectionWithoutNotify(new []{0});
        }

        void UpdateUI()
        {
            if (m_StyleData?.state == EState.Loaded)
            {
                m_TrainingImagesTitleText.text = StringConstants.trainingImagesTitleTrained;
                m_TrainingImagesHintContainer.style.display = DisplayStyle.None;
                m_TrainingImagesTextCount.text = StringConstants.trainingImagesLocked;
            }
            else
            {
                if (m_StyleData?.state == EState.Training)
                {
                    var trainingData = (List<TrainingData>)m_GridView?.itemsSource;

                    if(trainingData?.Count > 0 && trainingData[0].guid == k_PlusButtonGuid)
                    {
                        m_GridView.itemsSource.RemoveAt(0);
                    }
                }

                m_TrainingImagesTitleText.text = StringConstants.trainingImagesTitleUntrained;
                m_TrainingImagesHintContainer.style.display = DisplayStyle.Flex;

                while (m_StyleData?.trainingSetData[0].Count > StyleData.maxTrainingSetImages)
                {
                    m_StyleData?.trainingSetData[0].RemoveAt(m_StyleData.trainingSetData[0].Count - 1);
                    m_GridView.itemsSource.RemoveAt(m_GridView.itemsSource.Count - 1);
                }

                m_TrainingImagesTextCount.text = $"{m_StyleData?.trainingSetData[0].Count}/{StyleData.maxTrainingSetImages}";
            }

            m_TextDragDropContainer.style.display = m_StyleData?.trainingSetData[0].Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshThumbnailSize(m_ThumbnailSize);
        }

        public void CanModify(bool canModify)
        {
            m_CanModify = canModify;
            UpdateUI();

            m_GridView.EnableInClassList("styletrainer-trainingsetview-gridview-disable", !canModify);
            m_GridView.selectionType = canModify ? SelectionType.Multiple : SelectionType.None;
            m_GridView.Refresh();
        }

        public void SelectItems(IList<int> indices)
        {
            m_GridView.SetSelectionWithoutNotify(indices);
            m_GridView.ScrollToItem(indices[0]);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<TrainingSetView, UxmlTraits> { }
#endif
    }
}