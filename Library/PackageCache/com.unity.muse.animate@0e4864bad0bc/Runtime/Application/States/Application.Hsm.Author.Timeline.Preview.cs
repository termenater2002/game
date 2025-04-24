using System;
using Hsm;
using UnityEngine;
using UnityEngine.EventSystems;
using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// The user previews the output of a <see cref="KeySequenceTake"/>.
            /// </summary>
            public class AuthorTimelinePreview : ApplicationState<AuthorTimelinePreviewContext>, IKeyDownHandler, IPointerClickHandler
            {
                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    // Clear entity selection
                    Context.EntitySelection.Clear();
                    Context.AuthoringModel.Title = $"{Context.AuthoringModel.TargetName}";
                    
                    RegisterEvents();
                    RefreshOutputVisibility();
                    DisplayCurrentFrame();
                }

                public override void OnExit()
                {
                    base.OnExit();
                    
                    // Hide the baking notice
                    Context.BakingNoticeUI.Hide();
                    
                    if (Context.Playback.IsPlaying)
                        Context.Playback.Pause();
                    
                    Context.ViewLogic.IsVisible = false;
                    UnregisterEvents();
                }

                void RegisterEvents()
                {
                    Context.TimelineBaking.OnBakingCompleted += OnBakingCompleted;
                    
                    Context.Playback.OnLooped += OnPlaybackLooped;
                    Context.Playback.OnChanged += OnPlaybackChanged;
                }

                void UnregisterEvents()
                {
                    Context.TimelineBaking.OnBakingCompleted -= OnBakingCompleted;
                    Context.Playback.OnLooped -= OnPlaybackLooped;
                    Context.Playback.OnChanged -= OnPlaybackChanged;
                }

                void OnBakingCompleted(BakingLogic.BakingEventData eventData)
                {
                    RefreshOutputVisibility();
                }

                void RefreshOutputVisibility()
                {
                    var isPlaybackAvailable = Context.BakedTimeline.FramesCount > 0 && !Context.TimelineBaking.IsRunning && !Context.TimelineBaking.NeedToUpdate;
                    
                    if (isPlaybackAvailable)
                    {
                        // Show and play the output playback if it is not visible
                        if (!Context.ViewLogic.IsVisible)
                        {
                            /*if (!Context.Playback.IsPlaying)
                                Context.Playback.Play();
                            */
                            Context.ViewLogic.IsVisible = true;
                            Context.ViewLogic.ResetAllLoopOffsets();
                            
                            // Hide the baking notice
                            Context.BakingNoticeUI.Hide();
                        }
                        return;
                    }
                    
                    // In case animation is still baking:
                    // --------------------------------------
                    
                    // Show a baking notice
                    Context.BakingNoticeUI.Show("Solving Animation...", "clap-board");
                    
                    // Stop the playback
                    if (Context.Playback.IsPlaying)
                        Context.Playback.Stop();
                    
                    // Hide the output playback
                    Context.ViewLogic.IsVisible = false;
                }
                
                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    Context.Playback.Update(aDeltaTime);
                }

                void DisplayCurrentFrame()
                {
                    var currentFrame = Context.Playback.CurrentFrame;
                    Context.ViewLogic.DisplayFrame(currentFrame);
                }

                void OnSeekedToFrame()
                {
                    Context.ViewLogic.ResetAllLoopOffsets();
                }

                void OnPlaybackChanged(PlaybackModel model, PlaybackModel.Property property)
                {
                    switch (property)
                    {
                        case PlaybackModel.Property.IsPlaying:
                        case PlaybackModel.Property.IsLooping:
                            Context.ViewLogic.ResetAllLoopOffsets();
                            break;

                        case PlaybackModel.Property.CurrentTime:
                            Context.ViewLogic.DisplayFrame(model.CurrentFrame);
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

                void OnPlaybackLooped(PlaybackModel model)
                {
                    if (Context.BakedTimeline.FramesCount == 0)
                        return;

                    if (!Context.BakedTimelineMapping.TryGetKey(Context.Timeline, Context.BakedTimeline.FramesCount - 1, out var lastFrameKey))
                        return;

                    if (lastFrameKey.Key.Type != KeyData.KeyType.Loop)
                        return;

                    var loop = lastFrameKey.Key.Loop;
                    if (loop.StartFrame != 0)
                        return;

                    for (var i = 0; i < Context.Stage.NumActors; i++)
                    {
                        var actorID = Context.Stage.GetActorID(i);

                        if (!loop.TryGetOffset(actorID.EntityID, out var loopOffset))
                            continue;

                        Context.ViewLogic.AddLoopOffset(actorID.EntityID, loopOffset.Position, loopOffset.Rotation);
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

                    // TODO: Add preview-specific keybindings here
                }
            }
        }
    }
}
