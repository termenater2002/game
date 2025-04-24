using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    class TutorialLogic
    {
        // Events meant to be used by both View Models and Authoring States
        public delegate void StepAction(in TutorialTrackData trackData, in TutorialTrackStepData stepData, ActionType actionType);

        public event StepAction OnStepAction;

        public delegate void StepOpened(in TutorialTrackData trackData, in TutorialTrackStepData stepData);

        public event StepOpened OnStepOpened;

        public delegate void TrackOpened(in TutorialTrackData trackData);

        public event TrackOpened OnTrackOpened;

        public enum ActionType
        {
            Accept,
            Next,
            Previous,
            Dismiss,
            Skip
        }

        TutorialData m_Data;

        readonly TutorialTrackStepViewModel m_TrackStepViewModel;
        readonly VisualElement m_RootVisualElement;
        readonly Mask m_TutorialMask;

        TutorialTrackStepView m_TrackStepView;
        Popover m_Popover;
        Modal m_Modal;

        public string CurrentTrackId => m_Data.CurrentTrackId;
        public string CurrentStepId => m_Data.CurrentStepId;
        public TutorialTrackData CurrentTrack => m_Data.Tracks[CurrentTrackId];
        public TutorialTrackStepData CurrentStep => CurrentTrack.Steps[CurrentStepId];

        public TutorialLogic(VisualElement rootElement)
        {
            m_RootVisualElement = rootElement;

            m_TutorialMask = m_RootVisualElement.Q<Mask>("tutorial-mask");

            m_TrackStepViewModel = new TutorialTrackStepViewModel(this);
            m_TrackStepViewModel.OnRequestedAction += OnRequestedAction;

            m_Data = new TutorialData();
        }

        public void OpenStep(string trackId, string stepId)
        {
            if (!TryGetTrack(trackId, out var trackData))
                throw new Exception($"Could not open step ({stepId}), track ({trackId}) does not exist.");

            if (!trackData.TryGetStep(stepId, out var stepData))
                throw new Exception($"Could not open step ({stepId}), it does not exist in specified track ({trackId}).");

            m_Data.CurrentTrackId = trackData.TrackId;
            m_Data.CurrentStepId = stepData.StepId;

            OpenUI(trackData, stepData);

            OnTrackOpened?.Invoke(trackData);
            OnStepOpened?.Invoke(trackData, stepData);
        }

        public void AddTrack(string trackId, string title, string label)
        {
            m_Data.AddTrack(trackId, title, label);
        }

        public void AddStep(string trackId, in TutorialTrackStepData stepData)
        {
            m_Data.AddStep(trackId, stepData);
        }

        public void SkipLastTrack()
        {
            // Basically is the same as a dismiss action
            InvokeStepActionEvent(ActionType.Dismiss);
        }

        public void DismissStep()
        {
            Dismiss();
        }

        // TODO: (@james.mccafferty) find a way to drive the animation without a Mono behavior, possibly using UI Toolkit.
        // The RunShrinkAnimation and ShrinkAnimation classes are badly organized. They were written quickly and
        // will likely need to be refactored. They are also not very well documented. Sorry about that.
        public void RunShrinkAnimation()
        {
            VisualElement infoButton = m_RootVisualElement.Q<ActionButton>("tutorial");
            
            var go = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("ModalShrinkAnimation");
            var shrinkAnimation = go.AddComponent<ShrinkAnimation>();
            shrinkAnimation.opacityTargets = new[] {m_TrackStepView.parent, m_TrackStepView.parent.parent};
            shrinkAnimation.target = m_TrackStepView.parent;
            shrinkAnimation.shrinkTime = 0.5f;
            shrinkAnimation.Logic = this;
            shrinkAnimation.lerpTo = infoButton;
        }

        [ExecuteAlways]
        public class ShrinkAnimation : MonoBehaviour
        {
            public VisualElement[] opacityTargets;
            public VisualElement target;
            public VisualElement lerpTo;
            public float shrinkTime = 1f;
            public TutorialLogic Logic;

            public void Start()
            {
                if (Locator.TryGet<ICoroutineRunner>(out var runner))
                {
                    runner.StartCoroutine(ShrinkCoroutine());
                }
                else
                {
                    StartCoroutine(ShrinkCoroutine());
                }
            }

            IEnumerator ShrinkCoroutine()
            {
                float time = shrinkTime;
                
                // Note: Tutorial animation doesn't seem to work in editor,
                // also, some of the UI changed, so the targets are sometime null
                if (lerpTo == null)
                {
                    GameObjectUtils.Destroy(gameObject);
                    Logic.DismissStep();
                    //target.style.display = DisplayStyle.None;
                }
                else
                {
                    Vector2 transformPosition = lerpTo.worldBound.center;
                    Vector2 targetPosition = target.worldBound.center;
                    Vector2 delta = transformPosition - targetPosition;

                    while (time > 0f)
                    {
                        time -= Time.deltaTime;
                        time = Mathf.Max(time, 0f);

                        target.style.scale = new StyleScale(Vector2.Lerp(Vector3.zero, Vector3.one, time / shrinkTime));

                        foreach (VisualElement opacityTarget in opacityTargets)
                            opacityTarget.style.opacity = time / shrinkTime;

                        Vector2 currentDelta = delta * (1f - time / shrinkTime);
                        target.style.translate =
                            new StyleTranslate(new Translate(currentDelta.x, currentDelta.y, 0));

                        yield return null;
                    }

                    Logic.RunTempTutorialReminder();
                    Logic.DismissStep();

                    target.style.display = DisplayStyle.None;

                    // WaitForEndOfFrame is not supported in Edit Mode when using Editor Coroutines
                    yield return UnityEngine.Application.isPlaying ? new WaitForEndOfFrame() : null;

                    target.style.scale = new StyleScale(Vector3.one);
                    target.style.opacity = 1;

                    foreach (VisualElement opacityTarget in opacityTargets)
                        opacityTarget.style.opacity = 1;

                    GameObjectUtils.Destroy(gameObject);
                }
            }
        }

        public void RunTempTutorialReminder()
        {
            VisualElement reminder = m_RootVisualElement.Q("temp-tutorial-callout");

            var go = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("TempTutorialReminder");
            TempTutorialReminder shrinkAnimation = go.AddComponent<TempTutorialReminder>();
            shrinkAnimation.reminder = reminder;
            shrinkAnimation.reminderTime = 4f;
        }

        [ExecuteAlways]
        public class TempTutorialReminder : MonoBehaviour
        {
            public VisualElement reminder;
            [FormerlySerializedAs("shrinkTime")] public float reminderTime = 1f;

            public void Start()
            {
                if (reminder == null)
                    return;

                if (Locator.TryGet<ICoroutineRunner>(out var runner))
                {
                    runner.StartCoroutine(ReminderCoroutine());
                }
                else
                {
                    StartCoroutine(ReminderCoroutine());
                }
            }

            IEnumerator ReminderCoroutine()
            {

                float time = reminderTime;

                reminder.style.display = DisplayStyle.Flex;

                while (time > 0f)
                {
                    time -= Time.deltaTime;
                    yield return null;
                }

                reminder.style.display = DisplayStyle.None;

                GameObjectUtils.Destroy(gameObject);
            }
        }

        void OpenUI(in TutorialTrackData trackData, in TutorialTrackStepData stepData)
        {
            // Close previous popover / modal
            CloseUI(DismissType.Consecutive);

            m_TrackStepView = new TutorialTrackStepView();
            m_TrackStepView.SetModel(m_TrackStepViewModel);

            // Open new popover / modal
            if (string.IsNullOrEmpty(stepData.TargetUI))
            {
                m_Modal = Modal.Build(m_RootVisualElement, m_TrackStepView);
                m_Modal.Show();
            }
            else
            {
                var referenceView = m_RootVisualElement.Q<VisualElement>(stepData.TargetUI);

                if (referenceView == null)
                    throw new Exception($"Could not locate targetUI VisualElement: {stepData.TargetUI}");

                m_Popover = Popover.Build(referenceView, m_TrackStepView)
                    .SetPlacement(stepData.Placement)
                    .SetOutsideClickDismiss(false)
                    .SetModalBackdrop(!stepData.UseMask)
                    .SetArrowVisible(!stepData.HidePopoverArrow);

                m_Popover.Show();
            }

            UpdateTutorialMask(stepData);
            UpdateView(trackData, stepData);
        }

        void UpdateTutorialMask(in TutorialTrackStepData stepData)
        {
            if (!stepData.UseMask)
            {
                m_TutorialMask.style.display = DisplayStyle.None;
                return;
            }

            m_TutorialMask.blur = 1;
            m_TutorialMask.outerMaskColor = new Color(0f, 0f, 0f, 0.6f);
            m_TutorialMask.innerMaskColor = new Color(0f, 0f, 0f, 0f);
            m_TutorialMask.radius = stepData.MaskRadius;

            // Hide the backdrop
            m_Popover?.SetModalBackdrop(false);

            if (stepData.MaskIsTargetingUI)
            {
                m_TutorialMask.useNormalizedMaskRect = false;

                var referenceView = m_RootVisualElement.Q<VisualElement>(stepData.TargetUI);
                if (referenceView == null)
                    throw new Exception($"Could not locate targetUI VisualElement: {stepData.TargetUI}");

                var padding = 4;
                var maskRect = referenceView.worldBound;

                maskRect.xMin -= padding;
                maskRect.yMin -= padding;
                maskRect.xMax += padding;
                maskRect.yMax += padding;

                m_TutorialMask.radius = 4;
                m_TutorialMask.maskRect = maskRect;
            }
            else
            {
                m_TutorialMask.maskRect = stepData.MaskRect;
                m_TutorialMask.useNormalizedMaskRect = stepData.UseNormalizedMaskRect;
            }

            m_TutorialMask.style.display = DisplayStyle.Flex;
        }

        void UpdateView(in TutorialTrackData trackData, in TutorialTrackStepData stepData)
        {
            // Configure the view
            m_TrackStepViewModel.IsVisible = true;
            m_TrackStepViewModel.Title = stepData.Title;
            m_TrackStepViewModel.Body = stepData.Body;
            m_TrackStepViewModel.Footer = trackData.NbSteps > 1 ? $"Step {stepData.Index + 1} of {trackData.NbSteps}" : "";
            m_TrackStepViewModel.CanAccept = stepData.ShowAcceptButton;
            m_TrackStepViewModel.CanNext = stepData.Index < trackData.NbSteps - 1;
            m_TrackStepViewModel.CanPrevious = stepData.Index > 0 && trackData.NbSteps > 0 && stepData.Index < trackData.NbSteps;
            m_TrackStepViewModel.CanDismiss = stepData.ShowDismissButton;
            m_TrackStepViewModel.CanSkip = stepData.ShowSkipButton;

            m_TrackStepViewModel.BackgroundImage = stepData.BackgroundImage;

            if (!string.IsNullOrEmpty(stepData.AcceptButtonLabel))
                m_TrackStepViewModel.SetActionButtonName(ActionType.Accept, stepData.AcceptButtonLabel);
            if (!string.IsNullOrEmpty(stepData.NextButtonLabel))
                m_TrackStepViewModel.SetActionButtonName(ActionType.Next, stepData.NextButtonLabel);
            if (!string.IsNullOrEmpty(stepData.PreviousButtonLabel))
                m_TrackStepViewModel.SetActionButtonName(ActionType.Previous, stepData.PreviousButtonLabel);
            if (!string.IsNullOrEmpty(stepData.DismissButtonLabel))
                m_TrackStepViewModel.SetActionButtonName(ActionType.Dismiss, stepData.DismissButtonLabel);
            if (!string.IsNullOrEmpty(stepData.SkipButtonLabel))
                m_TrackStepViewModel.SetActionButtonName(ActionType.Skip, stepData.SkipButtonLabel);
        }

        void CloseUI(DismissType dismissType)
        {
            m_TrackStepViewModel.IsVisible = false;
            m_Popover?.Dismiss(dismissType);
            m_Popover = null;
            m_Modal?.Dismiss(dismissType);
            m_Modal = null;
            m_TutorialMask.style.display = DisplayStyle.None;
        }

        void OnRequestedAction(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Accept:
                    Accept();
                    break;
                case ActionType.Next:
                    NextStep();
                    break;
                case ActionType.Previous:
                    PreviousStep();
                    break;
                case ActionType.Dismiss:
                    RunShrinkAnimation();
                    break;
                case ActionType.Skip:
                    Skip();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }

        void InvokeStepActionEvent(ActionType actionType)
        {
            OnStepAction?.Invoke(CurrentTrack, CurrentStep, actionType);
        }

        void Accept()
        {
            // Perform base logic here

            // Afterwards, emit the event
            InvokeStepActionEvent(ActionType.Accept);
        }

        void Dismiss()
        {
            // Perform base logic here
            CloseUI(DismissType.Action);

            // Afterwards, emit the event
            InvokeStepActionEvent(ActionType.Dismiss);
        }

        void Skip()
        {
            // Perform base logic here
            CloseUI(DismissType.Action);

            // Afterwards, emit the event
            InvokeStepActionEvent(ActionType.Skip);
        }

        void NextStep()
        {
            // Perform base logic here
            if (!m_Data.Tracks.TryGetValue(CurrentTrackId, out var track))
                throw new Exception($"Could not go to next track step, current track ({CurrentTrackId}) does not exist.");

            if (!track.Steps.TryGetValue(CurrentStepId, out var stepCurrent))
                throw new Exception($"Could not go to next track step, current step ({CurrentStepId}) does not exist in current track ({CurrentTrackId}).");

            OpenStepIndex(stepCurrent.Index + 1);

            // Afterwards, emit the event
            InvokeStepActionEvent(ActionType.Next);
        }

        void PreviousStep()
        {
            // Perform base logic here
            if (!m_Data.Tracks.TryGetValue(CurrentTrackId, out var track))
                throw new Exception($"Could not go to next track step, current track ({CurrentTrackId}) does not exist.");

            if (!track.Steps.TryGetValue(CurrentStepId, out var stepCurrent))
                throw new Exception($"Could not go to next track step, current step ({CurrentStepId}) does not exist in current track ({CurrentTrackId}).");

            OpenStepIndex(stepCurrent.Index - 1);

            // Afterwards, emit the event
            InvokeStepActionEvent(ActionType.Previous);
        }

        void OpenStepIndex(int stepIndex)
        {
            if (!m_Data.Tracks.TryGetValue(CurrentTrackId, out var track))
                throw new Exception($"Could not go to track step index ({stepIndex}) because current track ({CurrentTrackId}) does not exist.");

            var keys = track.Steps.Keys.ToArray();

            if (stepIndex >= keys.Length || stepIndex < 0)
                throw new Exception($"Could not recover step, {stepIndex} is out of range.");

            OpenStep(CurrentTrackId, keys[stepIndex]);
        }

        bool TryGetTrack(string trackId, out TutorialTrackData track)
        {
            return m_Data.Tracks.TryGetValue(trackId, out track);
        }
    }
}
