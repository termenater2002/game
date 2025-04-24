namespace Unity.Muse.Animate.UserActions
{
    class DuplicateTimelineKey : TimelineAction
    {
        int m_FromIndex;
        int m_ToIndex;

        internal DuplicateTimelineKey(
            int id, int groupId,
            AuthoringModel authoring,
            TimelineModel timeline,
            int fromIndex,
            int toIndex)
            : base(
                id, groupId,
                "Duplicate Key " + fromIndex,
                TimelineActionType.DuplicateKey,
                authoring,
                timeline)
        {
            m_FromIndex = fromIndex;
            m_ToIndex = toIndex;
        }

        public override void Redo()
        {
            Log("Redo()");

            // Duplicate the key
            Authoring.Timeline.RequestDuplicateKey(m_FromIndex, m_ToIndex);
            var key = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_ToIndex);
            Authoring.Timeline.RequestEditKey(key);
        }

        public override void Undo()
        {
            Log("Undo()");

            // Delete the key that was created from the duplication
            Authoring.Timeline.RequestDeleteKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_ToIndex));
            var key = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_FromIndex);
            Authoring.Timeline.RequestEditKey(key);
        }
    }
}
