using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used when extracting motion from video.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.AuthorVideoToMotion"/> state.
    /// </remarks>
    class AuthorVideoToMotionTakeContext
    {
        const string k_VideoToMotionPlaybackUIName = "video-to-motion-ui";
        public ApplicationModel ApplicationModel { get; }

        public ApplicationLibraryModel ApplicationLibraryModel => ApplicationModel.ApplicationLibraryModel;
        
        /// <summary>Model of the broader Authoring workflow inside which this context exists.</summary>
        public AuthoringModel AuthoringModel { get; }
        
        /// <summary>Model of the Video To Motion workflow.</summary>
        public VideoToMotionAuthoringModel Model { get; }
        
        public VideoToMotionPlaybackUIModel UIModel { get; }
        
        /// <summary>UI component used to display a spinning circle at the top right of a scene viewport.</summary>
        public BakingTaskStatusViewModel BakingTaskStatusUI { get; }
        
        /// <summary>UI component used to display a text notice at the top center of a scene viewport.</summary>
        public BakingNoticeViewModel BakingNoticeUI { get; }
        
        /// <summary>UI used to display controls for the TextToMotion playback.</summary>
        public BakedTimelinePlaybackUIModel BakedPlaybackUIModel { get; }
        
        public PlaybackModel Playback { get; }
        
        /// <summary>View of the output of the TextToMotion baker.</summary>
        public BakedTimelineViewLogic OutputBakedTimelineViewLogic { get; }

        /// <summary>Baked animation of the final timeline.
        /// The final timeline is the result of the TextToMotion and MotionToKeys decimation.
        /// It is the final output of the whole TextToMotion workflow.</summary>
        public BakedTimelineModel OutputBakedTimeline { get; }
        public CameraMovementModel CameraMovement => CameraContext.CameraMovementModel;
        public CameraContext CameraContext { get; }
        public SelectionModel<EntityID> EntitySelection { get; }
        
        public TakesUIModel TakesUI { get; }

        public AuthorVideoToMotionTakeContext(
            VisualElement root,
            ApplicationModel applicationModel,
            AuthoringModel authoringModel,
            VideoToMotionAuthoringModel model, 
            CameraContext cameraContext,
            SelectionModel<EntityID> entitySelection,
            TakesUIModel takesUIModel,
            BakingTaskStatusViewModel bakingTaskStatus,
            BakingNoticeViewModel bakingNotice)
        {
            ApplicationModel = applicationModel;
            AuthoringModel = authoringModel;
            Model = model;

            BakedPlaybackUIModel = new BakedTimelinePlaybackUIModel();
            Playback = new PlaybackModel(0, ApplicationConstants.FramesPerSecond);
            BakedPlaybackUIModel.SetPlaybackModel(Playback);
            
            UIModel = new VideoToMotionPlaybackUIModel(model, BakedPlaybackUIModel);
            var videoToMotionPlaybackUI = root.Q<VideoToMotionPlaybackUI>();
            videoToMotionPlaybackUI.SetModel(UIModel);

            CameraContext = cameraContext;
            EntitySelection = entitySelection;
            TakesUI = takesUIModel;
            
            OutputBakedTimeline = new BakedTimelineModel();
            OutputBakedTimelineViewLogic = new BakedTimelineViewLogic("V2M Take", OutputBakedTimeline, EntitySelection);
            

            // Baking Task Status & Notice
            BakingTaskStatusUI = bakingTaskStatus;
            BakingNoticeUI = bakingNotice;
        }
    }
}
