namespace Unity.Muse.Animate.UserActions
{
    class DeleteTimelineKey : TimelineAction
    {
        KeyModel m_Before;

        internal DeleteTimelineKey(
            int id, int groupId, 
            AuthoringModel authoring, 
            TimelineModel timeline, 
            KeyModel before) 
            : base(
                id, groupId,
                "Delete Key "+before.ListIndex, 
                TimelineActionType.DeleteKey,
                authoring,
                timeline) 
        {
            m_Before = before.Clone();
        }

        public override void Redo()
        {
            Log("Redo()");
            var keyToDelete = Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_Before.ListIndex);
            
            // Delete the key
            Authoring.Timeline.RequestDeleteKey(keyToDelete);
        }

        public override void Undo()
        {
            Log("Undo()");
            // Re-insert a new key where the previous key was deleted
            Authoring.Timeline.RequestInsertKey(m_Before.ListIndex, out var recoveredKey);
            
            // Overwrite the key with the data of the key before it was deleted
            m_Before.CopyTo(recoveredKey.Key);
            
            // Return to editing the key
            Authoring.Timeline.RequestEditKey(recoveredKey);
        }

        public override void Clear()
        {
            Log("Clear()");
            m_Before = null;
            base.Clear();
        }
    }
}
