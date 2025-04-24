namespace Unity.Muse.Animate.UserActions
{
    class InsertTimelineKeyWithEffectorRecovery : TimelineAction
    {
        int m_KeyIndex;
        float m_TransitionProgress;
        int m_BakedFrameIndex;

        internal InsertTimelineKeyWithEffectorRecovery(
            int id, int groupId,
            AuthoringModel authoring,
            TimelineModel timeline,
            int bakedFrameIndex,
            int keyIndex,
            float transitionProgress)
            : base(
                id, groupId,
                "Insert Key " + keyIndex + " (Effector Recovery)",
                TimelineActionType.InsertKeyWithRecovery,
                authoring,
                timeline)
        {
            m_KeyIndex = keyIndex;
            m_BakedFrameIndex = bakedFrameIndex;
            m_TransitionProgress = transitionProgress;
            Log("Created()");
        }

        public override void Redo()
        {
            Log("Redo()");

            // Insert a key at the specific index
            Authoring.Timeline.RequestInsertKeyWithEffectorRecovery(m_BakedFrameIndex, m_KeyIndex, m_TransitionProgress, out var insertedKey);

            // Edit the inserted key
            Authoring.Timeline.RequestEditKey(insertedKey);
        }

        public override void Undo()
        {
            Log("Undo()");

            // Delete the key that was inserted
            Authoring.Timeline.RequestDeleteKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_KeyIndex));

            // Return to editing the key that was selected before inserting a key
            Authoring.Timeline.RequestEditKey(Authoring.Context.TimelineContext.Stage.WorkingTimeline.GetKey(m_KeyIndex));
        }
    }
}
