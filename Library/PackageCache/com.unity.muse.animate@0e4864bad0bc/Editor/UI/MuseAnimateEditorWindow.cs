using System;
using System.Collections;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.AppUI.Core;
using Unity.Muse.AppUI.UI;
using Unity.EditorCoroutines.Editor;
using Unity.Muse.Animate.Fbx;
using Unity.Muse.Common;
using Unity.Muse.Common.Account;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using AnimationMode = Unity.Muse.AppUI.UI.AnimationMode;
using DragAndDrop = UnityEditor.DragAndDrop;
using Message = UnityEditor.VersionControl.Message;

namespace Unity.Muse.Animate.Editor
{
    class MuseAnimateEditorWindow : EditorWindow, ICoroutineRunner
    {
        public event Action AuthoringStarted;
        public event Action AuthoringEnded;

        [SerializeField]
        Application m_ApplicationPrefab;

        [SerializeField]
        VisualTreeAsset m_ApplicationUxml;

        /// <summary>
        /// Remembers the current session so we can restore it if there's a domain reload.
        /// </summary>
        /// <remarks>
        /// Why are we saving the path instead of the object reference? Because the object reference doesn't
        /// seem to contain valid data when we restart Unity (maybe a Unity bug?).
        /// </remarks>
        [SerializeField, HideInInspector]
        string m_ActiveLibraryItemAssetPath;

        public Application Application => m_Application;

        [SerializeField]
        ThemeStyleSheet m_ApplicationTheme;

        [SerializeField]
        UITemplatesRegistry m_UITemplatesRegistry;

        const string k_WindowName = "Muse Animate Generator";

        Application m_Application;
        bool m_OwnsApplication;
        VisualElement m_RootVisualElement;
        VisualElement m_Placeholder;
        bool m_IsUsable;
        Scene m_Scene;

        public static MuseAnimateEditorWindow Window { get; set; }

        [UnityEditor.MenuItem("Muse/New Animate Generator", false, 100)]
        public static void ShowWindow()
        {
            if (Window)
            {
                Window.RestartApplication();
            }
            else
            {
                Window = GetWindow<MuseAnimateEditorWindow>(k_WindowName);
            }
        }

        public static bool IsWindowShown => Window;

#if UNITY_MUSE_DEV
        [UnityEditor.MenuItem("internal:Muse/Internals/Deprecate Muse Animate")]
        public static void DeprecateMuseAnimate()
        {
            ClientStatus.Instance.Status = new ClientStatusResponse
            {
                obsolete_date = DateTime.Today.Subtract(TimeSpan.FromDays(1)).ToString("O"),
                status = "Deprecated"
            };
        }

        [UnityEditor.MenuItem("internal:Muse/Internals/Undeprecate Muse Animate")]
        public static void UndeprecateMuseAnimate()
        {
            ClientStatus.Instance.Status = new ClientStatusResponse
            {
                obsolete_date = DateTime.Today.AddDays(1).ToString("O"),
                status = ""
            };
        }
#endif

        void OnClientStatusChanged(ClientStatusResponse status)
        {
            // Log($"OnClientStatusChanged() -> Status: " + status.status);

            if (!ClientStatus.Instance.IsClientUsable)
            {
                ShowUpdatePackageUI();
            }
            else if (m_IsUsable)
            {
                // TODO: Rework this crappy logic
                HidePlaceHolder();
            }
        }

        void OnGUI()
        {
            Event e = Event.current;

            if (e.isKey)
            {
                if (e.keyCode != KeyCode.None)
                {
                    if (e.type == EventType.KeyDown)
                    {
                        InputUtils.KeyDown(e.keyCode);
                    }
                    else if (e.type == EventType.KeyUp)
                    {
                        InputUtils.KeyUp(e.keyCode);
                    }
                }
            }
            else
            {
                // WZ HACK: We should stop using OnGUI to handle modifier keys.
                if (e.alt)
                    InputUtils.AltKeyDown();
                else
                    InputUtils.AltKeyUp();

                if (e.control)
                    InputUtils.ControlKeyDown();
                else
                    InputUtils.ControlKeyUp();

                if (e.shift)
                    InputUtils.ShiftKeyDown();
                else
                    InputUtils.ShiftKeyUp();
            }
            m_Application?.Render();
        }

