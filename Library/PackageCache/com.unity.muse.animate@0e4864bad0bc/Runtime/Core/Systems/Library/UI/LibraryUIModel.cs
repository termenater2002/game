using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class LibraryUIModel
    {
        public event Action OnItemsChanged;
        public event Action OnSelectionChanged;

        public event Action<LibraryItemUIModel> OnRequestedScrollToItem;
        public event Action<LibraryItemUIModel> OnRequestedItemContextualMenu;
        public event Action<SelectionModel<LibraryItemAsset>, LibraryItemAsset> OnRequestedSelectItem;
        public event Action<LibraryItemAsset> OnRequestedEditItem;
        public event Action<LibraryItemAsset> OnRequestedDuplicateItem;
        public event Action<LibraryItemAsset> OnRequestRenameItem;
        public event Action<LibraryItemAsset> OnRequestedExportItem;
        public event Action<LibraryItemAsset> OnRequestedExportItemToFbx;
        public event Action<LibraryItemAsset> OnRequestedDeleteItem;
        public event Action<SelectionModel<LibraryItemAsset>> OnRequestedDeleteSelectedItems;
        public event Action<LibraryItemAsset> OnRequestedDragItem;

        public event Action<string> OnRequestedUsePrompt;

        public IReadOnlyList<LibraryItemUIModel> Items => m_Items;
        public LibraryItemAsset SelectedItem => m_SelectionModel.HasSelection ? m_SelectionModel.GetSelection(0) : null;
        public ClipboardService ClipboardService => m_ClipboardService;
        public SelectionModel<LibraryItemAsset> SelectionModel => m_SelectionModel;
        public ThumbnailsService ThumbnailsService => m_ThumbnailsService;

        readonly ClipboardService m_ClipboardService;
        readonly SelectionModel<LibraryItemAsset> m_SelectionModel;
        readonly ThumbnailsService m_ThumbnailsService;

        List<LibraryItemUIModel> m_Items = new();
        static IReadOnlyList<LibraryItemAsset> BrowsingList => LibraryRegistry.ItemsList;

        LibraryItemUIModel m_HighlightedItem;
        LibraryItemUIModel m_RenamingItem;

        protected internal LibraryUIModel(SelectionModel<LibraryItemAsset> selectionModel,
            ClipboardService clipboardService,
            ThumbnailsService thumbnailsService)
        {
            m_SelectionModel = selectionModel;
            m_SelectionModel.OnSelectionChanged += OnSelectionModelChanged;
            m_ClipboardService = clipboardService;
            m_ThumbnailsService = thumbnailsService;
            LibraryRegistry.OnChanged += OnLibraryChanged;
            UpdateItems();
        }

        void OnSelectionModelChanged(SelectionModel<LibraryItemAsset> model)
        {
            Log($"OnSelectionModelChanged({model})");
            UpdateItemSelection();
            OnSelectionChanged?.Invoke();
        }

        void OnLibraryChanged()
        {
            Log($"OnLibraryChanged()");
            UpdateItems();
        }

        void HideItem(int index)
        {
            Log($"HideItem({index})");
            m_Items[index].IsVisible = false;
        }

        protected void RegisterItem(LibraryItemUIModel item)
        {
            Log($"RegisterItem("+item+")");
            m_Items.Add(item);
            item.OnRequestEdit += OnRequestEditItem;
            item.OnLeftClickedSingle += OnItemLeftClickedSingle;
            item.OnDragStarted += OnItemDragStarted;
            item.OnContextMenuRequested += OnItemRequestedContextMenu;
            item.OnPlayThumbnail += OnItemRequestedPlayThumbnail;
            item.OnStopThumbnail += OnItemRequestedStopThumbnail;

            // This is an event meant for the UI to update itself, but we need for some library-wide logic
            item.OnStateChanged += OnItemStateChanged;
        }

        void OnItemLeftClickedSingle(LibraryItemUIModel obj)
        {
            RequestSelectItem(obj.Target);
        }

        protected void CreateItem(int index)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, "TakesLibraryUIModel-> CreateItem("+index+")");

            var target = LibraryRegistry.ItemsList[index];
            var item = new LibraryItemUIModel();
            item.SetTarget(target);
            RegisterItem(item);
        }

        void UpdateItems()
        {
            Log($"UpdateItems()");

            // Add new items
            for (var i = m_Items.Count; i < BrowsingList.Count; i++)
            {
                CreateItem(i);
            }

            // Update or Hide extra items.
            for (var i = 0; i < m_Items.Count; i++)
            {
                if (i < BrowsingList.Count)
                {
                    m_Items[i].SetTarget(BrowsingList[i]);
                    UpdateItem(m_Items[i]);
                }
                else
                {
                    HideItem(i);
                }
            }

            OnItemsChanged?.Invoke();
        }

        void UpdateItemSelection()
        {
            foreach (var item in m_Items)
            {
                UpdateItem(item);
            }
        }

        void UpdateItem(LibraryItemUIModel item)
        {
            Log($"UpdateItem({item})");

            item.IsVisible = true;
            item.IsSelected = IsSelected(item);
        }

        bool IsSelected(LibraryItemUIModel key)
        {
            var selected = m_SelectionModel.IsSelected(key.Target);
            Log($"IsSelected({key}) -> {selected}");
            return selected;
        }

        public void RequestDeleteSelectedItems()
        {
            Log($"RequestDeleteSelectedItems()");
            OnRequestedDeleteSelectedItems?.Invoke(m_SelectionModel);
        }

        void RequestSelectItem(LibraryItemAsset item)
        {
            Log($"RequestSelectItem({item})");
            OnRequestedSelectItem?.Invoke(m_SelectionModel, item);
        }

        void RequestDragItem(LibraryItemAsset item)
        {
            Log($"RequestDragItem({item})");
            OnRequestedDragItem?.Invoke(item);
        }

        public void RequestScrollToItem(LibraryItemAsset item)
        {
            Log($"RequestScrollToItem({item})");
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i].Target == item)
                {
                    OnRequestedScrollToItem?.Invoke(m_Items[i]);
                    return;
                }
            }
        }

        void RequestEditItem(LibraryItemAsset item)
        {
            if (!item.IsPreviewable)
                return;

            Log($"RequestEditItem({item})");
            OnRequestedEditItem?.Invoke(item);
        }

        void RequestItemContextualMenu(LibraryItemUIModel item)
        {
            Log($"RequestItemContextualMenu({item})");
            OnRequestedItemContextualMenu?.Invoke( item);
        }

        public void RequestItemContextMenuAction(LibraryItemContextualMenu.ActionType type, ClipboardService clipboard, SelectionModel<LibraryItemAsset> selectionModel, LibraryItemUIModel target)
        {
            Log($"RequestItemContextMenuAction({type}, {clipboard}, {selectionModel}, {target})");
            switch (type)
            {
                case LibraryItemContextualMenu.ActionType.Delete:
                    OnRequestedDeleteItem?.Invoke(target.Target);
                    break;
                case LibraryItemContextualMenu.ActionType.Export:
                    OnRequestedExportItem?.Invoke(target.Target);
                    break;
                case LibraryItemContextualMenu.ActionType.ExportToFbx:
                    OnRequestedExportItemToFbx?.Invoke(target.Target);
                    break;
                case LibraryItemContextualMenu.ActionType.Duplicate:
                    OnRequestedDuplicateItem?.Invoke(target.Target);
                    break;
                case LibraryItemContextualMenu.ActionType.Rename:
                    StartItemRename(target);
                    break;
                case LibraryItemContextualMenu.ActionType.UsePrompt when target.Model is TextToMotionTake take:
                    OnRequestedUsePrompt?.Invoke(take.Prompt);
                    break;
            }
        }

        public void OnRequestSelectItem(LibraryItemUIModel item)
        {
            Log($"OnItemLeftClicked({item})");

            // If already selected, switch authoring mode between posing and previewing
            if (item.IsSelected)
            {
                return;
            }

            RequestSelectItem(item.Target);
        }

        public void OnRequestEditItem(LibraryItemUIModel item)
        {
            Log($"OnItemDoubleClicked({item})");
            RequestEditItem(item.Target);
        }

        void OnItemDragStarted(LibraryItemUIModel item)
        {
            Log($"OnItemDragStarted({item})");
            RequestSelectItem(item.Target);
            RequestDragItem(item.Target);
        }

        void OnItemRequestedContextMenu(LibraryItemUIModel item)
        {
            Log($"OnItemRequestedContextMenu({item})");
            RequestItemContextualMenu(item);
        } 

        void OnItemRequestedPlayThumbnail(RenderTexture target, LibraryItemAsset libraryItemAsset)
        {
            var bakedTimeline = libraryItemAsset.Data.Model switch
            {
                DenseTake denseTake => denseTake.BakedTimelineModel,
                KeySequenceTake keySequenceTake => keySequenceTake.BakedTimelineModel,
                _ => null
            };

            if (bakedTimeline == null || bakedTimeline.FramesCount == 0)
                return;

            var position = libraryItemAsset.Preview.Thumbnail.Position;
            var rotation = libraryItemAsset.Preview.Thumbnail.Rotation;
            m_ThumbnailsService.RequestAnimatedThumbnail(target, bakedTimeline, position, rotation);
        }

        void OnItemRequestedStopThumbnail(RenderTexture target)
        {
            m_ThumbnailsService.CancelAnimatedRequestOf(target);
        }

        void OnItemStateChanged(LibraryItemUIModel item, LibraryItemUIModel.ItemState state)
        {
            if (state is LibraryItemUIModel.ItemState.Highlighted && item.IsHighlighted)
            {
                if (m_HighlightedItem != null && m_HighlightedItem != item)
                {
                    m_HighlightedItem.IsHighlighted = false;
                }

                m_HighlightedItem = item;
            }

            else if (state is LibraryItemUIModel.ItemState.EditingName && item.IsEditingName)
            {
                if (m_RenamingItem != null && m_RenamingItem != item)
                {
                    m_RenamingItem.IsEditingName = false;
                }

                m_RenamingItem = item;
            }
        }

        void StartItemRename(LibraryItemUIModel item)
        {
            Log($"StartItemRename({item})");
            item.IsEditingName = true;
        }

        void EndItemRename(LibraryItemUIModel item)
        {
            Log($"EndItemRename({item})");
            item.IsEditingName = false;
            OnRequestRenameItem?.Invoke(item.Target);
        }

        // [Section] Debugging
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void Log(string msg)
        {
            if (!ApplicationConstants.DebugLibraryUI)
                return;

            DevLogger.LogInfo($"{GetType().Name} -> {msg}");
        }
        
        
    }
}
