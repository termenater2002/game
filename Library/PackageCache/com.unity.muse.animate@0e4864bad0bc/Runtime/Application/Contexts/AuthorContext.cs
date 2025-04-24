using System;
using Unity.DeepPose.Components;
using UnityEngine;
using Unity.DeepPose.Core;
using Unity.Muse.Animate.Toolbar;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Holds all the objects needed when authoring any <see cref="LibraryItemAsset"/>.
    /// </summary>
    /// <remarks>
    /// Used by the <see cref="Application.ApplicationHsm.Author"/> state.
    /// </remarks>
    class AuthorContext
    {
        public CameraMovementView CameraMovementView { get; }
        public StageModel Stage { get; }
        public AuthoringModel Authoring { get; }
        public ApplicationModel ApplicationModel { get; }
        public ApplicationLibraryModel ApplicationLibraryModel => ApplicationModel.ApplicationLibraryModel;
        public LibraryUIModel LibraryUIModel { get; }
        public LibraryUI LibraryUI { get; }
        public TakesUIModel TakesUIModel { get; }
        public TakesUI TakesUI { get; }
        public VideoToMotionUIModel VideoToMotionUIModel { get; }
        public VideoToMotionUI VideoToMotionUI { get; }
        public SelectionModel<LibraryItemAsset> ItemsSelection { get; }
        public ThumbnailsService ThumbnailsService { get; }
        public ClipboardService Clipboard { get; }
        public PoseLibraryService PoseLibrary { get; }
        public CameraModel Camera => CameraContext.CameraModel;
        public CameraContext CameraContext { get; }
        public ApplicationMenuUIModel ApplicationMenuUIModel { get; }
        public VisualElement RootUI { get; }
        public SceneViewUI SceneViewUI { get; }
        public AuthorTextToMotionTakeContext TextToMotionTakeContext { get; }
        public AuthorVideoToMotionTakeContext VideoToMotionTakeContext { get; }
        public AuthorMotionToTimelineContext MotionToTimelineContext { get; }
        public AuthorTimelineContext TimelineContext { get; }
        public TextToMotionService TextToMotionService { get; }
        public VideoToMotionService VideoToMotionService { get; }
        public PoseAuthoringLogic PoseAuthoringLogic { get; }
        public SidePanelUIModel SidePanel { get; }
        public SidePanelUI SidePanelUI { get; }
        public ToolbarsManager Toolbars { get; }
        public Button ReturnToLibraryButton { get; }

        // Shared selections
        // Logic models

        // UI View Models
        SelectionType m_LastSelectionType;

        // Text To Motion
        readonly BakedTimelineViewLogic m_TextToMotionOutputViewLogic;
        readonly BakedTimelineModel m_TextToMotionOutput;
        readonly MotionToTimelineAuthoringModel m_MotionToTimelineAuthoringModel;
        readonly TimelineAuthoringModel m_TimelineAuthoringModel;

        public AuthorContext(
            ApplicationModel applicationModel, 
            StageModel stageModel, 
            CameraContext cameraContext, 
            CameraMovementView cameraMovementView,
            VisualElement rootVisualElement, 
            Button returnToLibraryButton,
            SelectionModel<LibraryItemAsset> itemsSelection,
            ApplicationMenuUIModel applicationMenuUIModel,
            ToolbarsManager toolbarsManager,
            SceneViewUI sceneViewUI,
            InspectorsPanelViewModel inspectorsPanelViewModel, 
            SidePanelUI sidePanelUI, 
            SidePanelUIModel sidePanelUIModel, 
            BakingNoticeViewModel bakingNoticeViewModel, 
            BakingTaskStatusViewModel bakingTaskStatusViewModel, 
            LibraryUI libraryUI,
            LibraryUIModel libraryUIModel,
            TakesUI takesUI,
            TakesUIModel takesUIModel, 
            TextToMotionTakeUI textToMotionTakeUI,
            TextToMotionTakeUIModel textToMotionTakeUIModel,
            VideoToMotionUI videoToMotionUI, 
            VideoToMotionUIModel videoToMotionUIModel,
            PhysicsSolverComponent posingPhysicsSolver, 
            PhysicsSolverComponent bakingPhysicsSolver,
            ThumbnailsService thumbnailsService, 
            ClipboardService clipboardService, 
            PoseLibraryService poseLibraryService,
            TextToMotionService textToMotionService, 
            VideoToMotionService videoToMotionService)
        {
            // Root VisualElement
            RootUI = rootVisualElement;
            ReturnToLibraryButton = returnToLibraryButton;
            
            CameraContext = cameraContext;
            ApplicationModel = applicationModel;
            
            // Selection Models
            var entitySelectionModel = new SelectionModel<EntityID>();
            ItemsSelection = itemsSelection;
            
            // Services
            ThumbnailsService = thumbnailsService;
            Clipboard = clipboardService;
            PoseLibrary = poseLibraryService;
            TextToMotionService = textToMotionService;
            VideoToMotionService = videoToMotionService;

            // Core Models
            Stage = stageModel;
            
            // Authoring Models
            Authoring = new AuthoringModel(this);

            // Pose Authoring Logic
            // Note: PoseAuthoringLogic is in the global Authoring context because
            // MotionToKeysSampling uses PoseAuthoringLogic for pose reconstruction.
            PoseAuthoringLogic = new PoseAuthoringLogic(Authoring, posingPhysicsSolver, entitySelectionModel, cameraContext.CameraModel, rootVisualElement);
            Authoring.PoseAuthoringLogic = PoseAuthoringLogic;
            
            // Timeline Baking (Motion Completion & Physics)
            // Note: This is in AuthorContext because the bakingLogics can only take 1 timeline, bakedTimelineMapping, bakedTimeline
            TimelineBakerBase motionCompletionBaker = ApplicationConstants.UseMotionCloudInference &&
                ApplicationConstants.MotionSynthesisEnabled
                    ? new TimelineBakerCloud(bakingPhysicsSolver)
                    : new TimelineBakerAutoRegressive(bakingPhysicsSolver);

            var timelineBakingLogic = new BakingLogic(Stage.WorkingTimeline, Stage.WorkingBakedTimeline, Stage.WorkingBakedTimelineMapping, motionCompletionBaker);

            // Camera View Interactions
            CameraMovementView = cameraMovementView;

            //----------------------------
            // Toolbars Manager
            Toolbars = toolbarsManager;

            //----------------------------
            // UI Models
            ApplicationMenuUIModel = applicationMenuUIModel;

            // Side Panels
            SidePanelUI = sidePanelUI;
            SidePanel = sidePanelUIModel;

            LibraryUIModel = libraryUIModel;
            LibraryUI = libraryUI;

            // Side Panels - Text to Motion Takes
            TakesUI = takesUI;
            TakesUIModel = takesUIModel;
            
            // Side Panels - Motion to Timeline
            
            // Side Panels - Video To Motion Takes
            VideoToMotionUI = videoToMotionUI;
            VideoToMotionUIModel = videoToMotionUIModel;
            VideoToMotionUIModel.SetAuthoringModel(Authoring.VideoToMotionAuthoringModel);
            
            // Scene View UI Elements
            SceneViewUI = sceneViewUI;
            
            //----------------------------
            // Child Contexts

            TimelineContext = new AuthorTimelineContext(
                Stage,
                ApplicationModel,
                ApplicationLibraryModel,
                Authoring,
                rootVisualElement,
                entitySelectionModel,
                bakingTaskStatusViewModel,
                bakingNoticeViewModel,
                TakesUIModel,
                PoseAuthoringLogic,
                timelineBakingLogic,
                CameraContext,
                ThumbnailsService,
                Clipboard,
                PoseLibrary,
                inspectorsPanelViewModel,
                Toolbars,
                SidePanel
            );

            TextToMotionTakeContext = new AuthorTextToMotionTakeContext(
                rootVisualElement,
                ApplicationModel,
                Authoring,
                ItemsSelection,
                entitySelectionModel,
                CameraContext,
                TextToMotionService,
                TakesUIModel,
                textToMotionTakeUI,
                textToMotionTakeUIModel,
                bakingTaskStatusViewModel,
                bakingNoticeViewModel,
                SidePanel
            );

            VideoToMotionTakeContext = new AuthorVideoToMotionTakeContext(
                rootVisualElement,
                ApplicationModel,
                Authoring,
                Authoring.VideoToMotionAuthoringModel,
                CameraContext,
                entitySelectionModel,
                TakesUIModel,
                bakingTaskStatusViewModel,
                bakingNoticeViewModel
                );

            MotionToTimelineContext = new AuthorMotionToTimelineContext(
                rootVisualElement,
                ApplicationModel,
                Authoring,
                Stage,
                PoseAuthoringLogic,
                bakingPhysicsSolver,
                ItemsSelection,
                entitySelectionModel,
                CameraContext,
                bakingTaskStatusViewModel,
                bakingNoticeViewModel,
                ThumbnailsService,
                Clipboard,
                SidePanel);

            TextToMotionTakeContext.MotionToTimelineContext = MotionToTimelineContext;
        }

        public bool TryGetPoseCopyHumanoidAnimator(out Animator animator)
        {
            animator = null;
            if (TimelineContext.EntitySelection.Count != 1)
                return false;

            var entityID = TimelineContext.EntitySelection.GetSelection(0);

            var armature = GetCurrentViewArmature(entityID);
            if (armature == null)
                return false;

            return armature.gameObject.TryGetHumanoidAnimator(out animator);
        }

        public ArmatureMappingComponent GetCurrentViewArmature(EntityID entityID)
        {
            switch (Authoring.Mode)
            {
                case AuthoringModel.AuthoringMode.Unknown:
                    return null;

                case AuthoringModel.AuthoringMode.Timeline:
                    switch (Authoring.Timeline.Mode)
                    {
                        case TimelineAuthoringModel.AuthoringMode.Unknown:
                            return null;

                        case TimelineAuthoringModel.AuthoringMode.Preview:
                            return TimelineContext.BakedTimelineViewLogic.GetPreviewArmature(entityID);

                        case TimelineAuthoringModel.AuthoringMode.EditKey:
                            if (!TimelineContext.KeySelection.HasSelection)
                                return null;

                            var selectedKey = TimelineContext.KeySelection.GetSelection(0);

                            return selectedKey.Key.Type switch
                            {
                                KeyData.KeyType.Empty => null,
                                KeyData.KeyType.FullPose => PoseAuthoringLogic.GetViewArmature(entityID),
                                KeyData.KeyType.Loop => null,
                                _ => throw new ArgumentOutOfRangeException()
                            };

                        case TimelineAuthoringModel.AuthoringMode.EditTransition:
                            return TimelineContext.TransitionContext.BakedTimelineViewLogic.GetPreviewArmature(entityID);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case AuthoringModel.AuthoringMode.TextToMotionTake:
                    return TextToMotionTakeContext.OutputBakedTimelineViewLogic.GetPreviewArmature(entityID);
                case AuthoringModel.AuthoringMode.VideoToMotionTake:
                    return VideoToMotionTakeContext.OutputBakedTimelineViewLogic.GetPreviewArmature(entityID);

                case AuthoringModel.AuthoringMode.ConvertMotionToTimeline:

                    switch (MotionToTimelineContext.Model.Step)
                    {
                        case MotionToTimelineAuthoringModel.AuthoringStep.None:
                            return null;

                        case MotionToTimelineAuthoringModel.AuthoringStep.NoPreview:
                            return MotionToTimelineContext.InputBakedTimelineViewLogic.GetPreviewArmature(entityID);

                        case MotionToTimelineAuthoringModel.AuthoringStep.PreviewIsAvailable:
                            return MotionToTimelineContext.OutputBakedTimelineViewLogic.GetPreviewArmature(entityID);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        // [Section] Capacities Checks

        internal bool CanPasteKey()
        {
            if (TimelineContext.KeySelection.Count != 1)
                return false;

            var keyModel = TimelineContext.KeySelection.GetSelection(0);
            return TimelineContext.Clipboard.CanPaste(keyModel.Key);
        }

        internal bool CanMoveKey(bool left = false)
        {
            if (TimelineContext.KeySelection.Count != 1)
                return false;

            var keyModel = TimelineContext.KeySelection.GetSelection(0);
            var keyIndex = TimelineContext.Stage.WorkingTimeline.IndexOf(keyModel);
            return left ? keyIndex > 0 : keyIndex < TimelineContext.Stage.WorkingTimeline.KeyCount - 1;
        }

        internal bool CanDeleteKey() => TimelineContext.KeySelection.Count == 1 && TimelineContext.Stage.WorkingTimeline.KeyCount > 1;

        internal bool CanCopyPose()
        {
            return TryGetPoseCopyHumanoidAnimator(out var _);
        }

        internal bool CanPastePose()
        {
            if (TimelineContext.EntitySelection.Count != 1 || TimelineContext.KeySelection.Count != 1)
                return false;

            var selectedEntityID = TimelineContext.EntitySelection.GetSelection(0);
            var selectedKey = TimelineContext.KeySelection.GetSelection(0);
            if (!selectedKey.Key.TryGetKey(selectedEntityID, out var entityKeyModel))
                return false;

            var canPasteKey = TimelineContext.Clipboard.CanPaste(entityKeyModel);
            return canPasteKey;
        }

        
    }
}