        void CreateGUI()
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI()");
            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Unsubscribing from ClientStatus...");
            ClientStatus.Instance.OnClientStatusChanged -= OnClientStatusChanged;
            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Subscribing to ClientStatus...");
            ClientStatus.Instance.OnClientStatusChanged += OnClientStatusChanged;
            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Updating ClientStatus...");
            ClientStatus.Instance.UpdateStatus();

            // Inject the template registry for Edit Mode (this needs to be done before
            // we can create the components in the visual tree.)
            if (m_UITemplatesRegistry == null)
            {
                DevLogger.LogError("CreateGUI() -> UITemplatesRegistry asset not found");
                return;
            }

            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Providing m_UITemplatesRegistry...");
            Locator.Provide(m_UITemplatesRegistry);
            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Providing ICoroutineRunner...");
            Locator.Provide<ICoroutineRunner>(this);

            if (m_Application != null)
            {
                DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> m_Application is not null, shutting it down....");
                m_Application.Shutdown();
            }

            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Loading application...");
            m_Application = LoadApplication();

            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Creating Root VisualElement...");
            ClearRootVisualElement(m_RootVisualElement);
            m_RootVisualElement = CreateRootVisualElement();

            DevLogger.LogSeverity(TraceLevel.Verbose, "CreateGUI() -> Initializing Application...");
            StartApplication();

            // If we are coming from a domain reload, we need to load the current session again.
            if (!string.IsNullOrEmpty(m_ActiveLibraryItemAssetPath))
            {
                DevLogger.LogSeverity(TraceLevel.Verbose, $"CreateGUI() -> Loading previous active library item from: {m_ActiveLibraryItemAssetPath}");
            }

            DevLogger.LogSeverity(TraceLevel.Verbose, $"CreateGUI() -> Registering to account controller...");
            AccountController.Register(this);
            Log($"CreateGUI() -> Done");
        }

        VisualElement CreateRootVisualElement()
        {
            Log("CreateRootVisualElement()");

            m_ApplicationUxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(m_ApplicationTheme);
            return rootVisualElement;
        }

        static void ClearRootVisualElement(VisualElement rootVisualElement)
        {
            Log($"ClearRootVisualElement({rootVisualElement})");

            if (rootVisualElement == null) return;

            foreach (var element in rootVisualElement.Children())
            {
                element.RemoveFromHierarchy();
            }
        }

        void ShowPlaceholder()
        {
            Log("ShowPlaceholder()");

            if (m_Placeholder == null)
            {
                m_Placeholder = new VisualElement
                {
                    style =
                    {
                        position = Position.Absolute,
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0,
                        backgroundColor = new Color(0, 0, 0, 1.0f),
                        display = DisplayStyle.Flex
                    }
                };

                var preloader = new Preloader
                {
                    style =
                    {
                        width = new Length(100, LengthUnit.Percent),
                        height = new Length(100, LengthUnit.Percent)
                    }
                };

                m_Placeholder.Add(preloader);
                rootVisualElement.Add(m_Placeholder);
            }

            m_Placeholder.style.display = DisplayStyle.Flex;
            m_Placeholder.BringToFront();
        }

        void HidePlaceHolder()
        {
            Log("HidePlaceHolder()");

            if (m_Placeholder == null)
                return;
            m_Placeholder.style.display = DisplayStyle.None;
            m_Placeholder.SendToBack();
        }

        void ShowUpdatePackageUI()
        {
            Log("ShowUpdatePackageUI()");

            if (m_Placeholder != null)
            {
                m_Placeholder.style.display = DisplayStyle.Flex;
                m_Placeholder.BringToFront();

                var preloader = m_Placeholder.Q<Preloader>();
                preloader.style.display = DisplayStyle.None;

                var textGroup = new VisualElement
                {
                    name = "muse-node-disable-message-group",
                    style =
                    {
                        width = new Length(100, LengthUnit.Percent),
                        height = new Length(100, LengthUnit.Percent)
                    }
                };
                textGroup.Add(new Text
                {
                    text = TextContent.clientStatusUpdateMessage, enableRichText = true,
                    style =
                    {
                        width = new Length(100, LengthUnit.Percent),
                        height = new Length(100, LengthUnit.Percent),
                        unityTextAlign = TextAnchor.MiddleCenter
                    }
                });
                textGroup.AddToClassList("muse-node-message-link");
                textGroup.RegisterCallback<PointerUpEvent>(_ => UpdateAnimatePackage());
                m_Placeholder.Add(textGroup);
            }
        }

