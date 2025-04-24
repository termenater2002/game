using System;
using System.Collections;
using System.Collections.Generic;
using Hsm;
using Unity.DeepPose.Components;
using Unity.Muse.Animate.UserActions;
using Unity.Muse.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Represents the Application instance, responsible for main logic of the app
    /// </summary>
    partial class Application : MonoBehaviour, ICoroutineRunner
    {
        public static bool IsInitialized { get; private set; }

        [SerializeField]
        public ApplicationConfiguration ApplicationConfiguration;

        [SerializeField]
        public PhysicsSolverComponent PosingPhysicsSolver;

        [SerializeField]
        public PhysicsSolverComponent MotionPhysicsSolver;

        public SelectionModel<LibraryItemAsset> ItemsSelection;
        ApplicationModel ApplicationModel { get; set; }
        public ApplicationLibraryModel ApplicationLibraryModel => ApplicationModel.ApplicationLibraryModel;

        Camera ViewCamera { get; set; }

        Camera ThumbnailCamera { get; set; }

        UIDocument UIDocument { get; set; }

        Transform Environment { get; set; }

        public bool IsAuthoringAssetLoaded { get; set; }

        [NonSerialized]
        public ApplicationContext ApplicationContext;

        readonly MessageBus m_MessageBus = new();

        public async void GetAuthHeaders(Action<Dictionary<string, string>> onSuccess)
        {
            var headers = new Dictionary<string, string>();
#if UNITY_EDITOR
            var token = await UnityConnectUtils.GetUserAccessTokenAsync();
            headers.Add(ApplicationConstants.AuthorizationHeaderName, $"Bearer {token}");
#endif
            onSuccess?.Invoke(headers);
        }

        partial class ApplicationHsm { }

        StateMachine m_ApplicationFlow = null;
        bool m_Paused;

#if UNITY_EDITOR
        float DeltaTime => m_Paused ? 0 : m_DeltaTime;
#else
        float DeltaTime => m_Paused ? 0 : Time.deltaTime;
#endif

        static Application s_Instance;
        Scene m_Scene;
        PhysicsScene m_PhysicsScene;

        double m_LastEditorUpdateTime;
        float m_DeltaTime;

        public static Application Instance
        {
            get
            {
                // UI Toolkit uses Instance before the Awake call is made from Application,
                // in that case we recover it in the scene:
                if (s_Instance == null)
                {
#if UNITY_2023_1_OR_NEWER
                    s_Instance = FindFirstObjectByType<Application>();
#else
                    s_Instance = FindObjectOfType<Application>();
#endif
                }

                Assert.IsNotNull(s_Instance, "Application was not found in the scene");

                IsInitialized = true;
                return s_Instance;
            }
        }

        public StateMachine ApplicationFlow => m_ApplicationFlow;

        void OnEnable()
        {
            if (UnityEngine.Application.isPlaying)
            {
                Locator.Provide(ApplicationConfiguration.UITemplatesRegistry);
                Locator.Provide<IRootObjectSpawner<GameObject>>(new RuntimeRootObjectSpawner(transform));
                Locator.Provide<ICoroutineRunner>(this);
            }
        }

        void Start()
        {
            Assert.IsNotNull(ApplicationConfiguration, "No ApplicationConfiguration is defined in Application Prefab.");
            UIDocument = Instantiate(ApplicationConfiguration.UIDocumentPrefab, transform);
            StartApplication(UIDocument.rootVisualElement);
        }

        void OnDisable()
        {
            Shutdown();
        }

        public void BindToScene(Scene scene)
        {
            m_Scene = scene;
            m_PhysicsScene = m_Scene.GetPhysicsScene();
        }

        public void StartApplication(VisualElement rootVisualElement)
        {
            Log($"StartApplication({rootVisualElement})");
            Assert.IsNotNull(ApplicationConfiguration, "No ApplicationConfiguration is defined in Application Prefab.");
            Assert.IsTrue(s_Instance == null || s_Instance == this, "There must be only one instance of Application in the scene");

            s_Instance = this;

            LibraryRegistry.RefreshAssets("StartApplication");

            ApplicationConstants.LoopGhostMaterial = ApplicationConfiguration.LoopGhostMaterial;

            InitializeCameras();
            InitializeEnvironment();

            ApplicationLayers.AssignLayers(transform);
            PhysicsUtils.SetLayerCollisionMatrix();

            ItemsSelection = new SelectionModel<LibraryItemAsset>();
            ApplicationModel = new ApplicationModel(ItemsSelection);
            ApplicationContext = new ApplicationContext(
                rootVisualElement,
                ApplicationModel,
                ItemsSelection,
                ApplicationConfiguration.ActorRegistry,
                ApplicationConfiguration.PropRegistry,
                ViewCamera,
                ThumbnailCamera,
                PosingPhysicsSolver,
                MotionPhysicsSolver);

            m_ApplicationFlow = new StateMachine();
            m_ApplicationFlow.Init<ApplicationHsm.Root>(this);

            if (!RenderPipelineUtils.IsUsingUrp() && !RenderPipelineUtils.IsUsingHdrp())
            {
                ApplicationContext.Camera.RenderScaling = Vector2.one * 2f;
            }

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
            
            // Subscribe to the physics solver
            PhysicsSolver.OnSimulate += SimulatePhysics;
        }

        void InitializeCameras()
        {
            Camera cameraPrefab = ApplicationConfiguration.CameraPrefab;
            Shader.DisableKeyword("RENDER_PIPELINE_HDRP");

            if (RenderPipelineUtils.IsUsingHdrp())
            {
                cameraPrefab = ApplicationConfiguration.CameraPrefabHDRP;
                Shader.EnableKeyword("RENDER_PIPELINE_HDRP");
            }
            else if (RenderPipelineUtils.IsUsingUrp())
            {
                cameraPrefab = ApplicationConfiguration.CameraPrefabURP;
            }

            ViewCamera = Instantiate(cameraPrefab, transform);
            ViewCamera.cullingMask = ApplicationLayers.LayerMaskAll;
            ViewCamera.name = "ViewCamera";
            // We will call Render() manually.
            ViewCamera.enabled = false;

            ThumbnailCamera = Instantiate(cameraPrefab, transform);
            ThumbnailCamera.cullingMask = ApplicationLayers.LayerMaskThumbnail | ApplicationLayers.LayerMaskHandles | ApplicationLayers.LayerMaskPosing;
            ThumbnailCamera.gameObject.SetActive(false);    // Disable the lights attached to the thumbnail camera
            ThumbnailCamera.name = "ThumbnailCamera";
            // We will call Render() manually.
            ThumbnailCamera.enabled = false;
            ThumbnailCamera.scene = m_Scene;
            ViewCamera.scene = m_Scene;
        }

        void ClearCameras()
        {
            if (ViewCamera)
            {
                GameObjectUtils.Destroy(ViewCamera.gameObject);
            }

            if (ThumbnailCamera)
            {
                GameObjectUtils.Destroy(ThumbnailCamera.gameObject);
            }
        }

        void InitializeEnvironment()
        {
            if (RenderPipelineUtils.IsUsingHdrp())
            {
                Environment = Instantiate(ApplicationConfiguration.EnvironmentPrefabHDRP, transform);
            }
            else if (RenderPipelineUtils.IsUsingUrp())
            {
                Environment = Instantiate(ApplicationConfiguration.EnvironmentPrefabURP, transform);
            }
            else
            {
                Environment = Instantiate(ApplicationConfiguration.EnvironmentPrefab, transform);
            }
        }

        void ClearEnvironment()
        {
            if (Environment != null)
            {
                GameObjectUtils.Destroy(Environment.gameObject);
            }
        }

        public void Shutdown()
        {
            Log("Shutdown()");
            
            PhysicsSolver.OnSimulate -= SimulatePhysics;
            UserActionsManager.Instance.ClearAll();

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif

            // Request a save before clearing the stage
            if (IsAuthoringAssetLoaded)
            {
                Log("Shutdown() -> SessionIsLoaded, Publish SaveSessionMessage");
                var activeItem = ApplicationLibraryModel.ActiveLibraryItem;

                if (activeItem != null)
                {
                    Log($"ActivateLibraryItemAssetFromPath() -> Saving previous active item...");
                    PublishMessage(new SaveLibraryItemAssetMessage(activeItem));
                }
            }

            m_ApplicationFlow?.Shutdown();
            m_ApplicationFlow = null;

            ClearCameras();
            ClearEnvironment();

            IsInitialized = false;
            s_Instance = null;
        }

        void Log(string s)
        {
            DevLogger.LogInfo($"Application -> {s}");
        }

        public void Update()
        {
            m_ApplicationFlow?.Update(DeltaTime);
        }

        public void LateUpdate()
        {
            m_ApplicationFlow?.LateUpdate(DeltaTime);
        }

        void SimulatePhysics(float deltaTime)
        {
            if (!m_PhysicsScene.IsValid())
                return;
            
            m_PhysicsScene.Simulate(deltaTime);
        }

        void EditorUpdate()
        {
            m_DeltaTime = (float)(EditorApplication.timeSinceStartup - m_LastEditorUpdateTime);
            m_LastEditorUpdateTime = EditorApplication.timeSinceStartup;
            m_ApplicationFlow?.Update(DeltaTime);
            m_ApplicationFlow?.LateUpdate(DeltaTime);
        }

        public void Render()
        {
            m_ApplicationFlow?.Render(DeltaTime);
        }

        public void PublishMessage<T>(T message)
        {
            m_MessageBus.Publish(message);
        }

        public void PublishMessage<T>() where T : new()
        {
            m_MessageBus.Publish(new T());
        }

        public void SubscribeToMessage<T>(Action<T> handler)
        {
            m_MessageBus.Subscribe(handler);
        }

        public void UnsubscribeFromMessage<T>(Action<T> handler)
        {
            m_MessageBus.Unsubscribe(handler);
        }

        public new void StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
        }

        public void Pause()
        {
            Log("Pause()");
            m_Paused = true;
        }

        public void Resume()
        {
            Log("Resume()");
            m_Paused = false;
        }
    }
}
