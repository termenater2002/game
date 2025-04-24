namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring a <see cref="TimelineModel"/> and viewing its <see cref="BakedTimelineModel"/> output.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTimelinePreview"/> state.
    /// </remarks>
    class AuthorTimelinePreviewContext
    {
        public AuthoringModel AuthoringModel { get; }
        public StageModel Stage { get; }
        public PlaybackModel Playback { get; }
        public BakingLogic TimelineBaking { get; }
        public BakedTimelineModel BakedTimeline { get; }
        public BakedTimelineMappingModel BakedTimelineMapping { get; }
        public TimelineModel Timeline { get; }
        public BakedTimelineViewLogic ViewLogic { get; }
        public SelectionModel<EntityID> EntitySelection { get; }
        
        public BakingNoticeViewModel BakingNoticeUI { get; }
        public CameraMovementModel CameraMovement => m_CameraContext.CameraMovementModel;
        
        CameraContext m_CameraContext;

        public AuthorTimelinePreviewContext(
            AuthoringModel authoringModel, 
            StageModel stage,
            TimelineModel timeline,
            BakingLogic timelineBaking,
            BakedTimelineModel bakedTimeline, 
            BakedTimelineMappingModel bakedTimelineMapping,
            SelectionModel<EntityID> entitySelection, 
            PlaybackModel playbackModel,
            BakedTimelineViewLogic viewLogic, 
            BakingNoticeViewModel bakingNotice,
            CameraContext cameraContext)
        {
            Stage = stage;
            AuthoringModel = authoringModel;
            Timeline = timeline;
            TimelineBaking = timelineBaking;
            Playback = playbackModel;
            ViewLogic = viewLogic;
            BakedTimeline = bakedTimeline;
            BakedTimelineMapping = bakedTimelineMapping;
            EntitySelection = entitySelection;
            BakingNoticeUI = bakingNotice;
            m_CameraContext = cameraContext;
        }
    }
}
