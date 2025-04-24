using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class ControlToolbar : VisualElement, IControl
    {
        const string k_USSClassName = "muse-controltoolbar";
        const string k_LeftContentContainerUssClassName = k_USSClassName + "__leftarea";
        const string k_RightContentContainerUssClassName = k_USSClassName + "__rightarea";

        const string k_ActionGroupUssClassName = k_USSClassName + "__actiongroup";

        Model m_Model;
        VisualElement m_Settings;
        bool m_Initialized;

        ActionGroup m_ActionGroup;
        List<ICanvasTool> m_Tools = new();
        List<string> m_ToolNames;

        VisualElement m_LefContentContainer;
        VisualElement m_RightContentContainer;

        private ActionButton m_CloseButton;

        VisualElement m_ToolContainer;

#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<ControlToolbar, UxmlTraits> { }
#endif

        public ControlToolbar()
        {
            this.RegisterContextChangedCallback<Model>(context => SetModel(context.context));
        }

        internal List<ICanvasTool> GetTools()
        {
            return m_Tools;
        }

        public void SetModel(Model model)
        {
            if (m_Model == model)
                return;

            Unbind();
            m_Model = model;
            Bind();
        }

        void Bind()
        {
            if (!m_Initialized)
                Init();

            m_Model.OnActiveToolChanged += OnActiveToolChanged;
            m_Model.OnArtifactSelected += OnArtifactSelected;
            m_Model.OnRefineArtifact += OnArtifactSelected;
            m_Model.OnFinishRefineArtifact += OnFinishRefineArtifact;
            m_Model.OnOperatorUpdated += OnOperatorUpdated;
            m_Model.OnDispose += Unbind;
            m_Model.OnUpdateToolState += UpdateView;
            m_Model.OnAddToolbar += AddToToolbar;
            m_Model.OnRemoveToolbar += RemoveFromToolbar;

            UpdateView();
        }

        void OnOperatorUpdated(IEnumerable<IOperator> arg1, bool arg2)
        {
            RefreshTools();
        }

        void Unbind()
        {
            if(!m_Model) 
                return;

            m_Model.OnActiveToolChanged -= OnActiveToolChanged;
            m_Model.OnArtifactSelected -= OnArtifactSelected;
            m_Model.OnRefineArtifact -= OnArtifactSelected;
            m_Model.OnFinishRefineArtifact -= OnFinishRefineArtifact;
            m_Model.OnOperatorUpdated -= OnOperatorUpdated;
            m_Model.OnDispose -= Unbind;
            m_Model.OnUpdateToolState -= UpdateView;
            m_Model.OnAddToolbar -= AddToToolbar;
            m_Model.OnRemoveToolbar -= RemoveFromToolbar;
        }

        void OnActiveToolChanged(ICanvasTool obj)
        {
            UpdateView();
        }

        void OnArtifactSelected(Artifact artifact)
        {
            if (artifact is null)
                CleanToolbar();

            UpdateView();
        }

        void Init()
        {
            m_LefContentContainer ??= this.Q<VisualElement>(classes:k_LeftContentContainerUssClassName);
            m_RightContentContainer ??= this.Q<VisualElement>(classes:k_RightContentContainerUssClassName);

            m_CloseButton = new() { name = "close", icon = "caret-left", label = "Generations", tooltip = TextContent.backButtonTooltip };
            m_CloseButton.clicked += OnCloseRefining;

            AddToToolbar(m_CloseButton, 0, ToolbarPosition.Left);

            m_ActionGroup ??= this.Q<ActionGroup>(k_ActionGroupUssClassName);
            m_ActionGroup.Clear();
            m_ToolContainer = panel.visualTree.Q("control-top-content");

            UpdateView();
            m_Initialized = true;
        }

        void OnFinishRefineArtifact(Artifact artifact)
        {
            CleanToolbar();
            UpdateView();
        }

        void CleanToolbar()
        {
            RemoveSettings();
            if (m_Model)
                m_Model.SetActiveTool(null);
        }

        void RemoveSettings()
        {
            if (m_Settings != null)
            {
                Remove(m_Settings);
                m_Settings = null;
            }
        }

        void PopulateToolbarContainer()
        {
            var toolNames = AvailableToolsFactory.GetAvailableToolNames(m_Model);
            if (m_ToolNames != null && m_ToolNames.SequenceEqual(toolNames))
                return;

            m_ToolNames = toolNames;
            
            foreach (var tool in m_Tools)
            {
                (tool as IDisposable)?.Dispose();
            }            
            m_Tools = AvailableToolsFactory.GetAvailableTools(m_Model);
            m_ToolContainer.Clear();
            foreach (var tool in m_Tools)
            {
                m_ToolContainer.Add(tool.GetToolView());
            }
        }

        public void UpdateView()
        {
            PopulateToolbarContainer();
            
            if (m_Tools == null)
                return;
            
            foreach (var tool in m_Tools)
            {
                var enabled = tool.EvaluateEnableState(m_Model?.SelectedArtifact);
                
                var toolView = tool.GetToolView();
                toolView?.EnableInClassList(Styles.hiddenUssClassName, !enabled);

                if (!enabled && m_Model && m_Model.ActiveTool == tool)
                    m_Model.SetActiveTool(null);
            }

            UpdateCloseButton();
        }

        void RefreshTools() { }

        void AddToToolbar(VisualElement element, int priority, ToolbarPosition position)
        {
            var container = position == ToolbarPosition.Left ? m_LefContentContainer : m_RightContentContainer;
            try
            {
                container.Insert(priority, element);
            }
            catch (ArgumentOutOfRangeException)
            {
                container.Add(element);
            }
        }

        void RemoveFromToolbar(VisualElement element)
        {
            if (m_LefContentContainer.Contains(element))
            {
                m_LefContentContainer.Remove(element);
            }
            else if(m_RightContentContainer.Contains(element))
            {
                m_RightContentContainer.Remove(element);
            }
        }

        void OnCloseRefining()
        {
            m_Model.FinishRefineArtifact();
        }

        void UpdateCloseButton()
        {
            m_CloseButton.style.display = m_Model.isRefineMode ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    internal enum ToolbarPosition
    {
        Left,
        Right
    }
}
