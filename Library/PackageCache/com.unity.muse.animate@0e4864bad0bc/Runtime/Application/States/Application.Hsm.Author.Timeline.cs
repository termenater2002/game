using System;
using System.Collections.Generic;
using Hsm;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// The user authors a <see cref="KeySequenceTake"/>.
            /// </summary>
            public class AuthorTimeline : ApplicationState<AuthorTimelineContext>, IKeyDownHandler, IPointerClickHandler
            {
                Transition m_NextTransition = Transition.None();
                float m_NextTick;

                const float k_TickInterval = 1f;
                int m_SolvedPoseTick;

                HashSet<int> m_QueuedChangedKeyIndices = new();
                HashSet<int> m_QueuedChangedTransitionIndices = new();

                TimelineAuthoringModel TimelineAuthoring => Context.AuthoringModel.Timeline;

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);
                    StartEditingActiveTake();
                }

                public override void OnExit()
                {
                    base.OnExit();
                    StopEditing();
                }
                
                void StartEditingActiveTake()
                {
                    if (Context.ApplicationLibraryModel.ActiveLibraryItem == null)
                    {
                        DevLogger.LogError("StartEditingActiveTake() -> Context.ApplicationLibraryModel.ActiveLibraryItem is null");
                        return;
                    }
                    
                    if (Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model == null )
                    {
                        DevLogger.LogError("StartEditingActiveTake() -> Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model is null");
                        return;
                    }
                    
                    var take = Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model as KeySequenceTake;

                    if (take == null)
                    {
                        DevLogger.LogError("StartEditingActiveTake() -> Could not convert to KeySequenceTake");
                        return;
                    }
                    
                    StartEditingTake(take);
                }
                
                void StartEditingTake(KeySequenceTake take)
                {
                    // Set the title
                    Context.AuthoringModel.Title = Context.AuthoringModel.TargetName;

                    // Copy the data from the KeySequenceTake to the main authoring/editing timeline
                    take.TimelineModel.CopyTo(Context.Stage.WorkingTimeline);
                    take.BakedTimelineModel.CopyTo(Context.Stage.WorkingBakedTimeline);
                    take.BakedTimelineMappingModel.CopyTo(Context.Stage.WorkingBakedTimelineMapping);
                    
                    // Stop the playback
                    Context.BakedTimelineViewLogic.ResetAllLoopOffsets();
                    Context.Playback.MaxFrame = Context.Stage.WorkingBakedTimeline.FramesCount - 1;
                    Context.Playback.CurrentFrame = 0;
                    Context.Playback.Stop();
                    
                    // Clear entity selection
                    Context.EntitySelection.Clear();

                    // Setup the UI
                    SetupUI();

                    // Register to models
                    RegisterEvents();
                    
                    // Add the two first keys if none is present
                    if (Context.Stage.WorkingTimeline.KeyCount == 0)
                    {
                        CreateStartingKeys();
                    }

                    // Request a thumbnail update on every key of the timeline
                    foreach (var key in Context.Stage.WorkingTimeline.Keys)
                    {
                        Context.ThumbnailsService.RequestThumbnail(key.Key.Thumbnail, key.Key, Context.Camera.Position, Context.Camera.Rotation);
                    }
                    
                    if (Context.Stage.WorkingBakedTimeline.FramesCount == 0)
                    {
                        // Queue the baking of the timeline (run motion synthesis)
                        // Note: Forcing update to allow Context.TimelineBakingLogic.BakedTimelineMapping to be populated
                        Context.TimelineBakingLogic.QueueBaking(true);
                    }
                    
                    TimelineAuthoring.Mode = TimelineAuthoringModel.AuthoringMode.Unknown;

                    // Enter Posing Mode on the first key
                    TimelineAuthoring.RequestEditKey(Context.Stage.WorkingTimeline.GetKey(0));
                    TimelineAuthoring.DoFrameCamera();
                }

                void StopEditing()
                {
                    // Stop the animation
                    Context.Playback.Stop();

                    if (Context.TimelineBakingLogic.IsRunning)
                    {
                        // If we're baking, this means that we haven't saved any of the working changes.
                        // It also means that the baked data is not up to date, so clear it out.
                        Context.Stage.WorkingBakedTimelineMapping.Clear();
                        Context.Stage.WorkingBakedTimeline.Clear(true);

                        Context.TimelineBakingLogic.Cancel();
                    }

                    if (TimelineAuthoring.IsDirty)
                    {
                        TimelineAuthoring.RequestSaveWorkingStage();
                    }

                    TimelineAuthoring.ActiveKey = null;
                    TimelineAuthoring.ActiveTransition = null;

                    TimelineAuthoring.Mode = TimelineAuthoringModel.AuthoringMode.Unknown;

                    // Hide the animation
                    Context.BakedTimelineViewLogic.IsVisible = false;

                    // Close the UI
                    ClearUI();
                    
                    // Unregister the events
                    UnregisterEvents();
                }
                
                void SetupUI()
                {
                    // Hide the side panel
                    Context.SidePanelUI.IsVisible = false;
                    
                    // Show the Inspectors Panel
                    Context.InspectorsPanelViewModel.IsVisible = true;
                    
                    // Setup TimelineUI to target the updated Stage's components
                    Context.TimelineUI.TimelineModel = Context.Stage.WorkingTimeline;
                    Context.TimelineUI.PlaybackModel = Context.Playback;
                    Context.TimelineUI.BakedTimelineMappingModel = Context.Stage.WorkingBakedTimelineMapping;
                    Context.TimelineUI.IsReadOnly = false;
                    
                    // Refresh the UI
                    UpdateUI();
                }

                void ClearUI()
                {
                    // Hide the Timeline UI
                    Context.TimelineUI.IsVisible = false;
                    
                    // Clear TimelineUI components
                    Context.TimelineUI.TimelineModel = null;
                    Context.TimelineUI.PlaybackModel = null;
                    Context.TimelineUI.BakedTimelineMappingModel = null;
                    
                    // Hide the Inspectors Panel
                    Context.InspectorsPanelViewModel.IsVisible = false;
                }

                void RegisterEvents()
                {
                    // CAMERA
                    Context.Camera.OnIsDraggingControlChanged += OnCameraIsDraggingControlChanged;

                    // TIMELINE UI REQUESTS
                    TimelineAuthoring.RegisterToEvents();

                    // AUTHORING REQUESTS

                    // Entities Requests
                    TimelineAuthoring.OnRequestedDeleteSelectedEntities += OnRequestedDeleteSelectedEntities;

                    // Playback Requests
                    TimelineAuthoring.OnRequestedSeekToFrame += OnPlaybackRequestedSeekToFrame;

                    // Keys Authoring Requests
                    TimelineAuthoring.OnRequestedAddKey += OnRequestedAddKey;
                    TimelineAuthoring.OnRequestedDeleteSelectedKeys += OnRequestedDeleteSelectedKeys;
                    TimelineAuthoring.OnRequestedEditKey += OnRequestedEditKey;
                    TimelineAuthoring.OnRequestedEditKeyIndex += OnRequestedEditKeyIndex;
                    TimelineAuthoring.OnRequestedPreviewKey += OnRequestedPreviewKey;
                    TimelineAuthoring.OnRequestedSeekToKey += OnRequestedSeekToKey;
                    TimelineAuthoring.OnRequestedInsertKeyWithEffectorRecovery += OnRequestedInsertKeyWithEffectorRecovery;
                    TimelineAuthoring.OnRequestedInsertKey += OnRequestedInsertKey;
                    TimelineAuthoring.OnRequestedInsertKeyCopy += OnRequestedInsertKeyCopy;
                    TimelineAuthoring.OnRequestedMoveKey += OnRequestedMoveKey;
                    TimelineAuthoring.OnRequestedDuplicateKey += OnRequestedDuplicateKey;
                    TimelineAuthoring.OnRequestedDeleteKey += OnRequestedDeleteKey;
                    TimelineAuthoring.OnRequestedPreview += OnRequestedPreview;

                    TimelineAuthoring.OnRequestedResetPose += OnRequestedResetPose;
                    TimelineAuthoring.OnRequestedCopyPose += OnRequestedCopyPose;
                    TimelineAuthoring.OnRequestedPastePose += OnRequestedPastePose;

                    // Transitions Authoring Requests
                    TimelineAuthoring.OnRequestedSelectTransition += OnRequestedSelectTransition;
                    TimelineAuthoring.OnRequestedEditTransition += OnRequestedEditTransition;
                    TimelineAuthoring.OnRequestedPreviewTransition += OnRequestedPreviewTransition;
                    TimelineAuthoring.OnRequestedSeekToTransition += OnRequestedSeekToTransition;

                    //----------------
                    // EVENTS
                    //----------------

                    // Authoring Events
                    TimelineAuthoring.OnChanged += OnTimelineAuthoringChanged;
                    TimelineAuthoring.OnModeChanged += OnTimelineAuthoringModeChanged;

                    // Posing Events
                    Context.PoseAuthoringLogic.OnSolveFinished += OnPosingSolveFinished;

                    // Playback Events
                    Context.Playback.OnChanged += OnPlaybackChanged;

                    // Timeline Events
                    Context.Stage.WorkingTimeline.OnKeyAdded += OnTimelineKeyAdded;
                    Context.Stage.WorkingTimeline.OnKeyChanged += OnTimelineKeyChanged;
                    Context.Stage.WorkingTimeline.OnKeyRemoved += OnTimelineKeyRemoved;
                    Context.Stage.WorkingTimeline.OnTransitionChanged += OnTimelineTransitionChanged;

                    // Timeline Baking Events
                    Context.Stage.WorkingBakedTimeline.OnChanged += OnBakedTimelineChanged;
                    Context.TimelineBakingLogic.OnBakingCompleted += OnBakingCompleted;
                    
                    // Selection Events
                    Context.EntitySelection.OnSelectionChanged += OnEntitySelectionChanged;
                }

                void OnBakingCompleted(BakingLogic.BakingEventData eventData)
                {
                    TimelineAuthoring.RequestSaveWorkingStage();
                }

                void UnregisterEvents()
                {
                    //-----------------------
                    // CAMERA
                    //-----------------------
                    Context.Camera.OnIsDraggingControlChanged -= OnCameraIsDraggingControlChanged;

                    //----------------------
                    // TIMELINE UI REQUESTS
                    //----------------------
                    TimelineAuthoring.UnregisterFromEvents();

                    //--------------------
                    // AUTHORING REQUESTS
                    //--------------------

                    // Entities Requests
                    TimelineAuthoring.OnRequestedDeleteSelectedEntities -= OnRequestedDeleteSelectedEntities;

                    // Playback Requests
                    TimelineAuthoring.OnRequestedSeekToFrame -= OnPlaybackRequestedSeekToFrame;

                    // Keys Authoring Requests
                    TimelineAuthoring.OnRequestedAddKey -= OnRequestedAddKey;
                    TimelineAuthoring.OnRequestedDeleteSelectedKeys -= OnRequestedDeleteSelectedKeys;
                    TimelineAuthoring.OnRequestedEditKey -= OnRequestedEditKey;
                    TimelineAuthoring.OnRequestedEditKeyIndex -= OnRequestedEditKeyIndex;
                    TimelineAuthoring.OnRequestedPreviewKey -= OnRequestedPreviewKey;
                    TimelineAuthoring.OnRequestedSeekToKey -= OnRequestedSeekToKey;
                    TimelineAuthoring.OnRequestedInsertKeyWithEffectorRecovery -= OnRequestedInsertKeyWithEffectorRecovery;
                    TimelineAuthoring.OnRequestedInsertKey -= OnRequestedInsertKey;
                    TimelineAuthoring.OnRequestedInsertKeyCopy -= OnRequestedInsertKeyCopy;
                    TimelineAuthoring.OnRequestedMoveKey -= OnRequestedMoveKey;
                    TimelineAuthoring.OnRequestedDuplicateKey -= OnRequestedDuplicateKey;
                    TimelineAuthoring.OnRequestedDeleteKey -= OnRequestedDeleteKey;

                    TimelineAuthoring.OnRequestedResetPose -= OnRequestedResetPose;
                    TimelineAuthoring.OnRequestedCopyPose -= OnRequestedCopyPose;
                    TimelineAuthoring.OnRequestedPastePose -= OnRequestedPastePose;

                    // Transitions Authoring Requests
                    TimelineAuthoring.OnRequestedSelectTransition -= OnRequestedSelectTransition;
                    TimelineAuthoring.OnRequestedEditTransition -= OnRequestedEditTransition;
                    TimelineAuthoring.OnRequestedPreviewTransition -= OnRequestedPreviewTransition;
                    TimelineAuthoring.OnRequestedSeekToTransition -= OnRequestedSeekToTransition;

                    //----------------
                    // EVENTS
                    //----------------

                    // Authoring Events
                    TimelineAuthoring.OnChanged -= OnTimelineAuthoringChanged;
                    TimelineAuthoring.OnModeChanged -= OnTimelineAuthoringModeChanged;

                    // Posing Events
                    Context.PoseAuthoringLogic.OnSolveFinished -= OnPosingSolveFinished;

                    // Playback Events
                    Context.Playback.OnChanged -= OnPlaybackChanged;

                    // Timeline Events
                    Context.Stage.WorkingTimeline.OnKeyAdded -= OnTimelineKeyAdded;
                    Context.Stage.WorkingTimeline.OnKeyChanged -= OnTimelineKeyChanged;
                    Context.Stage.WorkingTimeline.OnKeyRemoved -= OnTimelineKeyRemoved;

                    // Timeline Baking Events
                    Context.Stage.WorkingBakedTimeline.OnChanged -= OnBakedTimelineChanged;
                    Context.TimelineBakingLogic.OnBakingCompleted -= OnBakingCompleted;
                    
                    // Selection Events
                    Context.EntitySelection.OnSelectionChanged -= OnEntitySelectionChanged;
                }

                void CreateStartingKeys()
                {
                    Context.Stage.WorkingTimeline.AddKey(false);
                    Context.Stage.WorkingTimeline.AddKey(false);

                    for (var i = 0; i < Context.Stage.WorkingTimeline.KeyCount; i++)
                    {
                        var key = Context.Stage.WorkingTimeline.Keys[i];

                        // Load the key
                        Context.PoseAuthoringLogic.RestorePosingStateFromKey(key.Key);

                        // Solve the physics for the key
                        Context.PoseAuthoringLogic.SolvePhysicsFully();

                        // Apply the solved pose on the key
                        Context.PoseAuthoringLogic.ApplyPosingStateToKey(key.Key);

                        // Request and render the thumbnail right away
                        RefreshKeyThumbnail(key.Key.Thumbnail, key.Key);

                        Context.ThumbnailsService.Update();
                    }
                }

                public override void LateUpdate(float aDeltaTime)
                {
                    base.LateUpdate(aDeltaTime);

                    ProcessChangedKeys(out var hasAnyKeyChanged, out var hasActiveKeyChanged);
                    ProcessChangedTransitions(out var hasAnyTransitionChanged, out var hasActiveTransitionChanged);

                    if (hasAnyKeyChanged || hasAnyTransitionChanged)
                    {
                        UserActionsManager.Instance.BackupForUndo();

                        QueueTimelineBaking();
                        TimelineAuthoring.IsDirty = true;
                        
                        var timelineKey = Context.Stage.WorkingTimeline.GetKey(0);
                        Assert.IsNotNull(timelineKey, $"Could not locate timeline thumbnail key at index 0");
                        
                        RefreshKeyThumbnail(Context.ApplicationLibraryModel.ActiveLibraryItem.Preview.Thumbnail, timelineKey.Key);

                        if (!Context.Camera.IsDraggingControl)
                        {
                            UserActionsManager.Instance.ResetUserEdit();
                        }
                    }
                }

                void ProcessChangedTransitions(out bool hasAnyChanged, out bool hasActiveChanged)
                {
                    if (Context.Camera.IsDraggingControl
                        || m_QueuedChangedTransitionIndices.Count <= 0)
                    {
                        hasAnyChanged = false;
                        hasActiveChanged = false;
                        return;
                    }

                    hasAnyChanged = true;
                    hasActiveChanged = false;

                    foreach (var index in m_QueuedChangedTransitionIndices)
                    {
                        ProcessChangedTransition(index, out var hasActiveChangedNow);
                        hasActiveChanged = hasActiveChanged || hasActiveChangedNow;
                    }

                    m_QueuedChangedTransitionIndices.Clear();
                }

                void ProcessChangedTransition(int index, out bool hasActiveChanged)
                {
                    var sequenceTransition = Context.Stage.WorkingTimeline.GetTransition(index);
                    Assert.IsNotNull(sequenceTransition, $"Could not locate changed transition at index {index}");
                    var transition = sequenceTransition.Transition;

                    // Save the modification and backup the active key again for the next user edit
                    if (TimelineAuthoring.ActiveTransition != null && TimelineAuthoring.ActiveTransition.Transition.ListIndex == index)
                    {
                        hasActiveChanged = true;
                        if (UserActionsManager.Instance.IsUserEditing)
                        {
                            UserActionsManager.Instance.RecordModifiedTimelineTransition(Context.Stage.WorkingTimeline, transition.ListIndex, transition);
                        }
                    }
                    else if (TimelineAuthoring.ActiveKey != null
                             && TimelineAuthoring.ActiveKey.OutTransition != null
                             && TimelineAuthoring.ActiveKey.OutTransition.Transition.ListIndex == index)
                    {
                        hasActiveChanged = true;
                        if (UserActionsManager.Instance.IsUserEditing)
                        {
                            UserActionsManager.Instance.RecordModifiedTimelineTransition(Context.Stage.WorkingTimeline, transition.ListIndex, transition);
                        }
                    }

                    hasActiveChanged = false;
                }

                void ProcessChangedKeys(out bool hasAnyKeyChanged, out bool hasActiveKeyChanged)
                {
                    if (Context.Camera.IsDraggingControl
                        || m_QueuedChangedKeyIndices.Count <= 0)
                    {
                        hasAnyKeyChanged = false;
                        hasActiveKeyChanged = false;
                        return;
                    }

                    hasAnyKeyChanged = true;
                    hasActiveKeyChanged = false;

                    foreach (var index in m_QueuedChangedKeyIndices)
                    {
                        ProcessChangedKey(index, out var hasActiveKeyChangedNow);
                        hasActiveKeyChanged = hasActiveKeyChanged || hasActiveKeyChangedNow;
                    }

                    m_QueuedChangedKeyIndices.Clear();
                }

                void ProcessChangedKey(int keyIndex, out bool hasActiveKeyChanged)
                {
                    var sequenceKey = Context.Stage.WorkingTimeline.GetKey(keyIndex);
                    Assert.IsNotNull(sequenceKey, $"Could not locate changed key at index {keyIndex}");
                    var key = sequenceKey.Key;

                    RefreshKeyThumbnail(sequenceKey.Thumbnail, key);

                    // Save the modification and backup the active key again for the next user edit
                    if (TimelineAuthoring.ActiveKey != null && TimelineAuthoring.ActiveKey.Key.ListIndex == keyIndex)
                    {
                        if (UserActionsManager.Instance.IsUserEditing)
                        {
                            UserActionsManager.Instance.RecordModifiedTimelineKey(Context.Stage.WorkingTimeline, key.ListIndex, key);
                        }

                        hasActiveKeyChanged = true;
                    }
                    else
                    {
                        hasActiveKeyChanged = false;
                    }
                }

                public override bool UpdateBaking(float delta)
                {
                    if (base.UpdateBaking(delta))
                        return true;

                    // Note: Dont do baking of the timeline at this level because
                    // it is used outside of the timeline editing context.
                    // See Author state, 1 level above.

                    return false;
                }

                void UpdateStateTransition()
                {
                    switch (TimelineAuthoring.Mode)
                    {
                        case TimelineAuthoringModel.AuthoringMode.Unknown:
                            m_NextTransition = Transition.None();
                            break;

                        case TimelineAuthoringModel.AuthoringMode.Preview:
                            m_NextTransition = Transition.Inner<AuthorTimelinePreview>(Context.PreviewContext);
                            break;

                        case TimelineAuthoringModel.AuthoringMode.EditKey:
                        {
                            if (!Context.KeySelection.HasSelection)
                            {
                                m_NextTransition = Transition.Inner<AuthorTimelinePreview>(Context.PreviewContext);
                                break;
                            }

                            var selectedKey = Context.KeySelection.GetSelection(0);

                            m_NextTransition = selectedKey.Key.Type switch
                            {
                                KeyData.KeyType.Empty => Transition.Inner<AuthorTimelinePreview>(Context.PreviewContext),
                                KeyData.KeyType.FullPose => Transition.Inner<AuthorTimelineKeyPose>(Context.KeyPoseContext),
                                KeyData.KeyType.Loop => Transition.Inner<AuthorTimelineKeyLoop>(Context.KeyLoopContext),
                                _ => throw new ArgumentOutOfRangeException()
                            };
                            break;
                        }

                        case TimelineAuthoringModel.AuthoringMode.EditTransition:
                        {
                            if (!Context.TransitionSelection.HasSelection)
                            {
                                m_NextTransition = Transition.Inner<AuthorTimelinePreview>(Context.PreviewContext);
                                break;
                            }

                            m_NextTransition = Transition.Inner<AuthorTimelineTransition>(Context.TransitionContext);
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                void UpdateUI()
                {
                    
                    // Update Timeline UI
                    Context.TimelineUI.IsEditingKey = TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.EditKey;
                    Context.TimelineUI.IsEditingTransition = TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.EditTransition;
                    Context.TimelineUI.CurrentFrame = Context.Playback.CurrentFrame;
                    Context.TimelineUI.IsPlaying = Context.Playback.IsPlaying;

                    // Update Toolbars
                    Context.Toolbars.Update();

                    // Update Playback UI
                    Context.TimelineUI.PlaybackViewModel.EmphasizeTransition = TimelineAuthoring.Mode != TimelineAuthoringModel.AuthoringMode.Preview;
                    Context.TimelineUI.PlaybackViewModel.ShowPlusButton = TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.Preview;

                    // Update Timeline Sequencer Current Key / Transition
                    var index = Mathf.FloorToInt(Context.Playback.CurrentFrame);

                    if (Context.Stage.WorkingBakedTimelineMapping.TryGetKeyIndex(index, out var keyIndex))
                    {
                        Context.TimelineUI.CurrentKeyIndex = keyIndex;
                        Context.TimelineUI.CurrentTransitionIndex = keyIndex;
                        TimelineAuthoring.RequestSelectKey(Context.Stage.WorkingTimeline.GetKey(keyIndex));
                    }
                    else if (Context.Stage.WorkingBakedTimelineMapping.TryGetTransitionIndex(index, out var transitionIndex))
                    {
                        Context.TimelineUI.CurrentKeyIndex = transitionIndex;
                        Context.TimelineUI.CurrentTransitionIndex = transitionIndex;
                        TimelineAuthoring.RequestSelectKey(Context.Stage.WorkingTimeline.GetKey(transitionIndex));
                    }
                    else
                    {
                        Context.TimelineUI.CurrentKeyIndex = -1;
                        Context.TimelineUI.CurrentTransitionIndex = -1;
                    }
                    
                    // Update UI Visibility
                    switch (TimelineAuthoring.Mode)
                    {
                        default:
                        case TimelineAuthoringModel.AuthoringMode.Unknown:
                            Context.TimelineUI.IsVisible = false;
                            break;

                        case TimelineAuthoringModel.AuthoringMode.Preview:
                        case TimelineAuthoringModel.AuthoringMode.EditKey:
                        case TimelineAuthoringModel.AuthoringMode.EditTransition:
                            Context.TimelineUI.IsVisible = true;
                            break;
                    }
                }

                void QueueTimelineBaking()
                {
                    Context.TimelineBakingLogic.QueueBaking(true);
                }

                void RefreshKeyThumbnail(ThumbnailModel target, KeyModel key)
                {
                    Context.ThumbnailsService.RequestThumbnail(target, key, Context.Camera.Position, Context.Camera.Rotation);
                }

                // [Section] Keyboard and Mouse Inputs

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnKeyDown({eventData.KeyCode})");

                    switch (eventData.KeyCode)
                    {
                        case KeyCode.F:
                            TimelineAuthoring.DoFrameCamera();
                            eventData.Use();
                            break;
                        
                        case KeyCode.Space:
                            DoTogglePlayback();
                            eventData.Use();
                            break;
                        
                        case KeyCode.LeftArrow:
                            Context.TimelineUI.SequenceViewModel.SelectPreviousItem();
                            eventData.Use();
                            break;
                        
                        case KeyCode.RightArrow:
                            Context.TimelineUI.SequenceViewModel.SelectNextItem();
                            eventData.Use();
                            break;
                        
                        case KeyCode.R:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.AuthoringModel.Timeline.AskResetPose();
                                eventData.Use();
                            }

                            break;
                        
                        // TODO: Fix shortcuts
                        // Disabling this until we fix keyboard shortcuts of different parts of the UI interfering with each other
                        /*
                        case KeyCode.Delete:
                            if (Context.EntitySelection.Count > 0 && Context.AuthoringModel.LastSelectionType == AuthoringModel.SelectionType.Entity)
                            {
                                Context.AuthoringModel.AskDeleteSelectedEntities();
                                eventData.Use();
                            }
                            else if (Context.AuthoringModel.LastSelectionType == AuthoringModel.SelectionType.SequenceKey)
                            {
                                Context.AuthoringModel.AskDeleteSelectedKey();
                                eventData.Use();
                            }
                            
                            break;
                        case KeyCode.D:
                            
                            if (eventData.IsControlOrCommand)
                            {
                                Context.AuthoringModel.AskDuplicateAny();
                                eventData.Use();
                            }         
                            
                            break;
                        case KeyCode.C:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.AuthoringModel.AskCopyAny();
                                eventData.Use();
                            }

                            break;
                        case KeyCode.V:
                            if (eventData.IsControlOrCommand)
                            {
                                Context.AuthoringModel.AskPasteAny();
                                eventData.Use();
                            }

                            break;
                    
                        */
                    }
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnPointerClick({eventData.button})");

                    Context.EntitySelection.Clear();
                    eventData.Use();
                }
                
                // [Section] Actions

                void DoTogglePlayback()
                {
                    if (Context.Playback.IsPlaying)
                    {
                        Context.Playback.Pause();
                    }
                    else
                    {
                        Context.Playback.Play(Context.Playback.CurrentFrame >= Context.Playback.MaxFrame);
                    }
                }
                
                // [Section] Capabilities Checks

                void RefreshCanDeleteSelectedEntities()
                {
                    TimelineAuthoring.RefreshCanDeleteSelectedEntities();
                }

                void RefreshCanCopyPose()
                {
                    TimelineAuthoring.RefreshCanCopyPose();
                }
                
                // [Section] Camera Events Handlers

                void OnCameraIsDraggingControlChanged(CameraModel camera, bool isDragging)
                {
                    if (isDragging)
                    {
                        UserActionsManager.Instance.StartUserEdit("Pose");
                    }
                }
                
                // [Section] Timeline Authoring Events Handlers

                //----------------------------
                // Timeline Authoring Events
                //----------------------------

                void OnTimelineAuthoringModeChanged()
                {
                    RefreshCanCopyPose();
                    RefreshCanDeleteSelectedEntities();
                    UpdateUI();
                    UpdateStateTransition();
                }

                void OnTimelineAuthoringChanged()
                {
                    RefreshCanCopyPose();
                    RefreshCanDeleteSelectedEntities();
                    UpdateUI();
                }

                //----------------------------
                // Timeline Authoring Requests
                //----------------------------

                void OnRequestedDeleteSelectedEntities()
                {
                    TimelineAuthoring.DoDeleteSelectedEntities();
                }

                void OnRequestedAddKey()
                {
                    TimelineAuthoring.DoAddKey();
                }

                void OnRequestedDeleteKey(TimelineModel.SequenceKey key)
                {
                    TimelineAuthoring.DoRemoveKey(key);
                    /*
                    ShowConfirmationPrompt("Delete Key", "Are you sure you want to delete this sequence key?",
                        () =>
                        {
                            TimelineAuthoring.DoRemoveKey(key);
                        },"Delete"
                        , "Cancel");*/
                }

                TimelineModel.SequenceKey OnRequestedDuplicateKey(int fromIndex, int toIndex)
                {
                    return TimelineAuthoring.DoDuplicateKey(fromIndex, toIndex);
                }

                void OnRequestedMoveKey(int fromIndex, int toIndex)
                {
                    TimelineAuthoring.DoMoveKey(fromIndex, toIndex);
                }

                void OnRequestedEditKeyIndex(int index)
                {
                    OnRequestedEditKey(Context.Stage.WorkingTimeline.GetKey(index));
                }

                void OnRequestedEditKey(TimelineModel.SequenceKey key)
                {
                    TimelineAuthoring.DoEditKey(key);
                }

                void OnRequestedPreviewKey(TimelineModel.SequenceKey key)
                {
                    TimelineAuthoring.DoPreviewKey(key);
                }

                void OnRequestedSeekToKey(TimelineModel.SequenceKey key)
                {
                    TimelineAuthoring.DoSeekToKey(key);
                }

                void OnRequestedInsertKey(int keyIndex, out TimelineModel.SequenceKey sequenceKey)
                {
                    TimelineAuthoring.DoInsertKey(keyIndex, out sequenceKey);
                }

                void OnRequestedInsertKeyCopy(out TimelineModel.SequenceKey insertedKey, int atIndex, KeyModel keyToPaste, TransitionModel keyOutTransitionToPaste, bool splitTransition, float splitTimePercent)
                {
                    TimelineAuthoring.DoInsertKeyCopy(out insertedKey, atIndex, keyToPaste, keyOutTransitionToPaste, splitTransition, splitTimePercent);
                }

                void OnRequestedInsertKeyWithEffectorRecovery(int bakedFrameIndex, int keyIndex, float progress, out TimelineModel.SequenceKey key)
                {
                    TimelineAuthoring.DoInsertKeyWithEffectorRecovery(bakedFrameIndex, keyIndex, progress, out key);
                }

                void OnRequestedDeleteSelectedKeys()
                {
                    TimelineAuthoring.DoDeleteSelectedKey();
                }

                // Transitions Authoring Requests Handlers

                void OnRequestedSelectTransition(TimelineModel.SequenceTransition transition)
                {
                    TimelineAuthoring.DoSelectTransition(transition);
                }

                void OnRequestedEditTransition(TimelineModel.SequenceTransition transition)
                {
                    TimelineAuthoring.DoEditTransition(transition);
                }

                void OnRequestedPreviewTransition(TimelineModel.SequenceTransition transition)
                {
                    TimelineAuthoring.DoPreviewTransition(transition);
                }

                void OnRequestedSeekToTransition(TimelineModel.SequenceTransition transition)
                {
                    TimelineAuthoring.DoSeekToTransition(transition);
                }

                void OnRequestedPreview()
                {
                    TimelineAuthoring.DoPreview();
                }

                //----------------------------
                // Pose Authoring Requests
                //----------------------------

                void OnRequestedResetPose()
                {
                    TimelineAuthoring.DoResetPose();
                }

                void OnRequestedCopyPose()
                {
                    TimelineAuthoring.DoCopyPose();
                }

                void OnRequestedPastePose()
                {
                    TimelineAuthoring.DoPastePose();
                }
                
                // [Section] Selection Events Handlers

                // Selection Events Handlers

                void OnEntitySelectionChanged(SelectionModel<EntityID> model)
                {
                    if (model.HasSelection)
                    {
                        Context.AuthoringModel.LastSelectionType = AuthoringModel.SelectionType.Entity;
                    }

                    RefreshCanCopyPose();
                    RefreshCanDeleteSelectedEntities();

                    // Exit preview mode if selecting an entity
                    if (model.HasSelection && TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.Preview)
                    {
                        var currentFrame = Mathf.FloorToInt(Context.Playback.CurrentFrame);
                        if (Context.Stage.WorkingBakedTimelineMapping.TryGetFirstKeyBefore(currentFrame, out _, out var keyTimelineIndex))
                        {
                            var key = Context.Stage.WorkingTimeline.GetKey(keyTimelineIndex);
                            TimelineAuthoring.RequestEditKey(key);
                        }
                    }
                }
                
                // [Section] Playback Requests Handlers

                void OnPlaybackRequestedSeekToFrame(float frame)
                {
                    Context.Playback.CurrentFrame = frame;
                    Context.BakedTimelineViewLogic.ResetAllLoopOffsets();

                    if (TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.EditKey || TimelineAuthoring.Mode == TimelineAuthoringModel.AuthoringMode.EditTransition)
                    {
                        TimelineAuthoring.DoPreview();
                    }
                }

                // [Section] Timeline / Playback / Posing Events Handlers

                void OnPosingSolveFinished(PoseAuthoringLogic logic)
                {
                    if (ApplicationConstants.DebugPosingSolver)
                        Debug.Log($"OnPosingSolveFinished({m_SolvedPoseTick})");

                    var sequenceKey = TimelineAuthoring.ActiveKey;
                    Context.PoseAuthoringLogic.ApplyPosingStateToKey(sequenceKey.Key);
                    m_SolvedPoseTick++;
                }

                void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
                {
                    UpdateUI();

                    // Go to preview mode if started playing
                    if (property != PlaybackModel.Property.IsPlaying)
                        return;

                    if (!model.IsPlaying)
                        return;

                    Context.AuthoringModel.Timeline.RequestPreview();
                }

                // Timeline Events Handlers

                void OnBakedTimelineChanged(BakedTimelineModel model)
                {
                    Context.Playback.MaxFrame = model.FramesCount - 1;
                }

                void OnTimelineTransitionChanged(TimelineModel model, TransitionModel transition, TransitionModel.Property property)
                {
                    switch (property)
                    {
                        case TransitionModel.Property.Type:
                            OnTimelineAuthoringModeChanged();
                            break;

                        case TransitionModel.Property.Duration:
                            if (!m_QueuedChangedTransitionIndices.Contains(transition.ListIndex))
                            {
                                m_QueuedChangedTransitionIndices.Add(transition.ListIndex);
                            }

                            break;
                    }
                }

                void OnTimelineKeyChanged(TimelineModel model, KeyModel key, KeyModel.Property property)
                {
                    if (property is not KeyModel.Property.Thumbnail)
                    {
                        m_QueuedChangedKeyIndices.Add(key.ListIndex);
                    }

                    if (property is KeyModel.Property.Type)
                    {
                        OnTimelineAuthoringModeChanged();
                    }
                }

                void OnTimelineKeyRemoved(TimelineModel model, TimelineModel.SequenceKey key)
                {
                    Context.KeySelection.Unselect(key);
                    Context.ThumbnailsService.CancelRequestOf(key.Key.Thumbnail);
                }

                void OnTimelineKeyAdded(TimelineModel model, TimelineModel.SequenceKey key)
                {
                    RefreshKeyThumbnail(key.Thumbnail, key.Key);
                }
                
                // [Section] Getters

                public override Transition GetTransition()
                {
                    return m_NextTransition;
                }
                
                // [Section] Depreciated Methods

                // Here lies the methods that were used before features were cut. 
                // The methods here might one day be used again,
                // so they are kept here to save time/brain cycles later.

                void InitializeKeyPoses(ActorModel actorModel)
                {
                    for (var i = 0; i < Context.Stage.WorkingTimeline.KeyCount; i++)
                    {
                        var key = Context.Stage.WorkingTimeline.Keys[i];
                        Context.PoseAuthoringLogic.ApplyPosingStateToKey(actorModel.EntityID, key.Key);
                    }
                }

                void ShowConfirmationPrompt(string title, string description, Action callback, string confirmLabel = "Delete", string cancelLabel = "Cancel")
                {
                    var dialog = new AlertDialog
                    {
                        title = title,
                        description = description,
                        variant = AlertSemantic.Destructive
                    };

                    dialog.SetPrimaryAction(99, confirmLabel, callback);
                    dialog.SetCancelAction(1, cancelLabel);

                    var modal = Modal.Build(Context.RootUI, dialog);
                    modal.Show();
                }

                
            }
        }
    }
}
