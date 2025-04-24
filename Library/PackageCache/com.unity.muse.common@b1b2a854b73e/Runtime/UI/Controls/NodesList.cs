using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Services;
using Unity.Muse.Common.Utils;
using Dragger = Unity.Muse.Common.Baryon.UI.Manipulators.Dragger;

namespace Unity.Muse.Common
{
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class NodesList : VisualElement, IControl
    {
        const string k_USSClassName = "muse-nodeslist";
        const string k_DragBarUssClassName = k_USSClassName + "__dragbar";
        const string k_ContentUssClassName = k_USSClassName + "__content";
        const string k_RefineUssClassName = k_USSClassName + "__refine";
        const string k_RefineUssScrollClassName = k_USSClassName + "__scroll" + "--refine";
        const string k_SpacerUssClassName = k_USSClassName + "__spacer";
        const string k_SpacerRefineUssClassName = k_SpacerUssClassName + "-refine";
        const string k_NotificationArea = "notifications-area";

        VisualElement m_BottomSpacer;
        VisualElement m_Container;
        public VisualElement content => m_Container;
        ScrollView m_ScrollView;

        Dictionary<IOperator, VisualElement> m_OperatorMap = new(); // Maps operator to their UI
        List<IOperator> m_Operators;

        Model m_CurrentModel;

        internal VisualElement draggerElement { get; private set; }

        Dragger m_HorizontalDraggable;

        VisualElement m_VerticalScrollerDragContainer;
        int m_CurrentMode = -1;

        public event Action OnResized;

#if ENABLE_UXML_TRAITS
        internal new class UxmlFactory : UxmlFactory<NodesList, UxmlTraits> { }
#endif

        private DateTime? m_LastGenerateTime;

        /// <summary>
        /// Generation Cooldown time in seconds.
        /// </summary>
        public static float GenerateCooldownTime = 1.5f;

        public NodesList()
        {
            this.ApplyTemplate(PackageResources.nodesListTemplate);
            Init();
        }

        void Init()
        {
            m_Operators = new List<IOperator>();
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            m_ScrollView = this.Q<ScrollView>();
            m_ScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            m_ScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            m_VerticalScrollerDragContainer = m_ScrollView.verticalScroller.slider.Q(classes: BaseSlider<float>.dragContainerUssClassName);
            m_Container = this.Q<VisualElement>(classes: k_ContentUssClassName);
            draggerElement = this.Q<VisualElement>(k_DragBarUssClassName);
            m_HorizontalDraggable = new Dragger(OnResizeBarClicked, OnResizeBarDrag, OnResizeBarUp, OnResizeBarDown);
            draggerElement.AddManipulator(m_HorizontalDraggable);
            m_BottomSpacer = this.Q<VisualElement>(classes: k_SpacerUssClassName);
            var notifications = this.Q<VisualElement>(k_NotificationArea);
            notifications.Add(new SessionStatusNotifications());
            notifications.Add(new ExperimentalProgramNotifications());

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            SetModel(this.GetContext<Model>());
            this.RegisterContextChangedCallback<Model>(OnModelContextChanged);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            this.UnregisterContextChangedCallback<Model>(OnModelContextChanged);
            SetModel(null);
        }

        void OnModelContextChanged(ContextChangedEvent<Model> evt)
        {
            SetModel(evt.context);
        }

        void SubscribeToEvents()
        {
            if (!m_CurrentModel)
                return;
            UnsubscribeFromEvents();

            m_CurrentModel.OnDispose += OnModelDispose;
            m_CurrentModel.OnOperatorUpdated += OnOperatorUpdated;
            m_CurrentModel.OnOperatorRemoved += OnOperatorRemoved;
            m_CurrentModel.OnGenerateButtonClicked += OnGenerateButtonClicked;
            m_CurrentModel.OnModeChanged += OnModeChanged;
            m_CurrentModel.OnSetReferenceOperator += OnSetReferenceOperator;
            m_CurrentModel.OnRefineArtifact += OnRefineArtifact;
            m_CurrentModel.OnFinishRefineArtifact += OnRefineArtifact;
            UpdateView();
        }

        void UnsubscribeFromEvents()
        {
            if (!m_CurrentModel)
                return;

            m_CurrentModel.OnDispose -= OnModelDispose;
            m_CurrentModel.OnOperatorUpdated -= OnOperatorUpdated;
            m_CurrentModel.OnOperatorRemoved -= OnOperatorRemoved;
            m_CurrentModel.OnGenerateButtonClicked -= OnGenerateButtonClicked;
            m_CurrentModel.OnModeChanged -= OnModeChanged;
            m_CurrentModel.OnSetReferenceOperator -= OnSetReferenceOperator;
            m_CurrentModel.OnRefineArtifact -= OnRefineArtifact;
            m_CurrentModel.OnFinishRefineArtifact -= OnRefineArtifact;
        }

        void OnModelDispose()
        {
            SetModel(null);
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (m_VerticalScrollerDragContainer.HasPointerCapture(evt.pointerId))
                return;

            m_ScrollView.verticalScroller.experimental.animation.Start(m_ScrollView.verticalScroller.resolvedStyle.opacity, 0f, 120,
                (element, f) => element.style.opacity = f);
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            if (draggerElement.HasPointerCapture(evt.pointerId))
                return;

            m_ScrollView.verticalScroller.experimental.animation.Start(m_ScrollView.verticalScroller.resolvedStyle.opacity, 1f, 120,
                (element, f) => element.style.opacity = f);
        }

        void OnResizeBarDown(Dragger manipulator) { }

        void OnResizeBarUp(Dragger manipulator) { }

        void OnResizeBarDrag(Dragger manipulator)
        {
            var width = layout.width;
            width += manipulator.deltaPos.x;
            var totalWidth = parent.layout.width;
            width = Mathf.Clamp(width, MainUI.k_NodeListMinWidth, totalWidth - MainUI.k_AssetListMinWidth);

            if (!Mathf.Approximately(width, layout.width))
            {
                style.width = width;
                OnResized?.Invoke();
            }
        }

        void OnResizeBarClicked() { }

        void SetDefaultOperators()
        {
            m_CurrentModel.UpdateOperators(null, true);
        }

        /// <summary>
        /// Set the operators to be displayed in the list, clearing any previous ones.
        /// </summary>
        /// <param name="operators">The operators to display.</param>
        public void ResetOperators(IEnumerable<IOperator> operators)
        {
            foreach (var op in m_Operators)
            {
                op.UnregisterFromEvents(m_CurrentModel);
            }

            var previousScrollOffset = m_ScrollView.scrollOffset;
            m_Operators.Clear();
            m_Container.Clear();
            foreach (var op in operators)
            {
                if (m_CurrentMode >= 0 && op is GenerateOperator generateOperator)
                {
                    generateOperator.SetDropdownValue(m_CurrentMode);
                }

                SetOperator(op);
            }

            schedule.Execute(() => { m_ScrollView.scrollOffset = previousScrollOffset; });
        }

        /// <summary>
        /// Adds or replace, if it exists, the provided operator.
        /// </summary>
        void SetOperator(IOperator op)
        {
            // Replace operator if it exists
            RemoveOperator(op, out var index, out var insertAtIndex);
            if (op == null)
                return;
            if (index >= 0)
                m_Operators.Insert(index, op);
            else
                m_Operators.Add(op);

            op.RegisterToEvents(m_CurrentModel);

            if (!op.Enabled() || op.Hidden)
                return;
            if (insertAtIndex == -1)
                insertAtIndex = m_Container.childCount; // Insert at the end by default

            var operatorView = op.GetOperatorView(m_CurrentModel);
            if (operatorView != null)
            {
                operatorView.AddManipulator(new SessionStatusTracker(m_CurrentModel));
                m_OperatorMap[op] = operatorView;
                m_Container.Insert(insertAtIndex, operatorView);
            }
        }

        void RemoveOperator(IOperator op, out int foundIndex, out int removedAtIndex)
        {
            foundIndex = m_Operators.FindIndex(o => o.GetOperatorKey() == op.GetOperatorKey());
            removedAtIndex = -1;
            if (foundIndex >= 0)
            {
                op.UnregisterFromEvents(m_CurrentModel);
                m_OperatorMap.TryGetValue(m_Operators[foundIndex], out var view);
                if (view != null)
                {
                    removedAtIndex = m_Container.IndexOf(view);
                    if (removedAtIndex >= 0)
                        m_Container.RemoveAt(removedAtIndex);
                }

                m_Operators.RemoveAt(foundIndex);
            }
        }

        void SetView()
        {
            SetDefaultOperators();
            UpdateView();
        }

        void OnModeChanged(int modeIndex)
        {
            if (modeIndex == m_CurrentMode)
                return;

            m_CurrentMode = modeIndex;
            SetView();
        }

        void OnGenerateButtonClicked()
        {
            var currentTime = DateTime.Now;

            Cooldown();
            if (m_LastGenerateTime != null && (currentTime - m_LastGenerateTime.Value).TotalSeconds < GenerateCooldownTime)
                return;

            m_LastGenerateTime = currentTime;

            GenerationService.GenerateImage(m_CurrentModel, m_Operators, m_CurrentMode);
        }

        void Cooldown()
        {
            if (!m_CurrentModel)
                return;

            var buttonData = m_CurrentModel.GetData<GenerateButtonData>();
            if (!buttonData.isEnabled)
                return;

            buttonData.SetCooldown(this, GenerateCooldownTime);
        }

        public void SetViewContext(Model model)
        {
            this.ProvideContext(model);
        }

        public void SetModel(Model model)
        {
            if (model == m_CurrentModel)
                return;

            UnsubscribeFromEvents();

            m_CurrentModel = model;

            if (!m_CurrentModel)
                return;

            var currentMode = ModesFactory.GetModeIndexFromKey(model.CurrentMode);
            if (currentMode >= 0)
            {
                m_CurrentMode = currentMode;
            }

            SubscribeToEvents();
            SetView();
        }

        public void UpdateView()
        {
            if (m_CurrentModel == null)
            {
                return;
            }

            m_Container.EnableInClassList(k_RefineUssClassName, m_CurrentModel.isRefineMode);
            m_ScrollView.EnableInClassList(k_RefineUssScrollClassName, m_CurrentModel.isRefineMode);
            m_BottomSpacer.EnableInClassList(k_SpacerRefineUssClassName, m_CurrentModel.isRefineMode);
        }

        void OnOperatorUpdated(IEnumerable<IOperator> operators, bool set)
        {
            if (set)
            {
                ResetOperators(operators);
            }
            else
            {
                foreach (var op in operators)
                    SetOperator(op);
            }
        }

        void OnOperatorRemoved(IEnumerable<IOperator> operators)
        {
            foreach (var op in operators)
                RemoveOperator(op, out _, out _);
        }

        void OnSetReferenceOperator(Artifact artifact)
        {
            var referenceOp = m_Operators.GetOperator<ReferenceOperator>();
            if (referenceOp is null)
            {
                referenceOp = new ReferenceOperator();
                m_Operators.Add(referenceOp);
            }

            referenceOp.SetColorImage(artifact as Artifact<Texture2D>);
            m_CurrentModel.UpdateOperators(referenceOp);
            m_CurrentModel.SetActiveTool(null);
        }

        void OnRefineArtifact(Artifact obj)
        {
            UpdateView();
        }

        public void LoadDefaultTheme()
        {
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));
        }
    }
}