        static void UpdateAnimatePackage()
        {
#if UNITY_EDITOR
            UnityEditor.PackageManager.UI.Window.Open("com.unity.muse.animate");
#endif
        }

        void OnAuthoringStarted(AuthoringStartedMessage message)
        {
            Log($"OnAuthoringStarted({message})");
            m_IsUsable = true;

            // TODO: Rework this crappy logic
            if (ClientStatus.Instance.IsClientUsable)
                HidePlaceHolder();

            AuthoringStarted?.Invoke();
        }

        void OnAuthoringEnded(AuthoringEndedMessage message)
        {
            // Declare the session as unloaded
            // Note: This is a bit hacky, but it allows the automatic
            // saving to know if the session is actually loaded or not
            Application.IsAuthoringAssetLoaded = false;

            AuthoringEnded?.Invoke();
        }

        Application LoadApplication()
        {
            minSize = new Vector2(600, 240);

            Log("LoadApplication()");

#if UNITY_2023_1_OR_NEWER
            var application = FindFirstObjectByType<Application>();
#else
            var application = FindObjectOfType<Application>();
#endif
            if ((m_Application = application) == null)
            {
                var appPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(m_ApplicationPrefab);
                PrefabUtility.LoadPrefabContentsIntoPreviewScene(appPrefabPath, m_Scene);

                // The application is the first root object in the scene
                m_Application = m_Scene.GetRootGameObjects()[0].GetComponent<Application>();
                m_Application.BindToScene(m_Scene);

                Locator.Provide<IRootObjectSpawner<GameObject>>(new RuntimeRootObjectSpawner(m_Application.transform));
                SetupVideoServices();

                m_Application.SubscribeToMessage<SaveLibraryItemAssetMessage>(OnSaveLibraryItemAsset);
                m_Application.SubscribeToMessage<ExportAnimationMessage>(OnExportAnimation);
                m_Application.SubscribeToMessage<VersionErrorMessage>(OnVersionError);
                m_Application.SubscribeToMessage<AuthoringStartedMessage>(OnAuthoringStarted);
                m_Application.SubscribeToMessage<AuthoringEndedMessage>(OnAuthoringEnded);
                m_Application.SubscribeToMessage<ApplicationStartedMessage>(OnApplicationStarted);

                m_OwnsApplication = true;
            }

            return m_Application;
        }

        void SetupVideoServices()
        {
            if (m_Application == null)
                return;

            var go = new GameObject("VideoConverter");
            go.transform.SetParent(m_Application.transform);
            var vp = go.AddComponent<VideoPlayer>();
            Locator.Provide<IVideoConverter>(new VideoConverter(vp));
        }

        void OnApplicationStarted(ApplicationStartedMessage obj)
        {
            Log($"OnApplicationStarted()");
            HidePlaceHolder();
            if (m_ActiveLibraryItemAssetPath != null)
            {
                ActivateLibraryItemAssetFromPath(m_ActiveLibraryItemAssetPath);
            }
        }

        void OnChangedActiveLibraryItem()
        {
            m_ActiveLibraryItemAssetPath = m_Application.ApplicationContext.ApplicationLibraryModel.ActiveLibraryItemPath;
            Log($"OnChangedActiveLibraryItem() -> Asset path: {m_ActiveLibraryItemAssetPath}");
        }

        void OnVersionError(VersionErrorMessage message)
        {
            const string toastMessage = "Version incompatibility detected. Please update to the latest version of Muse Animate.";
            m_RootVisualElement.Q<Panel>().OpenToast(toastMessage,
                NotificationStyle.Negative,
                NotificationDuration.Indefinite,
                AnimationMode.Slide);
        }

        void StartApplication()
        {
            Log("StartApplication()");
            m_IsUsable = false;

            if (m_Application == null)
                return;

            m_Application.StartApplication(m_RootVisualElement);
            m_Application.ApplicationLibraryModel.OnChangedActiveItem += OnChangedActiveLibraryItem;
        }

        /// <summary>
        /// Perform any cleanup necessary to stop the application, cancelling any pending requests, as well as
        /// triggering a SaveSessionMessage.
        /// </summary>
        void StopApplication()
        {
            if (m_Application)
            {
                Log("StopApplication()");
                m_Application.Shutdown();
            }
            else
            {
                Log("StopApplication() -> m_Application is null.");
            }
        }

