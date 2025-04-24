namespace Unity.Muse.Animate.UserActions
{
    class EditTimelineKey : TimelineAction
    {
        int m_IndexBefore;
        int m_IndexAfter;

        internal EditTimelineKey(
            int id, int groupId,
            AuthoringModel authoring,
            TimelineModel timeline,
            int indexBefore,
            int indexAfter)
            : base(
                id, groupId,
                "Edit Key " + indexAfter,
                TimelineActionType.EditKey,
                authoring,
                timeline)
        {
            m_IndexBefore = indexBefore;
            m_IndexAfter = indexAfter;
        }

        public override void Redo()
        {
            Log("Redo()");
            Authoring.Timeline.RequestEditKey(Timeline.GetKey(m_IndexAfter));
        }

        public override void Undo()
        {
            Log("Undo()");
            Authoring.Timeline.RequestEditKey(Timeline.GetKey(m_IndexBefore));
        }
    }
}
