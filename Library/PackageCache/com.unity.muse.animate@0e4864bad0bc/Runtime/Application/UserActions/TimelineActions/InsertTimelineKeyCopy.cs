namespace Unity.Muse.Animate.UserActions
{
    class InsertTimelineKeyCopy : TimelineAction
    {
        int m_FromIndex;
        int m_ToIndex;
        KeyModel m_KeyToPaste;
        TransitionModel m_KeyOutTransitionToPaste;
        bool m_SplitTransition;
        float m_SplitTimePercent;
        
        internal InsertTimelineKeyCopy(
            int id, int groupId,
            AuthoringModel authoring,
            TimelineModel timeline,
            KeyModel keyToPaste,
            TransitionModel keyOutTransitionToPaste,
            int fromIndex,
            int toIndex,
            bool splitTransition,
            float splitTimePercent)
            : base(
                id, groupId,
                $"Paste Key {keyToPaste.ListIndex} at {toIndex}",
                TimelineActionType.DuplicateKey,
                authoring,
                timeline)
        {
            m_FromIndex = fromIndex;
            m_ToIndex = toIndex;
            m_KeyToPaste = keyToPaste;
            m_KeyOutTransitionToPaste = keyOutTransitionToPaste;
            m_SplitTransition = splitTransition;
            m_SplitTimePercent = splitTimePercent;
        }

        public override void Redo()
        {
            Log("Redo()");
            Authoring.Timeline.RequestInsertKeyCopy(out var insertedKey, m_ToIndex, m_KeyToPaste, m_KeyOutTransitionToPaste, m_SplitTransition, m_SplitTimePercent);
            Authoring.Timeline.RequestEditKey(insertedKey);
        }

        public override void Undo()
        {
            Log("Undo()");
            // Delete the key that was created from the duplication
            Authoring.Timeline.RequestDeleteKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_ToIndex));
            var activeKeyBeforeAction = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_FromIndex);
            Authoring.Timeline.RequestEditKey(activeKeyBeforeAction);
        }
    }
}
