using System;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when authoring a <see cref="TextToMotionTake"/>.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorTextToMotion"/> state.
    /// </remarks>
    class AuthorTextToMotionTakeContext
    {
        const string k_TextToMotionUIName = "text-to-motion-ui";
        
        public ApplicationModel ApplicationModel { get; }
        public ApplicationLibraryModel ApplicationLibraryModel => ApplicationModel.ApplicationLibraryModel;
        
        /// <summary>Model of the TextToMotion workflow.</summary>
        public TextToMotionAuthoringModel Model => AuthoringModel.TextToMotion;

        /// <summary>Model of the broader Authoring workflow inside which this context exists.</summary>
        public AuthoringModel AuthoringModel { get; }

        /// <summary>A local Playback Logic for this tool, used to preview the dense motion output of the Text to Motion model </summary>
        public PlaybackModel Playback { get; }
        
        /// <summary>UI of the TextToMotion workflow.</summary>
        public TextToMotionUIModel UI { get; }
        public TextToMotionTakeUI TextToMotionTakeUI { get; }
        public TextToMotionTakeUIModel TextToMotionTakeUIModel { get; }
        
        /// <summary>UI component used to display a spinning circle at the top right of a scene viewport.</summary>
        public BakingTaskStatusViewModel BakingTaskStatusUI { get; }
        
        /// <summary>UI component used to display a text notice at the top center of a scene viewport.</summary>
        public BakingNoticeViewModel BakingNoticeUI { get; }
        
        /// <summary>UI used to display controls for the TextToMotion playback.</summary>
        public BakedTimelinePlaybackUIModel PlaybackUI => UI?.PlaybackUI;
        
        /// <summary>View of the output of the TextToMotion baker.</summary>
        public BakedTimelineViewLogic OutputBakedTimelineViewLogic { get; }

        /// <summary>Baked animation of the final timeline.
        /// The final timeline is the result of the TextToMotion and MotionToKeys decimation.
        /// It is the final output of the whole TextToMotion workflow.</summary>
        public BakedTimelineModel OutputBakedTimeline { get; }
        public CameraMovementModel CameraMovement => CameraContext.CameraMovementModel;
        public CameraContext CameraContext { get; }
        public SelectionModel<EntityID> EntitySelection { get; }
        public SelectionModel<LibraryItemAsset> ItemSelection { get; }
        public VisualElement RootUI { get; }
        
        public TimelineModel InputTimeline { get; }
        
        public TextToMotionService TextToMotionService { get; }
        
        public TakesUIModel TakesUI { get; }
        
        public SidePanelUIModel SidePanelUI { get; }
        
        public AuthorMotionToTimelineContext MotionToTimelineContext { get; set; }
        
        public AuthorTextToMotionTakeContext(
            VisualElement root,
            ApplicationModel applicationModel,
            AuthoringModel authoringModel,
            SelectionModel<LibraryItemAsset> itemSelection,
            SelectionModel<EntityID> entitySelectionModel,
            CameraContext cameraContext,
            TextToMotionService textToMotionService,
            TakesUIModel takesUIModel,
            TextToMotionTakeUI textToMotionTakeUI,
            TextToMotionTakeUIModel textToMotionTakeUIModel,
            BakingTaskStatusViewModel bakingTaskStatus,
            BakingNoticeViewModel bakingNotice,
            SidePanelUIModel sidePanelUIModel)
        {
            RootUI = root;
            CameraContext = cameraContext;
            ApplicationModel = applicationModel;
            AuthoringModel = authoringModel;
            ItemSelection = itemSelection;
            TextToMotionService = textToMotionService;
            TakesUI = takesUIModel;
            EntitySelection = entitySelectionModel;
            InputTimeline = new TimelineModel();
            OutputBakedTimeline = new BakedTimelineModel();
            OutputBakedTimelineViewLogic = new BakedTimelineViewLogic("T2M Take", OutputBakedTimeline, EntitySelection);
            TextToMotionTakeUIModel = textToMotionTakeUIModel;
            TextToMotionTakeUI = textToMotionTakeUI;
            SidePanelUI = sidePanelUIModel;
            
            // Text to Motion UI
            UI = new TextToMotionUIModel(Model);
            var textToMotionUI = root.Q<TextToMotionUI>(k_TextToMotionUIName);
            textToMotionUI.SetModel(UI);

            // Playback & UI
            Playback = new PlaybackModel(OutputBakedTimeline.FramesCount / ApplicationConstants.FramesPerSecond, ApplicationConstants.FramesPerSecond);
            PlaybackUI.SetPlaybackModel(Playback);
            
            // Baking Task Status & Notice
            BakingTaskStatusUI = bakingTaskStatus;
            BakingNoticeUI = bakingNotice;
        }
    }
}
