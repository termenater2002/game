using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Tools
{
    class BrushTool<T> : ICanvasTool where T: class, IMaskOperator
    {
        protected Model m_CurrentModel;
        PaintCanvasToolManipulator<T> m_CurrentManipulator;
        protected PanTool m_PanTool;
        PaintingManipulatorSettings<T> m_Settings;
        MuseToolbar m_Toolbar;

        public DoodleModifierState currentState => m_CurrentManipulator.currentState;

        public CanvasManipulator GetToolManipulator()
        {
            Initialize();
            return m_CurrentManipulator;
        }

        public void SetModel(Model model)
        {
            m_CurrentModel = model;
            Initialize();
            m_Settings?.SetModel(m_CurrentModel);
        }

        protected virtual void Initialize()
        {
            m_PanTool ??= new PanTool();
            m_CurrentManipulator ??= new PaintCanvasToolManipulator<T>(m_CurrentModel, new Vector2Int(2, 2));
            m_Settings ??= new PaintingManipulatorSettings<T>(m_PanTool, this, m_CurrentManipulator);
            m_CurrentModel.OnArtifactSelected += OnArtifactSelected;
            m_CurrentModel.SetDefaultRefineTool(m_PanTool);
        }

        protected virtual void OnArtifactSelected(Artifact artifact)
        {
            if (m_CurrentModel && m_CurrentModel.isRefineMode)
                m_CurrentModel.SetActiveTool(m_PanTool);
        }

        public virtual bool EvaluateEnableState(Artifact artifact)
        {
            return m_CurrentModel.isRefineMode && ArtifactCache.IsInCache(artifact);
        }

        public void ActivateOperators()
        {
            if (!m_CurrentModel) 
                return;

            var opMask = m_CurrentModel.CurrentOperators.Find(x => x.GetType() == typeof(T)) ??
                m_CurrentModel.AddOperator<T>();

            if (opMask != null && !opMask.Enabled())
            {
                opMask.Enable(true);
                m_CurrentModel.UpdateOperators(opMask);
            }
        }

        public VisualElement GetToolView()
        {
            return m_Settings?.GetSettings();
        }

        public ICanvasTool.ToolButtonData GetToolData()
        {
            return new ICanvasTool.ToolButtonData()
            {
                Name = "muse-brush-tool-button",
                Label = "",
                Icon = "paint-brush",
                Tooltip = TextContent.controlMaskToolTooltip
            };
        }

        public VisualElement GetSettings()
        {
            return m_Settings?.GetSettings();
        }

        public float radius
        {
            get => m_CurrentManipulator.radius;
            set => m_CurrentManipulator.radius = value;
        }

        public void SetEraserMode(bool isEraser)
        {
            m_CurrentManipulator?.SetEraserMode(isEraser);
        }

        public void Clear()
        {
            m_CurrentManipulator?.ClearPainting();
        }
    }

    internal class PaintingManipulatorSettings<T> where T: class, IMaskOperator
    {
        MuseToolbar m_MuseToolbar;
        BrushTool<T> m_BrushTool;
        PanTool m_PanTool;
        PaintCanvasToolManipulator<T> m_ToolManipulator;
        bool m_IsInitialized;
        List<MuseShortcut> m_Shortcuts;
        Model m_CurrentModel;

        private PaintingManipulatorSettings() { }

        public PaintingManipulatorSettings(PanTool panTool, BrushTool<T> brushTool, PaintCanvasToolManipulator<T> paintManipulator)
        {
            m_PanTool = panTool;
            m_BrushTool = brushTool;
            m_ToolManipulator = paintManipulator;
            Init();
        }

        public void SetModel(Model model)
        {
           m_CurrentModel = model;
        }

        void Init()
        {
            if (m_IsInitialized)
                return;

            m_MuseToolbar = new MuseToolbar();

            m_MuseToolbar.SizeSlider.label = "Radius";
            m_MuseToolbar.SizeSlider.tooltip = TextContent.controlMaskBrushSizeTooltip;
            m_MuseToolbar.SizeSlider.incrementFactor = 0.1f;
            m_MuseToolbar.SizeSlider.formatString = "F1";
            m_MuseToolbar.SizeSlider.lowValue = 3.0f;
            m_MuseToolbar.SizeSlider.highValue = 50.0f;
            m_MuseToolbar.SizeSlider.value = m_BrushTool.radius;
            m_MuseToolbar.SizeSlider.style.width = 150.0f;

            m_MuseToolbar.SizeSlider.RegisterValueChangedCallback(evt =>
            {
                m_BrushTool.radius = evt.newValue;
            });

            m_MuseToolbar.EraseBtn.clickable.clicked += SelectEraser;

            m_MuseToolbar.PaintBtn.clickable.clicked += SelectPaintBrush;

            m_MuseToolbar.DeleteBtn.clickable.clicked += ClearDoodle;

            m_MuseToolbar.PanBtn.clickable.clicked += SelectPan;

            m_MuseToolbar.RegisterCallback<AttachToPanelEvent>(OnAttach);
            m_MuseToolbar.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            m_IsInitialized = true;
        }

        void OnAttach(AttachToPanelEvent evt)
        {
            m_CurrentModel.OnActiveToolChanged += OnActiveToolChanged;
            OnActiveToolChanged(m_CurrentModel.ActiveTool);
        }

        internal virtual void OnActiveToolChanged(ICanvasTool canvasTool)
        {
            AddShortcuts();
            if (canvasTool is not BrushTool<T>)
            {
                m_MuseToolbar.SelectButton(m_MuseToolbar.PanBtn);
                m_MuseToolbar.PanBtn.Focus();
            }
        }

        void OnDetach(DetachFromPanelEvent evt)
        {
            RemoveShortcuts();

            m_CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;
        }

        void AddShortcuts()
        {
            RemoveShortcuts();

            m_Shortcuts = new List<MuseShortcut>();

            if (m_CurrentModel.ActiveTool is BrushTool<T>)
            {
                m_Shortcuts.AddRange(GetBrushToolShortcuts());
            }

            m_Shortcuts.AddRange(GetCommonShortcuts());

            MuseShortcuts.AddShortcuts(m_Shortcuts);
        }

        IEnumerable<MuseShortcut> GetCommonShortcuts()
        {
            var shortcuts = new List<MuseShortcut>()
            {
                new("Select Pan", SelectPan, KeyCode.Alpha1, source: m_MuseToolbar),
                new("Select Pan", SelectPan, KeyCode.P, source: m_MuseToolbar),
                new("Select Paint Brush", SelectPaintBrush, KeyCode.Alpha2, source: m_MuseToolbar),
                new("Select Paint Brush", SelectPaintBrush, KeyCode.B, source: m_MuseToolbar),
                new("Select Eraser", SelectEraser, KeyCode.Alpha3, source: m_MuseToolbar),
                new("Select Eraser", SelectEraser, KeyCode.E, source: m_MuseToolbar)
            };

            return shortcuts;
        }

        IEnumerable<MuseShortcut> GetBrushToolShortcuts()
        {
            var shortcuts = new List<MuseShortcut>()
            {
                new("Increase Brush Size", OnIncreaseBrushSize, KeyCode.UpArrow, source: m_MuseToolbar),
                new("Decrease Brush Size", OnDecreaseBrushSize, KeyCode.DownArrow, source: m_MuseToolbar)

            };

            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                shortcuts.Add(new MuseShortcut("Clear", ClearDoodle, KeyCode.Backspace, KeyModifier.Action, source: m_MuseToolbar));
            }
            else
            {
                shortcuts.Add(new MuseShortcut("Clear", ClearDoodle, KeyCode.Delete, source: m_MuseToolbar));
            }

            return shortcuts;
        }

        void RemoveShortcuts()
        {
            if (m_Shortcuts != null)
            {
                MuseShortcuts.RemoveShortcuts(m_Shortcuts);
            }
        }

        public VisualElement GetSettings()
        {
            return m_MuseToolbar;
        }

        void OnIncreaseBrushSize()
        {
            if (!isFocused)
                return;

            m_MuseToolbar.SizeSlider.value += k_RadiusStep;
        }

        const float k_RadiusStep = 3f;

        void OnDecreaseBrushSize()
        {
            if (!isFocused)
                return;

            m_MuseToolbar.SizeSlider.value -= k_RadiusStep;
        }

        void SelectPan()
        {
            if (!isFocused)
                return;

            m_MuseToolbar.SelectButton(m_MuseToolbar.PanBtn);

            if (m_CurrentModel)
                m_CurrentModel.SetActiveTool(m_PanTool);

            m_BrushTool.SetEraserMode(false);
        }

        void SelectPaintBrush()
        {
            if (!isFocused)
                return;

            m_MuseToolbar.SelectButton(m_MuseToolbar.PaintBtn);

            if (m_CurrentModel)
                m_CurrentModel.SetActiveTool(m_BrushTool);

            m_BrushTool.SetEraserMode(false);

            m_BrushTool.ActivateOperators();
            m_BrushTool.radius = m_MuseToolbar.SizeSlider.value;

        }

        void SelectEraser()
        {
            if (!isFocused)
                return;

            m_MuseToolbar.SelectButton(m_MuseToolbar.EraseBtn);

            if (m_CurrentModel)
                m_CurrentModel.SetActiveTool(m_BrushTool);

            m_BrushTool.SetEraserMode(true);

            m_BrushTool.ActivateOperators();
            m_BrushTool.radius = m_MuseToolbar.SizeSlider.value;
        }

        void ClearDoodle()
        {
            if (!isFocused)
                return;

            m_BrushTool.Clear();
            m_BrushTool.radius = m_MuseToolbar.SizeSlider.value;
        }

        bool isFocused
        {
            get
            {
                var focusedElement = m_ToolManipulator?.target?.panel?.focusController?.focusedElement;
                var toolbarFocusedElement = m_MuseToolbar?.focusController?.focusedElement;

                return (focusedElement == m_ToolManipulator?.target) || (focusedElement == toolbarFocusedElement);
            }
        }
    }
}