        void RestartApplication()
        {
            Log($"RestartApplication()");

            StopApplication();
            StartApplication();
        }

        void DestroyApplication()
        {
            Log("DestroyApplication()");
            StopApplication();

            if (m_OwnsApplication)
            {
                // Is this necessary?
                DestroyImmediate(m_Application.gameObject);
            }

            m_Application.UnsubscribeFromMessage<SaveLibraryItemAssetMessage>(OnSaveLibraryItemAsset);
            m_Application.UnsubscribeFromMessage<ExportAnimationMessage>(OnExportAnimation);
            m_Application.UnsubscribeFromMessage<VersionErrorMessage>(OnVersionError);
            m_Application.UnsubscribeFromMessage<AuthoringStartedMessage>(OnAuthoringStarted);
            m_Application.UnsubscribeFromMessage<AuthoringEndedMessage>(OnAuthoringEnded);
            m_Application.UnsubscribeFromMessage<ApplicationStartedMessage>(OnApplicationStarted);

            m_Application = null;
            m_OwnsApplication = false;
            Window = null;

            EditorSceneManager.ClosePreviewScene(m_Scene);
            ClientStatus.Instance.OnClientStatusChanged -= OnClientStatusChanged;
            EditorApplication.quitting -= SaveBeforeQuitting;
        }

        void OnEnable()
        {
            Log($"OnEnable()");

            // When the editor is closing, OnDisable/OnDestroy is not called. Since the Application isn't being
            // shut down normally, we need to catch this event to save the session. (Note: this may introduce a
            // slight delay when trying to close the Editor.)
            m_Scene = EditorSceneManager.NewPreviewScene();
            EditorApplication.quitting += SaveBeforeQuitting;
        }

        // TODO: Why does OnDisable get called twice on Unity startup?
        void OnDisable()
        {
            Log($"OnDisable()");
            DestroyApplication();
        }

        void OnFocus()
        {
            Log($"OnFocus()");

            if (m_Application != null)
            {
                m_Application.Resume();
            }
        }

        void OnLostFocus()
        {
            Log($"OnLostFocus()");

            if (m_Application != null)
            {
                m_Application.Pause();
            }
        }

        void SaveLibraryItemAsset(LibraryItemAsset item)
        {
            if (item == null)
            {
                LogError($"SaveLibraryItemAsset() -> item is null.");
                return;
            }

            Log($"SaveLibraryItemAsset({item.name})");

            try
            {
                item.Save(m_Application.ApplicationContext.Stage);
            }
            catch (SerializationException e)
            {
                DevLogger.LogError($"Failed to save item: {e.Message}");
            }
        }

        void SaveBeforeQuitting()
        {
            var item = m_Application.ApplicationLibraryModel.ActiveLibraryItem;

            if (item == null)
            {
                Log($"SaveActiveLibraryItemAsset() -> ActiveLibraryItem is null.");
                return;
            }

            Log($"SaveActiveLibraryItemAsset({item.name})");
            SaveLibraryItemAsset(item);
        }

        void OnSaveLibraryItemAsset(SaveLibraryItemAssetMessage message)
        {
            Log($"OnSaveLibraryItemAsset({message})");
            SaveLibraryItemAsset(message.ItemAsset);
        }

        static void OnExportAnimation(ExportAnimationMessage message)
        {
            Log($"OnExportAnimation({message})");
            switch (message.ExportType)
            {
                case ApplicationLibraryModel.ExportType.HumanoidAnimation:
                    DoExportHumanoidAnimation(message);
                    break;
                case ApplicationLibraryModel.ExportType.FBX:
                    DoExportFBXAnimation(message);
                    break;
                default:
                    throw new InvalidOperationException($"Export Type {message.ExportType} is not supported");
            }
        }

