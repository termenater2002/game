using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using Unity.Muse.Animate.Fbx;
using Unity.Muse.Animate.Usd;
using Unity.Muse.Animate.UserActions;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using AnimationMode = Unity.Muse.AppUI.UI.AnimationMode;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Model of the Application's Asset Library.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to the Application Library.
    /// Responsible for managing <see cref="LibraryItemAsset"/> assets.
    /// </remarks>
    class ApplicationLibraryModel
    {
        public event Action OnChangedActiveItem;

        /// <summary>
        /// The source library item of the model currently being edited.
        /// </summary>
        /// <remarks>
        /// If accessing this property from a SelectionChanged event, the TakeModel corresponds to the previous
        /// selection. This is useful if you want to do something with the previous selection before it is lost.
        /// </remarks>
        public LibraryItemAsset ActiveLibraryItem
        {
            get { return m_ActiveLibraryItem; }
            internal set
            {
                if (m_ActiveLibraryItem != value)
                {
                    m_ActiveLibraryItem = value;
#if UNITY_EDITOR
                    m_ActiveLibraryItemPath = UnityEditor.AssetDatabase.GetAssetPath(value);
#endif
                    InvokeChangedActiveItem();
                }
            }
        }

        public string ActiveLibraryItemPath => m_ActiveLibraryItemPath;

        void InvokeChangedActiveItem()
        {
            OnChangedActiveItem?.Invoke();
        }

        public enum ExportType
        {
            None,
            FBX,
            USD,
            HumanoidAnimation,
        }

        public enum ExportFlow
        {
            Manually,
            Drag,
            SubAsset
        }

        AuthoringModel m_AuthoringModel;
        LibraryItemAsset m_ActiveLibraryItem;

        ExportType m_LastExportType;
        bool m_EditSelectedTimelineNextFrame;
        bool m_EditSelectedTakeNextFrame;
        string m_ActiveLibraryItemPath;
        ApplicationContext m_Context;
        
        public LibraryItemAsset ExportingItem { get; private set; }

        internal ApplicationContext Context
        {
            get => m_Context;
            set
            {
                if (m_Context != null)
                {
                    Unsubscribe(Context.ApplicationModel, Context.LibraryUIModel);
                }

                m_Context = value;

                if (m_Context != null)
                {
                    Subscribe(Context.ApplicationModel, Context.LibraryUIModel);
                }
            }
        }

        internal ApplicationModel ApplicationModel => m_Context.ApplicationModel;
        internal AuthoringModel AuthoringModel => m_Context.AuthorContext.Authoring;

        SelectionModel<LibraryItemAsset> ItemsSelection { get; }

        internal ApplicationLibraryModel(SelectionModel<LibraryItemAsset> itemsSelection)
        {
            DevLogger.LogInfo("Created ApplicationModel");
            ItemsSelection = itemsSelection;
        }

        void Subscribe(ApplicationModel applicationModel, LibraryUIModel libraryUIModel)
        {
            Log($"Subscribe({libraryUIModel})");
            
            LibraryRegistry.OnItemPropertyChanged += OnItemPropertyChanged;
            
            // Subscribe to a LibraryUIModel and listen for user requests
            libraryUIModel.OnRequestedDragItem += OnLibraryUIRequestedDragItem;
            libraryUIModel.OnRequestedDeleteItem += OnLibraryUIRequestedDeleteItem;
            libraryUIModel.OnRequestedEditItem += OnLibraryUIRequestedEditItem;
            libraryUIModel.OnRequestedDuplicateItem += OnLibraryUIRequestedDuplicateItem;
            libraryUIModel.OnRequestedSelectItem += OnLibraryUIRequestedSelectItem;
            libraryUIModel.OnRequestedExportItem += OnLibraryUIRequestedExportItem;
            libraryUIModel.OnRequestedExportItemToFbx += OnLibraryUIRequestedExportItemToFbx;
            libraryUIModel.OnRequestedDeleteSelectedItems += OnLibraryUIRequestedDeleteSelectedItems;
            libraryUIModel.OnRequestedUsePrompt += OnLibraryUIRequestedUsePrompt;

            ResumeItems();
        }

        void Unsubscribe(ApplicationModel applicationModel, LibraryUIModel libraryUIModel)
        {
            Log($"Unsubscribe({libraryUIModel})");
            
            LibraryRegistry.OnItemPropertyChanged -= OnItemPropertyChanged;
            
            // Unsubscribe from the LibraryUIModel
            libraryUIModel.OnRequestedDeleteItem -= OnLibraryUIRequestedDeleteItem;
            libraryUIModel.OnRequestedEditItem -= OnLibraryUIRequestedEditItem;
            libraryUIModel.OnRequestedDuplicateItem -= OnLibraryUIRequestedDuplicateItem;
            libraryUIModel.OnRequestedSelectItem -= OnLibraryUIRequestedSelectItem;
            libraryUIModel.OnRequestedExportItem -= OnLibraryUIRequestedExportItem;
            libraryUIModel.OnRequestedExportItemToFbx -= OnLibraryUIRequestedExportItemToFbx;
            libraryUIModel.OnRequestedDeleteSelectedItems -= OnLibraryUIRequestedDeleteSelectedItems;
            libraryUIModel.OnRequestedUsePrompt -= OnLibraryUIRequestedUsePrompt;
        }

        internal void ResumeItems()
        {
            Log($"ResumeItems()");
            LibraryRegistry.RefreshAssets($"{GetType().Name}.ResumeItems()");

            // Look for Text to motion takes that were not
            // done solving when the previous session was saved
            foreach (var entry in LibraryRegistry.Items)
            {
                var itemAsset = entry.Value;

                if (itemAsset == null)
                    continue;

                if (itemAsset.Data == null)
                    continue;

                var model = itemAsset.Data.Model;

                if (model is TakeModel take)
                {
                    if (take.Progress < 1f && take is TextToMotionTake textToMotionTake)
                    {
                        Context.TextToMotionService.Request(textToMotionTake);
                    }
                }
            }
        }

        void OnItemPropertyChanged(LibraryItemAsset itemAsset, LibraryItemModel.Property property)
        {
            var model = itemAsset.Data.Model;
            
            switch(property)
            {
                case LibraryItemModel.Property.ItemType:
                    break;
                case LibraryItemModel.Property.IsValid:
                    break;
                case LibraryItemModel.Property.IsEditable:
                    break;
                case LibraryItemModel.Property.IsBaking:
                    // Just finished solving/baking
                    if (model.Progress >= 1f && !model.IsBaking)
                    {
                        // We need a new thumbnail.
                        RequestThumbnailUpdate(itemAsset);
                        RequestExportLibraryItem(itemAsset, ExportFlow.SubAsset);
                        Application.Instance.PublishMessage(new SaveLibraryItemAssetMessage(itemAsset));
                    }
                    break;
                
                case LibraryItemModel.Property.Title:
                    break;
                case LibraryItemModel.Property.Description:
                    break;
                case LibraryItemModel.Property.Progress:
                    break;
                case LibraryItemModel.Property.Thumbnail:
                    break;
                case LibraryItemModel.Property.OutputAnimationData:
                    break;
                case LibraryItemModel.Property.InputAnimationData:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }
        }

        // [Section] Do Methods

        void DoEditSelectedItem()
        {
            if (ItemsSelection.HasSelection && ItemsSelection.Count == 1)
            {
                if (ActiveLibraryItem != ItemsSelection.GetSelection(0))
                {
                    DoEditLibraryItem(ItemsSelection.GetSelection(0));
                }
            }
        }

        void DoEditLibraryItem(LibraryItemAsset itemAsset)
        {
            // Don't trigger the edit flow if the item is already being edited
            if (ActiveLibraryItem == itemAsset)
                return;
            
            // Save any ongoing work
            if (ActiveLibraryItem != null)
            {
                Application.Instance.PublishMessage(new SaveLibraryItemAssetMessage(ActiveLibraryItem));
            }

            // Tell the app to load the asset from it's path
#if UNITY_EDITOR
            Application.Instance.PublishMessage(new LoadLibraryItemAssetMessage(UnityEditor.AssetDatabase.GetAssetPath(itemAsset)));
#endif
        }

        /// <summary>
        /// Initiate export animation export.
        /// </summary>
        /// <param name="exportType">The type of file to export.</param>
        /// <param name="item">The item to export. If null, then we export either the working timeline
        /// or the selected take.</param>
        /// <param name="exportFlow">The export flow responsible for this export.</param>
        /// <remarks>
        /// The app calls this method when the user requests an export. It gathers the data to be exported and
        /// triggers the platform to start the export process. The platform will call <see cref="OnExport"/>
        /// when it is ready to receive data.
        /// </remarks>
        void DoExportLibraryItem(ExportType exportType, LibraryItemAsset item, ExportFlow exportFlow = ExportFlow.Manually)
        {
            if (Context.Stage.NumActors == 0)
            {
                DevLogger.LogError("DoExportLibraryItem() -> Cannot export stage, no actors!");
            }
            
            switch (exportType)
            {
                // TODO: This will be removed once platform passes export type
                case ExportType.None:
                    Debug.LogError($"Cannot start export if no export type has been set");
                    return;
                case ExportType.HumanoidAnimation when item == null:
                    Debug.LogError($"Cannot start export, item is null.");
                    return;
                case ExportType.HumanoidAnimation when item.Data == null:
                    Debug.LogError($"Cannot start export, item.Data is null.");
                    return;
                case ExportType.HumanoidAnimation:
                case ExportType.FBX:
                    var fileName = "";

                    if (item.Data.Model is TextToMotionTake take)
                    {
                        fileName = take.Prompt;
                    }
                    else if (item.Data.Model is KeySequenceTake denseTake)
                    {
                        fileName = denseTake.Title;
                    }

                    Unity.Muse.Common.Model.SendAnalytics(new ExportAnalytic(item.Description, exportType.ToString(), exportFlow.ToString()));
					
                    if (exportFlow == ExportFlow.SubAsset)
                    {
                        Application.Instance.PublishMessage(
                            new ExportAnimationMessage(
                                exportType,
                                item,
                                CollectExportData(item.Data.Model),
                                fileName,
                                exportFlow)
                        );
                        
                        return;
                    }

                    Application.Instance.PublishMessage(
                        new ExportAnimationMessage(
                            exportType,
                            item,
                            CollectExportData(item.Data.Model),
                            fileName,
                            exportFlow)
                    );
                    
                    break;
            }
        }

        void DoSelectLibraryItem(SelectionModel<LibraryItemAsset> selectionModel, LibraryItemAsset item)
        {
            selectionModel.Clear();
            selectionModel.Select(item);
        }

        void DoScrollToLibraryItem(LibraryItemAsset item)
        {
            Context.AuthorContext.LibraryUIModel.RequestScrollToItem(item);
        }

        void DoAddLibraryItem(LibraryItemAsset item)
        {
            DevLogger.LogInfo($"ApplicationLibraryModel -> DoAddLibraryItem({item})");

            // Note: Since the item is imported after it's creation, there is nothing to do here anymore.
            // See LibraryItemAssetPostProcessor and LibraryRegistry for more info.
        }

        void DoDeleteLibraryItem(LibraryItemAsset item)
        {
            if (item == ActiveLibraryItem)
            {
                ActiveLibraryItem = null;
            }

            if (ItemsSelection.IsSelected(item))
            {
                ItemsSelection.Unselect(item);

                // TODO: This should be handled by leaving the AuthoringModel.AuthoringMode.TextToMotionTake mode
                if (Context.AuthorContext.Authoring.Mode == AuthoringModel.AuthoringMode.TextToMotionTake)
                {
                    Context.AuthorContext.TextToMotionTakeContext.OutputBakedTimelineViewLogic.IsVisible = false;
                    Context.AuthorContext.TextToMotionTakeContext.Playback.MaxFrame = 0;
                    Context.AuthorContext.TextToMotionTakeContext.Model.Target = null;
                }
            }

            UserActionsManager.Instance.ClearAll(item);
            LibraryRegistry.Delete(item);
        }

        void DoDuplicateLibraryItem(LibraryItemAsset item)
        {
            LibraryRegistry.Duplicate(item);
        }

        void DoDeleteSelectedLibraryItems(SelectionModel<LibraryItemAsset> selectionModel)
        {
            throw new NotImplementedException();
        }

        // [Section] Ask Methods

        internal void AskEditLibraryItem(LibraryItemAsset item)
        {
            UserActionsManager.Instance.RecordEditLibraryItem(ActiveLibraryItem, item);
            RequestEditLibraryItem(item);
        }

        void AskDeleteLibraryItem(LibraryItemAsset item)
        {
            if (!EditorUtility.DisplayDialog(L10n.Tr("Delete asset permanently?"),
                    string.Format(L10n.Tr("Are you sure you want to delete {0} permanently? This cannot be reverted."), item.name),
                    L10n.Tr("Ok"), L10n.Tr("Cancel"), DialogOptOutDecisionType.ForThisMachine, "MuseAnimateDeletePermanently"))
                return;
            
            RequestDeleteLibraryItem(item);
        }

        void AskDuplicateLibraryItem(LibraryItemAsset item)
        {
            RequestDuplicateLibraryItem(item);
        }

        public LibraryItemAsset AskAddLibraryItem(LibraryItemModel itemModel, StageModel stage)
        {
            var asset = LibraryRegistry.CreateNewAsset(itemModel, stage);
            RequestAddLibraryItem(asset);
            return asset;
        }

        // [Section] Request Methods

        internal void RequestThumbnailUpdate(LibraryItemAsset item)
        {
            item.RequestThumbnailUpdate(Context.ThumbnailsService, Context.Camera);
        }
        
        void RequestDeleteSelectedLibraryItems(SelectionModel<LibraryItemAsset> selectionModel)
        {
            DoDeleteSelectedLibraryItems(selectionModel);
        }

        internal void RequestDeleteLibraryItem(LibraryItemAsset item)
        {
            DoDeleteLibraryItem(item);
        }

        internal void RequestAddLibraryItem(LibraryItemAsset item)
        {
            DoAddLibraryItem(item);
        }

        internal void RequestEditLibraryItem(LibraryItemAsset item)
        {
            Log($"RequestEditLibraryItem({item})");
            if (item == null)
            {
                //TODO: Requests to the ApplicationModel to navigate to the library should be passed through messages or events instead of direct invocation (MUSEANIM-416)
                ApplicationModel.DoGoToLibrary();
                return;
            }
            
            if (!item.IsPreviewable)
                return;
            
            DoEditLibraryItem(item);
        }

        internal void RequestScrollToLibraryItem(LibraryItemAsset item)
        {
            Log($"RequestScrollToLibraryItem({item})");
            DoScrollToLibraryItem(item);
        }

        internal void RequestDeleteLibraryItemModel(LibraryItemModel model)
        {
            // Export requests are handled by the authoring model
            if (!LibraryRegistry.TryGetOwnerOf(model, out var itemAsset))
            {
                LogError($"RequestDeleteLibraryItemModel() -> Could not find library item asset owning the model.");
                return;
            }

            RequestDeleteLibraryItem(itemAsset);
        }

        internal void RequestExportLibraryItem(LibraryItemAsset item, ExportFlow exportFlow = ExportFlow.Manually)
        {
            DoExportLibraryItem(ExportType.HumanoidAnimation, item, exportFlow);
        }
        
        internal void RequestExportLibraryItemToFbx(LibraryItemAsset item, ExportFlow exportFlow = ExportFlow.Manually)
        {
            DoExportLibraryItem(ExportType.FBX, item, exportFlow);
        }

        internal void RequestExportLibraryItemModel(LibraryItemModel model)
        {
            // Export requests are handled by the authoring model
            if (!LibraryRegistry.TryGetOwnerOf(model, out var itemAsset))
            {
                DevLogger.LogError($"AuthoringModel -> RequestExportLibraryItemModel() -> Could not find library item asset owning model to export.");
                return;
            }

            RequestExportLibraryItem(itemAsset);
        }

        internal void RequestDuplicateLibraryItem(LibraryItemAsset item)
        {
            DoDuplicateLibraryItem(item);
        }

        internal void RequestSelectLibraryItem(SelectionModel<LibraryItemAsset> selectionModel, LibraryItemAsset item)
        {
            DoSelectLibraryItem(selectionModel, item);
        }

        internal void RequestDragLibraryItem(LibraryItemAsset item)
        {
            var errorMessage = "";
            
            if (item.Data.Model is DenseTake denseTake)
            {
                if (Context.AuthorContext.TimelineContext.TimelineBakingLogic.IsRunning)
                {
                    errorMessage = "Current timeline is baking.";
                }
                else if(denseTake.BakedTimelineModel.FramesCount <= 0)
                {
                    errorMessage = "Dense take has no baked frames.";
                }
            }
            else if (item.Data.Model is KeySequenceTake keySequenceTake)
            {
                if (Context.AuthorContext.TimelineContext.TimelineBakingLogic.IsRunning)
                {
                    errorMessage = "Current timeline is baking.";
                }
                else if(keySequenceTake.BakedTimelineModel.FramesCount <= 0)
                {
                    errorMessage = "Current timeline has no baked frames.";
                }
            }
            
            if (errorMessage != "")
            {
                Context.RootVisualElement.OpenToast($"Dragging is disabled: {errorMessage}", NotificationStyle.Informative, NotificationDuration.Long, AnimationMode.Slide);
                return;
            }
            
            DoExportLibraryItem(ExportType.HumanoidAnimation, item, ExportFlow.Drag);
        }

        // [Section] UI Events Handlers

        // Library UI Events Handlers
        void OnLibraryUIRequestedDragItem(LibraryItemAsset item)
        {
            RequestDragLibraryItem(item);
        }

        void OnLibraryUIRequestedDeleteSelectedItems(SelectionModel<LibraryItemAsset> selectionModel)
        {
            RequestDeleteSelectedLibraryItems(selectionModel);
        }

        void OnLibraryUIRequestedDeleteItem(LibraryItemAsset item)
        {
            AskDeleteLibraryItem(item);
        }

        void OnLibraryUIRequestedSelectItem(SelectionModel<LibraryItemAsset> selectionModel, LibraryItemAsset item)
        {
            RequestSelectLibraryItem(selectionModel, item);
        }

        void OnLibraryUIRequestedEditItem(LibraryItemAsset item)
        {
            AskEditLibraryItem(item);
        }

        void OnLibraryUIRequestedExportItem(LibraryItemAsset item)
        {
            RequestExportLibraryItem(item);
        }
        
        void OnLibraryUIRequestedExportItemToFbx(LibraryItemAsset item)
        {
            RequestExportLibraryItemToFbx(item);
        }

        void OnLibraryUIRequestedDuplicateItem(LibraryItemAsset item)
        {
            AskDuplicateLibraryItem(item);
        }

        void OnLibraryUIRequestedUsePrompt(string prompt)
        {
            Context.TakesUIModel.Prompt = prompt;
        }

        // [Section] Export Methods

        // OnExport is called when the PLATFORM calls export.
        byte[] OnExport()
        {
            // TODO: This will be removed once platform passes export type
            if (m_LastExportType == ExportType.None)
            {
                LogWarning("Cannot perform export if the export type is not set");
                return Array.Empty<byte>();
            }

            try
            {
                if (m_LastExportType == ExportType.FBX)
                {
                    if (FBXExport.TryGetExportData(out byte[] data))
                    {
                        return data;
                    }

                    // TODO: Make exception not USD specific
                    throw new IOException("FBX Export failed");
                }

                throw new ArgumentOutOfRangeException(nameof(m_LastExportType), "Unsupported export type.");
            }
            catch (Exception e)
            {
                var toast = Toast.Build(Context.AuthorContext.RootUI, e.Message, NotificationDuration.Short);

                toast.SetStyle(NotificationStyle.Negative);
                toast.SetAnimationMode(AnimationMode.Slide);
                toast.Show();
            }

            m_LastExportType = ExportType.None;
            return Array.Empty<byte>();
        }

        ExportData CollectExportData(LibraryItemModel source)
        {
            if (Context.AuthorContext.TimelineContext.TimelineBakingLogic.IsRunning)
                throw new InvalidOperationException("Cannot export: Animation is still baking.");
           
            if (source == null)
                throw new InvalidOperationException("Cannot export: source is null.");

            var contextBakedTimeline = source switch
            {
                DenseTake take => take.BakedTimelineModel,
                KeySequenceTake keySequenceTake => keySequenceTake.BakedTimelineModel,
                _ => null
            };
            
            if (contextBakedTimeline == null)
                throw new InvalidOperationException($"Cannot export: source type {source.GetType()} is not supported.");
            
            if(Context.Stage.NumActors == 0)
                throw new InvalidOperationException($"Cannot export: Context.Stage has 0 actors.");
            
            var actors = new ExportData.ActorExportData[Context.Stage.NumActors];
            for (var i = 0; i < actors.Length; ++i)
            {
                var exportTargetActor = Context.Stage.GetActorModel(i);
                var actorDefinitionComponent = Context.Stage.GetActorInstance(exportTargetActor.ID);
                
                
                var posingArmature = Context.AuthorContext.PoseAuthoringLogic.GetPosingArmature(exportTargetActor.EntityID);
                actors[i] = new ExportData.ActorExportData(actorDefinitionComponent, exportTargetActor, i, posingArmature);
            }

            var props = new ExportData.PropExportData[Context.Stage.NumProps];
            for (var i = 0; i < props.Length; ++i)
            {
                var exportTargetProp = Context.Stage.GetPropModel(i);
                var propDefinitionComponent = Context.Stage.GetPropInstance(exportTargetProp.ID);
                Context.Stage.PropRegistry.TryGetPropInfo(exportTargetProp.PrefabID, out var propDef);
                
                props[i] = new ExportData.PropExportData(propDef.Prefab.name, exportTargetProp, propDefinitionComponent, i);
            }

            return new ExportData(contextBakedTimeline, actors, props);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Log(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Info, GetType().Name + " -> " + msg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogError(string msg)
        {
            DevLogger.LogError(GetType().Name + " -> " + msg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void LogWarning(string msg)
        {
            DevLogger.LogWarning(GetType().Name + " -> " + msg);
        }
    }
}
