namespace Unity.Muse.Animate.UserActions
{
    class MoveTimelineKey : TimelineAction
    {
        int m_FromIndex;
        int m_ToIndex;

        internal MoveTimelineKey(
            int id, int groupId, 
            AuthoringModel authoring, 
            TimelineModel timeline,
            int fromIndex, 
            int toIndex)
            : base(id, groupId,
                $"Move Key ({fromIndex}->{toIndex})", 
                TimelineActionType.MoveKey,
                authoring,
                timeline
                )
        {
            m_FromIndex = fromIndex;
            m_ToIndex = toIndex;
        }

        public override void Redo()
        {
            Log("Redo()");
            // Move the key to it's target index
            Authoring.Timeline.RequestMoveKey(m_FromIndex, m_ToIndex);
        }

        public override void Undo()
        {
            Log("Undo()");
            // Move the key back to its original index
            Authoring.Timeline.RequestMoveKey(m_ToIndex, m_FromIndex);
        }
    }
}