        static void DoExportHumanoidAnimation(ExportAnimationMessage message)
        {
            Log($"OnExportAnimation({message})");

            if (message.ExportType != ApplicationLibraryModel.ExportType.HumanoidAnimation)
                return;

            var clip = HumanoidAnimationExport.Export(message.ExportData);

            if (clip == null)
                return;

            var path = "";

            if (message.ExportFlow is ApplicationLibraryModel.ExportFlow.Drag)
            {
                path = AssetDatabase.GenerateUniqueAssetPath($"{AnimatePreferences.AssetGeneratedFolderPath}/{message.FileName}.anim");
            }
            else if (message.ExportFlow is ApplicationLibraryModel.ExportFlow.SubAsset)
            {
                path = message.Asset.Path;
            }
            else
            {
                path = EditorUtility.SaveFilePanelInProject("Save Animation", "Animation", "anim", "Save animation");
            }

            if (string.IsNullOrEmpty(path))
                return;

            switch (message.ExportFlow)
            {
                case ApplicationLibraryModel.ExportFlow.Manually:
                    AssetDatabase.CreateAsset(clip, path);
                    AssetDatabase.SaveAssetIfDirty(clip);
                    AssetDatabase.Refresh();
                    break;

                case ApplicationLibraryModel.ExportFlow.Drag:
                    AssetDatabase.CreateAsset(clip, path);
                    AssetDatabase.SaveAssetIfDirty(clip);
                    AssetDatabase.Refresh();

                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { clip };
                    DragAndDrop.StartDrag(message.FileName);
                    Selection.activeObject = clip;
                    break;

                case ApplicationLibraryModel.ExportFlow.SubAsset:

                    clip.name = message.Asset.Title;

                    // Remove the previous sub-asset animation clip, if any
                    var existingClipAsset = AssetDatabase.LoadAssetAtPath<AnimationClip>(message.Asset.Path);

                    if (existingClipAsset != null)
                    {
                        AnimationClipUtils.DeepCopyAnimation(clip, existingClipAsset);
                    }
                    else
                    {
                        AssetDatabase.AddObjectToAsset(clip, message.Asset);

                        if (message.Asset.Data.Model is KeySequenceTake keySequenceTake)
                        {
                            keySequenceTake.BakedAnimationClip = clip;
                        }
                        else if (message.Asset.Data.Model is DenseTake denseTake)
                        {
                            denseTake.BakedAnimationClip = clip;
                        }
                    }

                    AssetDatabase.SaveAssetIfDirty(clip);
                    AssetDatabase.SaveAssetIfDirty(message.Asset);
                    AssetDatabase.Refresh();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DoExportFBXAnimation(ExportAnimationMessage message)
        {
            if (message.ExportFlow == ApplicationLibraryModel.ExportFlow.Drag)
                throw new InvalidOperationException("FBX Export through drag and drop is not supported");

            var path = EditorUtility.SaveFilePanelInProject("Export to FBX", message.FileName, "fbx", "export to fbx");

            if (!string.IsNullOrEmpty(path))
            {
                LocalFbxExport.Export(message.ExportData, message.FileName, path);
                AssetDatabase.Refresh();
            }
        }

        void ActivateLibraryItemAssetFromPath(string path)
        {
            Log($"ActivateLibraryItemAssetFromPath() -> Path: {path}");

            // If we are creating a new Application instance, so we need to wait a frame before loading the session
            // because the Application is not fully initialized until the next Update
            var asset = AssetDatabase.LoadAssetAtPath<LibraryItemAsset>(path);
            if (asset != null)
            {
                m_Application.ApplicationLibraryModel.AskEditLibraryItem(asset);
            }
        }

        void ScheduleAssetLoad(string path)
        {
            m_ActiveLibraryItemAssetPath = path;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is LibraryItemAsset itemAsset)
            {
                if (Window != null)
                {
                    // If the window is already open, load the selected session asset without closing the window.
                    Window.ActivateLibraryItemAssetFromPath(AssetDatabase.GetAssetPath(itemAsset));
                }
                else
                {
                    // The window is not open, so we need to open it and then load the session asset.
                    // The act of opening the window will initialize the application
                    Window = GetWindow<MuseAnimateEditorWindow>(k_WindowName);
                    Window.ScheduleAssetLoad(AssetDatabase.GetAssetPath(itemAsset));
                }

                return true;
            }

            return false;
        }

        public void StartCoroutine(IEnumerator routine)
        {
            EditorCoroutineUtility.StartCoroutine(routine, this);
        }

        #region Debugging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        static void Log(string message)
        {
            DevLogger.LogInfo($"MuseAnimateEditorWindow -> {message}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        void LogError(string message)
        {
            DevLogger.LogError($"MuseAnimateEditorWindow -> Error: {message}");
        }

        #endregion
    }
}
