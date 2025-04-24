namespace Unity.Muse.Animate.UserActions
{
    class ModifiedTimelineTransition : TimelineAction
    {
        TransitionModel m_Before;
        TransitionModel m_After;
        int m_Index;

        internal ModifiedTimelineTransition(
            int id, int groupId, 
            string userEditDescription,
            AuthoringModel authoring, 
            TimelineModel timeline, 
            int index, 
            TransitionModel before, 
            TransitionModel after)
            : base(id, groupId,
                $"Modified Transition {after.ListIndex}: {userEditDescription}",
                TimelineActionType.ModifiedTimelineTransition,
                authoring,
                timeline)
        {
            m_Index = index;
            m_Before = before.Clone();
            m_After = after.Clone();
        }

        public override void Redo()
        {
            Log("Redo()");
            m_After.CopyTo(Timeline.Transitions[m_Index].Transition);
        }

        public override void Undo()
        {
            Log("Undo()");
            m_Before.CopyTo(Timeline.Transitions[m_Index].Transition);
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
