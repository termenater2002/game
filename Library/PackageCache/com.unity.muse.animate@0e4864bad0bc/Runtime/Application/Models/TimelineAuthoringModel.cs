using System;
using Unity.DeepPose.Core;
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the Timeline Authoring Model of the Application.
    /// </summary>
    /// <remarks>
    /// Handles requests for actions related to Authoring a <see cref="KeySequenceTake"/>.
    /// </remarks>
    class TimelineAuthoringModel
    {
        // Events meant to be used by both UI Models and Author.Timeline States
        public event Action OnModeChanged;

        // Events meant to be used by the UI Models
        public event Action OnChanged;

        // Events meant to be used by Author.Timeline States
        // - Timeline - Preview
        public event Action OnRequestedPreview;

        // - Timeline - Playback
        public event Action<float> OnRequestedSeekToFrame;

        // - Timeline - Keys
        public event Action OnRequestedAddKey;
        public event Action<int> OnRequestedSelectKeyIndex;
        public event Action<TimelineModel.SequenceKey> OnRequestedSelectKey;
        public event Action<TimelineModel.SequenceKey> OnRequestedSeekToKey;
        public event Action<int> OnRequestedEditKeyIndex;
        public event Action<TimelineModel.SequenceKey> OnRequestedEditKey;
        public event Action<TimelineModel.SequenceKey> OnRequestedDeleteKey;
        public event Action<TimelineModel.SequenceKey> OnRequestedPreviewKey;
        public event Action OnRequestedDeleteSelectedKeys;
        public event Action OnRequestedCopyPose;
        public event Action OnRequestedPastePose;
        public event Action OnRequestedResetPose;

        public delegate void RequestedInsertKeyWithEffectorRecovery(int bakedFrameIndex, int keyIndex, float progress, out TimelineModel.SequenceKey key);

        public event RequestedInsertKeyWithEffectorRecovery OnRequestedInsertKeyWithEffectorRecovery;

        public delegate void RequestedInsertKey(int keyIndex, out TimelineModel.SequenceKey key);

        public event RequestedInsertKey OnRequestedInsertKey;

        public delegate void RequestedInsertKeyCopy(out TimelineModel.SequenceKey insertedKey, int atIndex, KeyModel keyToPaste, TransitionModel keyOutTransitionToPaste = null, bool splitTransition = false, float splitTimePercent = 0f);

        public event RequestedInsertKeyCopy OnRequestedInsertKeyCopy;

        public delegate void RequestedMoveKey(int fromIndex, int toIndex);

        public event RequestedMoveKey OnRequestedMoveKey;

        public delegate TimelineModel.SequenceKey RequestedDuplicateKey(int fromIndex, int toIndex);

        public event RequestedDuplicateKey OnRequestedDuplicateKey;

        // - Timeline - Transitions
        public event Action<TimelineModel.SequenceTransition> OnRequestedSelectTransition;
        public event Action<TimelineModel.SequenceTransition> OnRequestedSeekToTransition;
        public event Action<TimelineModel.SequenceTransition> OnRequestedEditTransition;
        public event Action<TimelineModel.SequenceTransition> OnRequestedPreviewTransition;

        public event Action OnRequestSaveWorkingStage;

        // Timeline - Entities
        public event Action OnRequestedDeleteSelectedEntities;

        // - Posing interactions
        public event Action OnRequestedDisableSelectedEffectors;

        /// <summary>
        /// Authoring modes
        /// </summary>
        public enum AuthoringMode
        {
            /// <summary>
            /// No authoring mode set
            /// </summary>
            Unknown,

            /// <summary>
            /// Previewing final animation
            /// </summary>
            Preview,

            /// <summary>
            /// Authoring a key
            /// </summary>
            EditKey,

            /// <summary>
            /// Authoring a transition
            /// </summary>
            EditTransition
        }

        public enum SelectionType
        {
            Entity,
            Effector,
            SequenceKey,
            SequenceTransition
        }

        public SelectionType LastSelectionType { get; set; }

        public AuthoringMode Mode
        {
            get => m_Mode;
            set
            {
                if (m_Mode == value)
                    return;

                m_Mode = value;
                OnModeChanged?.Invoke();
                OnChanged?.Invoke();
            }
        }

        public bool CanCopyPose
        {
            get => m_CanCopyPose;
            set
            {
                if (m_CanCopyPose == value)
                    return;

                m_CanCopyPose = value;
                OnChanged?.Invoke();
            }
        }

        public bool CanDeleteSelectedEntities
        {
            get => m_CanDeleteSelectedEntities;
            set
            {
                if (m_CanDeleteSelectedEntities == value)
                    return;

                m_CanDeleteSelectedEntities = value;
                OnChanged?.Invoke();
            }
        }

        public bool CanDisableSelectedEffectors
        {
            get => m_CanDisableSelectedEffectors;
            set
            {
                if (m_CanDisableSelectedEffectors == value)
                    return;

                m_CanDisableSelectedEffectors = value;
                OnChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets whether the timeline has been modified since the last save.
        /// </summary>
        public bool IsDirty { get; internal set; }

        /// <summary>
        /// Sequence Key currently being edited.
        /// </summary>
        public TimelineModel.SequenceKey ActiveKey { get; set; }

        ///<summary>
        /// Sequence Transition currently being edited.
        /// </summary>
        public TimelineModel.SequenceTransition ActiveTransition { get; set; }

        AuthoringModel Authoring { get; }
        TimelineModel Timeline => Context.Stage.WorkingTimeline;
        AuthorTimelineContext Context => Authoring.Context.TimelineContext;
        AuthoringMode m_Mode = AuthoringMode.Unknown;

        bool m_CanCopyPose;
        bool m_CanDeleteSelectedEntities;
        bool m_CanDisableSelectedEffectors;

        internal bool IsEditingKey()
        {
            return Mode == AuthoringMode.EditKey;
        }

        internal bool IsEditingKeyPose()
        {
            return Mode == AuthoringMode.EditKey && ActiveKey.Key.Type == KeyData.KeyType.FullPose;
        }

        internal bool IsEditingKeyLoop()
        {
            return Mode == AuthoringMode.EditKey && ActiveKey.Key.Type == KeyData.KeyType.Loop;
        }

        internal TimelineAuthoringModel(AuthoringModel authoringModel)
        {
            Authoring = authoringModel;
        }

        internal void RegisterToEvents()
        {
            RegisterToUI(Context.TimelineUI);
        }

        internal void UnregisterFromEvents()
        {
            UnregisterFromUI(Context.TimelineUI);
        }

        void RegisterToUI(TimelineViewModel timelineViewModel)
        {
            timelineViewModel.OnRequestedAddKey += OnTimelineUIRequestedAddKey;
            timelineViewModel.OnRequestedDeleteSelectedKeys += OnTimelineUIRequestedDeleteSelectedKeys;
            timelineViewModel.OnRequestedEditKey += OnTimelineUIRequestedEditKey;
            timelineViewModel.OnRequestedEditTransition += OnTimelineUIRequestedEditTransition;
            timelineViewModel.OnRequestedInsertKey += OnTimelineUIRequestedInsertKey;
            timelineViewModel.OnRequestedInsertKeyWithEffectorRecovery += OnTimelineUIRequestedInsertKeyWithEffectorRecovery;
            timelineViewModel.OnRequestedKeyToggle += OnTimelineUIRequestedKeyToggle;
            timelineViewModel.OnRequestedSelectTransition += OnTimelineUIRequestedSelectTransition;
            timelineViewModel.OnRequestedTransitionToggle += OnTimelineUIRequestedTransitionToggle;
            timelineViewModel.OnRequestedSeekToKey += OnTimelineUIRequestedSeekToKey;
            timelineViewModel.OnRequestedSeekToTransition += OnTimelineUIRequestedSeekToTransition;
            timelineViewModel.OnRequestedSeekToFrame += OnTimelineUIRequestedSeekToFrame;
            timelineViewModel.OnRequestedMoveKey += OnTimelineUIRequestedMoveKey;
            timelineViewModel.OnRequestedDuplicateKey += OnTimelineUIRequestedDuplicateKey;
            timelineViewModel.OnRequestedDeleteKey += OnTimelineUIRequestedDeleteKey;
            timelineViewModel.OnRequestedPasteKey += OnTimelineUIRequestedPasteKey;
            timelineViewModel.OnRequestedPasteReplaceKey += OnTimelineUIRequestedPasteReplaceKey;
        }

        void UnregisterFromUI(TimelineViewModel timelineViewModel)
        {
            timelineViewModel.OnRequestedAddKey -= OnTimelineUIRequestedAddKey;
            timelineViewModel.OnRequestedDeleteSelectedKeys -= OnTimelineUIRequestedDeleteSelectedKeys;
            timelineViewModel.OnRequestedEditKey -= OnTimelineUIRequestedEditKey;
            timelineViewModel.OnRequestedEditTransition -= OnTimelineUIRequestedEditTransition;
            timelineViewModel.OnRequestedInsertKey -= OnTimelineUIRequestedInsertKey;
            timelineViewModel.OnRequestedInsertKeyWithEffectorRecovery -= OnTimelineUIRequestedInsertKeyWithEffectorRecovery;
            timelineViewModel.OnRequestedKeyToggle -= OnTimelineUIRequestedKeyToggle;
            timelineViewModel.OnRequestedSelectTransition -= OnTimelineUIRequestedSelectTransition;
            timelineViewModel.OnRequestedTransitionToggle -= OnTimelineUIRequestedTransitionToggle;
            timelineViewModel.OnRequestedSeekToKey -= OnTimelineUIRequestedSeekToKey;
            timelineViewModel.OnRequestedSeekToTransition -= OnTimelineUIRequestedSeekToTransition;
            timelineViewModel.OnRequestedSeekToFrame -= OnTimelineUIRequestedSeekToFrame;
            timelineViewModel.OnRequestedMoveKey -= OnTimelineUIRequestedMoveKey;
            timelineViewModel.OnRequestedDuplicateKey -= OnTimelineUIRequestedDuplicateKey;
            timelineViewModel.OnRequestedDeleteKey -= OnTimelineUIRequestedDeleteKey;
            timelineViewModel.OnRequestedPasteKey -= OnTimelineUIRequestedPasteKey;
            timelineViewModel.OnRequestedPasteReplaceKey -= OnTimelineUIRequestedPasteReplaceKey;
        }

        internal void RequestDeleteSelectedEntities()
        {
            OnRequestedDeleteSelectedEntities?.Invoke();
        }

        internal void RequestDisableSelectedEffectors()
        {
            OnRequestedDisableSelectedEffectors?.Invoke();
        }

        internal void RequestPreview()
        {
            OnRequestedPreview?.Invoke();
        }

        //------------------------
        // Keys Requests
        //------------------------

        internal void RequestDeleteKey(TimelineModel.SequenceKey key)
        {
            OnRequestedDeleteKey?.Invoke(key);
        }

        internal void RequestMoveKey(int fromIndex, int toIndex)
        {
            OnRequestedMoveKey?.Invoke(fromIndex, toIndex);
        }

        internal void RequestSeekToFrame(float frame)
        {
            OnRequestedSeekToFrame?.Invoke(frame);
        }

        public TimelineModel.SequenceKey RequestDuplicateKey(int fromIndex, int toIndex)
        {
            return OnRequestedDuplicateKey?.Invoke(fromIndex, toIndex);
        }

        internal void RequestPreviewKey(TimelineModel.SequenceKey key)
        {
            OnRequestedPreviewKey?.Invoke(key);
        }

        internal void RequestEditKey(TimelineModel.SequenceKey key)
        {
            OnRequestedEditKey?.Invoke(key);
        }

        internal void RequestEditKeyIndex(int index)
        {
            OnRequestedEditKeyIndex?.Invoke(index);
        }

        internal void RequestAddKey()
        {
            OnRequestedAddKey?.Invoke();
        }

        internal void RequestInsertKeyWithEffectorRecovery(int bakedFrameIndex, int keyIndex, float progress, out TimelineModel.SequenceKey key)
        {
            key = null;
            OnRequestedInsertKeyWithEffectorRecovery?.Invoke(bakedFrameIndex, keyIndex, progress, out key);
        }

        internal void RequestInsertKey(int keyIndex, out TimelineModel.SequenceKey key)
        {
            key = null;
            OnRequestedInsertKey?.Invoke(keyIndex, out key);
        }

        internal void RequestInsertKeyCopy(out TimelineModel.SequenceKey insertedKey, int atIndex, KeyModel keyToPaste, TransitionModel keyOutTransitionToPaste = null, bool splitTransition = false, float splitTimePercent = 0f)
        {
            insertedKey = null;
            OnRequestedInsertKeyCopy?.Invoke(out insertedKey, atIndex, keyToPaste, keyOutTransitionToPaste, splitTransition, splitTimePercent);
        }

        internal void RequestSelectKey(TimelineModel.SequenceKey key)
        {
            OnRequestedSelectKey?.Invoke(key);
        }

        internal void RequestSeekToKey(TimelineModel.SequenceKey key)
        {
            OnRequestedSeekToKey?.Invoke(key);
        }

        internal void RequestSelectKeyIndex(int index)
        {
            OnRequestedSelectKeyIndex?.Invoke(index);
        }

        internal void RequestDeleteSelectedKeys()
        {
            OnRequestedDeleteSelectedKeys?.Invoke();
        }

        //------------------------
        // Transitions Requests
        //------------------------

        internal void RequestEditTransition(TimelineModel.SequenceTransition transition)
        {
            OnRequestedEditTransition?.Invoke(transition);
        }

        internal void RequestPreviewTransition(TimelineModel.SequenceTransition key)
        {
            OnRequestedPreviewTransition?.Invoke(key);
        }

        internal void RequestSelectTransition(TimelineModel.SequenceTransition transition)
        {
            OnRequestedSelectTransition?.Invoke(transition);
        }

        internal void RequestSeekToTransition(TimelineModel.SequenceTransition transition)
        {
            OnRequestedSeekToTransition?.Invoke(transition);
        }

        internal void RequestSaveWorkingStage()
        {
            OnRequestSaveWorkingStage?.Invoke();
            IsDirty = false;
        }

        internal void RequestResetPose()
        {
            OnRequestedResetPose?.Invoke();
        }

        internal void RequestCopyPose()
        {
            OnRequestedCopyPose?.Invoke();
        }

        internal void RequestPastePose()
        {
            OnRequestedPastePose?.Invoke();
        }

        // [Section] User Asks

        void AskAddKey()
        {
            UserActionsManager.Instance.RecordAddTimelineKey(Timeline);

            // Edit the new key
            RequestAddKey();

            var key = Timeline.GetLastKey();
            RequestEditKey(key);
        }

        void AskEditKey(TimelineModel.SequenceKey key)
        {
            UserActionsManager.Instance.BackupForUndo();
            var beforeKey = ActiveKey.Key;
            var beforeIndex = 0;

            if (beforeKey != null)
                beforeIndex = beforeKey.ListIndex;

            UserActionsManager.Instance.RecordEditTimelineKey(Timeline, beforeIndex, key.Key.ListIndex);
            RequestEditKey(key);
        }

        void AskPreviewKey(TimelineModel.SequenceKey key)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestPreviewKey(key);
        }

        void AskDeleteKey(TimelineModel.SequenceKey key)
        {
            UserActionsManager.Instance.BackupForUndo();
            UserActionsManager.Instance.RecordDeleteTimelineKey(Timeline, key.Key);
            RequestDeleteKey(key);
        }

        void AskDuplicateKey(int fromIndex, int toIndex)
        {
            UserActionsManager.Instance.BackupForUndo();
            var keyToDuplicate = Timeline.GetKey(fromIndex);
            UserActionsManager.Instance.RecordDuplicateTimelineKey(Timeline, keyToDuplicate, fromIndex, toIndex);
            var duplicatedKey = RequestDuplicateKey(fromIndex, toIndex);
            RequestEditKey(duplicatedKey);
        }

        internal void AskDuplicateSelectedKey(bool left)
        {
            if (ActiveKey == null)
                return;

            var fromIndex = ActiveKey.Key.ListIndex;
            var toIndex = left ? fromIndex : fromIndex + 1;
            AskDuplicateKey(fromIndex, toIndex);
        }

        internal void AskMoveSelectedKey(bool left)
        {
            if (ActiveKey == null)
                return;

            var fromIndex = ActiveKey.Key.ListIndex;
            var toIndex = left ? fromIndex - 1 : fromIndex + 1;
            AskMoveKey(fromIndex, toIndex);
        }

        void AskMoveKey(int fromIndex, int toIndex)
        {
            UserActionsManager.Instance.BackupForUndo();
            UserActionsManager.Instance.RecordMoveTimelineKey(Timeline, fromIndex, toIndex);
            RequestMoveKey(fromIndex, toIndex);
        }

        void AskEditTransition(TimelineModel.SequenceTransition transition)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestEditTransition(transition);
        }

        void AskPreviewTransition(TimelineModel.SequenceTransition transition)
        {
            RequestPreviewTransition(transition);
        }

        void AskInsertKey(int keyIndex, float transitionProgress)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestInsertKey(keyIndex, out var key);
            UserActionsManager.Instance.RecordInsertTimelineKey(Timeline, keyIndex, key);
            RequestEditKey(key);
        }

        void AskInsertKeyWithEffectorRecovery(int currentFrameIndex, int keyIndex, float transitionProgress)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestInsertKeyWithEffectorRecovery(currentFrameIndex, keyIndex, transitionProgress, out var key);
            UserActionsManager.Instance.RecordInsertTimelineKeyWithEffectorRecovery(Timeline, currentFrameIndex, keyIndex, transitionProgress, key);
            RequestEditKey(key);
        }

        void AskSeekToFrame(float frame)
        {
            RequestSeekToFrame(frame);
        }

        void AskSeekToKey(TimelineModel.SequenceKey key)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestSeekToKey(key);
        }

        void AskSeekToTransition(TimelineModel.SequenceTransition key)
        {
            UserActionsManager.Instance.BackupForUndo();
            RequestSeekToTransition(key);
        }

        internal void AskDuplicateAny()
        {
            throw new NotImplementedException();
        }

        internal void AskCopyAny()
        {
            throw new NotImplementedException();
        }

        internal void AskPasteAny()
        {
            throw new NotImplementedException();
        }

        internal void AskResetPose()
        {
            UserActionsManager.Instance.StartUserEdit("Reset Pose");
            RequestResetPose();
        }

        internal void AskCopyPose()
        {
            RequestCopyPose();
        }

        internal void AskPastePose()
        {
            UserActionsManager.Instance.StartUserEdit("Paste Pose");
            RequestPastePose();
        }

        internal void AskPasteKey(bool left)
        {
            if (ActiveKey == null)
                return;

            var previousIndex = ActiveKey.Key.ListIndex;
            var toIndex = left ? previousIndex : previousIndex + 1;

            var keyToPaste = ActiveKey.Key.Clone();

            TransitionModel keyOutTransitionToPaste = null;

            if (ActiveKey.OutTransition != null)
            {
                keyOutTransitionToPaste = ActiveKey.OutTransition.Transition.Clone();
            }

            Context.Clipboard.Paste(keyToPaste);
            Context.Clipboard.Paste(keyOutTransitionToPaste);

            UserActionsManager.Instance.RecordInsertTimelineKeyCopy(Context.Stage.WorkingTimeline, previousIndex, toIndex, keyToPaste, keyOutTransitionToPaste, false, 0f);
            RequestInsertKeyCopy(out var insertedKey, toIndex, keyToPaste, keyOutTransitionToPaste);
            RequestEditKey(insertedKey);
        }

        internal void AskDeleteSelectedKey()
        {
            RequestDeleteSelectedKeys();
        }

        internal void AskDeleteSelectedEntities()
        {
            RequestDeleteSelectedEntities();
        }

        internal void AskPasteReplaceKey()
        {
            UserActionsManager.Instance.StartUserEdit("Paste Replace Key");
            DoPasteReplaceKey();
        }

        internal void AskCopyKey()
        {
            DoCopyKey();
        }

        // [Section] Timeline UI Event Handlers

        void OnTimelineUIRequestedKeyToggle(TimelineModel.SequenceKey key)
        {
            if (Mode == TimelineAuthoringModel.AuthoringMode.Preview)
            {
                AskEditKey(key);
            }
            else if (Mode == TimelineAuthoringModel.AuthoringMode.EditKey)
            {
                AskPreviewKey(key);
            }
        }

        void OnTimelineUIRequestedDeleteKey(TimelineModel.SequenceKey key)
        {
            AskDeleteKey(key);
        }

        void OnTimelineUIRequestedDuplicateKey(int fromIndex, int toIndex)
        {
            AskDuplicateKey(fromIndex, toIndex);
        }

        void OnTimelineUIRequestedMoveKey(int fromIndex, int toIndex)
        {
            AskMoveKey(fromIndex, toIndex);
        }

        void OnTimelineUIRequestedEditKey(TimelineModel.SequenceKey key)
        {
            AskEditKey(key);
        }

        void OnTimelineUIRequestedSeekToFrame(float frame)
        {
            AskSeekToFrame(frame);
        }

        void OnTimelineUIRequestedTransitionToggle(TimelineModel.SequenceTransition transition)
        {
            if (Mode == TimelineAuthoringModel.AuthoringMode.Preview)
            {
                AskEditTransition(transition);
            }
            else if (Mode == TimelineAuthoringModel.AuthoringMode.EditTransition)
            {
                AskPreviewTransition(transition);
            }
        }

        void OnTimelineUIRequestedSelectTransition(TimelineModel.SequenceTransition transition)
        {
            RequestSelectTransition(transition);
        }

        void OnTimelineUIRequestedDeleteSelectedKeys()
        {
            RequestDeleteSelectedKeys();
        }

        void OnTimelineUIRequestedAddKey()
        {
            AskAddKey();
        }

        void OnTimelineUIRequestedInsertKey(int keyIndex, float transitionProgress)
        {
            AskInsertKey(keyIndex, transitionProgress);
        }

        void OnTimelineUIRequestedEditTransition(TimelineModel.SequenceTransition transition)
        {
            AskEditTransition(transition);
        }

        void OnTimelineUIRequestedSeekToTransition(TimelineModel.SequenceTransition transition)
        {
            AskSeekToTransition(transition);
        }

        void OnTimelineUIRequestedSeekToKey(TimelineModel.SequenceKey key)
        {
            AskSeekToKey(key);
        }

        void OnTimelineUIRequestedInsertKeyWithEffectorRecovery(int currentFrameIndex, int keyIndex, float transitionProgress)
        {
            AskInsertKeyWithEffectorRecovery(currentFrameIndex, keyIndex, transitionProgress);
        }

        void OnTimelineUIRequestedPasteReplaceKey(int keyIndex)
        {
            AskPasteReplaceKey();
        }

        void OnTimelineUIRequestedPasteKey(int fromIndex, bool left)
        {
            AskPasteKey(left);
        }

        // [Section] Actions

        internal void DoFrameCamera()
        {
            if (!Context.EntitySelection.HasSelection)
            {
                Context.CameraMovement.Frame(Context.Stage.WorkingTimeline.GetWorldBounds());
                return;
            }

            var entityID = Context.EntitySelection.GetSelection(0);
            var bounds = GetEntityBounds(entityID);
            for (var i = 1; i < Context.EntitySelection.Count; i++)
            {
                entityID = Context.EntitySelection.GetSelection(i);
                var actorBounds = GetEntityBounds(entityID);
                bounds.Encapsulate(actorBounds);
            }

            DeepPoseAnalytics.SendActionOfInterestEvent(DeepPoseAnalytics.ActionOfInterest.FrameCameraOnEntitySelection);
            Context.CameraMovement.Frame(bounds);
        }

        internal void DoPasteAny()
        {
            // TODO: Handle what to paste between (library items/timeline items/entities/effectors)
            DoPasteReplaceKey();
            DoPastePose();
        }

        internal void DoPasteReplaceKey()
        {
            if (Context.KeySelection.Count != 1)
                return;

            //UserActionsManager.Instance.BackupForUndo();

            var selectedKey = Context.KeySelection.GetSelection(0);
            Context.Clipboard.Paste(selectedKey.Key);

            if (selectedKey.OutTransition != null)
                Context.Clipboard.Paste(selectedKey.OutTransition.Transition);

            Context.Stage.WorkingTimeline.UpdateIndices();
            Context.PoseAuthoringLogic.RestorePosingStateFromKey(selectedKey.Key);
        }

        internal void DoInsertKeyCopy(out TimelineModel.SequenceKey insertedKey, int atIndex, KeyModel keyToPaste, TransitionModel keyOutTransitionToPaste = null, bool splitTransition = false, float splitTimePercent = 0f)
        {
            insertedKey = Context.Stage.WorkingTimeline.InsertKeyCopy(atIndex, keyToPaste, false, keyOutTransitionToPaste, splitTransition, splitTimePercent);
        }

        internal void DoPastePose()
        {
            if (Context.KeySelection.Count != 1 || Context.EntitySelection.Count != 1)
                return;

            var selectedKey = Context.KeySelection.GetSelection(0);
            var selectedEntityID = Context.EntitySelection.GetSelection(0);
            if (!selectedKey.Key.TryGetKey(selectedEntityID, out var entityKeyModel))
                return;

            Context.Clipboard.Paste(entityKeyModel);
            Context.PoseAuthoringLogic.RestorePosingStateFromKey(selectedKey.Key);
            DeepPoseAnalytics.SendActionOfInterestEvent(DeepPoseAnalytics.ActionOfInterest.PastePose);
        }

        internal void DoResetPose()
        {
            if (Context.KeySelection.Count != 1)
                return;

            var selectedKey = Context.KeySelection.GetSelection(0);

            for (var i = 0; i < Context.EntitySelection.Count; i++)
            {
                var selectedEntity = Context.EntitySelection.GetSelection(i);

                if (!Context.Stage.TryGetActorModel(selectedEntity, out var actorModel))
                    continue;

                if (!Context.PoseLibrary.TryGetDefaultPose(actorModel, out var sourceEntityKey))
                    continue;

                if (!selectedKey.Key.TryGetKey(selectedEntity, out var destinationEntityKey))
                    continue;

                sourceEntityKey.CopyTo(destinationEntityKey);
            }

            Context.PoseAuthoringLogic.RestorePosingStateFromKey(selectedKey.Key);
            Context.PoseAuthoringLogic.ApplyPosingStateToKey(selectedKey.Key);
            DeepPoseAnalytics.SendActionOfInterestEvent(DeepPoseAnalytics.ActionOfInterest.ResetPose);
        }

        internal void DoPreviewKey(TimelineModel.SequenceKey key)
        {
            DoSelectKey(key);
            DoSeekToKey(key);
            DoPreview();
        }

        internal void DoSelectKey(TimelineModel.SequenceKey key)
        {
            Context.KeySelection.SetSelection(key);
            LastSelectionType = SelectionType.SequenceKey;
        }

        internal void DoSelectTransition(TimelineModel.SequenceTransition transition)
        {
            Context.TransitionSelection.SetSelection(transition);
            LastSelectionType = SelectionType.SequenceTransition;
        }

        internal void DoInsertKey(int keyIndex, out TimelineModel.SequenceKey insertedKey)
        {
            insertedKey = Context.Stage.WorkingTimeline.InsertKey(keyIndex, false);
            Context.PoseAuthoringLogic.ApplyPosingStateToKey(insertedKey.Key);
        }

        internal void DoEditSelectedKey()
        {
            if (Context.KeySelection.Count != 1)
                return;

            DoEditKey(Context.KeySelection.GetSelection(0));
        }

        internal void DoEditSelectedTransition()
        {
            if (Context.TransitionSelection.Count != 1)
                return;

            DoEditTransition(Context.TransitionSelection.GetSelection(0));
        }

        internal void DoEditKey(TimelineModel.SequenceKey key)
        {
            ActiveTransition = null;

            if (ActiveKey != key)
            {
                ActiveKey = key;
            }

            DoSelectKey(key);
            DoSeekToKey(key);

            Context.PoseAuthoringLogic.RestorePosingStateFromKey(key.Key);

            // This logic looks a bit strange, since we are leaving the state and entering it again.
            // Basically, we need this to trigger a state transition,
            // i.e. change the sub-state to the correct type.

            Mode = AuthoringMode.Unknown;
            Mode = AuthoringMode.EditKey;
        }

        internal void DoEditTransition(TimelineModel.SequenceTransition transition)
        {
            ActiveKey = null;
            ActiveTransition = transition;

            DoSelectTransition(transition);
            DoSeekToTransition(transition);

            Mode = AuthoringMode.Unknown;
            Mode = AuthoringMode.EditTransition;
        }

        internal void DoPreview()
        {
            Mode = AuthoringMode.Unknown;
            Mode = AuthoringMode.Preview;
        }

        internal void DoSeekToTransition(TimelineModel.SequenceTransition transition)
        {
            Context.BakedTimelineViewLogic.ResetAllLoopOffsets();

            if (Context.Stage.WorkingBakedTimelineMapping.TryGetBakedTransitionSegment(
                    Context.Stage.WorkingTimeline.IndexOf(transition),
                    out var startBakedFrameIndex, out var endBakedFrameIndex))
            {
                Context.Playback.CurrentFrame = (startBakedFrameIndex + endBakedFrameIndex) / 2f;
            }
        }

        internal void DoSeekToKey(TimelineModel.SequenceKey key)
        {
            Context.BakedTimelineViewLogic.ResetAllLoopOffsets();

            if (Context.Stage.WorkingBakedTimelineMapping.TryGetBakedKeyIndex(
                    Context.Stage.WorkingTimeline.IndexOf(key),
                    out var bakedFrameIndex))
            {
                Context.Playback.CurrentFrame = bakedFrameIndex;
            }
        }

        internal void DoAddKey()
        {
            // Add a new key to the timeline
            var sequenceKey = Context.Stage.WorkingTimeline.AddKey(false);

            // Save the current pose to the newly created key
            Context.PoseAuthoringLogic.ApplyPosingStateToKey(sequenceKey.Key);

            DeepPoseAnalytics.SendActionOfInterestEvent(DeepPoseAnalytics.ActionOfInterest.AddKey);
            QueueTimelineBaking();
        }

        internal void DoRemoveKey(TimelineModel.SequenceKey key)
        {
            Context.Stage.WorkingTimeline.RemoveKey(key, false);
            QueueTimelineBaking();
        }

        internal TimelineModel.SequenceKey DoDuplicateKey(int fromIndex, int toIndex)
        {
            var newKey = Context.Stage.WorkingTimeline.DuplicateKey(fromIndex, toIndex, false);
            QueueTimelineBaking();
            return newKey;
        }

        internal void DoCopyAny()
        {
            // TODO: Handle what to copy between (library items/timeline items/entities/effectors)
            DoCopyKey();
            DoCopyPose();
        }

        internal void DoDuplicateAny()
        {
            // TODO: Handle what to duplicate between (library items/timeline items/entities)
            DoDuplicateSelectedKey();
        }

        internal void DoDuplicateSelectedKey(bool left = false)
        {
            if (Context.KeySelection.Count != 1)
                return;

            var selectedKey = Context.KeySelection.GetSelection(0);
            var oldIndex = Context.Stage.WorkingTimeline.IndexOf(selectedKey);

            var toIndex = left ? oldIndex : oldIndex + 1;
            var newKey = Context.Stage.WorkingTimeline.DuplicateKey(oldIndex, toIndex, false);

            Context.KeySelection.Clear();
            Context.KeySelection.Select(newKey);
        }

        internal void DoMoveKey(int fromIndex, int toIndex)
        {
            var movingActiveKey = false;
            var movingActiveTransition = false;

            if (Context.AuthoringModel.Timeline.ActiveKey != null)
            {
                if (Context.AuthoringModel.Timeline.ActiveKey.Key.ListIndex == fromIndex)
                {
                    movingActiveKey = true;
                }
            }
            else if (Context.AuthoringModel.Timeline.ActiveTransition != null)
            {
                if (Context.AuthoringModel.Timeline.ActiveTransition.Transition.ListIndex == fromIndex)
                {
                    movingActiveTransition = true;
                }
            }

            Context.Stage.WorkingTimeline.MoveKey(fromIndex, toIndex, false);

            if (movingActiveKey)
            {
                var key = Context.Stage.WorkingTimeline.GetKey(toIndex);
                Context.AuthoringModel.Timeline.RequestEditKey(key);
            }
            else if (movingActiveTransition)
            {
                var transition = Context.Stage.WorkingTimeline.GetTransition(toIndex);
                Context.AuthoringModel.Timeline.RequestEditTransition(transition);
            }

            QueueTimelineBaking();
        }

        internal void DoMoveSelectedKey(bool left = false)
        {
            if (Context.KeySelection.Count != 1)
                return;

            var keyModel = Context.KeySelection.GetSelection(0);
            var keyIndex = Context.Stage.WorkingTimeline.IndexOf(keyModel);
            var toIndex = left ? keyIndex - 1 : keyIndex + 1;
            DoMoveKey(keyIndex, toIndex);
        }

        internal void DoTogglePlayback()
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

        internal void DoCopyKey()
        {
            if (ActiveKey == null)
                return;

            Context.Clipboard.Copy(ActiveKey.Key);

            if (ActiveKey.OutTransition != null)
                Context.Clipboard.Copy(ActiveKey.OutTransition.Transition);
        }

        internal void DoDeleteSelectedKey()
        {
            // Note: There is no multiple selection allowed on keys for the moment
            if (Context.KeySelection.Count != 1 || !CanDeleteSelectedKeys())
                return;

            var keyModel = Context.KeySelection.GetSelection(0);
            var oldIndex = Context.Stage.WorkingTimeline.IndexOf(keyModel);
            Context.Stage.WorkingTimeline.RemoveKey(keyModel, false);

            Context.TransitionSelection.Clear();
            Context.KeySelection.Clear();

            DoEditKey(Context.Stage.WorkingTimeline.GetKey(Mathf.Min(oldIndex, Context.Stage.WorkingTimeline.KeyCount - 1)));
        }

        internal void DoDeleteSelectedEntities()
        {
            // 26th June Release: disabling multiple characters and props
            return;
            /*
            using var toRemoveList = TempList<EntityID>.Allocate();

            for (var i = 0; i < Context.EntitySelection.Count; i++)
            {
                var entityID = Context.EntitySelection.GetSelection(i);
                toRemoveList.Add(entityID);
            }

            foreach (var entityID in toRemoveList.List)
            {
                Context.Stage.RemoveEntity(entityID);
            }*/
        }

        internal void DoCopyPose()
        {
            if (Context.KeySelection.Count != 1 || Context.EntitySelection.Count != 1)
                return;

            var selectedKey = Context.KeySelection.GetSelection(0);
            var selectedEntityID = Context.EntitySelection.GetSelection(0);
            if (!selectedKey.Key.TryGetKey(selectedEntityID, out var entityKeyModel))
                return;

            Context.Clipboard.Copy(entityKeyModel);
            DeepPoseAnalytics.SendActionOfInterestEvent(DeepPoseAnalytics.ActionOfInterest.CopyPose);
        }

        /// <summary>
        /// Insert or replace a key with a key automatically built from effectors recovery.
        /// The key pose is built from the current timeline baked output animation, at the given bakedFrameIndex.
        /// </summary>
        /// <param name="bakedFrameIndex">The frame to use from the current timeline baked output animation.</param>
        /// <param name="keyIndex">The key index to insert the new key at.</param>
        /// <param name="progress">A 0 to 1 ratio representing the position in time of the inserted key in relation it's previous and next keys.</param>
        /// <param name="key">The key built from effectors recovery.</param>
        internal void DoInsertKeyWithEffectorRecovery(int bakedFrameIndex, int keyIndex, float progress, out TimelineModel.SequenceKey key)
        {
            // Check if there is a key directly at the bakedFrameIndex
            if (Context.Stage.WorkingBakedTimelineMapping.TryGetKeyIndex(bakedFrameIndex, out var existingKeyIndex))
            {
                key = Context.Stage.WorkingTimeline.GetKey(existingKeyIndex);
            }
            else
            {
                key = Context.Stage.WorkingTimeline.InsertKey(keyIndex, false, true, progress);
            }

            RecoverEffectorsFromBakedTimeline(bakedFrameIndex);
            Context.PoseAuthoringLogic.ApplyPosingStateToKey(key.Key);

            Context.AuthoringModel.RequestGenerateKeyThumbnail(key.Thumbnail, key.Key);

            // A bake step has to be performed here, to update the Context.BakedTimelineMapping
            Context.TimelineBakingLogic.QueueBaking(true);

            Context.AuthoringModel.Timeline.RequestEditKeyIndex(keyIndex);
        }

        internal void DoPreviewTransition(TimelineModel.SequenceTransition transition)
        {
            DoSeekToTransition(transition);
            DoPreview();
        }

        internal void QueueTimelineBaking()
        {
            Context.TimelineBakingLogic.QueueBaking(true);
        }

        void RecoverEffectorsFromBakedTimeline(int bakedFrameIndex)
        {
            var from = Context.Stage.WorkingBakedTimeline.GetFrame(bakedFrameIndex);

            for (var i = 0; i < Context.Stage.NumActors; i++)
            {
                var actorModel = Context.Stage.GetActorModel(i);

                if (from.TryGetModel(actorModel.EntityID, out var bakedPose))
                {
                    // Apply the pose from the baked frame on the posing armature
                    var posingArmature = Context.PoseAuthoringLogic.GetPosingArmature(actorModel.EntityID);
                    bakedPose.ApplyTo(posingArmature.ArmatureMappingData);

                    // We snap physics BEFORE effector recovery as we know the baked frame should have a physically-accurate pose,
                    // EXCEPT for the first keyframe. See note below.
                    Context.PoseAuthoringLogic.SnapPhysicsToPosing(actorModel.EntityID);
                    Context.PoseAuthoringLogic.DoEffectorRecovery(actorModel.EntityID);

                    // For the first baked frame, we need to make sure there is no ground penetration. This can
                    // happen if we are extracting keys from a baked timeline does not obey physics (e.g.
                    // text-to-motion).
                    if (bakedFrameIndex == 0)
                        Context.PoseAuthoringLogic.ResolveGroundPenetration(actorModel.EntityID);

                    // We do not snap physics AFTER effector recovery as this could lead to penetration when effector recovery is imperfect.
                    // Instead we let the physics solve try to match the recovered pose
                }
                else
                {
                    Debug.Log($"Failed to locate pose in frame for entityID: {actorModel.EntityID}");
                }
            }
        }

        // [Section] Capabilities Checks

        internal bool CanDeleteKey()
        {
            return Context.KeySelection.Count == 1 && Context.Stage.WorkingTimeline.KeyCount > 1;
        }

        internal bool CanDeleteSelectedKeys()
        {
            return Context.KeySelection.Count < Context.Stage.WorkingTimeline.KeyCount;
        }

        internal bool CanPastePose()
        {
            if (Context.EntitySelection.Count != 1 || Context.KeySelection.Count != 1)
                return false;

            var selectedEntityID = Context.EntitySelection.GetSelection(0);
            var selectedKey = Context.KeySelection.GetSelection(0);
            if (!selectedKey.Key.TryGetKey(selectedEntityID, out var entityKeyModel))
                return false;

            var canPasteKey = Context.Clipboard.CanPaste(entityKeyModel);
            return canPasteKey;
        }

        internal bool CanPasteKey()
        {
            if (Context.KeySelection.Count != 1)
                return false;

            var keyModel = Context.KeySelection.GetSelection(0);
            return Context.Clipboard.CanPaste(keyModel.Key);
        }

        internal bool CanMoveKey(bool left = false)
        {
            if (Context.KeySelection.Count != 1)
                return false;

            var keyModel = Context.KeySelection.GetSelection(0);
            var keyIndex = Context.Stage.WorkingTimeline.IndexOf(keyModel);
            return left ? keyIndex > 0 : keyIndex < Context.Stage.WorkingTimeline.KeyCount - 1;
        }
        
        internal void RefreshCanDeleteSelectedEntities()
        {
            CanDeleteSelectedEntities = GetCanDeleteSelectedEntities();
        }

        internal void RefreshCanCopyPose()
        {
            CanCopyPose = GetCanCopyPose();
        }

        // [Section] Getters

        bool TryGetPoseCopyHumanoidAnimator(out Animator animator)
        {
            animator = null;
            if (Context.EntitySelection.Count != 1)
                return false;

            var entityID = Context.EntitySelection.GetSelection(0);

            var armature = GetCurrentViewArmature(entityID);
            if (armature == null)
                return false;

            return armature.gameObject.TryGetHumanoidAnimator(out animator);
        }

        Bounds GetEntityBounds(EntityID entityID)
        {
            var viewGameObject = Context.PoseAuthoringLogic.GetPosingGameObject(entityID);
            var bounds = viewGameObject.GetRenderersWorldBounds();
            return bounds;
        }

        ArmatureMappingComponent GetCurrentViewArmature(EntityID entityID)
        {
            switch (Context.AuthoringModel.Timeline.Mode)
            {
                case AuthoringMode.Unknown:
                    return null;

                case AuthoringMode.Preview:
                case AuthoringMode.EditTransition:
                    return Context.BakedTimelineViewLogic.GetPreviewArmature(entityID);

                case AuthoringMode.EditKey:
                {
                    if (!Context.KeySelection.HasSelection)
                        return null;

                    var selectedKey = Context.KeySelection.GetSelection(0);
                    return selectedKey.Key.Type switch
                    {
                        KeyData.KeyType.Empty => null,
                        KeyData.KeyType.FullPose => Context.PoseAuthoringLogic.GetViewArmature(entityID),
                        KeyData.KeyType.Loop => null,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        bool GetCanCopyPose()
        {
            return TryGetPoseCopyHumanoidAnimator(out var _);
        }

        bool GetCanDeleteSelectedEntities()
        {
            return Context.EntitySelection.HasSelection;
        }

        
    }
}
