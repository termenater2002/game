using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    internal struct ContextMenuAction
    {
        public int id;

        public string label;

        public string icon;

        public string shortcut;

        public bool enabled;
    }

    internal struct ActionContext
    {
        public readonly bool isMultiSelect;

        public readonly IReadOnlyList<ArtifactView> selectedArtifacts;

        public ActionContext(IReadOnlyList<ArtifactView> selectedArtifacts)
        {
            this.selectedArtifacts = selectedArtifacts;
            isMultiSelect = selectedArtifacts.Count > 1;
        }
    }

    enum Actions
    {
        UserDefined = 255,
        Delete = UserDefined + 1,
        Duplicate = UserDefined + 2,
        SwitchPreview = UserDefined + 3,
        Upscale = UserDefined + 4,
        SetAsReference = UserDefined + 5,
        Save = UserDefined + 6,
        Download = UserDefined + 7,
        CreateVariations = UserDefined + 8,
        Debug = UserDefined + 9,
        Refine = UserDefined + 10,
        SetAsThumbnail = UserDefined + 11,
        Star = UserDefined + 12,
        UnStar = UserDefined + 13,
        Branch = UserDefined + 14,
        GenerationSettings = UserDefined + 15,
        Feedback = UserDefined + 16,
        FeedbackLike = UserDefined + 17,
        RemoveBackground = UserDefined + 18,
        UsePrompt = UserDefined + 19
    }

    internal abstract class ArtifactView : VisualElement
    {
        public Artifact Artifact => m_Artifact;

        /// <summary>
        /// Texture Preview of the artifact.
        /// </summary>
        public abstract Texture Preview { get; }

        public abstract VisualElement PaintSurfaceElement { get; }

        protected Model CurrentModel;

        protected Artifact m_Artifact = null;

        protected readonly StackIndicator m_StackIndicator;

        List<ArtifactView> childrenViews = new();

        public List<ArtifactView> CreateChildrenViews()
        {
            List<Artifact> children = m_Artifact.children;
            var views = new List<ArtifactView>();
            foreach (var child in children)
            {
                views.Add(child.CreateView());
            }
            childrenViews = views;
            return views;
        }

        public ArtifactView GetSelectedArtifact(PointerMoveEvent evt)
        {
            foreach (var childView in childrenViews)
            {
                var childBounds = childView.worldBound;
                if (childBounds.Contains(evt.position))
                {
                    return childView;
                }
            }
            return this;
        }

        public virtual bool TryGoToRefineMode()
        {
            return false;
        }

        protected void OnActionMenu(VisualElement menuAnchor)
        {
            if (!CurrentModel)
                return;

            CurrentModel.DeselectAll();
            CurrentModel.ArtifactSelected(Artifact);
            var actionContext = new ActionContext(new List<ArtifactView> { this });

            var availableActions = GetAvailableActions(actionContext);
            if (!availableActions.Any())
                return;

            var menuBuilder = MenuBuilder.Build(menuAnchor).SetPlacement(PopoverPlacement.BottomEnd);
            menuBuilder.dismissed += (_, _) => MenuDismissed();
            var actionIds = new List<int>();

            foreach (var availableAction in availableActions)
            {
                if (actionIds.Contains(availableAction.id)) continue;
                actionIds.Add(availableAction.id);
                menuBuilder.AddAction(
                    availableAction.id,
                    availableAction.label,
                    availableAction.icon,
                    availableAction.shortcut,
                    evt => PerformAction(availableAction.id, actionContext, evt));
                menuBuilder.currentMenu
                    .ElementAt(menuBuilder.currentMenu.childCount - 1)
                    .SetEnabled(availableAction.enabled);
            }

            menuBuilder.Show();
        }

        protected virtual void MenuDismissed() { }

        internal void DeleteCurrentModel()
        {
            if (GlobalPreferences.deleteWithoutWarning)
                DoDelete();
            else
            {
                var checkbox = new Checkbox
                {
                    label = TextContent.deleteDialogOkDontShowAgain,
                    style =
                    {
                        flexGrow = 1
                    }
                };
                var dialog = new AlertDialog
                {
                    title = TextContent.deleteDialogTitle,
                    description = TextContent.deleteDialogMessage,
                    variant = AlertSemantic.Destructive
                };
                dialog.SetPrimaryAction(2, TextContent.deleteDialogOk, () =>
                {
                    if (checkbox.value == CheckboxState.Checked)
                        GlobalPreferences.deleteWithoutWarning = true;
                    DoDelete();
                });
                dialog.SetCancelAction(0, TextContent.cancel);
                dialog.actionContainer.Insert(0, checkbox);
                var modal = Modal.Build(this, dialog);
                modal.Show();
            }
        }

        void DoDelete()
        {
            CurrentModel.RemoveAssets(Artifact);
        }

        /// <summary>
        /// Perform action to perform on the selected artifact.
        /// </summary>
        /// <param name="actionId">The action to perform.</param>
        /// <param name="context">The action context.</param>
        /// <param name="pointerEvent">The pointer event at the source of the action.</param>
        public virtual void PerformAction(int actionId, ActionContext context, IPointerEvent pointerEvent)
        {
            switch ((Actions)actionId)
            {
                case Actions.Star:
                    CurrentModel.GetData<BookmarkManager>().Bookmark(m_Artifact, true);
                    break;
                case Actions.UnStar:
                    CurrentModel.GetData<BookmarkManager>().Bookmark(m_Artifact, false);
                    break;
                case Actions.Save:
#if UNITY_EDITOR
                    CurrentModel.ExportArtifact(m_Artifact);
#endif
                    break;
                case Actions.Refine:
                    CurrentModel.RefineArtifact(m_Artifact);
                    break;
                case Actions.Delete:
                    DeleteCurrentModel();
                    break;
                case Actions.GenerationSettings:
                    GenerationSettings.ShowGenerationSettings(m_Artifact, this, CurrentModel);
                    break;
                case Actions.Branch:
                    CurrentModel.Branch(m_Artifact);
                    break;
                case Actions.CreateVariations:
                    CurrentModel.GetData<BookmarkManager>().SetFilter(false);
                    var numVariations = CurrentModel.CurrentOperators.GetOperator<GenerateOperator>()?.GetCount() ?? 4;
                    (m_Artifact as IVariateArtifact)?.Variate(CurrentModel, numVariations);
                    break;
                case Actions.SetAsThumbnail:
                    CurrentModel.SetAsThumbnail(m_Artifact);
                    break;
                case Actions.Upscale:
                    CurrentModel.GetData<BookmarkManager>().SetFilter(false);
                    (m_Artifact as IUpscaleArtifact)?.Upscale(CurrentModel);
                    break;
                case Actions.SetAsReference:
                    CurrentModel.SetReferenceOperator(m_Artifact);
                    break;
                case Actions.RemoveBackground:
                    CurrentModel.GetData<BookmarkManager>().SetFilter(false);
                    (m_Artifact as IRemoveBackgroundArtifact)?.RemoveBackground(CurrentModel);
                    break;
                case Actions.UsePrompt:
                    var toOperator = CurrentModel.CurrentOperators.GetOperator<PromptOperator>();
                    var fromOperator = m_Artifact.GetOperators().GetOperator<PromptOperator>();
                    toOperator.SetPrompt(fromOperator.GetPrompt());
                    toOperator.SetNegativePrompt(fromOperator.GetNegativePrompt());
                    break;
            }
        }

        public virtual bool TrySaveAsset(string directory, Action<string> onExport = null) => false;

        public virtual IEnumerable<ContextMenuAction> GetAvailableActions(ActionContext context) => new List<ContextMenuAction>();

        public ArtifactView(Artifact artifact)
        {
            m_Artifact = artifact;

            m_StackIndicator = new StackIndicator();
            CurrentModel = this.GetContext<Model>();
            this.RegisterContextChangedCallback<Model>(ContextChanged);
        }

        void ContextChanged(ContextChangedEvent<Model> evt)
        {
            SetModel(evt.context);
        }

        public void SetModel(Model model)
        {
            if (CurrentModel == model)
                return;

            UnSubscribeFromModelEvents();
            CurrentModel = model;
            SubscribeToModelEvents();
        }

        protected virtual void SubscribeToModelEvents()
        {
            if (CurrentModel)
            {
                CurrentModel.OnArtifactRemoved += OnArtifactRemoved;
                CurrentModel.GetData<BookmarkManager>().OnModified += OnBookmarkChanged;
            }
        }

        protected virtual void UnSubscribeFromModelEvents()
        {
            if (CurrentModel)
            {
                CurrentModel.OnArtifactRemoved -= OnArtifactRemoved;
                CurrentModel.GetData<BookmarkManager>().OnModified -= OnBookmarkChanged;
            }
        }

        void OnArtifactRemoved(Artifact[] artifacts)
        {
            if (artifacts.Contains(m_Artifact))
            {
                UnSubscribeFromModelEvents();
            }
        }

        public virtual void DragEditor() { }

        public virtual (string name, IList<Artifact> artifacts) GetArtifactsAndType()
        {
            return (null, null);
        }

        public virtual void UpdateView()
        {
            var count = Artifact.history.Count;
            EnableInClassList("muse-artifact--with-refinements",
                CurrentModel && !CurrentModel.isRefineMode && count > 1);
            if (m_StackIndicator.parent  == null)
                Add(m_StackIndicator);
            m_StackIndicator.count = count;
        }

        /// <summary>
        /// Refine clicked action
        /// </summary>
        protected void OnRefineClicked()
        {
            // Need to schedule a frame away otherwise it seems like the focus is not released properly leading to issues
            // such as having to click on the "Back to generations" button in refinement mode twice in order to work.
            var done = false;
            schedule.Execute(() =>
            {
                // Need to add a done check here otherwise the event gets called multiple times on click
                // and later on can get called seemingly without reason which leads to going in and out of refinement
                // constantly.
                if (done) return;
                done = true;
                CurrentModel.RefineArtifact(m_Artifact);
            }).ExecuteLater(1);
        }

        protected virtual void OnBookmarkChanged(){}
    }
}
