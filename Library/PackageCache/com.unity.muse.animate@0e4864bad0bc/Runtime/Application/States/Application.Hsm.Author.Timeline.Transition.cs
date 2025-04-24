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
            /// The user authors a <see cref="KeySequenceTake"/>'s <see cref="TimelineModel.SequenceTransition"/>.
            /// </summary>
            public class AuthorTimelineTransition : ApplicationState<AuthorTimelineTransitionContext>, IKeyDownHandler, IPointerClickHandler
            {
                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    Context.AuthoringModel.Title = "Editing Transition";
                    Context.BakedTimelineViewLogic.IsVisible = true;
                    Context.BakedTimelineViewLogic.ResetAllLoopOffsets();

                    DisplayCurrentFrame();
                }

                public override void OnExit()
                {
                    base.OnExit();

                    if (Context.Playback.IsPlaying)
                        Context.Playback.Pause();
                    Context.BakedTimelineViewLogic.IsVisible = false;
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    Context.Playback.Update(aDeltaTime);
                }

                void DisplayCurrentFrame()
                {
                    var currentFrame = Context.Playback.CurrentFrame;
                    Context.BakedTimelineViewLogic.DisplayFrame(currentFrame);
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    Context.EntitySelection.Clear();
                    eventData.Use();
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    // TODO: Add transition-specific keybindings here
                }
            }
        }
    }
}
