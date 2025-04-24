using System;
using Hsm;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Muse.AppUI.UI;
using Unity.AppUI.Core;
using Unity.Muse.Animate.UserActions;
using Unity.Muse.Common;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// The user defines parameters to ultimately sample keys and generate a new <see cref="KeySequenceTake"/> from a <see cref="DenseTake"/>.
            /// </summary>
            public class AuthorMotionToTimeline : ApplicationState<AuthorMotionToTimelineContext>, IKeyDownHandler, IPointerClickHandler
            {
                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    // Show the UI
                    Context.MotionToTimelineUIModel.IsVisible = true;

                    // Set the title
                    Context.AuthoringModel.Title = "Converting Motion To Keys";

                    // Clear entity selection
                    Context.EntitySelectionModel.Clear();

                    // Start with looping on
                    Context.Playback.IsLooping = true;

                    // Register Events
                    RegisterEvents();

                    // Show the currently selected take
                    ActivateSelectedTake();

                    // Show the UI in the side panel
                    Context.SidePanelUI.AddPanel(SidePanelUtils.PageType.ConvertToFrames, Context.SamplingUI);
                    Context.SidePanelUI.SelectedPageIndex = (int)SidePanelUtils.PageType.ConvertToFrames;
                    Context.SidePanelUI.IsVisible = true;
                }

                public override void OnExit()
                {
                    base.OnExit();

                    // Reset the authoring step to "None"
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.None;

                    // Pause the playback
                    if (Context.Playback.IsPlaying)
                        Context.Playback.Pause();

                    // Stop any baking or parsing
                    if (Context.MotionToKeysSampling.IsRunning)
                        Context.MotionToKeysSampling.Cancel();

                    Context.OutputTimelineBaking.Cancel();

                    // Hide the Views
                    Context.OutputBakedTimelineViewLogic.IsVisible = false;
                    Context.InputBakedTimelineViewLogic.IsVisible = false;
                    Context.SidePanelUI.RemovePanel(SidePanelUtils.PageType.ConvertToFrames, Context.SamplingUI);

                    // Hide the UI
                    Context.MotionToTimelineUIModel.IsVisible = false;

                    // Unregister events
                    UnregisterEvents();
                }

                void RegisterEvents()
                {
                    // Track the baking and sampling to show it on the UI
                    Context.BakingTaskStatusUI.TrackBakingLogics(Context.OutputTimelineBaking);
                    Context.BakingTaskStatusUI.TrackSamplingLogics(Context.MotionToKeysSampling);

                    // Timeline UI Events
                    Context.Model.RegisterToUI(Context.MotionToTimelineUIModel.TimelineUIModel);

                    // Model Events
                    Context.Model.OnChanged += OnModelChanged;

                    // Model User Requests
                    Context.Model.OnRequestConfirm += RequestConfirm;
                    Context.Model.OnRequestPreview += RequestPreview;
                    Context.Model.OnRequestCancel += RequestCancel;

                    // Timeline events
                    Context.OutputTimeline.OnKeyAdded += OnTimelineKeyAdded;
                    Context.OutputTimeline.OnKeyChanged += OnTimelineKeyChanged;
                    Context.OutputTimeline.OnKeyRemoved += OnTimelineKeyRemoved;

                    // Playback Requests
                    Context.Model.OnRequestedSeekToFrame += OnTimelineRequestedSeekToFrame;
                    Context.Model.OnRequestedSeekToKey += OnTimelineRequestedSeekToKey;

                    // Playback Events
                    Context.Playback.OnChanged += OnPlaybackChanged;
                    Context.MotionToTimelineUIModel.PlaybackUIModel.OnSeekedToFrame += OnPlaybackSeekedToFrame;

                    // Motion to keys sampling events
                    Context.MotionToKeysSampling.OnCompleted += OnMotionToKeysSamplingComplete;
                    Context.MotionToKeysSampling.OnFailed += OnMotionToKeysSamplingFailed;

                    // Baking events
                    Context.OutputTimelineBaking.OnBakingCompleted += OnOutputTimelineBakingCompleted;
                    Context.OutputTimelineBaking.OnBakingFailed += OnOutputTimelineBakingFailed;

                    Context.ItemsSelection.OnSelectionChanged += OnItemsSelectionChanged;
                }

                void UnregisterEvents()
                {
                    // Stop tracking the baking and sampling
                    Context.BakingTaskStatusUI.UntrackBakingLogics(Context.OutputTimelineBaking);
                    Context.BakingTaskStatusUI.UntrackSamplingLogics(Context.MotionToKeysSampling);

                    // Timeline UI Events
                    Context.Model.UnregisterFromUI(Context.MotionToTimelineUIModel.TimelineUIModel);

                    // Model Events
                    Context.Model.OnChanged -= OnModelChanged;

                    // Model User Requests
                    Context.Model.OnRequestConfirm -= RequestConfirm;
                    Context.Model.OnRequestPreview -= RequestPreview;
                    Context.Model.OnRequestCancel -= RequestCancel;

                    // Timeline Events
                    Context.OutputTimeline.OnKeyAdded -= OnTimelineKeyAdded;
                    Context.OutputTimeline.OnKeyChanged -= OnTimelineKeyChanged;
                    Context.OutputTimeline.OnKeyRemoved -= OnTimelineKeyRemoved;

                    // Playback Requests
                    Context.AuthoringModel.MotionToTimeline.OnRequestedSeekToFrame += OnTimelineRequestedSeekToFrame;
                    Context.AuthoringModel.MotionToTimeline.OnRequestedSeekToKey += OnTimelineRequestedSeekToKey;

                    // Playback Events
                    Context.Playback.OnChanged -= OnPlaybackChanged;
                    Context.MotionToTimelineUIModel.PlaybackUIModel.OnSeekedToFrame -= OnPlaybackSeekedToFrame;

                    // Sampling Events
                    Context.MotionToKeysSampling.OnCompleted -= OnMotionToKeysSamplingComplete;
                    Context.MotionToKeysSampling.OnFailed -= OnMotionToKeysSamplingFailed;

                    // Baking Events
                    Context.OutputTimelineBaking.OnBakingCompleted -= OnOutputTimelineBakingCompleted;
                    Context.OutputTimelineBaking.OnBakingFailed -= OnOutputTimelineBakingFailed;

                    // Selection Events
                    Context.ItemsSelection.OnSelectionChanged -= OnItemsSelectionChanged;
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);

                    if (Context.Model.Step == MotionToTimelineAuthoringModel.AuthoringStep.NoPreview || Context.Model.Step == MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable)
                    {
                        Context.Playback.Update(aDeltaTime);
                    }
                }

                /// <summary>
                /// Update the various baking logics of this state.
                /// </summary>
                /// <param name="delta"></param>
                /// <returns>Returns true if is interrupting further bakes deeper down the states tree.</returns>
                public override bool UpdateBaking(float delta)
                {
                    if (base.UpdateBaking(delta))
                        return true;

                    if (Context.OutputTimelineBaking.NeedToUpdate)
                    {
                        Context.OutputTimelineBaking.Update(delta, false);
                        return true;
                    }

                    if (Context.MotionToKeysSampling.NeedToUpdate)
                    {
                        Context.MotionToKeysSampling.Update(delta, false);
                        return true;
                    }

                    return false;
                }

                void UpdateViews()
                {
                    Context.MotionToTimelineUIModel.TimelineUIModel.IsEditingKey = false;
                    Context.MotionToTimelineUIModel.TimelineUIModel.IsEditingTransition = false;
                    Context.MotionToTimelineUIModel.TimelineUIModel.CurrentFrame = Context.Playback.CurrentFrame;
                    Context.MotionToTimelineUIModel.TimelineUIModel.IsPlaying = Context.Playback.IsPlaying;

                    // Input
                    if (Context.Model.Step != MotionToTimelineAuthoringModel.AuthoringStep.NoPreview)
                    {
                        Context.InputBakedTimelineViewLogic.IsVisible = false;
                    }
                    else
                    {
                        Context.InputBakedTimelineViewLogic.IsVisible = true;
                        Context.InputBakedTimelineViewLogic.DisplayFrame(Context.Playback.CurrentFrame);
                    }

                    // Output
                    if (Context.Model.Step != MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable)
                    {
                        Context.OutputBakedTimelineViewLogic.IsVisible = false;
                    }
                    else
                    {
                        Context.OutputBakedTimelineViewLogic.IsVisible = true;
                        Context.OutputBakedTimelineViewLogic.DisplayFrame(Context.Playback.CurrentFrame);
                    }
                }

                void DoMotionToKeys()
                {
                    Context.Playback.Stop();
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsSamplingMotionToKeys;
                    Context.MotionToTimelineUIModel.IsSamplingMotionToKeys = true;
                    Context.MotionToKeysSampling.QueueBaking(Context.Model.KeyFrameSamplingSensitivity,
                        Context.Model.UseMotionCompletion);
                    UpdateViews();
                }

                void FrameCamera()
                {
                    if (!Context.EntitySelectionModel.HasSelection)
                    {
                        Context.CameraMovement.Frame(Context.InputBakedTimeline.GetWorldBounds());
                        return;
                    }

                    var entityID = Context.EntitySelectionModel.GetSelection(0);
                    var bounds = GetEntityBounds(entityID);
                    for (var i = 1; i < Context.EntitySelectionModel.Count; i++)
                    {
                        entityID = Context.EntitySelectionModel.GetSelection(i);
                        var actorBounds = GetEntityBounds(entityID);
                        bounds.Encapsulate(actorBounds);
                    }

                    Context.CameraMovement.Frame(bounds);
                }

                void OnModelChanged(MotionToTimelineAuthoringModel.Property obj)
                {
                    switch (obj)
                    {
                        case MotionToTimelineAuthoringModel.Property.Target:
                            UpdateUI();
                            break;
                        case MotionToTimelineAuthoringModel.Property.Step:
                            UpdateUI();
                            break;
                        case MotionToTimelineAuthoringModel.Property.FrameDensity:
                            Context.Model.IsPreviewObsolete = true;
                            break;
                        case MotionToTimelineAuthoringModel.Property.UseMotionCompletion:
                            Context.Model.IsPreviewObsolete = true;
                            break;

                        case MotionToTimelineAuthoringModel.Property.IsPreviewObsolete:
                            if (Context.Model.IsPreviewObsolete)
                            {
                                Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsObsolete;
                            }

                            UpdateUI();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
                    }
                }

                // [Section] Playback Requests Handlers

                void OnTimelineRequestedSeekToKey(TimelineModel.SequenceKey key)
                {
                    ResetLoopOffsets();

                    if (Context.OutputBakedTimelineMapping.TryGetBakedKeyIndex(
                            Context.OutputTimeline.IndexOf(key),
                            out var bakedFrameIndex))
                    {
                        Context.Playback.CurrentFrame = bakedFrameIndex;

                        if(Context.Playback.IsPlaying)
                            Context.Playback.Pause();
                    }
                }

                void OnTimelineRequestedSeekToFrame(int frame)
                {
                    Context.Playback.CurrentFrame = frame;
                    ResetLoopOffsets();
                }

                void OnPlaybackSeekedToFrame()
                {
                    ResetLoopOffsets();
                }

                void ResetLoopOffsets()
                {
                    Context.InputBakedTimelineViewLogic.ResetAllLoopOffsets();
                    Context.OutputBakedTimelineViewLogic.ResetAllLoopOffsets();
                }

                void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
                {
                    switch (property)
                    {
                        case PlaybackModel.Property.IsPlaying:
                        case PlaybackModel.Property.IsLooping:
                            ResetLoopOffsets();
                            break;

                        case PlaybackModel.Property.CurrentTime:
                            UpdateViews();
                            break;

                        case PlaybackModel.Property.MinTime:
                        case PlaybackModel.Property.MaxTime:
                        case PlaybackModel.Property.FramesPerSecond:
                        case PlaybackModel.Property.PlaybackSpeed:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(property), property, null);
                    }
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log("OnPointerClick()");

                    Context.EntitySelectionModel.Clear();
                    eventData.Use();
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    switch (eventData.KeyCode)
                    {
                        case KeyCode.F:
                            FrameCamera();
                            eventData.Use();
                            break;
                    }
                }

                void RequestConfirm()
                {
                    if (Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model is not DenseTake take)
                        return;

                    Model.SendAnalytics(new MakeEditableAnalytic(take.Description));

                    // Copy the temporary output towards the working stage
                    Context.OutputTimeline.CopyTo(Context.Stage.WorkingTimeline, true);
                    Context.OutputBakedTimeline.CopyTo(Context.Stage.WorkingBakedTimeline, true);
                    Context.OutputBakedTimelineMapping.CopyTo(Context.Stage.WorkingBakedTimelineMapping, true);

                    // Create an editable take
                    var newTake = new KeySequenceTake(LibraryRegistry.GetNewM2KTakeTitle(), take.Description, Context.OutputTimeline, Context.OutputBakedTimeline, Context.OutputBakedTimelineMapping);
                    var newTakeAsset = Context.ApplicationLibraryModel.AskAddLibraryItem(newTake, Context.Stage);

                    // Show confirmation on UI
                    var toast = Toast.Build(Context.RootUI, "Success! A new Editable Take was created.", NotificationDuration.Short);
                    toast.SetStyle(NotificationStyle.Positive);
                    toast.SetAnimationMode(AnimationMode.Slide);
                    toast.Show();

                    // Update it's UI
                    Context.ApplicationLibraryModel.RequestThumbnailUpdate(newTakeAsset);

                    // Create the Animation Clip Sub-Asset right away
                    Context.ApplicationLibraryModel.RequestExportLibraryItem(newTakeAsset, ApplicationLibraryModel.ExportFlow.SubAsset);

                    // Start editing the editable take
                    Context.ApplicationLibraryModel.AskEditLibraryItem(newTakeAsset);
                }

                /// <summary>
                /// Called when the user presses "preview".
                /// </summary>
                void RequestPreview()
                {
                    DoMotionToKeys();
                }

                /// <summary>
                /// Called when the user wants to exit/cancel converting a motion to keys.
                /// </summary>
                void RequestCancel()
                {
                    // If there is no remaining takes, do nothing
                    if (LibraryRegistry.Items.Count == 0)
                        return;

                    // If there was a selected take, return to viewing the selected take
                    if (Context.ItemsSelection.HasSelection)
                    {
                        Context.ApplicationLibraryModel.RequestEditLibraryItem(Context.ItemsSelection.GetSelection(0));
                        return;
                    }

                    // If there was no selected take, select the first from the takes library
                    Context.ApplicationLibraryModel.RequestEditLibraryItem(LibraryRegistry.ItemsList[0]);
                }

                void OnMotionToKeysSamplingComplete(MotionToKeysSamplingLogic logic)
                {
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsBakingTimelineOutput;
                    Context.OutputTimelineBaking.QueueBaking(false);
                }

                void OnOutputTimelineBakingCompleted(BakingLogic.BakingEventData eventData)
                {
                    Context.Model.IsPreviewObsolete = false;
                    Context.Playback.MaxFrame = Context.OutputBakedTimeline.FramesCount;
                    Context.Playback.Play(true);
                    FrameCamera();
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable;
                    UpdateUI();
                }

                void OnOutputTimelineBakingFailed(BakingLogic.BakingEventData eventData)
                {
                    Context.Model.IsPreviewObsolete = true;
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.NoPreview;
                    Context.MotionToTimelineUIModel.IsBakingOutputTimeline = false;
                }

                void OnMotionToKeysSamplingFailed(MotionToKeysSamplingLogic logic, string error)
                {
                    Context.Model.IsPreviewObsolete = true;
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.NoPreview;
                    Context.MotionToTimelineUIModel.IsSamplingMotionToKeys = false;
                }

                Bounds GetEntityBounds(EntityID entityID)
                {
                    var viewGameObject = Context.InputBakedTimelineViewLogic.GetPreviewGameObject(entityID);
                    var bounds = viewGameObject.GetRenderersWorldBounds();
                    return bounds;
                }

                void CreateTakeThumbnail(KeySequenceTake take)
                {
                    var timeline = take.TimelineModel;
                    if (timeline.KeyCount == 0) return;

                    // var targetKeyIndex = timeline.KeyCount / 2;
                    var targetKey = timeline.GetKey(0);

                    Context.AuthoringModel.RequestGenerateKeyThumbnail(targetKey.Key.Thumbnail, targetKey.Key);
                }

                void OnItemsSelectionChanged(SelectionModel<LibraryItemAsset> model)
                {
                    ActivateSelectedTake();
                }

                void ActivateSelectedTake()
                {
                    ResetTake();

                    if (Context.ApplicationLibraryModel.ActiveLibraryItem == null)
                    {
                        DevLogger.LogError("ActivateSelectedTake() -> Context.ApplicationLibraryModel.ActiveLibraryItem is null");
                        return;
                    }

                    if (Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model == null)
                    {
                        DevLogger.LogError("ActivateSelectedTake() -> Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model is null");
                        return;
                    }

                    var take = Context.ApplicationLibraryModel.ActiveLibraryItem.Data.Model as DenseTake;

                    if (take == null)
                    {
                        DevLogger.LogError("ActivateSelectedTake() -> Could not convert to DenseTake");
                        return;
                    }

                    ActivateTake(take);
                }

                void ActivateTake(DenseTake take)
                {
                    Context.OutputTimeline.Clear();
                    Context.OutputBakedTimeline.Clear();
                    take.BakedTimelineModel.CopyTo(Context.InputBakedTimeline);
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.NoPreview;
                    Context.Playback.MaxFrame = Context.InputBakedTimeline.FramesCount - 1;
                    Context.Playback.Play(true);

                    if (take.BakedTimelineModel.FramesCount > 0)
                    {
                        FrameCamera();
                    }

                    UpdateUI();
                    UpdateViews();
                }

                void ResetTake()
                {
                    // Set the preview to be obsolete, shows the convert button as enabled
                    Context.Model.IsPreviewObsolete = true;

                    // Reset the Views
                    Context.InputBakedTimelineViewLogic.IsVisible = false;
                    Context.InputBakedTimelineViewLogic.ResetAllLoopOffsets();
                    Context.OutputBakedTimelineViewLogic.IsVisible = false;
                    Context.OutputBakedTimelineViewLogic.ResetAllLoopOffsets();

                    // Set the authoring step
                    Context.Model.Step = MotionToTimelineAuthoringModel.AuthoringStep.None;
                }

                void UpdateUI()
                {
                    Context.MotionToTimelineUIModel.IsSamplingMotionToKeys = Context.MotionToKeysSampling.IsRunning;
                    Context.MotionToTimelineUIModel.IsBakingOutputTimeline = Context.OutputTimelineBaking.IsRunning;
                    Context.MotionToTimelineUIModel.Refresh();
                }

                void OnTimelineKeyChanged(TimelineModel model, KeyModel key, KeyModel.Property property)
                {
                    if (property is not (KeyModel.Property.EntityKey or KeyModel.Property.EntityList))
                        return;

                    RequestKeyThumbnail(key.Thumbnail, key);
                }

                void OnTimelineKeyRemoved(TimelineModel model, TimelineModel.SequenceKey key)
                {
                    Context.KeySelectionModel.Unselect(key);
                    Context.ThumbnailsService.CancelRequestOf(key.Key.Thumbnail);
                }

                void OnTimelineKeyAdded(TimelineModel model, TimelineModel.SequenceKey key)
                {
                    RequestKeyThumbnail(key.Thumbnail, key.Key);
                }

                void RequestKeyThumbnail(ThumbnailModel target, KeyModel key)
                {
                    Context.ThumbnailsService.RequestThumbnail(target, key, Context.Camera.Position, Context.Camera.Rotation);
                }
            }
        }
    }
}
