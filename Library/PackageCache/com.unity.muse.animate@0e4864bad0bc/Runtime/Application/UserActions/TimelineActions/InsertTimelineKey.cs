namespace Unity.Muse.Animate.UserActions
{
    class InsertTimelineKey : TimelineAction
    {
        int m_KeyIndex;

        internal InsertTimelineKey(
            int id, int groupId,
            AuthoringModel authoring, 
            TimelineModel timeline, 
            int atIndex) 
            : base(
                id, groupId,
                "Insert Key "+atIndex, 
                TimelineActionType.InsertKey,
                authoring,
                timeline)
        {
            m_KeyIndex = atIndex;
        }

        public override void Redo()
        {
            Log("Redo()");
            // Insert a key at the specific index
            Authoring.Timeline.RequestInsertKey(m_KeyIndex, out var key);
            // Edit the inserted key
            Authoring.Timeline.RequestEditKey(key);
        }

        public override void Undo()
        {
            Log("Undo()");
            // Delete the key that was inserted
            Authoring.Timeline.RequestDeleteKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_KeyIndex));
            // Return to editing the key that was selected before inserting a key
            Authoring.Timeline.RequestEditKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_KeyIndex-1));
        }
    }
}
