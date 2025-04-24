using Unity.DeepPose.Components;
using Unity.Muse.Animate.Toolbar;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects used by the base application.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.Root"/> state.
    /// </remarks>
    class ApplicationContext
    {
        public Button ReturnToLibraryButton { get; }
        const string k_MenuName = "application-menu";
        const string k_TakesUIElementName = "takes-ui";
        const string k_TaskStatusViewName = "task-status-view";
        const string k_NoticeViewName = "notice-view";
        const string k_SceneViewElementName = "scene-view";
        const string k_ReturnToLibraryButtonName = "top-bar-back-button";
        const string k_TextToMotionTakeUIElementName = "text-to-motion-take-ui";

        public CameraModel Camera => CameraContext.CameraModel;
        public CameraMovementModel CameraMovement => CameraContext.CameraMovementModel;
        public CameraMovementViewModel CameraMovementViewModel { get; }
        public Panel RootVisualElement { get; }
        public ApplicationMenuUIModel ApplicationMenuUIModel { get; }
        internal ApplicationMenuUI ApplicationMenuUI { get; }
        public StageModel Stage { get; }
        public TutorialLogic Tutorial { get; }
        public ThumbnailsService ThumbnailsService { get; }
        public TextToMotionService TextToMotionService { get; }
        public ApplicationLibraryModel ApplicationLibraryModel => ApplicationModel.ApplicationLibraryModel;
        public ApplicationModel ApplicationModel { get; }
        public SelectionModel<LibraryItemAsset> ItemsSelection { get; }
        public AuthorContext AuthorContext { get; }
        public TextToMotionTakeUI TextToMotionTakeUI { get; }
        public TextToMotionTakeUIModel TextToMotionTakeUIModel { get; }
        internal SidePanelUIModel SidePanelUIModel { get; }
        internal SidePanelUI SidePanelUI { get; }
        internal ToolbarsManager Toolbars { get; }
        internal SceneViewUI SceneViewUI { get; }
        internal LibraryUIModel LibraryUIModel { get; }
        internal LibraryUI LibraryUI { get; }

        public TakesUIModel TakesUIModel { get; }
        internal TakesUI TakesUI { get; }
        internal VideoToMotionUIModel VideoToMotionUIModel { get; }
        internal VideoToMotionUI VideoToMotionUI { get; }
        internal BakingTaskStatusView BakingTaskStatusView { get; }
        internal BakingTaskStatusViewModel BakingTaskStatusViewModel { get; }
        internal BakingNoticeView BakingNoticeView { get; }
        internal BakingNoticeViewModel BakingNoticeViewModel { get; }
        internal InspectorsPanelView InspectorsPanelView { get; }
        internal InspectorsPanelViewModel InspectorsPanelViewModel { get; }
        ClipboardService Clipboard{ get; }
        PoseLibraryService PoseLibrary{ get; }
        CameraMovementView CameraMovementView{ get; }
        CameraContext CameraContext{ get; }
        PhysicsSolverComponent PosingPhysicsSolver{ get; }
        PhysicsSolverComponent BakingPhysicsSolver{ get; }
        internal VideoToMotionService VideoToMotionService{ get; }
        CameraModel CameraModel { get; }
        CameraMovementModel CameraMovementModel { get; }

        internal ActorRegistry ActorRegistry { get; }
        internal PropRegistry PropRegistry { get; }

        TimelineBakerBase MotionCompletionBaker { get; }

        public ApplicationContext(
            VisualElement rootVisualElement,
            ApplicationModel applicationModel,
            SelectionModel<LibraryItemAsset> itemsSelection,
            ActorRegistry actorRegistry,
            PropRegistry propRegistry,
            Camera camera,
            Camera thumbnailCamera,
            PhysicsSolverComponent posingPhysicsSolver,
            PhysicsSolverComponent bakingPhysicsSolver)
        {
            // [Solvers]
            
            PosingPhysicsSolver = posingPhysicsSolver;
            BakingPhysicsSolver = bakingPhysicsSolver;

            if (!UnityEngine.Application.isPlaying)
            {
                PosingPhysicsSolver.Initialize();
                BakingPhysicsSolver.Initialize();
            }
            
            // [Registries]
            
            ActorRegistry = actorRegistry;
            PropRegistry = propRegistry;
            
            // [Services]
            
            ThumbnailsService = new ThumbnailsService(thumbnailCamera);
            Clipboard = new ClipboardService();
            PoseLibrary = new PoseLibraryService();
            TextToMotionService = new TextToMotionService();
            VideoToMotionService = new VideoToMotionService();

            // Timeline Baking (Motion Completion & Physics)
            // Note: This is in AuthorContext because the bakingLogics can only take 1 timeline, bakedTimelineMapping, bakedTimeline
            MotionCompletionBaker = ApplicationConstants.UseMotionCloudInference &&
                ApplicationConstants.MotionSynthesisEnabled
                    ? new TimelineBakerCloud(BakingPhysicsSolver)
                    : new TimelineBakerAutoRegressive(BakingPhysicsSolver);

            // [Camera Stuff]
            CameraModel = new CameraModel(camera);
            CameraMovementModel = new CameraMovementModel(CameraModel);
            CameraMovementViewModel = new CameraMovementViewModel(CameraMovementModel);
            var cameraGameObject = camera.gameObject;
            CameraMovementView = cameraGameObject.GetComponent<CameraMovementView>();
            if (CameraMovementView == null)
                CameraMovementView = cameraGameObject.AddComponent<CameraMovementView>();
            CameraMovementView.SetModel(CameraMovementViewModel);
            
            // [Core Models]
            ApplicationModel = applicationModel;
            Stage = new StageModel("Application Stage", "Scene");
            ItemsSelection = itemsSelection;
            
            // [UI Stuff]
            RootVisualElement = rootVisualElement.Q<Panel>();

            // Tutorial
            Tutorial = new TutorialLogic(RootVisualElement);

            // Toolbars Manager
            Toolbars = new ToolbarsManager();

            // Scene View
            SceneViewUI = RootVisualElement.Q<SceneViewUI>(k_SceneViewElementName);

            // Library Button
            ReturnToLibraryButton = rootVisualElement.Q<Button>(k_ReturnToLibraryButtonName);

            // Side Panels
            SidePanelUIModel = new SidePanelUIModel();
            SidePanelUI = RootVisualElement.Q<SidePanelUI>();
            SidePanelUI.hideMenuIfPageShown = true;

            // Library UI
            LibraryUIModel = new LibraryUIModel(ItemsSelection, Clipboard, ThumbnailsService);
            LibraryUI = RootVisualElement.Q<LibraryUI>();

            // Side Panels - Text to Motion Takes (Generation Panel)
            TakesUIModel = new TakesUIModel();
            TakesUIModel.IsVisible = true;

            TakesUI = new TakesUI();
            TakesUI.name = k_TakesUIElementName;

            // Side Panels - Active Text to Motion Take
            TextToMotionTakeUIModel = new TextToMotionTakeUIModel();
            TextToMotionTakeUI = new TextToMotionTakeUI();
            TextToMotionTakeUI.name = k_TextToMotionTakeUIElementName;

            // Side Panels - Video To Motion Takes
            VideoToMotionUIModel = new VideoToMotionUIModel();
            VideoToMotionUI = TakesUI.Q<VideoToMotionUI>();

            // Inspectors UI
            InspectorsPanelViewModel = new InspectorsPanelViewModel();
            InspectorsPanelView = RootVisualElement.Q<InspectorsPanelView>();

            // Task Status UI
            BakingTaskStatusViewModel = new BakingTaskStatusViewModel();
            BakingTaskStatusView = RootVisualElement.Q<BakingTaskStatusView>(k_TaskStatusViewName);

            // Notice UI
            BakingNoticeViewModel = new BakingNoticeViewModel();
            BakingNoticeView = RootVisualElement.Q<BakingNoticeView>(k_NoticeViewName);

            // Application Menu
            ApplicationMenuUIModel = new ApplicationMenuUIModel();
            ApplicationMenuUI = RootVisualElement.Q<ApplicationMenuUI>(k_MenuName);
            
            // Child Contexts
            CameraContext = new CameraContext(CameraModel, CameraMovementModel);
            AuthorContext = new AuthorContext(
                ApplicationModel,
                Stage,
                CameraContext,
                CameraMovementView,
                RootVisualElement,
                ReturnToLibraryButton,
                ItemsSelection,
                ApplicationMenuUIModel,
                Toolbars,
                SceneViewUI,
                InspectorsPanelViewModel,
                SidePanelUI,
                SidePanelUIModel,
                BakingNoticeViewModel,
                BakingTaskStatusViewModel,
                LibraryUI,
                LibraryUIModel,
                TakesUI,
                TakesUIModel,
                TextToMotionTakeUI,
                TextToMotionTakeUIModel,
                VideoToMotionUI,
                VideoToMotionUIModel,
                PosingPhysicsSolver,
                BakingPhysicsSolver,
                ThumbnailsService,
                Clipboard,
                PoseLibrary,
                TextToMotionService,
                VideoToMotionService
            );
        }
    }
}
