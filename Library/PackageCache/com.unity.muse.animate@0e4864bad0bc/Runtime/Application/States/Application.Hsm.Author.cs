using System;
using Hsm;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
    using UnityEngine.InputSystem;
#endif

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// Used when Authoring a <see cref="LibraryItemAsset"/>.
            /// </summary>
            public class Author : ApplicationState<AuthorContext>, IPointerClickHandler, IKeyDownHandler
            {
                Transition m_NextTransition = Transition.None();
                LibraryItemAsset m_LoadedAsset;
                LibraryItemModel m_LoadedModel;
                
                int m_NextAvailableTakeNameId;
                bool m_EditSelectedTakeNextFrame;
                bool m_EditSelectedTimelineNextFrame;
                

                bool IsEditingKey => Context.Authoring.Mode is AuthoringModel.AuthoringMode.Timeline &&
                    Context.Authoring.Timeline.Mode is TimelineAuthoringModel.AuthoringMode.EditKey;

                public bool IsBusy
                {
                    get
                    {
                        if (Context.TimelineContext.TimelineBakingLogic.IsRunning)
                            return true;

                        if (Context.TextToMotionService.Baking.IsRunning)
                            return true;

                        if (Context.MotionToTimelineContext.OutputTimelineBaking.IsRunning)
                            return true;

                        if (Context.MotionToTimelineContext.MotionToKeysSampling.IsRunning)
                            return true;

                        return false;
                    }
                }

                public override Transition GetTransition()
                {
                    return m_NextTransition;
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(new[] { args[0] });

                    if (args[1] is not LibraryItemAsset itemAsset)
                    {
                        LogError("OnEnter() -> Could not retrieve m_Asset LibraryItemAsset from args[1]");
                        return;
                    }

                    m_LoadedAsset = itemAsset;
                    
                    if (m_LoadedAsset.Data == null)
                    {
                        LogError("OnEnter() -> m_Asset.Data is null!");
                        return;
                    }
                    if (m_LoadedAsset.Data.Model == null)
                    {
                        LogError("OnEnter() -> m_Asset.Data.Model is null!");
                        return;
                    }

                    m_LoadedModel = m_LoadedAsset.Data.Model;

                    // Setup the UI
                    SetupUI();

                    // Register to models
                    RegisterEvents();
                    
                    // Setup the Posing Controls
                    // - Hide the posing controls by default
                    Context.PoseAuthoringLogic.IsVisible = false;

                    // - Force an update since the value might not have changed
                    Context.PoseAuthoringLogic.ForceUpdateAllViews();

                    // Setup the Playback View
                    Context.TimelineContext.BakedTimelineViewLogic.IsVisible = false;

                    // Broadcast the fact that the authoring has started (hides the loading overlay)
                    Instance.PublishMessage(new AuthoringStartedMessage());
                    
                    if (m_LoadedModel is not TakeModel take)
                    {
                        LogError("OnEnter() -> m_LoadedModel is not a TakeModel!");
                        return;
                    }
                    
                    switch (take)
                    {
                        case KeySequenceTake keyedTake:
                            // Copy the data from the KeySequenceTake to the main authoring/editing timeline
                            Context.Authoring.Mode = AuthoringModel.AuthoringMode.Timeline;
                            break;

                        case TextToMotionTake motionTake:
                            // Display the output
                            Context.TextToMotionTakeContext.Model.Target = motionTake;
                            // Set the authoring mode
                            Context.Authoring.Mode = AuthoringModel.AuthoringMode.TextToMotionTake;
                            break;

                        case VideoToMotionTake videoTake:
                            // Display the output
                            Context.VideoToMotionTakeContext.Model.PlaybackTarget = videoTake;
                            // Set the authoring mode
                            Context.Authoring.Mode = AuthoringModel.AuthoringMode.VideoToMotionTake;
                            break;
                    }
                }

                public override void OnExit()
                {
                    base.OnExit();
                    
                    UnregisterEvents();
                    ClearUI();
                    
                    Instance.PublishMessage(new AuthoringEndedMessage());
                }

                void RegisterEvents()
                {
                    // Library Registry Events
                    LibraryRegistry.OnItemMoved += OnItemMoved;
                    LibraryRegistry.OnItemAdded += OnItemAdded;
                    LibraryRegistry.OnItemRemoved += OnItemRemoved;

                    // Thumbnail Requests
                    Context.Authoring.OnRequestedGenerateKeyThumbnail += OnRequestedKeyThumbnail;
                    Context.Authoring.OnRequestedGenerateFrameThumbnail += OnRequestedFrameThumbnail;

                    // Video to Motion Requests
                    // Skip the round-trip through the authoring model, for now
                    Context.TextToMotionTakeContext.BakingTaskStatusUI.TrackBakingLogics(Context.VideoToMotionService.Baking);

                    // Authoring Events
                    Context.Authoring.OnModeChanged += OnAuthoringModeChanged;
                    Context.Authoring.OnTitleChanged += OnAuthoringTitleChanged;
                    Context.Authoring.Timeline.OnRequestSaveWorkingStage += OnRequestedSaveWorkingStage;
                }

                void UnregisterEvents()
                {
                    // Library Registry Events
                    LibraryRegistry.OnItemMoved -= OnItemMoved;
                    LibraryRegistry.OnItemAdded -= OnItemAdded;
                    LibraryRegistry.OnItemRemoved -= OnItemRemoved;

                    // Thumbnail Requests
                    Context.Authoring.OnRequestedGenerateKeyThumbnail -= OnRequestedKeyThumbnail;
                    Context.Authoring.OnRequestedGenerateFrameThumbnail -= OnRequestedFrameThumbnail;
                    
                    // Authoring Events
                    Context.Authoring.OnModeChanged -= OnAuthoringModeChanged;
                    Context.Authoring.OnTitleChanged -= OnAuthoringTitleChanged;
                    Context.Authoring.Timeline.OnRequestSaveWorkingStage -= OnRequestedSaveWorkingStage;
                }

                void OnItemRemoved(LibraryItemAsset item)
                {
                    if (item == m_LoadedAsset)
                    {
                        Instance.ApplicationModel.DoGoToLibrary();
                    }
                }

                void OnItemAdded(LibraryItemAsset item)
                {
                    if (item == m_LoadedAsset)
                    {
                        Debug.LogError("Adding a take that is currently being authored is not supposed to happen.");
                    }
                }

                void OnItemMoved(LibraryItemAsset itemBeforeMove, string previousPath, string newPath)
                {
                    if (itemBeforeMove == m_LoadedAsset)
                    {
                        Instance.IsAuthoringAssetLoaded = false;
                        Context.Authoring.Mode = AuthoringModel.AuthoringMode.Unknown;

                        var message2 = new LoadLibraryItemAssetMessage(newPath);
                        Instance.PublishMessage(message2);
                    }
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnPointerClick({eventData.button})");
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnKeyDown(KeyCode: {eventData.KeyCode})");

                    if (Context.TakesUIModel.IsWriting)
                    {
                        eventData.Use();
                        return;
                    }

                    switch (eventData.KeyCode)
                    {
                        case KeyCode.R:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.Authoring.Timeline.AskResetPose();
                                eventData.Use();
                            }

                            break;

                        // TODO: Fix shortcuts
                        // Disabling this until we fix keyboard shortcuts of different parts of the UI interfering with each other
                        /*
                        case KeyCode.D:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.Authoring.AskDuplicateAny();
                                eventData.Use();
                            }

                            break;

                        case KeyCode.C:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.Authoring.AskCopyAny();
                                eventData.Use();
                            }

                            break;

                        case KeyCode.V:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.Authoring.AskPasteAny();
                                eventData.Use();
                            }

                            break;
                        */
                    }
                }

                // [Section] Update Methods

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    Context.SceneViewUI.Update();
                    UpdateStateTransition();
                }

                public override void LateUpdate(float aDeltaTime)
                {
                    base.LateUpdate(aDeltaTime);
                    Context.SceneViewUI.LateUpdate();
                }
                
                public override void Render(float aDeltaTime)
                {
                    base.Render(aDeltaTime);
                    Context.SceneViewUI.Render();
                }

                /// <summary>
                /// Update the various baking logics of this state.
                /// </summary>
                /// <param name="delta"></param>
                /// <returns>Returns true if is interrupting further bakes.</returns>
                public override bool UpdateBaking(float delta)
                {
                    if (base.UpdateBaking(delta))
                        return true;
                    
                    // Do not bake timeline if the posing solver is still solving a pose
                    // Note: This is necessary because the posing is done over multiple updates/frames.
                    if (Context.PoseAuthoringLogic.IsSolving)
                        return true;

                    // Do not bake when holding / dragging mouse (For a smoother experience)
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
                        //new system
                        if (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed)
                        return true;
#else

                    //old system
                    if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                        return true;
#endif

                    if (Context.ThumbnailsService.HasRequests)
                    {
                        Context.ThumbnailsService.Update();
                    }

                    return false;
                }

                void UpdateStateTransition()
                {
                    switch (Context.Authoring.Mode)
                    {
                        case AuthoringModel.AuthoringMode.Unknown:
                            //m_NextTransition = Transition.Inner<BrowseAssets>(Context);
                            break;

                        case AuthoringModel.AuthoringMode.Timeline:
                            m_NextTransition = Transition.Inner<AuthorTimeline>(Context.TimelineContext);
                            break;

                        case AuthoringModel.AuthoringMode.TextToMotionTake:
                            m_NextTransition = Transition.Inner<AuthorTextToMotion>(Context.TextToMotionTakeContext);
                            break;

                        case AuthoringModel.AuthoringMode.ConvertMotionToTimeline:
                            m_NextTransition = Transition.Inner<AuthorMotionToTimeline>(Context.MotionToTimelineContext);
                            break;

                        case AuthoringModel.AuthoringMode.VideoToMotionTake:
                            m_NextTransition = Transition.Inner<AuthorVideoToMotion>(Context.VideoToMotionTakeContext);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                void UpdateToolbars()
                {
                    Context.Toolbars.Update();
                }

                void UpdateTitle()
                {
                    Context.SceneViewUI.Header.TitleText.text = Context.Authoring.Title;
                }
                
                // [Section] Setup Methods

                void SetupUI()
                {
                    Context.Authoring.RegisterToUI();
                    UpdateTitle();
                }

                
                
                // [Section] Clearing Methods

                void ClearUI()
                {
                    Context.Authoring.UnregisterFromUI();
                }
                
                // [Section] Thumbnails Requests

                void OnRequestedKeyThumbnail(ThumbnailModel target, KeyModel key)
                {
                    Context.Authoring.DoRefreshKeyThumbnail(target, key);
                }

                void OnRequestedFrameThumbnail(ThumbnailModel target, BakedTimelineModel timeline, int frame)
                {
                    Context.Authoring.DoRefreshFrameThumbnail(target, timeline, frame);
                }
                
                // [Section] Authoring Requests Handlers

                /// <summary>
                /// Copies the StageModel's working Timeline, BakedTimelineData and BakedTimelineMappingData into the currently active take.
                /// </summary>
                void OnRequestedSaveWorkingStage()
                {
                    var activeItem = Context.ApplicationLibraryModel.ActiveLibraryItem;
                    
                    if (activeItem != null)
                    {
                        if (activeItem.Data.Model is KeySequenceTake activeTake)
                        {
                            activeTake.SaveWorkingStage(Context.Stage);
                            Context.ApplicationLibraryModel.RequestExportLibraryItem(activeItem, ApplicationLibraryModel.ExportFlow.SubAsset);
                        }
                    }
                }
                
                // [Section] Authoring Events Handlers

                void OnAuthoringModeChanged()
                {
                    UpdateToolbars();
                    UpdateStateTransition();
                }

                void OnAuthoringTitleChanged()
                {
                    UpdateTitle();
                }

                
            }
        }
    }
}
