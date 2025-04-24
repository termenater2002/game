using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif
using Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        
        /// <summary>
        /// Hierarchical State Machine of the Application Workflow/UX.
        /// </summary>
        // Note: Without setting this to internal, the MuseChat API has compiler errors.
        internal partial class ApplicationHsm
        {
            public abstract class ApplicationState<T> : State
            {
                protected T Context => m_Context;

                T m_Context;

                public override void OnEnter()
                {
                    base.OnEnter();
                    throw new Exception("States must be passed a context");
                }

                public override void OnEnter(object[] aArgs)
                {
                    base.OnEnter(aArgs);

                    Assert.IsTrue(aArgs != null && aArgs.Length == 1, "Invalid state args");
                    m_Context = (T)aArgs[0];
                }

                internal void Log(string msg)
                {
                    DevLogger.LogSeverity(TraceLevel.Info, GetType().Name + " -> " + msg);
                }

                internal void LogError(string msg)
                {
                    DevLogger.LogSeverity(TraceLevel.Error, GetType().Name + " -> " + msg);
                }
            }

            public class Root : StateWithOwner<Application>, IKeyDownHandler
            {
                ApplicationContext Context { get; set; }
                AuthoringModel AuthoringModel => Context.AuthorContext.Authoring;
                ApplicationLibraryModel ApplicationLibraryModel => Context.ApplicationLibraryModel;
                static KeyCode[] s_AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
                bool m_WasAnyKey;
                Transition m_NextTransition;
                MenuItemModel[] m_AppMenuItems;
                bool m_IsLoadingAsset;

                public override Transition GetTransition()
                {
                    if (m_IsLoadingAsset)
                    {
                        if (Instance.IsAuthoringAssetLoaded)
                        {
                            m_IsLoadingAsset = false;
                            m_NextTransition = Transition.Inner<Author>(Context.AuthorContext, Context.ApplicationLibraryModel.ActiveLibraryItem);
                        }
                    }
                    else
                    {
                        if (!Instance.IsAuthoringAssetLoaded)
                        {
                            m_NextTransition = Transition.Inner<BrowseAssets>(Context);
                        }
                    }

                    return m_NextTransition;
                }

                public override void OnEnter()
                {
                    // Declare the session as unloaded
                    // Note: This is a bit hacky, but it allows the automatic
                    // saving to know if the session is actually loaded or not
                    Instance.IsAuthoringAssetLoaded = false;
                    Context = Owner.ApplicationContext;

                    // Initialize the tutorial system
                    TutorialTracks.Init(Context.Tutorial);

                    // Initialize the User Actions Manager
                    UserActionsManager.Instance.Initialize(AuthoringModel, Context.ApplicationLibraryModel);

                    m_NextTransition = Transition.None();

                    var uiApplication = Context.RootVisualElement.Q<Panel>();
                    uiApplication.scale = "medium";

                    if (EventSystem.current == null)
                    {
                        DevLogger.LogSeverity(TraceLevel.Verbose, "EventSystem.current = null");
                    }
                    else
                    {
                        DevLogger.LogSeverity(TraceLevel.Verbose, "EventSystem.current = " + EventSystem.current.name);
                    }

                    BindModels();
                    SetupStage();
                    SetupAppMenu();
                    SetupSceneView();
                    StartTutorialIfUserHasNotSeenIt();

                    Instance.SubscribeToMessage<LoadLibraryItemAssetMessage>(LoadLibraryItemAsset);
                    Instance.PublishMessage<ApplicationStartedMessage>();

                }

                void BindModels()
                {
                    // Subscribe to TextToMotionService
                    Context.TextToMotionService.OnRequestFailed += OnTextToMotionServiceRequestFailed;
                    Context.TextToMotionService.OnRequestProgressed += OnTextToMotionServiceRequestProgressed;
                    Context.TextToMotionService.OnStateChanged += OnTextToMotionServiceStateChanged;
                    
                    // Handle 3D Viewport Background Click
                    Context.CameraMovementViewModel.OnClickedWithoutDragging += OnBackgroundClicked;
                    
                    // Bind the ApplicationContext to the ApplicationModel
                    Context.ApplicationModel.SetContext(Context);

                    // Bind the Takes UI with the Application
                    Context.TakesUIModel.SetContext(Context);
            
                    // Bind the UI components with their models
                    Context.SceneViewUI.SetContext(Context.AuthorContext);
                    Context.SidePanelUI.SetModel(Context.SidePanelUIModel);
                    Context.LibraryUI.SetModel(Context.LibraryUIModel);
                    Context.TakesUI.SetModel(Context.TakesUIModel);
                    Context.TextToMotionTakeUI.SetModel(Context.TextToMotionTakeUIModel);
                    Context.VideoToMotionUI.SetModel(Context.VideoToMotionUIModel);
                    Context.InspectorsPanelView.SetModel(Context.InspectorsPanelViewModel);
                    Context.BakingTaskStatusView.SetModel(Context.BakingTaskStatusViewModel);
                    Context.BakingNoticeView.SetModel(Context.BakingNoticeViewModel);
                    Context.ApplicationMenuUI?.SetModel(Context.ApplicationMenuUIModel);
            
                    // Bind the stage last
                    Context.Stage.SetContext(Context);
                }
                
                void UnbindModels()
                {
                    // Subscribe to TextToMotionService
                    Context.TextToMotionService.OnRequestFailed -= OnTextToMotionServiceRequestFailed;
                    Context.TextToMotionService.OnRequestProgressed -= OnTextToMotionServiceRequestProgressed;
                    Context.TextToMotionService.OnStateChanged -= OnTextToMotionServiceStateChanged;
                    
                    // Handle 3D Viewport Background Click
                    Context.CameraMovementViewModel.OnClickedWithoutDragging -= OnBackgroundClicked;

                    
                    // Bind the ApplicationContext to the ApplicationModel
                    Context.ApplicationModel.SetContext(null);

                    // Bind the Takes UI with the Application
                    Context.TakesUIModel.SetContext(null);
            
                    // Bind the UI components with their models
                    Context.SceneViewUI.SetContext(null);
                    Context.SidePanelUI.SetModel(null);
                    Context.LibraryUI.SetModel(null);
                    Context.TakesUI.SetModel(null);
                    Context.TextToMotionTakeUI.SetModel(null);
                    Context.VideoToMotionUI.SetModel(null);
                    Context.InspectorsPanelView.SetModel(null);
                    Context.BakingTaskStatusView.SetModel(null);
                    Context.BakingNoticeView.SetModel(null);
                    Context.ApplicationMenuUI?.SetModel(null);
            
                    // Bind the stage last
                    Context.Stage.SetContext(null);
                }
                
                public override void OnExit()
                {
                    Context.Stage.Unload();
                    Context.ApplicationMenuUIModel.RemoveItems(m_AppMenuItems);
                    UnbindModels();
                    Instance.UnsubscribeFromMessage<LoadLibraryItemAssetMessage>(LoadLibraryItemAsset);
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnKeyDown(KeyCode: {eventData.KeyCode})");

                    if (Context.TakesUIModel.IsWriting)
                    {
                        eventData.Use();
                    }
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);

                    UpdateKeyPresses();
                    UpdateCamera(aDeltaTime);
                }

                public override bool UpdateBaking(float delta)
                {
                    if (base.UpdateBaking(delta))
                        return true;

                    Context.TextToMotionService.Update(delta, false);
                    Context.VideoToMotionService.Update(delta, false);

                    // Do not bake timeline if the posing solver is still solving a pose
                    // Note: This is necessary because the posing is done over multiple updates/frames.
                    if (Context.AuthorContext.PoseAuthoringLogic.IsSolving)
                        return true;

                    // Do not bake when holding / dragging mouse (For a smoother experience)
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
                        //new system
                        if (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed)
                        return true;
#else

                    //old system
                    if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                        return true;
#endif

                    // Note: I still do baking of the timeline at this level because
                    // I think it is used outside of the timeline editing context
                    if (Context.AuthorContext.TimelineContext.TimelineBakingLogic.NeedToUpdate)
                    {
                        Context.AuthorContext.TimelineContext.TimelineBakingLogic.Update(delta, false);
                    }

                    if (Context.ThumbnailsService.HasRequests)
                    {
                        Context.ThumbnailsService.Update(delta);
                    }

                    return false;
                }

                void LoadLibraryItemAsset(LoadLibraryItemAssetMessage message)
                {
                    DevLogger.LogInfo("LoadLibraryItemAsset("+message+")");

                    // Declare the library item asset as unloaded
                    // Note: This is a bit hacky, but it allows the automatic
                    // saving to know if the asset is actually loaded or not
                    Instance.IsAuthoringAssetLoaded = false;
                    m_IsLoadingAsset = true;
                    m_NextTransition = Transition.Inner<LoadAsset>(Context, message.Path);
                }

                void SetupStage()
                {
                    Context.Stage.Unload();
                    Context.Stage.LoadDefaultScene();
                }

                void SetupAppMenu()
                {
                    m_AppMenuItems = new MenuItemModel[]
                    {
                        new("Save Asset", SaveActiveAsset),
                        new("Pose",
                            action: null,
                            isEnabledCallback: () => AuthoringModel.Timeline.IsEditingKey(),
                            sectionName: "--Keyframe Authoring",
                            index: 0),
                        new("Pose/Copy",
                            action: () => AuthoringModel.Timeline.AskCopyPose(),
                            shortcutText: PlatformUtils.GetCommandLabel("C"),
                            isEnabledCallback:() => AuthoringModel.Timeline.CanCopyPose),
                        new("Pose/Paste",
                            action: () => AuthoringModel.Timeline.AskPastePose(),
                            shortcutText: PlatformUtils.GetCommandLabel("V"),
                            isEnabledCallback: () => AuthoringModel.Timeline.CanPastePose()),
                        new("Pose/Reset",
                            action: () => AuthoringModel.Timeline.AskResetPose(),
                            shortcutText: PlatformUtils.GetCommandLabel("R"),
                            isEnabledCallback:() => AuthoringModel.Timeline.CanCopyPose),
                        new("Key",
                            action: null,
                            isEnabledCallback: () => AuthoringModel.Timeline.IsEditingKey(),
                            sectionName: "--Keyframe Authoring",
                            index: 1),
                        new("Key/Move Left",
                            action: () => AuthoringModel.Timeline.AskMoveSelectedKey(true),
                            isEnabledCallback: () => AuthoringModel.Timeline.CanMoveKey(true),
                            sectionName: "--Move"),
                        new("Key/Move Right",
                            action: () => AuthoringModel.Timeline.AskMoveSelectedKey(false),
                            isEnabledCallback: () => AuthoringModel.Timeline.CanMoveKey(),
                            sectionName: "--Move"),
                        new("Key/Copy",
                            action: () => AuthoringModel.Timeline.AskCopyKey(),
                            shortcutText: PlatformUtils.GetCommandLabel("C"),
                            sectionName: "--CopyPaste"),
                        new("Key/Paste (Replace)",
                            action: () => AuthoringModel.Timeline.AskPasteReplaceKey(),
                            shortcutText: PlatformUtils.GetCommandLabel("V"),
                            isEnabledCallback: AuthoringModel.Timeline.CanPasteKey,
                            sectionName: "--CopyPaste"),
                        new("Key/Paste Left",
                            action: () => AuthoringModel.Timeline.AskPasteKey(true),
                            isEnabledCallback: AuthoringModel.Timeline.CanPasteKey,
                            sectionName: "--CopyPaste"),
                        new("Key/Paste Right",
                            action: () => AuthoringModel.Timeline.AskPasteKey(false),
                            isEnabledCallback: AuthoringModel.Timeline.CanPasteKey,
                            sectionName: "--CopyPaste"),
                        new("Key/Duplicate Left",
                            action: () => AuthoringModel.Timeline.AskDuplicateSelectedKey(true),
                            sectionName: "--Duplicate"),
                        new("Key/Duplicate Right",
                            action: () => AuthoringModel.Timeline.AskDuplicateSelectedKey(false),
                            shortcutText: PlatformUtils.GetCommandLabel("D"),
                            sectionName: "--Duplicate"),
                        new("Key/Delete",
                            action: () => AuthoringModel.Timeline.AskDeleteSelectedKey(),
                            shortcutText: "Delete",
                            isEnabledCallback: AuthoringModel.Timeline.CanDeleteKey,
                            sectionName: "--Delete")
                    };

                    Context.ApplicationMenuUIModel.AddItems(m_AppMenuItems);
                }

                void SaveActiveAsset()
                {
                    var activeItem = ApplicationLibraryModel.ActiveLibraryItem;

                    if (activeItem != null)
                    {
                        Log($"ActivateLibraryItemAssetFromPath() -> Saving previous active item...");
                        Instance.PublishMessage(new SaveLibraryItemAssetMessage(activeItem));
                    }
                }

                void SetupSceneView()
                {
                    Context.SceneViewUI.SetContext(Context.AuthorContext);
                    Context.Toolbars.SetContainer(Context.SceneViewUI.Header.ToolbarsContainer);

                    // Tools Toolbar
                    var tools = Context.Toolbars.AddToolbar("tools");
                    tools.IsVisibleCallback = AuthoringModel.Timeline.IsEditingKey;

                    // Tools - Pose Edit Modes
                    var posingSection = tools.AddSection("pose-edit-modes", SelectionType.Single);
                    posingSection.IsVisibleCallback = AuthoringModel.Timeline.IsEditingKeyPose;

                    var drag = posingSection.AddButton("drag", "recorder", "", "Drag (Q)");
                    drag.IsActiveCallback = () => AuthoringModel.PosingTool == AuthoringModel.PosingToolType.Drag;
                    drag.ClickedCallback = () => AuthoringModel.PosingTool = AuthoringModel.PosingToolType.Drag;

                    var translate = posingSection.AddButton("translate", "arrows-out-cardinal", "", "Translate (W)");
                    translate.IsActiveCallback = () => AuthoringModel.PosingTool == AuthoringModel.PosingToolType.Translate;
                    translate.ClickedCallback = () => AuthoringModel.PosingTool = AuthoringModel.PosingToolType.Translate;

                    var rotate = posingSection.AddButton("rotate", "arrows-clockwise", "", "Rotate (E)");
                    rotate.IsActiveCallback = () => AuthoringModel.PosingTool == AuthoringModel.PosingToolType.Rotate;
                    rotate.ClickedCallback = () => AuthoringModel.PosingTool = AuthoringModel.PosingToolType.Rotate;

                    var tolerance = posingSection.AddButton("tolerance", "target", "", "Adjust Tolerance (R)");
                    tolerance.IsActiveCallback = () => AuthoringModel.PosingTool == AuthoringModel.PosingToolType.Tolerance;
                    tolerance.ClickedCallback = () => AuthoringModel.PosingTool = AuthoringModel.PosingToolType.Tolerance;

                    // Tools - Selected Effectors
                    var selectedEffectorsSection = tools.AddSection("selected-effectors");
                    selectedEffectorsSection.IsVisibleCallback = AuthoringModel.Timeline.IsEditingKeyPose;

                    var disableEffectors = selectedEffectorsSection.AddButton("disable-effectors", "deep-pose-disable-effector", "", "Disable selected effectors");
                    disableEffectors.IsEnabledCallback = () => AuthoringModel.Timeline.CanDisableSelectedEffectors;
                    disableEffectors.ClickedCallback = () => AuthoringModel.Timeline.RequestDisableSelectedEffectors();

                    // Tools - Loop Edit Modes
                    var loopSection = tools.AddSection("loop-edit-modes", SelectionType.Single);
                    loopSection.IsVisibleCallback = AuthoringModel.Timeline.IsEditingKeyLoop;

                    translate = loopSection.AddButton("translate", "arrows-out-cardinal", "", "Translate (W)");
                    translate.IsActiveCallback = () => AuthoringModel.LoopTool == AuthoringModel.LoopToolType.Translate;
                    translate.ClickedCallback = () => AuthoringModel.LoopTool = AuthoringModel.LoopToolType.Translate;

                    rotate = loopSection.AddButton("rotate", "arrows-clockwise", "", "Rotate (E)");
                    rotate.IsActiveCallback = () => AuthoringModel.LoopTool == AuthoringModel.LoopToolType.Rotate;
                    rotate.ClickedCallback = () => AuthoringModel.LoopTool = AuthoringModel.LoopToolType.Rotate;

                    // Tutorials
                    var tutorials = Context.Toolbars.AddToolbar("tutorials");
                    tutorials.IsVisibleCallback = () => AuthoringModel.Mode == AuthoringModel.AuthoringMode.Timeline;

                    var mainTutorialSection = tutorials.AddSection("main");
                    var mainTutorial = mainTutorialSection.AddButton("tutorial", "learn", "", "Tutorial");
                    mainTutorial.ClickedCallback = TutorialTracks.StartAnimationTutorial;

                    // Timeline - Export
                    var timeline = Context.Toolbars.AddToolbar("timeline");
                    timeline.IsVisibleCallback = () => AuthoringModel.Mode == AuthoringModel.AuthoringMode.Timeline;
                    var mainTimelineSection = timeline.AddSection("main");
                    var exportTimeline = mainTimelineSection.AddButton("export", "export", "", "Export");
                    exportTimeline.ClickedCallback = () => Context.ApplicationLibraryModel.RequestExportLibraryItem(Context.ApplicationLibraryModel.ActiveLibraryItem);
                }
                void UpdateKeyPresses()
                {
                    // Disabling this if we receive the inputs from the editor
#if UNITY_EDITOR
#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM_PRESENT
                    //new input system
                    if (
                        !Keyboard.current.anyKey.wasPressedThisFrame
                        && !Keyboard.current.anyKey.isPressed
                        && !Keyboard.current.anyKey.wasReleasedThisFrame
                        && !m_WasAnyKey
                    )
                        return;

                    var allKeys = Keyboard.current.allKeys;

                    using var tmpDownKeysList = TempList<KeyControl>.Allocate();
                    using var tmpPressedKeysList = TempList<KeyControl>.Allocate();
                    using var tmpUpKeysList = TempList<KeyControl>.Allocate();

                    foreach (var key in allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            tmpDownKeysList.Add(key);
                        }
                        if (key.isPressed)
                        {
                            tmpPressedKeysList.Add(key);
                        }
                        if (key.wasReleasedThisFrame)
                        {
                            tmpUpKeysList.Add(key);
                        }
                    }

                    foreach (var key in tmpDownKeysList.List)
                    {
                        KeyPressEvent.Pool.Get(out var ev);
                        ev.KeyCode = KeyMapping.KeyToKeyCode(key.keyCode);
                        StateMachine.SendKeyDownEvent(ev);
                        KeyPressEvent.Pool.Release(ev);
                    }
                    foreach (var key in tmpPressedKeysList.List)
                    {
                        KeyPressEvent.Pool.Get(out var ev);
                        ev.KeyCode = KeyMapping.KeyToKeyCode(key.keyCode);
                        StateMachine.SendKeyHoldEvent(ev);
                        KeyPressEvent.Pool.Release(ev);
                    }
                    foreach (var key in tmpUpKeysList.List)
                    {
                        KeyPressEvent.Pool.Get(out var ev);
                        ev.KeyCode = KeyMapping.KeyToKeyCode(key.keyCode);
                        StateMachine.SendKeyUpEvent(ev);
                        KeyPressEvent.Pool.Release(ev);
                    }

                    m_WasAnyKey = Keyboard.current.anyKey.isPressed;
#else
                    //old input system

                    // Note: this is far from the ideal solution but this is what we have for now...
                    if (!Input.anyKey && !Input.anyKeyDown && !m_WasAnyKey)
                        return;

                    for (var i = 0; i < s_AllKeyCodes.Length; i++)
                    {
                        var keyCode = s_AllKeyCodes[i];
                        if (keyCode == KeyCode.None)
                            continue;

                        if (Input.GetKeyDown(keyCode))
                        {
                            KeyPressEvent.Pool.Get(out var ev);
                            ev.KeyCode = keyCode;
                            StateMachine.SendKeyDownEvent(ev);
                            KeyPressEvent.Pool.Release(ev);
                        }
                        else if (Input.GetKey(keyCode))
                        {
                            KeyPressEvent.Pool.Get(out var ev);
                            ev.KeyCode = keyCode;
                            StateMachine.SendKeyHoldEvent(ev);
                            KeyPressEvent.Pool.Release(ev);
                        }
                        else if (Input.GetKeyUp(keyCode))
                        {
                            KeyPressEvent.Pool.Get(out var ev);
                            ev.KeyCode = keyCode;
                            StateMachine.SendKeyUpEvent(ev);
                            KeyPressEvent.Pool.Release(ev);
                        }
                    }

                    m_WasAnyKey = Input.anyKey || Input.anyKeyDown;
#endif
#endif
                }

                void UpdateCamera(float deltaTime)
                {
                    // Mouse wheel zoom
                    Context.CameraMovement.Update(deltaTime);
                    SaveCameraViewpoint();
                }

                void OnBackgroundClicked(CameraMovementViewModel model, PointerEventData eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents)
                        Log("OnBackgroundClicked()");

                    StateMachine.SendPointerClickEvent(eventData);
                }

                // [Section] TextToMotionService Events Handlers

                void OnTextToMotionServiceStateChanged(TextToMotionService.Status state)
                {
                    // DevLogger.LogInfo($"Application.Hsm -> OnTextToMotionServiceStateChanged({state})");
                }

                void OnTextToMotionServiceRequestFailed(TextToMotionRequest request, string error)
                {
                    // DevLogger.LogInfo($"Application.Hsm -> OnTextToMotionServiceRequestFailed({error})");

                    // Remove incomplete take from the library
                    // TODO: This logic will need to change if we are able to regenerate previous takes
                    if (!LibraryRegistry.TryGetOwnerOf(request.Target, out var itemToRemove))
                    {
                        DevLogger.LogError($"Application.Hsm -> OnTextToMotionServiceRequestFailed(): Could not find asset owning {request.Target}");
                    }

                    LibraryRegistry.Delete(itemToRemove);
                }

                void OnTextToMotionServiceRequestProgressed(TextToMotionRequest request, float progress)
                {
                    //DevLogger.LogInfo($"Application.Hsm -> OnTextToMotionServiceRequestProgressed({progress})");
                }
                
                
                void SaveCameraViewpoint()
                {
                    var cameraViewpoint = Context.Stage.NumCameraViewpoints == 0
                        ? Context.Stage.AddCameraViewpoint()
                        : Context.Stage.GetCameraViewpoint(0);

                    cameraViewpoint.SetCoordinates(Context.CameraMovement.Pivot, Context.Camera.Position);
                }

                void StartTutorialIfUserHasNotSeenIt()
                {
                    var hasUserSeenTutorial
                        = PlayerPrefs.GetInt(ApplicationConstants.TutorialSeenPlayerPrefsKey, defaultValue: 0) != 0;

                    if (hasUserSeenTutorial && !ApplicationConstants.AlwaysShowTutorial) return;

                    TutorialTracks.StartPromptingTutorial();

                    PlayerPrefs.SetInt(ApplicationConstants.TutorialSeenPlayerPrefsKey, 1);
                }
                
                // [Section] Debugging
                
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [Conditional("UNITY_MUSE_DEV")]
                void Log(string msg)
                {
                    DevLogger.LogSeverity(TraceLevel.Info, GetType().Name + " -> " + msg);
                }
                
                

                public void RequestTextToMotionGenerate(string prompt, int? seed, int numberOfGenerations, float length, ITimelineBakerTextToMotion.Model model)
                {
                    Context.ApplicationModel.RequestTextToMotionGenerate(prompt, seed, numberOfGenerations, length, model);
                }
            }
        }
    }
}
