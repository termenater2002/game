namespace Unity.Muse.Animate.UserActions
{
    class AddTimelineKey : TimelineAction
    {
        KeyModel m_Before;
        
        internal AddTimelineKey(
            int id, int groupId,
            AuthoringModel authoring,
            TimelineModel timeline,
            KeyModel before)
            :
            base(
                id, groupId,
                "Add Key", 
                TimelineActionType.AddKey,
                authoring,
                timeline) 
        {
            m_Before = before.Clone();
        }

        public override void Redo()
        {
            Log("Redo()");

            // Add a key at the end of the timeline
            Authoring.Timeline.RequestAddKey();

            // Edit the key
            var key = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetLastKey();
            Authoring.Timeline.RequestEditKey(key);
        }

        public override void Undo()
        {
            Log("Undo()");

            // Delete the key that was added at the end of the timeline
            var keyToDelete = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetLastKey();
            Authoring.Timeline.RequestDeleteKey(keyToDelete);

            // Edit the key that was being edited beforehand
            var key = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_Before.ListIndex);
            Authoring.Timeline.RequestEditKey(key);
        }

        public override void Clear()
        {
            Log("Clear()");
            m_Before = null;
            base.Clear();
        }
    }
}
