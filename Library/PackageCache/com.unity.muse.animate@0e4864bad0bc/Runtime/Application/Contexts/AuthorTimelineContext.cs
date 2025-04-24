using Unity.Muse.Animate.Toolbar;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring a <see cref="TimelineModel"/>.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTimeline"/> state.
    /// </remarks>
    class AuthorTimelineContext
    {
        const string k_TimelineViewName = "timeline-view";
        const string k_TimelineOverlayUIName = "timeline-authoring-overlay";

        public StageModel Stage { get; }
        public PlaybackModel Playback { get; }
        public ApplicationModel ApplicationModel { get; }
        public ApplicationLibraryModel ApplicationLibraryModel { get; }
        public AuthoringModel AuthoringModel { get; }
        public SelectionModel<EntityID> EntitySelection { get; }
        public SelectionModel<TimelineModel.SequenceKey> KeySelection { get; }
        public SelectionModel<TimelineModel.SequenceTransition> TransitionSelection { get; }
        public SelectionModel<LibraryItemModel> TakeSelection { get; }
        public PoseAuthoringLogic PoseAuthoringLogic { get; }
        public BakingLogic TimelineBakingLogic { get; }
        public BakedTimelineViewLogic BakedTimelineViewLogic { get; }
        public LoopAuthoringLogic LoopAuthoringLogic { get; }
        public ThumbnailsService ThumbnailsService { get; }
        public ClipboardService Clipboard { get; }
        public PoseLibraryService PoseLibrary { get; }
        public AuthorTimelinePreviewContext PreviewContext { get; }
        public AuthorTimelineKeyPoseContext KeyPoseContext { get; }
        public AuthorTimelineKeyLoopContext KeyLoopContext { get; }
        public AuthorTimelineTransitionContext TransitionContext { get; }
        public TimelineViewModel TimelineUI { get; }
        public InspectorsPanelViewModel InspectorsPanelViewModel { get; }
        public VisualElement RootUI { get; }
        public BakingNoticeViewModel BakingNoticeUI { get; }
        public BakingTaskStatusViewModel BakingTaskStatusUI { get; }
        public ToolbarsManager Toolbars { get; }
        public CameraModel Camera => CameraContext.CameraModel;
        public CameraMovementModel CameraMovement => CameraContext.CameraMovementModel;
        
        public SidePanelUIModel SidePanelUI { get; }
        
        CameraContext CameraContext { get; }

        public AuthorTimelineContext(
            StageModel stageModel,
            ApplicationModel applicationModel,
            ApplicationLibraryModel applicationLibraryModel,
            AuthoringModel authoringModel,
            VisualElement rootVisualElement,
            SelectionModel<EntityID> selectionModel,
            BakingTaskStatusViewModel bakingTaskStatusUI,
            BakingNoticeViewModel bakingNoticeUI,
            TakesUIModel takesUIModel,
            PoseAuthoringLogic poseAuthoring,
            BakingLogic timelineBaking,
            CameraContext cameraContext,
            ThumbnailsService thumbnailsService,
            ClipboardService clipboardService,
            PoseLibraryService poseLibraryService,
            InspectorsPanelViewModel inspectorsPanel,
            ToolbarsManager toolbars,
            SidePanelUIModel sidePanelUIModel)
        {
            Stage = stageModel;
            ApplicationModel = applicationModel;
            ApplicationLibraryModel = applicationLibraryModel;
            AuthoringModel = authoringModel;
            RootUI = rootVisualElement;
            PoseAuthoringLogic = poseAuthoring;
            TimelineBakingLogic = timelineBaking;
            ThumbnailsService = thumbnailsService;
            Clipboard = clipboardService;
            PoseLibrary = poseLibraryService;
            EntitySelection = selectionModel;
            
            KeySelection = new SelectionModel<TimelineModel.SequenceKey>();
            TransitionSelection = new SelectionModel<TimelineModel.SequenceTransition>();
            TakeSelection = new SelectionModel<LibraryItemModel>();
            
            Playback = new PlaybackModel(0f, ApplicationConstants.FramesPerSecond) { MaxFrame = Stage.WorkingBakedTimeline.FramesCount - 1 };
            BakedTimelineViewLogic = new BakedTimelineViewLogic("Timeline - Baked Timeline View", Stage.WorkingBakedTimeline, EntitySelection);
            LoopAuthoringLogic = new LoopAuthoringLogic(EntitySelection, Stage.WorkingBakedTimeline, KeySelection);
            
            // UI Models
            Toolbars = toolbars;
            InspectorsPanelViewModel = inspectorsPanel;
            TimelineUI = new TimelineViewModel(
                AuthoringModel, 
                Stage.WorkingTimeline, 
                Playback, 
                TimelineBakingLogic, 
                Stage.WorkingBakedTimelineMapping, 
                KeySelection, 
                TransitionSelection, 
                Clipboard, 
                InspectorsPanelViewModel);

            BakingTaskStatusUI = bakingTaskStatusUI;
            BakingTaskStatusUI.TrackBakingLogics(TimelineBakingLogic);
            BakingNoticeUI = bakingNoticeUI;
            
            // UI Views
            var timelineView = RootUI.Q<TimelineView>(k_TimelineViewName);
            timelineView.SetModel(TimelineUI);
            
            SidePanelUI = sidePanelUIModel;
            
            // Contexts
            CameraContext = cameraContext;
            TransitionContext = new AuthorTimelineTransitionContext(AuthoringModel, EntitySelection, Playback, BakedTimelineViewLogic, CameraContext);
            KeyPoseContext = new AuthorTimelineKeyPoseContext(Stage, AuthoringModel, takesUIModel, PoseAuthoringLogic, EntitySelection, CameraContext, Toolbars);
            KeyLoopContext = new AuthorTimelineKeyLoopContext(LoopAuthoringLogic, AuthoringModel, KeySelection, EntitySelection, CameraContext, Toolbars);
            PreviewContext = new AuthorTimelinePreviewContext(
                AuthoringModel, 
                Stage, 
                Stage.WorkingTimeline, 
                TimelineBakingLogic, 
                Stage.WorkingBakedTimeline, 
                Stage.WorkingBakedTimelineMapping, 
                EntitySelection, 
                Playback, 
                BakedTimelineViewLogic, 
                BakingNoticeUI,
                CameraContext);
        }
    }
}
