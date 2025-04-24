namespace Unity.Muse.Animate.UserActions
{
    abstract class TimelineAction : UserAction
    {
        internal enum TimelineActionType
        {
            AddKey,
            DeleteKey,
            DuplicateKey,
            EditKey,
            InsertKey,
            InsertKeyWithRecovery,
            MoveKey,
            ModifiedTimelineKey,
            ModifiedTimelineTransition
        }

        internal TimelineActionType ActionType { get; }
        internal AuthoringModel Authoring { get; set; }
        internal TimelineModel Timeline { get; set; }
        
        internal TimelineAction(
            int id, int groupId,
            string name,
            TimelineActionType timelineActionType,
            AuthoringModel authoring,
            TimelineModel timeline)
            : base(name, id, groupId)
        {
            ActionType = timelineActionType;
            Authoring = authoring;
            Timeline = timeline;
            Log("Created()");
        }

        public override void Clear()
        {
            Authoring = null;
            Timeline = null;
        }
    }
}
