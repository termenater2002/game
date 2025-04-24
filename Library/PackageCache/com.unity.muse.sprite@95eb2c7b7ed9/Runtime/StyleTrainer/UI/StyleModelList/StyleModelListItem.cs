using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Events;
using Unity.Muse.StyleTrainer.Events.StyleModelEditorUIEvents;
using Unity.Muse.StyleTrainer.Events.StyleModelListUIEvents;
using Unity.Muse.StyleTrainer.Events.TrainingControllerEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class StyleModelListItem : ExVisualElement
    {
        Text m_StyleName;
        Text m_StyleStatus;
        PreviewImage m_StylePreviewImage;
        StyleData m_StyleData;
        EventBus m_EventBus;

        VisualElement m_StyleLoadingContainer;
        VisualElement m_ListViewContainer;
        ListView m_ListView;

        public StyleModelListItem()
        {
            name = "StyleModelListItem";
        }

        public void Init(StyleData o, EventBus evtBus)
        {
            m_StyleData = o;
            SetPreviewImage(null);

            if (m_StyleData is not null)
            {
                m_ListView = GetFirstAncestorOfType<ListView>();

                if (m_StyleData.state == EState.Loaded)
                {
                    var imageArtifact = m_StyleData.GetFavouriteOrLatestCheckPoint()?.GetFavoriteSampleOutputData()?.imageArtifact;
                    SetPreviewImage(imageArtifact);
                }

                m_StyleData.OnStateChanged += OnStyleStateChanged;
                OnStyleStateChanged(m_StyleData);

                //todo DISPOSE
                o.OnDataChanged += OnDataChanged;
                o.OnGUIDChanged += OnDataChanged;
                m_EventBus = evtBus;
                m_EventBus.RegisterEvent<StyleTrainingEvent>(OnStyleTrainingEvent);
                m_EventBus.RegisterEvent<FavoritePreviewSampleOutputEvent>(OnFavoriteSampleChanged);
                m_EventBus.RegisterEvent<SearchStyleEvent>(OnSearch);
                m_EventBus.RegisterEvent<StyleModelListCollapsedEvent>(OnStyleModelListCollapsed);

                RegisterCallback<ClickEvent>(OnItemClicked);
                RegisterCallback<PointerDownEvent>(OnPointerDownEvent);

                var contextMenu = new ContextualMenuManipulator(OpenContextualMenu);
                contextMenu.target = this;

                RefreshUI();
            }
        }

        void OnStyleModelListCollapsed(StyleModelListCollapsedEvent arg0)
        {
            const int tooltipDelayMsCollapsedListView = 250;
            tooltipDelayMsOverride = arg0.collapsed ? tooltipDelayMsCollapsedListView : new Optional<int>();
        }

        void OnPointerDownEvent(PointerDownEvent evt)
        {
            if (evt.button == (int)MouseButton.RightMouse)
            {
                var index = m_ListView.itemsSource.IndexOf(m_StyleData);
                m_ListView.SetSelection(index);
            }
        }

        void OpenContextualMenu(ContextualMenuPopulateEvent menuEvent)
        {
            menuEvent.menu.AppendAction("Duplicate", DuplicateStyle,
                m_StyleData.state != EState.New ? DropdownMenuAction.Status.Normal: DropdownMenuAction.Status.Disabled);
            menuEvent.menu.AppendAction("Delete", DeleteStyle);
        }

        void DuplicateStyle(DropdownMenuAction menuItem)
        {
            m_EventBus.SendEvent(new DuplicateButtonClickEvent());
        }

        void DeleteStyle(DropdownMenuAction menuItem)
        {
            m_EventBus.SendEvent(new StyleDeleteButtonClickedEvent
            {
                styleData = m_StyleData
            });
        }

        void OnSearch(SearchStyleEvent arg0)
        {
            if (m_StyleData == null)
                return;

            var searchToLowerCase = arg0.search.ToLower();

            var state = m_StyleData.state switch
            {
                EState.Loaded when m_StyleData.visible => StringConstants.stylePublishedText,
                EState.Loaded => StringConstants.styleTrainedText,
                EState.Training => StringConstants.styleTrainingText,
                EState.Loading => StringConstants.styleLoadingText,
                _ => StringConstants.styleUntrainedText
            };

            if (m_StyleData.title.ToLower().Contains(searchToLowerCase)
                || m_StyleData.description.ToLower().Contains(searchToLowerCase)
                || state.ToLower().Contains(searchToLowerCase))
            {
                style.display = DisplayStyle.Flex;
            }
            else
            {
                style.display = DisplayStyle.None;
            }
        }

        void OnStyleStateChanged(StyleData obj)
        {
            m_StyleLoadingContainer.style.display = obj.state is EState.Loading or EState.Training ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshUI();
        }

        public void UnbindItem()
        {
            if (m_StyleData is not null)
            {
                m_StyleData.OnStateChanged -= OnStyleStateChanged;
                m_StyleData.OnDataChanged -= OnDataChanged;
            }

            if (m_EventBus is not null)
            {
                m_EventBus.UnregisterEvent<StyleTrainingEvent>(OnStyleTrainingEvent);
                m_EventBus.UnregisterEvent<FavoritePreviewSampleOutputEvent>(OnFavoriteSampleChanged);
                m_EventBus.UnregisterEvent<SearchStyleEvent>(OnSearch);
                m_EventBus.UnregisterEvent<StyleModelListCollapsedEvent>(OnStyleModelListCollapsed);
            }

            ClearUI();
            m_StyleData = null;

            UnregisterCallback<ClickEvent>(OnItemClicked);
            UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
        }

        void OnStyleTrainingEvent(StyleTrainingEvent arg0)
        {
        }

        void OnDataChanged(StyleData obj)
        {
            if (obj == m_StyleData)
            {
                RefreshUI();
            }
        }

        void RefreshUI()
        {
            if (m_StyleData == null)
            {
                ClearUI();
                return;
            }

            m_StyleName.text = m_StyleData.styleTitle;
            m_StyleName.tooltip = "";
            this.SetTooltipTemplate(new Text(m_StyleData.title));
            preferredTooltipPlacementOverride = new OptionalEnum<PopoverPlacement>(PopoverPlacement.Right);
            SetStatusText(m_StyleData.state);
            UpdateStatusIcon();
        }

        void ClearUI()
        {
            m_StyleName.text = string.Empty;
            m_StyleName.tooltip = "";
            m_StyleStatus.text = string.Empty;
            SetPreviewImage(null);
            m_StyleLoadingContainer.style.display= DisplayStyle.None;
        }

        void UpdateStatusIcon()
        {
            var isNew = m_StyleData.state == EState.New && !Utilities.ValidStringGUID(m_StyleData.guid);
            var hasTraining = m_StyleData.HasTraining();
            m_StyleLoadingContainer.style.display= (!isNew && hasTraining) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void BindElements()
        {
            m_StyleLoadingContainer = this.Q<VisualElement>("StyleLoadingContainer");
            m_StyleLoadingContainer.style.display = DisplayStyle.None;
            m_ListViewContainer = this.Q<VisualElement>("ListViewContainer");
            m_ListViewContainer.style.display = DisplayStyle.Flex;

            m_StyleName = this.Q<Text>("StyleName");
            m_StyleName.tooltip = "";
            m_StyleStatus = this.Q<Text>("StyleStatus");
            m_StylePreviewImage = this.Q<PreviewImage>("StylePreviewImage");
        }

        void OnItemClicked(ClickEvent evt)
        {
            m_EventBus.SendEvent(new SeeTrainedStyleEvent());
        }

        internal static StyleModelListItem CreateFromUxml()
        {
            var visualTree = ResourceManager.Load<VisualTreeAsset>(PackageResources.styleModelListItemTemplate);
            var ve = (StyleModelListItem)visualTree.CloneTree().Q("StyleModelListItem");
            ve.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.styleModelListItemStyleSheet));
            ve.BindElements();
            return ve;
        }

        void SetStatusText(EState styleState)
        {
            const string orangeColorClass = "text-color-orange-800";
            const string blueColorClass = "text-color-blue-800";
            const string greenColorClass = "text-color-green-800";
            const string redColorClass = "text-color-red-800";

            m_StyleStatus.RemoveFromClassList(orangeColorClass);
            m_StyleStatus.RemoveFromClassList(blueColorClass);
            m_StyleStatus.RemoveFromClassList(greenColorClass);
            m_StyleStatus.RemoveFromClassList(redColorClass);

            switch (styleState)
            {
                case EState.Training:
                    m_StyleStatus.text = StringConstants.styleTrainingText;
                    break;
                case EState.Error:
                    m_StyleStatus.text = StringConstants.styleErrorText;
                    m_StyleStatus.AddToClassList(redColorClass);
                    break;
                case EState.Loading:
                    m_StyleStatus.text = StringConstants.styleLoadingText;
                    break;
                case EState.Initial:
                    m_StyleStatus.text = StringConstants.styleInitialText;
                    break;
                case EState.Loaded:
                    if (m_StyleData.visible)
                    {
                        m_StyleStatus.text = StringConstants.stylePublishedText;
                        m_StyleStatus.AddToClassList(greenColorClass);
                    }
                    else
                    {
                        m_StyleStatus.text = StringConstants.styleTrainedText;
                        m_StyleStatus.AddToClassList(blueColorClass);
                    }
                    break;
                default:
                    m_StyleStatus.text = StringConstants.styleUntrainedText;
                    m_StyleStatus.AddToClassList(orangeColorClass);
                    break;
            }
        }

        void OnFavoriteSampleChanged(FavoritePreviewSampleOutputEvent arg0)
        {
            if (m_StyleData.state == EState.Loaded)
            {
                if (m_StyleData.GetFavouriteOrLatestCheckPoint() == arg0.checkPointData)
                {
                    SetPreviewImage(arg0?.favoriteSampleOutputData?.imageArtifact);
                }
            }
        }

        void SetPreviewImage(ImageArtifact imageArtifact)
        {
            m_StylePreviewImage.SetArtifact(imageArtifact);
        }

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<StyleModelListItem, UxmlTraits> { }
#endif
    }
}