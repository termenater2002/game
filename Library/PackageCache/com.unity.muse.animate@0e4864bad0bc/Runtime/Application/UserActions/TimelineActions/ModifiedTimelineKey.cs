namespace Unity.Muse.Animate.UserActions
{
    class ModifiedTimelineKey : TimelineAction
    {
        KeyModel m_Before;
        KeyModel m_After;
        int m_KeyIndex;

        internal ModifiedTimelineKey(
            int id, int groupId, 
            string userEditDescription,
            AuthoringModel authoring, 
            TimelineModel timeline, 
            int keyIndex, 
            KeyModel before, 
            KeyModel after) 
            : base(id, groupId,
                "Modify Key "+after.ListIndex+": "+userEditDescription,
                TimelineActionType.ModifiedTimelineKey,
                authoring,
                timeline)
        {
            m_KeyIndex = keyIndex;
            m_Before = before.Clone();
            m_After = after.Clone();
        }

        public override void Redo()
        {
            Log("Redo()");
            m_After.CopyTo(Timeline.Keys[m_KeyIndex].Key);
            Authoring.PoseAuthoringLogic.RestorePosingStateFromKey(Timeline.Keys[m_KeyIndex].Key);
            UserActionsManager.Instance.BackupForUndo();
        }

        public override void Undo()
        {
            Log("Undo()");
            m_Before.CopyTo(Timeline.Keys[m_KeyIndex].Key);
            Authoring.PoseAuthoringLogic.RestorePosingStateFromKey(Timeline.Keys[m_KeyIndex].Key);
            UserActionsManager.Instance.BackupForUndo();
        }

        public override void Clear()
        {
            Log("Clear()");
            m_Before = null;
            m_After = null;
            base.Clear();
        }
    }
}
