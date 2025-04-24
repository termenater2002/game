using System;
using Hsm;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// The user authors a <see cref="VideoToMotionTake"/>.
            /// </summary>
            public class AuthorVideoToMotion : ApplicationState<AuthorVideoToMotionTakeContext>,
                IKeyDownHandler,
                IPointerClickHandler
            {
                VideoToMotionTake m_ActiveTake;

                public override void OnEnter(object[] aArgs)
                {
                    base.OnEnter(aArgs);
                    Context.EntitySelection.Clear();

                    Context.AuthoringModel.Title = "Video to Motion";
                    Context.OutputBakedTimelineViewLogic.IsVisible = true;
                    Context.OutputBakedTimelineViewLogic.ResetAllLoopOffsets();

                    Context.UIModel.IsVisible = true;

                    // Start stopped, with looping on
                    Context.Playback.IsLooping = true;

                    if (Context.Playback.IsPlaying)
                        Context.Playback.Stop();

                    // Register Events
                    RegisterEvents();

                    DevLogger.LogInfo("AuthorVideoToMotion entered.");
                    EditTake(Context.Model.PlaybackTarget);
                }

                public override void OnExit()
                {
                    base.OnExit();

                    ClearTake();

                    Context.OutputBakedTimelineViewLogic.IsVisible = false;
                    Context.UIModel.IsVisible = false;

                    UnregisterEvents();
                }

                void RegisterEvents()
                {
                    Context.Model.OnRequestExtractKeys += OnRequestedExtractKeys;
                    Context.Model.OnRequestExport += OnRequestedExport;
                    Context.Model.OnRequestDelete += OnRequestedDelete;
                    Context.Model.OnChanged += OnModelChanged;
                    Context.OutputBakedTimeline.OnChanged += OnOutputChanged;
                    Context.Playback.OnChanged += OnPlaybackChanged;
                    Context.BakedPlaybackUIModel.OnSeekedToFrame += OnPlaybackSeekedToFrame;
                }

                void UnregisterEvents()
                {
                    Context.Model.OnRequestExtractKeys -= OnRequestedExtractKeys;
                    Context.Model.OnRequestExport -= OnRequestedExport;
                    Context.Model.OnRequestDelete -= OnRequestedDelete;
                    Context.Model.OnChanged -= OnModelChanged;
                    Context.OutputBakedTimeline.OnChanged -= OnOutputChanged;
                    Context.Playback.OnChanged -= OnPlaybackChanged;
                    Context.BakedPlaybackUIModel.OnSeekedToFrame -= OnPlaybackSeekedToFrame;
                }

                public override void Update(float deltaTime)
                {
                    base.Update(deltaTime);
                    // Playback time is centrally maintained by the authoring model to ensure that that the
                    // pose is in sync with the video.
                    if (Context.Playback.IsPlaying)
                        Context.Playback.CurrentTime = Context.Model.DisplayTime;
                }

                void OnModelChanged(VideoToMotionAuthoringModel.Property property)
                {
                    if (property == VideoToMotionAuthoringModel.Property.Target)
                    {
                        EditTake(Context.Model.PlaybackTarget);
                    }
                }

                void OnRequestedExtractKeys(VideoToMotionTake take)
                {
                    Context.ApplicationModel.RequestConvertMotionToKeys(Context.Model.PlaybackTarget);
                }

                void OnRequestedExport(VideoToMotionTake take)
                {
                    Context.ApplicationLibraryModel.RequestExportLibraryItemModel(take);
                }
                
                void OnRequestedDelete(VideoToMotionTake take) { }

                void OnOutputChanged(BakedTimelineModel model)
                {
                    UpdateViews();
                }

                void OnPlaybackSeekedToFrame()
                {
                    Context.OutputBakedTimelineViewLogic.ResetAllLoopOffsets();
                }

                void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
                {
                    switch (property)
                    {
                        case PlaybackModel.Property.IsPlaying:
                        case PlaybackModel.Property.IsLooping:
                            Context.OutputBakedTimelineViewLogic.ResetAllLoopOffsets();
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
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnPointerClick({eventData.button})");

                    Context.EntitySelection.Clear();
                    eventData.Use();
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnKeyDown({eventData.KeyCode})");

                    if (Context.TakesUI.IsWriting)
                    {
                        eventData.Use();
                        return;
                    }

                    switch (eventData.KeyCode)
                    {
                        case KeyCode.F:
                            FrameCamera();
                            eventData.Use();
                            break;
                    }
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                void EditTake(VideoToMotionTake take)
                {
                    ClearTake();
                    m_ActiveTake = take;
                    if (m_ActiveTake != null)
                    {
                        m_ActiveTake.OnBakingComplete += OnTakeBakingComplete;
                        m_ActiveTake.OnBakingFailed += OnTakeBakingFailed;

                        // Only track the baking logic if it is currently baking this specific take
                        if (m_ActiveTake.IsBaking)
                        {
                            // Previously, we start tracking the baking logic here to display in the UI.
                            // However, this logic didn't work as intended since we can't track individual generations.
                            // Also, we need to be able to display errors even when a take is not selected. Therefore,
                            // we hook up the tracking instead in Hsm.Author when the context is created.

                            // Show a notice
                            Context.BakingNoticeUI.Show("Generating Animation...", "clap-board");
                        }

                        m_ActiveTake.BakedTimelineModel.CopyTo(Context.OutputBakedTimeline);

                        // Adjust the playback to match the new output
                        if (Context.OutputBakedTimeline.FramesCount > 0)
                        {
                            Context.Playback.MaxFrame = Context.OutputBakedTimeline.FramesCount - 1;

                            // Play the output
                            Context.Playback.Play(true);

                            FrameCamera();
                        }
                        else
                        {
                            // Stop the output playback
                            Context.Playback.MaxFrame = 0;
                            Context.Playback.Stop();
                        }
                    }
                    else
                    {
                        Context.Playback.MaxFrame = 0;
                        Context.OutputBakedTimeline.Clear();
                        Context.Playback.Stop();
                    }

                    UpdateViews();
                    UpdateUI();
                }

                void ClearTake()
                {
                    if (m_ActiveTake == null) return;

                    m_ActiveTake.OnBakingComplete -= OnTakeBakingComplete;
                    m_ActiveTake.OnBakingFailed -= OnTakeBakingFailed;

                    // Hide the notice
                    Context.BakingNoticeUI.Hide();

                    Context.Playback.MaxFrame = 0;

                    if (Context.Playback.IsPlaying)
                        Context.Playback.Stop();

                    Context.OutputBakedTimeline.Clear();
                    m_ActiveTake = null;
                }

                void FrameCamera()
                {
                    if (!Context.EntitySelection.HasSelection)
                    {
                        Context.CameraMovement.Frame(Context.OutputBakedTimeline.GetWorldBounds());
                        return;
                    }

                    Context.CameraMovement.Frame(GetSelectedEntitiesBounds());
                }

                Bounds GetEntityBounds(EntityID entityID)
                {
                    var viewGameObject = Context.OutputBakedTimelineViewLogic.GetPreviewGameObject(entityID);
                    var bounds = viewGameObject.GetRenderersWorldBounds();
                    return bounds;
                }

                Bounds GetSelectedEntitiesBounds()
                {
                    var entityID = Context.EntitySelection.GetSelection(0);
                    var bounds = GetEntityBounds(entityID);

                    for (var i = 1; i < Context.EntitySelection.Count; i++)
                    {
                        entityID = Context.EntitySelection.GetSelection(i);
                        var actorBounds = GetEntityBounds(entityID);
                        bounds.Encapsulate(actorBounds);
                    }

                    return bounds;
                }

                void OnTakeBakingComplete()
                {
                    // Copy the baked take onto the output
                    Context.Model.PlaybackTarget.BakedTimelineModel.CopyTo(Context.OutputBakedTimeline);

                    // Adjust to playback to the length of the output and play
                    Context.Playback.MaxFrame = Context.OutputBakedTimeline.FramesCount - 1;
                    Context.Playback.Play(true);

                    // Hide the baking notice
                    Context.BakingNoticeUI.Hide();

                    // Update the output views and the UI state
                    UpdateViews();
                    UpdateUI();

                    // Automatically Frame the camera
                    FrameCamera();
                }

                void OnTakeBakingFailed()
                {
                    // Hide the baking notice
                    Context.BakingNoticeUI.Hide();

                    // Remove the take from the library
                    ClearTake();

                    // Update the output views and the UI state
                    UpdateViews();
                    UpdateUI();

                    Context.ApplicationLibraryModel.RequestDeleteLibraryItemModel(m_ActiveTake);
                }

                void UpdateViews()
                {
                    if (Context.OutputBakedTimeline.FramesCount <= 0 || Context.Playback.MaxFrame <= Context.Playback.MinFrame)
                    {
                        Context.OutputBakedTimelineViewLogic.IsVisible = false;
                    }
                    else
                    {
                        Context.OutputBakedTimelineViewLogic.IsVisible = true;
                        Context.OutputBakedTimelineViewLogic.DisplayFrame(Context.Playback.CurrentFrame);
                    }
                }

                void UpdateUI()
                {
                    Context.UIModel.IsBakingCurrentTake = m_ActiveTake?.IsBaking ?? false;
                    Context.UIModel.IsBusy = m_ActiveTake?.IsBaking ?? false;

                    if (Context.OutputBakedTimeline.FramesCount <= 0 || Context.Playback.MaxFrame <= Context.Playback.MinFrame)
                    {
                        Context.UIModel.CanMakeEditable = false;
                        Context.UIModel.CanExport = false;
                    }
                    else
                    {
                        Context.UIModel.CanMakeEditable = true;
                        Context.UIModel.CanExport = true;
                    }
                }
            }
        }
    }
}
