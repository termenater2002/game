namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring the <see cref="TimelineModel.SequenceTransition"/> of a <see cref="TimelineModel"/>.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTimelineTransition"/> state.
    /// </remarks>
    class AuthorTimelineTransitionContext
    {
        public AuthoringModel AuthoringModel => m_AuthoringModel;
        public PlaybackModel Playback => m_PlaybackModel;
        public BakedTimelineViewLogic BakedTimelineViewLogic => m_BakedTimelineViewLogic;
        public CameraMovementModel CameraMovement => m_CameraContext.CameraMovementModel;
        public SelectionModel<EntityID> EntitySelection => m_EntitySelectionModel;

        PlaybackModel m_PlaybackModel;
        BakedTimelineViewLogic m_BakedTimelineViewLogic;
        CameraContext m_CameraContext;
        SelectionModel<EntityID> m_EntitySelectionModel;
        readonly AuthoringModel m_AuthoringModel;

        public AuthorTimelineTransitionContext(AuthoringModel authoringModel, SelectionModel<EntityID> entitySelection, PlaybackModel playbackModel,
            BakedTimelineViewLogic BakedTimelineViewLogic, CameraContext cameraContext)
        {
            m_AuthoringModel = authoringModel;
            m_CameraContext = cameraContext;
            m_PlaybackModel = playbackModel;
            m_BakedTimelineViewLogic = BakedTimelineViewLogic;
            m_EntitySelectionModel = entitySelection;
        }
    }
}
