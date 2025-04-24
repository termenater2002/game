using Unity.Muse.Animate.Toolbar;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring the <see cref="TimelineModel.SequenceKey"/> of a <see cref="TimelineModel"/>, when the key is a "Full Pose" type.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTimelineKeyPose"/> state.
    /// </remarks>
    class AuthorTimelineKeyPoseContext
    {
        public StageModel Stage { get; }
        public AuthoringModel AuthoringModel { get; }
        public PoseAuthoringLogic PosingLogic { get; }
        public SelectionModel<EntityID> EntitySelection { get; }
        public SelectionModel<EntityID> EntityEffectorSelection => PosingLogic.EntityManipulatorSelection;
        public TakesUIModel TakesUI { get; }
        public CameraModel Camera => m_CameraContext.CameraModel;
        public CameraMovementModel CameraMovement => m_CameraContext.CameraMovementModel;
        public ToolbarsManager Toolbars { get; }

        readonly CameraContext m_CameraContext;

        public AuthorTimelineKeyPoseContext(StageModel stageModel, AuthoringModel authoringModel, TakesUIModel takesUIModel, PoseAuthoringLogic posingLogic, SelectionModel<EntityID> entitySelection,
            CameraContext cameraContext, ToolbarsManager toolbars)
        {
            Stage = stageModel;
            AuthoringModel = authoringModel;
            m_CameraContext = cameraContext;
            PosingLogic = posingLogic;
            EntitySelection = entitySelection;
            TakesUI = takesUIModel;
            Toolbars = toolbars;
        }
    }
}
