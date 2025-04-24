using System;
using System.Linq;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;

#pragma warning disable 0067

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class MainUI : VisualElement, IControl
    {
        [Serializable]
        class UISize : IModelData
        {
            public event Action OnModified;
            public event Action OnSaveRequested;

            [SerializeField]
            float m_NodeListWidth = k_NodeListMinWidth;

            [SerializeField]
            float m_NodeListRefineWidth = k_NodeListMinWidth;

            [SerializeField]
            float m_AssetListRefineWidth = k_AssetListMinWidth;

            public float nodeListWidth
            {
                get => m_NodeListWidth;
                set
                {
                    m_NodeListWidth = value;
                    OnModified?.Invoke();
                }
            }

            public float nodeListRefineWidth
            {
                get => m_NodeListRefineWidth;
                set
                {
                    m_NodeListRefineWidth = value;
                    OnModified?.Invoke();
                }
            }

            public float assetListRefineWidth
            {
                get => m_AssetListRefineWidth;
                set
                {
                    m_AssetListRefineWidth = value;
                    OnModified?.Invoke();
                }
            }
        }

#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<MainUI, UxmlTraits> { }
#endif

        bool m_Initialized;
        Canvas m_Canvas;
        RefineAssetView m_RefineAssetView;
        ControlToolbar m_ControlToolbar;
        NodesList m_NodesList;
        AssetsList m_AssetsList;
        ScopeToolbar m_ScopeToolbar;
        VisualElement m_CanvasContainer;

        IUIMode m_UIMode;

        const float k_AssetListLeftMargin = 0f;//10f;
        internal const float k_NodeListMinWidth = 300;
        internal const float k_AssetListMinWidth = 200;
        UISize m_UISize;

        Artifact m_ArtifactToBeRefined;

        IVisualElementScheduledItem m_ScheduledFrameSelectedArtifact;

        public MainUI()
        {
            this.AddManipulator(new MuseShortcutHandler());
            this.RegisterContextChangedCallback<Model>(context => SetModel(context.context));
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

#if UNITY_EDITOR
            if(UnityEditor.EditorGUIUtility.isProSkin)
                AddToClassList("dark-mode");
#endif
        }

        public void SetModel(Model newModel)
        {
            if (newModel == model)
                return;

            UnsubscribeFromEvents();
            model = newModel;
            Init();
            SubscribeToEvents();

            UpdateView();
        }

        void OnModeChanged(int _)
        {
            UpdateView();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            SetModel(null);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            SetModel(this.GetContext<Model>());
        }

        public Canvas canvas => m_Canvas;
        public ControlToolbar controlToolbar => m_ControlToolbar;
        public NodesList nodesList => m_NodesList;
        public AssetsList assetsList => m_AssetsList;
        public ScopeToolbar scopeToolbar => m_ScopeToolbar;
        public Model model { get; private set; }

        public void UpdateView()
        {
            m_UIMode?.Deactivate();

            if (!model)
                return;

            var modeKey = model.CurrentMode;
            m_UIMode = UIModeFactory.GetUIMode(modeKey);
            m_UIMode?.Activate(this, modeKey);

            m_UISize = model.GetData<UISize>();
            assetsList.content.style.minWidth = k_AssetListMinWidth;
            model.SetLeftOverlay(nodesList.content);
            model.SetRightOverlay(assetsList.content);
            MaximiseAssetList();
            assetsList.MarkDirtyRepaint();

            OnRefineArtifact(model.RefinedArtifact);
        }

        void UnsubscribeFromEvents()
        {
            UnregisterCallback<GeometryChangedEvent>(OnMainUIGeometryChanged);

            if (assetsList != null)
            {
                assetsList.OnResized -= AssetListResized;
            }

            if (nodesList != null)
            {
                nodesList.OnResized -= NodeListResized;
                nodesList.UnregisterCallback<GeometryChangedEvent>(OnNodesListGeometryChanged);
            }

            RemoveModelListeners();
        }

        void SubscribeToEvents()
        {
            AddModelListeners();

            RegisterCallback<GeometryChangedEvent>(OnMainUIGeometryChanged);

            if (assetsList != null)
                assetsList.OnResized += AssetListResized;

            if (nodesList != null)
            {
                nodesList.OnResized += NodeListResized;
                nodesList.RegisterCallback<GeometryChangedEvent>(OnNodesListGeometryChanged);
            }
        }

        void Init()
        {
            if (panel == null)
                return;

            if (m_Initialized)
                return;

            m_Canvas = this.Q<Canvas>();
            m_ControlToolbar = this.Q<ControlToolbar>();

            var museOverlay = this.Q<VisualElement>("muse-overlay");
            var assetsListType = ModesFactory.GetModeData(model?.CurrentMode)?.assetslist_type;
            if (string.IsNullOrEmpty(assetsListType))
                assetsListType = "AssetsList";

            m_NodesList = this.Q<NodesList>();
            m_AssetsList = (AssetsList)ControlsFactory.GetControlInstance(assetsListType);
            m_AssetsList.pickingMode = PickingMode.Ignore;
            museOverlay.Add(m_AssetsList);

            m_RefineAssetView = m_AssetsList.CreateRefineAssetView();
            if (m_RefineAssetView != null)
                Add(m_RefineAssetView);

            m_ScopeToolbar = this.Q<ScopeToolbar>();
            m_CanvasContainer = this.Q<VisualElement>("muse-canvas-container");
            m_Initialized = true;
        }

        void AddModelListeners()
        {
            if (!model)
                return;

            model.OnRefineArtifact += OnRefineArtifact;
            model.OnFinishRefineArtifact += OnFinishRefineArtifact;
            model.OnDispose += OnDispose;
            model.OnArtifactSelected += OnArtifactSelected;
            model.OnModeChanged += OnModeChanged;
            model.OnServerError += OnServerError;
            GenerativeAIBackend.OnServerError += OnServerError;
        }

        void RemoveModelListeners()
        {
            if (!model)
                return;

            model.OnRefineArtifact -= OnRefineArtifact;
            model.OnFinishRefineArtifact -= OnFinishRefineArtifact;
            model.OnDispose -= OnDispose;
            model.OnArtifactSelected -= OnArtifactSelected;
            model.OnModeChanged -= OnModeChanged;
            model.OnServerError -= OnServerError;
            GenerativeAIBackend.OnServerError -= OnServerError;
        }

        void OnMainUIGeometryChanged(GeometryChangedEvent evt)
        {
            if (!model.isRefineMode)
                MaximiseAssetList();
            canvas.UpdateCanvasFrameContainer();
        }

        void UpdateCanvasVisibility()
        {
            var enabled = model.isRefineMode;
            canvas.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            EnableInClassList("muse--refinement-mode", enabled);
            if (enabled)
            {
                m_ScheduledFrameSelectedArtifact?.Pause();
                m_ScheduledFrameSelectedArtifact = schedule.Execute(() =>
                {
                    canvas.FrameAll();
                });
            }

            m_CanvasContainer.style.display = m_RefineAssetView != null && enabled ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void RefineModeAssetList()
        {
            assetsList.content.style.width = m_UISize.assetListRefineWidth;
            nodesList.style.width = m_UISize.nodeListRefineWidth;
            nodesList.draggerElement.RemoveFromClassList(Styles.hiddenUssClassName);
        }

        void MaximiseAssetList()
        {
            var leftMargin = model.isRefineMode ? k_AssetListLeftMargin : 0;

            assetsList.content.style.maxWidth = resolvedStyle.width - 300 - k_AssetListLeftMargin;
            assetsList.content.style.width = resolvedStyle.width - m_UISize.nodeListWidth - leftMargin;
            nodesList.style.width = m_UISize.nodeListWidth;
            nodesList.draggerElement.AddToClassList(Styles.hiddenUssClassName);
        }

        void OnNodesListGeometryChanged(GeometryChangedEvent evt)
        {
            canvas.UpdateCanvasFrameContainer();
        }

        void NodeListResized()
        {
            if (!model.isRefineMode)
            {
                m_UISize.nodeListWidth = nodesList.style.width.value.value;
                MaximiseAssetList();
            }
            else
            {
                m_UISize.nodeListRefineWidth = nodesList.style.width.value.value;
            }
        }

        void AssetListResized()
        {
            if (!model)
                return;

            var assetListWidth = assetsList.content.style.width.value.value;
            if (model.isRefineMode)
            {
                m_UISize.assetListRefineWidth = assetListWidth;
            }
            else
            {
                m_UISize.nodeListWidth = Mathf.Min(nodesList.resolvedStyle.maxWidth.value,
                    resolvedStyle.width - k_AssetListLeftMargin - assetListWidth);
                MaximiseAssetList();
            }
        }

        void OnRefineArtifact(Artifact artifact)
        {
            if (!model)
                return;

            if (model.isRefineMode)
            {
                if (m_RefineAssetView == null)
                {
                    model.CanvasRefineArtifact(artifact);
                    RefineModeAssetList();
                }
            }
            UpdateCanvasVisibility();

            if (m_RefineAssetView != null)
                m_RefineAssetView.SetModel(model);

        }

        void OnFinishRefineArtifact(Artifact artifact)
        {
            MaximiseAssetList();
            UpdateCanvasVisibility();
        }

        void OnArtifactSelected(Artifact artifact)
        {
            if (artifact is null || (m_RefineAssetView != null))
                return;

            if (model.isRefineMode && canvas.refinedArtifact?.Guid != artifact.Guid)
                model.CanvasRefineArtifact(artifact);
        }

        void OnDispose()
        {
            UnsubscribeFromEvents();
        }

        bool OnServerError(long code, string error)
        {
            switch (code)
            {
                case 429:
                    Debug.LogWarning("Your last request was rate-limited because of too many requests in a short amount of time. Please try again later.");
                    return true;
            }

            return false;
        }
    }
}
