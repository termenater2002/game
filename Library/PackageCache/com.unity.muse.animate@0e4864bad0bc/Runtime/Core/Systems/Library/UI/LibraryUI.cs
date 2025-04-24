using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using AppUI = Unity.Muse.AppUI.UI;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    /// <summary>
    /// UI used for browsing an Asset Library.
    /// Must be bound to a LibraryUIModel through SetModel().
    /// </summary>
    partial class LibraryUI : UITemplateContainer, IUITemplate
    {
        const string k_UssClassName = "deeppose-library";
        const string k_GridviewUssClassName = k_UssClassName + "__gridview";
        const string k_ThumbnailSizeSliderUssClassName = k_UssClassName + "__thumbnail-slider";
        const string k_ContextMenuAnchorUssClassName = k_UssClassName + "__context-menu-anchor";
        const string k_ExportButtonUssClassName = k_UssClassName + "__export-button";

        const string k_EmptyLibraryMessage = "You currently have no animations generated";
        const string k_EmptyFilterResultsMessage = "No items match the current filter";
        const string k_EmptyLibraryTextName = "deeppose-library-empty-text";

        const float k_DefaultThumbnailSize = 152;
        static readonly Type[] k_ItemTypes = {typeof(LibraryItemModel), typeof(TextToMotionTake), typeof(KeySequenceTake), typeof(ArmatureStaticPoseModel)};
        static readonly string[] k_FilterOptions = {"Everything", "Generations", "Edited generations", "Saved poses"};

        LibraryUIModel m_Model;
        LibraryItemContextualMenu m_LibraryItemContextualMenu;

        GridView m_GridView;

        bool m_Inited;

        SliderFloat m_ThumbnailSizeSlider;
        SearchBar m_SearchBar;
        Dropdown m_FilterDropdown;
        VisualElement m_ContextMenuAnchor;
        Text m_Title;

        Text m_EmptyLibraryText;

        string m_CurrentMode;
        int m_CountPerRow = 2;

        VisualElement m_VerticalScrollerDragContainer;
        public VisualElement content;

        List<LibraryItemUI> m_Items = new();
        List<LibraryItemUI> m_FilteredItemsList = new();

        float m_ResultsTraySize = 1f;

#if ENABLE_UXML_TRAITS
        public new class UxmlFactory : UxmlFactory<LibraryUI, UxmlTraits> { }
#endif

        public LibraryUI()
            : base(k_UssClassName) { }

        public void InitComponents()
        {
            m_LibraryItemContextualMenu = new LibraryItemContextualMenu();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnFilterChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            FilterItemsSource();
        }

        public void FindComponents()
        {
            m_GridView = this.Q<GridView>(k_GridviewUssClassName);
            m_GridView.makeItem = MakeItemView;
            m_GridView.bindItem = BindGridItem;

            m_GridView.scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            m_GridView.scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            m_GridView.scrollView.verticalScroller.style.opacity = 0;

            m_VerticalScrollerDragContainer = m_GridView.scrollView.verticalScroller.slider.Q(classes:BaseSlider<float>.dragContainerUssClassName);

            m_ThumbnailSizeSlider = this.Q<SliderFloat>(k_ThumbnailSizeSliderUssClassName);
            m_ThumbnailSizeSlider.lowValue = 0.5f;
            m_ThumbnailSizeSlider.highValue = 1.5f;

            m_SearchBar = this.Q<SearchBar>();

            m_FilterDropdown = this.Q<Dropdown>();

            m_EmptyLibraryText = this.Q<Text>(k_EmptyLibraryTextName);

            RefreshThumbnailSize();
        }

        void BindGridItem(VisualElement el, int index)
        {
            el.Clear();
            el.Add(m_FilteredItemsList[index]);
        }

        public void RegisterComponents()
        {
            m_LibraryItemContextualMenu.OnMenuAction += OnItemContextualMenuAction;
            m_ThumbnailSizeSlider.RegisterValueChangingCallback(OnThumbnailSizeSliderChanged);

            m_ThumbnailSizeSlider.value = m_ResultsTraySize;
            m_ThumbnailSizeSlider.tooltip = "Thumbnail size";

            m_SearchBar.RegisterValueChangingCallback(OnSearchFieldChanging);
            m_SearchBar.RegisterValueChangedCallback(OnSearchFieldChanged);

            m_SearchBar.trailingIconName = "x--regular";
            m_SearchBar.trailingElement.pickingMode = PickingMode.Position;
            m_SearchBar.trailingElement.RegisterCallback<PointerDownEvent>(_ => CancelSearch());
            m_SearchBar.trailingElement.style.display = DisplayStyle.None;

            m_FilterDropdown.bindItem = (item, i) => item.label = k_FilterOptions[i];
            m_FilterDropdown.sourceItems = k_ItemTypes;
            m_FilterDropdown.RegisterValueChangedCallback(OnFilterChanged);
            m_FilterDropdown.SetValueWithoutNotify(new []{0});

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        public void UnregisterComponents()
        {
            m_LibraryItemContextualMenu.OnMenuAction -= OnItemContextualMenuAction;
            m_ThumbnailSizeSlider.UnregisterValueChangingCallback(OnThumbnailSizeSliderChanged);
            m_SearchBar.UnregisterValueChangingCallback(OnSearchFieldChanging);
            m_SearchBar.UnregisterValueChangedCallback(OnSearchFieldChanged);
        }

        protected internal void SetModel(LibraryUIModel model)
        {
            LogVerbose("SetModel({model})");

            UnregisterModel();
            m_Model = model;
            RegisterModel();
            Update();
        }

        void RegisterModel()
        {
            if (m_Model == null)
                return;

            m_Items.Clear();
            m_FilteredItemsList.Clear();

            m_Model.OnItemsChanged += OnItemsChanged;
            m_Model.OnRequestedItemContextualMenu += OnRequestedItemContextualMenu;
            m_Model.OnRequestedScrollToItem += OnRequestedScrollToItem;

            // If rendering animated thumbnails, we need to repaint whenever the thumbnail
            // is updated (writing to a texture doesn't necessarily trigger a repaint).
            m_Model.ThumbnailsService.OnRendered += MarkDirtyRepaint;
        }

        void UnregisterModel()
        {
            if (m_Model == null)
                return;

            m_Model.OnItemsChanged -= OnItemsChanged;
            m_Model.OnRequestedItemContextualMenu -= OnRequestedItemContextualMenu;
            m_Model.OnRequestedScrollToItem += OnRequestedScrollToItem;
            m_Model.ThumbnailsService.OnRendered -= MarkDirtyRepaint;
        }

        public void Update()
        {
            LogVerbose("Update()");

            if(m_Model == null)
                return;

            if (!IsAttachedToPanel)
                return;

            // Create new item views if required.
            for (var i = 0; i < m_Model.Items.Count; i++)
            {
                if (i >= m_Items.Count)
                    CreateItem(i);
            }

            FilterItemsSource();
        }

        static bool MatchesType(LibraryItemUIModel item, Type type)
        {
            return type.IsInstanceOfType(item.Target.Data.Model);
        }

        void FilterItemsSource()
        {
            var search = m_SearchBar?.value?.ToLower();
            m_FilteredItemsList.Clear();

            foreach (var item in m_Items)
            {
                if (!item.Model.IsVisible)
                    continue;

                if (!MatchesType(item.Model, k_ItemTypes[m_FilterDropdown.value.FirstOrDefault()]))
                    continue;

                var label = item.Model.Target.GetSearchLabel();

                if (string.IsNullOrEmpty(search) || label.Contains(search) || label.Equals(search))
                    m_FilteredItemsList.Add(item);
            }

            m_FilteredItemsList.Reverse();
            m_GridView.itemsSource = m_FilteredItemsList;
            m_GridView.Refresh();

            m_EmptyLibraryText.style.display = m_FilteredItemsList.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            m_EmptyLibraryText.text = m_Items.Count == 0 ? k_EmptyLibraryMessage : k_EmptyFilterResultsMessage;
        }

        void RegisterItem(LibraryItemUI item)
        {
            LogVerbose($"RegisterItem({item})");
            m_Items.Add(item);
            /*item.OnClicked += OnItemClicked;*/
            /*item.OnDoubleClicked += OnItemDoubleClicked;*/
        }

        void CreateItem(int index)
        {
            LogVerbose($"CreateItem({index})");
            var item = new LibraryItemUI();
            var target = m_Model.Items[index];
            var placement = new OptionalEnum<PopoverPlacement>(PopoverPlacement.Top);
            item.SetPreferredTooltipPlacement(placement);
            item.SetModel(target);
            RegisterItem(item);
        }

        static VisualElement MakeItemView()
        {
            return new VisualElement();
        }

        void OnSearchFieldChanged(ChangeEvent<string> evt)
        {
            OnSearch();
        }

        void OnSearchFieldChanging(ChangingEvent<string> evt)
        {
            OnSearch();
        }

        void OnSearch()
        {
            m_SearchBar.trailingElement.style.display = string.IsNullOrEmpty(m_SearchBar.value) ? DisplayStyle.None : DisplayStyle.Flex;
            FilterItemsSource();
        }

        void CancelSearch()
        {
            m_SearchBar.value = string.Empty;
            FilterItemsSource();
        }

        void OnThumbnailSizeSliderChanged(ChangingEvent<float> evt)
        {
            m_ResultsTraySize = evt.newValue;
            RefreshThumbnailSize();
        }

        void RefreshThumbnailSize()
        {
            var size = m_ThumbnailSizeSlider.value * k_DefaultThumbnailSize;
            var sizeAndMargin = size;
            var width = m_GridView.scrollView.contentContainer.resolvedStyle.width;
            var newCountPerRow = Mathf.FloorToInt(width / sizeAndMargin);

            newCountPerRow = Mathf.Max(1, newCountPerRow);

            if (newCountPerRow != m_CountPerRow)
            {
                m_CountPerRow = newCountPerRow;
                m_GridView.columnCount = m_CountPerRow;
            }

            var itemHeight = Mathf.FloorToInt(width / m_CountPerRow);

            if (!Mathf.Approximately(itemHeight, m_GridView.itemHeight))
                m_GridView.itemHeight = itemHeight;
        }

        void OnItemContextualMenuAction(LibraryItemContextualMenu.ActionType type, ClipboardService clipboard, SelectionModel<LibraryItemAsset> selectionModel, LibraryItemUIModel target)
        {
            LogVerbose($"OnItemContextualMenuAction({type}, {clipboard}, {selectionModel}, {target})");

            m_Model.RequestItemContextMenuAction(type, clipboard, selectionModel, target);
        }

        void OnItemClicked(LibraryItemUI item)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"LibraryUI -> OnItemClicked({item})");
            m_Model.OnRequestSelectItem(item.Model);
        }

        void OnItemDoubleClicked(LibraryItemUI item)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"LibraryUI -> OnItemDoubleClicked({item})");
            m_Model.OnRequestEditItem(item.Model);
        }

        void OnRequestedItemContextualMenu(LibraryItemUIModel item)
        {
            // TODO: Finish hooking up the buttons

            LogVerbose($"OnRequestedItemContextualMenu({item})");

            if (m_Model == null)
                return;

            LibraryItemUI matchingView = null;

            foreach (var view in m_Items)
            {
                if (view.Model == item)
                {
                    matchingView = view;
                    break;
                }
            }

            Debug.Assert(matchingView != null, "No matching view found for item");
            m_LibraryItemContextualMenu.Open(m_Model.ClipboardService, m_Model.SelectionModel, item, matchingView.ContextMenuParent);
        }

        void OnRequestedScrollToItem(LibraryItemUIModel item)
        {
            LogVerbose("OnRequestedScrollToItem("+item+")");

            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Model == item)
                {
                    m_GridView.ScrollToItem(i);
                    return;
                }
            }
        }

        void OnItemsChanged()
        {
            LogVerbose("OnItemsChanged()");
            Update();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (panel == null || float.IsNaN(evt.newRect.width) || Mathf.Approximately(0, evt.newRect.width))
                return;

            RefreshThumbnailSize();
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            var scroller = m_GridView.scrollView.verticalScroller;
            scroller.experimental.animation
                .Start(scroller.resolvedStyle.opacity, 1, 120,
                    (element, f) => element.style.opacity = f);
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            var scroller = m_GridView.scrollView.verticalScroller;

            if (m_VerticalScrollerDragContainer.HasPointerCapture(evt.pointerId))
                return;

            scroller.experimental.animation
                .Start(scroller.resolvedStyle.opacity, 0, 120,
                    (element, f) => element.style.opacity = f);
        }
        
        // [Section] Debugging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void LogVerbose(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"{GetType().Name} -> {msg}");
        }
        
        
        
    }
}
