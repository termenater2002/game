using System;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    partial class Canvas : VisualElement
    {
#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<Canvas, UxmlTraits> { }
#endif

        Model m_CurrentModel;

        CanvasManipulator m_CurrentToolManipulator;

        readonly AppUI.UI.Canvas m_Canvas;

        Artifact m_RefinedArtifact;

        readonly VisualElement m_ControlContent;
        readonly VisualElement m_ControlTopContent;
        readonly VisualElement m_ControlMiddleContent;
        readonly VisualElement m_ControlBottomContent;

        IVisualElementScheduledItem m_ScheduledFrame;

        VisualElement m_LastLeftOverlay;
        VisualElement m_LastRightOverlay;

        const int k_FrameTopOffset = 42;

        ArtifactNode m_CurrentNode;

        public Artifact refinedArtifact
        {
            get => m_RefinedArtifact;
            private set
            {
                if (m_RefinedArtifact == value)
                    return;

                m_RefinedArtifact = value;
                UpdateView();
                FrameAll();
            }
        }

        public override VisualElement contentContainer => m_Canvas.contentContainer;

        void SetFrameContainer(Rect frameContainer)
        {
            m_Canvas.frameContainer = frameContainer;
            m_ControlContent.style.left = frameContainer.x;
            m_ControlContent.style.top = 0;
            m_ControlContent.style.width = frameContainer.width;
            m_ControlContent.style.height = frameContainer.height;
        }

        public AppUI.UI.CanvasManipulator primaryManipulator
        {
            get => m_Canvas.primaryManipulator;
            set => m_Canvas.primaryManipulator = value;
        }

        public Canvas()
        {
            m_Canvas = new AppUI.UI.Canvas
            {
                frameMargin = 24f,
                controlScheme = GlobalPreferences.canvasControlScheme
            };

            GlobalPreferences.preferencesChanged += OnPreferencesChanged;

            hierarchy.Add(m_Canvas);
            m_Canvas.StretchToParentSize();
            this.StretchToParentSize();

            this.RegisterContextChangedCallback<Model>(context => SetModel(context.context));

            m_ControlContent = new VisualElement()
            {
                name = "control-content",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1f
                }
            };

            m_ControlTopContent = new VisualElement()
            {
                name = "control-top-content",
            };

            m_ControlMiddleContent = new VisualElement()
            {
                name = "control-middle-content",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1f
                }
            };

            m_ControlBottomContent = new VisualElement()
            {
                name = "control-bottom-content",
            };

            m_ControlContent.Add(m_ControlTopContent);
            m_ControlContent.Add(m_ControlMiddleContent);
            m_ControlContent.Add(m_ControlBottomContent);

            hierarchy.Add(m_ControlContent);
        }

        void OnPreferencesChanged()
        {
            m_Canvas.controlScheme = GlobalPreferences.canvasControlScheme;
        }

        void FrameArtifact(Artifact artifact)
        {
            // we just have one node in the canvas so we just need to check the first child
            if (m_Canvas.childCount == 0 || m_Canvas.ElementAt(0) is not ArtifactNode node || node.artifact != artifact)
                return;

            FrameAll();
        }

        public void FrameAll()
        {
            //Scheduling since it can be done too quickly when Unity editor is opened while the
            //window was open before the editor was closed.
            schedule.Execute(() =>
            {
                UpdateCanvasFrameContainer();
                m_Canvas.FrameAll();
            });
        }
        public void SetModel(Model model)
        {
            UnSubscribeToModelEvents();
            m_CurrentModel = model;
            SubscribeToModelEvents();

            if (!m_CurrentModel)
                return;

            OnLeftOverlayChanged(m_CurrentModel.LeftOverlay);
            OnRightOverlayChanged(m_CurrentModel.RightOverlay);

            OnArtifactSelected(m_CurrentModel.SelectedArtifact);
        }

        public void UpdateCanvasFrameContainer()
        {
            if (!m_CurrentModel)
                return;

            var leftOverlay = m_CurrentModel.LeftOverlay.resolvedStyle;
            var rightOverlay = m_CurrentModel.RightOverlay.resolvedStyle;

            var width = resolvedStyle.width -
                        leftOverlay.width -
                        leftOverlay.marginLeft -
                        leftOverlay.marginRight -
                        rightOverlay.width -
                        rightOverlay.marginLeft -
                        rightOverlay.marginRight - 20;
            var x = leftOverlay.width +
                    leftOverlay.marginLeft +
                    leftOverlay.marginRight + 5;
            const int y = k_FrameTopOffset;
            var height = resolvedStyle.height - y;

            SetFrameContainer(new Rect(x, y, width, height));
        }

        void SubscribeToModelEvents()
        {
            if (!m_CurrentModel)
                return;

            m_CurrentModel.OnDispose += OnModelDispose;
            m_CurrentModel.OnActiveToolChanged += OnActiveToolChanged;
            m_CurrentModel.OnFrameArtifactRequested += FrameArtifact;
            m_CurrentModel.OnDispose += UnSubscribeToModelEvents;
            m_CurrentModel.OnCanvasRefineArtifact += OnRefineArtifact;
            m_CurrentModel.OnRefineArtifact += OnRefineArtifact;
            m_CurrentModel.OnArtifactSelected += OnArtifactSelected;
            m_CurrentModel.OnFinishRefineArtifact += OnFinishRefineArtifact;
            m_CurrentModel.OnLeftOverlayChanged += OnLeftOverlayChanged;
            m_CurrentModel.OnRightOverlayChanged += OnRightOverlayChanged;
        }

        void OnLeftOverlayChanged(VisualElement overlay)
        {
            m_LastLeftOverlay?.UnregisterCallback<GeometryChangedEvent>(OnOverlayGeometryChanged);
            m_LastLeftOverlay = overlay;
            m_LastLeftOverlay?.RegisterCallback<GeometryChangedEvent>(OnOverlayGeometryChanged);
            UpdateCanvasFrameContainer();
        }

        void OnRightOverlayChanged(VisualElement overlay)
        {
            m_LastRightOverlay?.UnregisterCallback<GeometryChangedEvent>(OnOverlayGeometryChanged);
            m_LastRightOverlay = overlay;
            m_LastRightOverlay?.RegisterCallback<GeometryChangedEvent>(OnOverlayGeometryChanged);
            UpdateCanvasFrameContainer();
        }

        void OnOverlayGeometryChanged(GeometryChangedEvent _)
        {
            UpdateCanvasFrameContainer();
        }

        void OnFinishRefineArtifact(Artifact obj)
        {
            refinedArtifact = null;
            UpdateView();
        }

        void OnArtifactSelected(Artifact artifact)
        {
            if (!m_CurrentModel || !m_CurrentModel.isRefineMode)
                return;

            OnActiveToolChanged(m_CurrentModel.ActiveTool);
            OnRefineArtifact(artifact);
        }

        void OnRefineArtifact(Artifact artifact)
        {
            if (!m_CurrentModel)
                return;

            refinedArtifact = artifact;
        }

        void UnSubscribeToModelEvents()
        {
            if (!m_CurrentModel)
                return;

            m_CurrentModel.OnDispose -= OnModelDispose;
            m_CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;
            m_CurrentModel.OnFrameArtifactRequested -= FrameArtifact;
            m_CurrentModel.OnCanvasRefineArtifact -= OnRefineArtifact;
            m_CurrentModel.OnRefineArtifact -= OnRefineArtifact;
            m_CurrentModel.OnArtifactSelected -= OnArtifactSelected;
            m_CurrentModel.OnFinishRefineArtifact -= OnFinishRefineArtifact;
            m_CurrentModel.OnLeftOverlayChanged -= OnLeftOverlayChanged;
            m_CurrentModel.OnRightOverlayChanged -= OnRightOverlayChanged;
        }

        void OnModelDispose()
        {
            SetModel(null);
        }

        void OnPanChanged(bool panEnabled)
        {
            foreach (var item in m_Canvas.Children())
            {
                item.SetEnabled(!panEnabled);
                item.pickingMode = panEnabled ? PickingMode.Ignore : PickingMode.Position;
                item.EnableInClassList("cursor--grab", panEnabled);
            }

            primaryManipulator = panEnabled ? AppUI.UI.CanvasManipulator.Pan : AppUI.UI.CanvasManipulator.None;
        }

        void OnActiveToolChanged(ICanvasTool _)
        {
            UpdateManipulators();
        }

        void UpdateManipulators()
        {
            if (m_CurrentToolManipulator != null)
                m_Canvas.RemoveManipulator(m_CurrentToolManipulator);

            OnPanChanged(m_CurrentModel?.ActiveTool is PanTool);

            if (m_CurrentModel?.ActiveTool == null)
                return;

            m_CurrentToolManipulator = m_CurrentModel.ActiveTool.GetToolManipulator();
            if (m_CurrentToolManipulator != null)
                m_Canvas.AddManipulator(m_CurrentToolManipulator);
        }

        public void UpdateView()
        {
            m_CurrentNode?.Dispose();
            m_CurrentNode = null;

            Clear();
            if (!m_CurrentModel || refinedArtifact is null)
                return;

            m_CurrentNode = new ArtifactNode
            {
                artifact = refinedArtifact
            };
            Add(m_CurrentNode);
            UpdateManipulators();
        }
    }
}
