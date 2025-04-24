using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AppUI.Core;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Analytics;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Custom model data.
    /// </summary>
    internal interface IModelData
    {
        /// <summary>
        /// Event raised when the data was modified.
        /// </summary>
        event Action OnModified;

        /// <summary>
        /// Save the data.
        /// </summary>
        event Action OnSaveRequested;
    }

    delegate IEnumerable<IOperator> SetOperatorDefault(IEnumerable<IOperator> currentOperators);

    /// <summary>
    /// Muse main Scriptable Object model that holds all the data.
    /// </summary>
    [Serializable]
    [Icon(IconHelper.assetIconPath)]
    public class Model : ScriptableObject, IContext, IEquatable<Model>
    {
        enum Versions
        {
            Initial = 0,
            MergedPrompts = 1,
        }
        static readonly int k_LatestVersion = (int)Versions.MergedPrompts;

        int m_Version;
        int? m_CostInMusePoints;

        bool m_SupportCostSimulation;

        internal event Action OnCloseWindowRequested;
        internal event Action OnWindowLostFocus;
        internal event Action OnCostRemoved;
        internal event Action<int?> OnCostChanged;
        internal event Action<bool> OnSupportCostSimulationChanged;
        /// <summary>
        /// Event raised when the model was modified.
        /// </summary>
        internal event Action OnModified;
        internal event Action<Artifact> OnArtifactAdded;
        internal event Action<Artifact[]> OnArtifactRemoved;
        internal event Action<string, IList<Artifact>> OnEditorDragStart;
        internal event Action<IList<(string name, IList<Artifact> artifacts)>> OnEditorMultiDragStart;
        internal event Action<IEnumerable<Artifact>, Vector3> OnItemsDropped;
        internal event Action<Artifact> OnArtifactSelected;
        internal event Action<ICanvasTool> OnActiveToolChanged;
        internal event Action<ICanvasTool> DefaultRefineToolChanged;
        internal event Action OnUpdateToolState;
        internal event Action<Texture2D, bool> OnMaskPaintDone;
        internal event Action<string> OnCurrentPromptChanged;
        internal event Action<IEnumerable<IOperator>, bool> OnOperatorUpdated;
        /// <summary>
        /// Called when removing an operator
        /// </summary>
        internal event Action<IEnumerable<IOperator>> OnOperatorRemoved;
        internal event Action OnGenerateButtonClicked;
        internal event Action<Artifact> OnExportArtifact;
        internal event Action<IList<ArtifactView>> OnMultiExport;
        internal event Action OnDeselectAll;
        internal event Action<bool> OnSetMaskSeamless;
        internal event Action<int> OnModeChanged;
        internal event Action<Artifact> OnFrameArtifactRequested;
        internal event Action OnDispose;
        internal event Action<Artifact> OnRefineArtifact;
        internal event Action<Artifact> OnCanvasRefineArtifact;
        internal event Action<Artifact> OnFinishRefineArtifact;
        internal event Action<Artifact> OnSetReferenceOperator;
        internal event Action<VisualElement, int, ToolbarPosition> OnAddToolbar;
        internal event Action<VisualElement> OnRemoveToolbar;
        internal event SetOperatorDefault OnSetOperatorDefaults;
        internal event Func<long, string, bool> OnServerError;
        internal event Action<VisualElement> OnLeftOverlayChanged;
        internal event Action<VisualElement> OnRightOverlayChanged;

        internal string guid = Guid.Empty.ToString();

        internal static bool shouldShowCost =>
#if UNITY_EDITOR
            UnityEditor.Unsupported.IsDeveloperMode();
#else
            false;
#endif

        internal void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            m_Version = k_LatestVersion;
        }

        void OnEnable()
        {
            if (string.IsNullOrEmpty(currentMode))
            {
                var mode = PlayerPrefs.GetInt("Muse.Mode", 0);
                currentMode = ModesFactory.GetModeKeyFromIndex(mode);
            }

            if (guid == Guid.Empty.ToString())
                guid = Guid.NewGuid().ToString();

            foreach (var modelData in m_Data)
            {
                if (modelData is not null)
                    modelData.OnSaveRequested += () => OnModified?.Invoke();
            }
        }

        internal List<Artifact> AssetsData
        {
            get => isRefineMode ? refinedArtifact.history : assetsData;
            private set => assetsData = value;
        }

        internal List<Artifact> UnrefinedAssetsData => assetsData;

        internal List<Artifact> DraggedArtifacts { get; private set; } = new List<Artifact>();

        internal string CurrentMode
        {
            get => currentMode;
            private set => currentMode = value;
        }

        internal int? CostInMusePoints
        {
            get => m_CostInMusePoints;
            set
            {
                m_CostInMusePoints = value;
                OnCostChanged?.Invoke(m_CostInMusePoints);
                if (value == null)
                {
                    OnCostRemoved?.Invoke();
                }
            }

        }

        internal bool SupportCostSimulation
        {
            get => m_SupportCostSimulation;
            set
            {
                var changed = m_SupportCostSimulation != value;
                m_SupportCostSimulation = value;
                if (changed)
                    OnSupportCostSimulationChanged?.Invoke(m_SupportCostSimulation);
            }
        }

        internal ICanvasTool ActiveTool { get; private set; } = null;

        internal ICanvasTool DefaultRefineTool { get; private set; } = null;

        internal void DeselectAll()
        {
            OnDeselectAll?.Invoke();
        }

        [SerializeReference]
        List<Artifact> assetsData;

        [SerializeField]
        string currentMode;

        [SerializeReference]
        List<IModelData> m_Data = new List<IModelData>();

        [SerializeReference]
        Artifact preRefinedArtifact; // The selected artifact prior to entering refine mode

        [SerializeReference]
        Artifact refinedArtifact;

        [SerializeReference]
        Artifact selectedArtifact;

        [SerializeReference]
        List<IOperator> m_Operators;

        [SerializeReference]
        List<IOperator> m_PreRefineOperators;

        [SerializeField]
        List<ExportedArtifact> m_ExportedArtifacts;

        /// <summary>
        /// Get the list of operators currently being used
        /// </summary>
        internal List<IOperator> CurrentOperators => currentOperators.ToList();

        /// <summary>
        /// The artifact currently being refined.
        /// </summary>
        internal Artifact RefinedArtifact => refinedArtifact;

        internal Artifact SelectedArtifact => selectedArtifact;

        internal VisualElement LeftOverlay { get; private set; }

        internal VisualElement RightOverlay { get; private set; }

        internal bool isRefineMode => refinedArtifact != null;

        internal T GetData<T>() where T : IModelData, new()
        {
            var data = m_Data.Find(d => d is T);
            if (data == null)
            {
                data = new T();
                m_Data.Add(data);

                data.OnSaveRequested += () => OnModified?.Invoke();
            }

            return (T)data;
        }

        internal void DeleteData<T>()
        {
            var index = m_Data.FindIndex(d => d is T);
            if (index > 0)
                m_Data.RemoveAt(index);
        }

        internal void GenerateButtonClicked()
        {
            OnGenerateButtonClicked?.Invoke();
        }

        List<IOperator> modeDefaultOperators => ModesFactory.GetMode(currentMode).Select(op => op.Clone()).ToList();
        IEnumerable<IOperator> currentOperators
        {
            get
            {
                if (m_Operators == null || !m_Operators.Any())
                    m_Operators = modeDefaultOperators;
                return m_Operators;
            }
        }

        internal IEnumerable<IOperator> preRefineOperators => m_PreRefineOperators;

        /// <summary>
        /// Set or replace operators in the nodes list.
        /// </summary>
        /// <param name="operators">Operators to update.</param>
        /// <param name="set">Set or update the operators</param>
        internal void UpdateOperators(IEnumerable<IOperator> operators, bool set)
        {
            if (set)
            {
                m_Operators = operators == null ? currentOperators.ToList() : operators.ToList();
            }
            else
            {
                foreach (var op in operators)
                {
                    var index = m_Operators.FindIndex(o => o.GetOperatorKey() == op.GetOperatorKey());
                    if (index >= 0)
                        m_Operators[index] = op;
                    else
                        m_Operators.Add(op);
                }
            }

            OnOperatorUpdated?.Invoke(m_Operators, set);
        }

        /// <summary>
        /// Set or replace operators in the nodes list.
        /// </summary>
        /// <param name="operators">Operators to update.</param>
        internal void UpdateOperators(params IOperator[] operators)
        {
            UpdateOperators(operators, false);
        }

        /// <summary>
        /// Remove operators in the nodes list.
        /// </summary>
        /// <param name="operators">Operators to remove.</param>
        internal void RemoveOperators(params IOperator[] operators)
        {
            var removed = m_Operators.RemoveAll(operators.Contains);
            if (removed > 0)
                OnOperatorRemoved?.Invoke(operators);
        }

        internal T AddOperator<T>() where T : class, IOperator
        {
            var op = modeDefaultOperators.GetOperator<T>();
            UpdateOperators(op);
            return op;
        }

        internal void SetOperatorEnable<T>(bool enabled) where T : class, IOperator
        {
            var op = m_Operators.GetOperator<T>();
            if (op != null)
            {
                op.Enable(enabled);
                UpdateOperators(op);
            }
        }

        internal void SetOperatorVisibility(IOperator op, bool visible)
        {
            if (op != null)
            {
                op.Hidden = !visible;
                UpdateOperators(op);
            }
        }

        internal void SetLeftOverlay(VisualElement overlay)
        {
            LeftOverlay = overlay;
            OnLeftOverlayChanged?.Invoke(overlay);
        }

        internal void SetRightOverlay(VisualElement overlay)
        {
            RightOverlay = overlay;
            OnRightOverlayChanged?.Invoke(overlay);
        }

        /// <summary>
        /// Sets the selected artifact.
        /// </summary>
        /// <param name="artifact">The artifact to select.</param>
        /// <param name="force">Force the selection change even if the artifact is the same as current selection.</param>
        internal void ArtifactSelected(Artifact artifact, bool force = false)
        {
            if (SelectedArtifact == artifact && !force)
                return;

            SelectedArtifact?.UnregisterFromEvents(this);
            selectedArtifact = artifact;
            SelectedArtifact?.RegisterToEvents(this);
            OnArtifactSelected?.Invoke(SelectedArtifact);

            SetOperatorDefaults();
        }

        internal void AddAsset(Artifact artifact)
        {
            AssetsData ??= new List<Artifact>();

            if (!string.IsNullOrEmpty(artifact.Guid) && AssetsData.Contains(artifact)) return;

            AssetsData.Add(artifact);

            OnArtifactAdded?.Invoke(artifact);
            OnModified?.Invoke();
        }

        bool IsArtifactUnused(Artifact artifact)
        {
            return !assetsData.Any(assetsArtifact =>
                !ReferenceEquals(assetsArtifact, artifact) // Don't check the artifact itself
                && assetsArtifact.history.Contains(artifact)); // Check if the artifact is present in any history
        }

        /// <summary>
        /// Remove give artifacts from this model.
        /// </summary>
        /// <param name="artifacts">Artifacts to remove from model.</param>
        internal void RemoveAssets(params Artifact[] artifacts)
        {
            if (artifacts.Length == 0)
                return;

            List<Artifact> removeFromCache = new();
            Artifact selected = null;
            Artifact setAsThumbnail = null;
            int setAsThumbnailIndex = RefinedArtifactGenerationsIndex; // Keep the generations index we might be replacing
            var finishRefine = false;

            // Remove from cache (will only actually be removed from cache if unused elsewhere
            if (isRefineMode)
                removeFromCache.AddRange(artifacts);
            else
                removeFromCache.AddRange(artifacts.SelectMany(a => a.history)); // Clear history when removing from generations

            var next = artifacts.Max(a => AssetsData.FindIndex(ad => ad.Guid == a.Guid));

            // Delete from generation or refinement list (AssetsData)
            foreach (var artifact in artifacts)
            {
                // Check by artifact reference rather then guid otherwise we might delete multiple top level items that have
                // previously been branched off.
                foreach (var assetData in AssetsData)
                {
                    assetData.children.RemoveAll(a => ReferenceEquals(a, artifact));
                }
                AssetsData.RemoveAll(a => ReferenceEquals(a, artifact));
                m_ExportedArtifacts?.RemoveAll(e => e.MuseGuid == artifact.Guid);
            }

            // After everything has been deleted, check if we need to set a new thumbnail or select a new item
            // Otherwise we might select an item that will end up being deleted.
            if (isRefineMode && artifacts.Contains(refinedArtifact))
            {
                // If we're deleting the root artifact, we need to set a new root artifact that will appear in the Generations list
                if (refinedArtifact.history.Count < 1)
                {
                    assetsData.RemoveAll(a => ReferenceEquals(a, refinedArtifact));
                    finishRefine = true;
                }
                else
                {
                    setAsThumbnail = refinedArtifact.history.Last();
                }
            }

            if (artifacts.Contains(SelectedArtifact) && !finishRefine && AssetsData.Count > 0)
            {
                // Select the closest artifact to the one we're deleting
                selected = AssetsData[Mathf.Clamp(next, 0, AssetsData.Count - 1)];
            }

            ArtifactCache.Delete(removeFromCache.Where(IsArtifactUnused));

            OnArtifactRemoved?.Invoke(artifacts.ToArray());
            OnModified?.Invoke();

            // Set new state
            if (finishRefine)
                FinishRefineArtifact();
            else if (setAsThumbnail != null)
                SetAsThumbnail(setAsThumbnail, setAsThumbnailIndex);
            else
                ArtifactSelected(selected);
        }

        internal void EditorStartDrag(string type, IList<Artifact> artifact)
        {
            OnEditorDragStart?.Invoke(type, artifact);
        }

        internal void EditorStartMultiDrag(IList<(string name, IList<Artifact> artifacts)> artifactsList)
        {
            OnEditorMultiDragStart?.Invoke(artifactsList);
        }

        internal void DragStart(IEnumerable<Artifact> artifacts)
        {
            DraggedArtifacts.Clear();
            DraggedArtifacts.AddRange(artifacts);
        }

        internal void DragEnd()
        {
            DraggedArtifacts.Clear();
        }

        internal void DragEnd(IEnumerable<Artifact> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                DraggedArtifacts.Remove(artifact);
            }
        }

        internal void DropItems(IEnumerable<Artifact> artifacts, Vector3 position)
        {
            OnItemsDropped?.Invoke(artifacts, position);
        }

        internal void SetActiveTool(ICanvasTool tool)
        {
            ActiveTool = tool;
            OnActiveToolChanged?.Invoke(ActiveTool);
        }

        internal void SetDefaultRefineTool(ICanvasTool tool)
        {
            DefaultRefineTool = tool;
            DefaultRefineToolChanged?.Invoke(DefaultRefineTool);
        }

        internal void MaskPaintDone(Texture2D texture, bool isClear)
        {
            OnMaskPaintDone?.Invoke(texture, isClear);
        }

        internal void SetMaskSeamless(bool seamless)
        {
            OnSetMaskSeamless?.Invoke(seamless);
        }

        internal void ExportArtifact(Artifact artifact)
        {
            OnExportArtifact?.Invoke(artifact);
        }

        internal void MultiExport(IList<ArtifactView> artifactViews)
        {
            OnMultiExport?.Invoke(artifactViews);
        }

        internal void ModeChanged(int mode)
        {
            if (mode < 0)
                return;
            var newMode = ModesFactory.GetModeKeyFromIndex(mode);
            if (newMode != currentMode)
            {
                m_Operators = null;
                currentMode = newMode;
                OnModeChanged?.Invoke(mode);
                SupportCostSimulation = ModesFactory.IsCostSimulationSupportedForMode(currentMode);
            }
        }

        internal void RequestFrameArtifact(Artifact artifact)
        {
            OnFrameArtifactRequested?.Invoke(artifact);
        }

        internal void Dispose()
        {
            ActiveTool = null;
            OnDispose?.Invoke();
        }

        internal void RefineArtifact(Artifact artifact, bool force = false)
        {
            if (artifact?.Guid == refinedArtifact?.Guid && !force)
                return;

            preRefinedArtifact = selectedArtifact;
            m_PreRefineOperators = m_Operators.ToList();
            refinedArtifact = artifact;
            m_Operators = modeDefaultOperators.ToList();

            BookmarkManager bookmarkManager = GetData<BookmarkManager>();
            if (bookmarkManager != null)
                bookmarkManager.SetFilter(false);

            OnRefineArtifact?.Invoke(artifact);

            ArtifactSelected(refinedArtifact, true);
        }

        internal void FinishRefineArtifact()
        {
            if (refinedArtifact is null)
                return;

            var previousArtifact = refinedArtifact;
            refinedArtifact = null;
            m_Operators = m_PreRefineOperators.ToList();

            ArtifactSelected(preRefinedArtifact, true);

            preRefinedArtifact = null;
            m_PreRefineOperators = null;

            OnFinishRefineArtifact?.Invoke(previousArtifact);
        }

        internal void CanvasRefineArtifact(Artifact artifact)
        {
            OnCanvasRefineArtifact?.Invoke(artifact);
        }

        internal void SetReferenceOperator(Artifact artifact)
        {
            OnSetReferenceOperator?.Invoke(artifact);
        }

        internal void SetCurrentPrompt(string prompt)
        {
            OnCurrentPromptChanged?.Invoke(prompt);
        }

        IEnumerable<IOperator> m_StaticOperators;

        internal IEnumerable<IOperator> SetOperatorDefaults()
        {
            // Cloning the operators as we can not modify a generated artifact's operators.
            var operators = currentOperators;

            // Keep static operators for UX consistency (such as generate so that the user does not lose its settings in and out of refine mode)
            if (m_StaticOperators is null || !m_StaticOperators.Any())
                m_StaticOperators = operators.Where(o => o is GenerateOperator);

            foreach (var staticOperator in m_StaticOperators ?? Array.Empty<IOperator>())
            {
                operators = currentOperators?.Select(o =>
                    o?.GetType() == staticOperator.GetType() ? staticOperator : o).ToList();
            }

            operators = OnSetOperatorDefaults?.Invoke(operators) ?? operators;
            UpdateOperators(operators?.ToArray(), true);

            return operators;
        }

        int RefinedArtifactGenerationsIndex => assetsData.FindIndex(a => a == refinedArtifact);

        /// <summary>
        /// Set the thumbnail of the generations list to the given artifact.
        /// </summary>
        /// <param name="artifact">New artifact to be used in the generations list.</param>
        /// <param name="indexToReplace">Index in the generations list to be replaced.</param>
        void SetAsThumbnailInternal(Artifact artifact, int indexToReplace)
        {
            BookmarkManager bookmarkManager = GetData<BookmarkManager>();
            if (bookmarkManager != null && bookmarkManager.IsBookmarked(refinedArtifact))
                bookmarkManager.Bookmark(artifact, true);

            // Swap with the previous parent
            artifact.history = refinedArtifact.history.ToList();
            refinedArtifact.history.Clear();
            assetsData[indexToReplace] = artifact;

            refinedArtifact = artifact;

            ArtifactSelected(refinedArtifact);

            OnRefineArtifact?.Invoke(artifact);
        }

        /// <summary>
        /// Set the thumbnail of the generations list to the given artifact.
        /// </summary>
        /// <param name="artifact">Artifact to set.</param>
        /// <param name="indexToReplace">The index in the generations list to replace. (optional)</param>
        internal void SetAsThumbnail(Artifact artifact, int? indexToReplace = null)
        {
            indexToReplace ??= RefinedArtifactGenerationsIndex;

            SetAsThumbnailInternal(artifact, indexToReplace.Value);
        }

        internal bool IsThumbnail(Artifact artifact)
        {
            return assetsData.Contains(artifact);
        }

        /// <summary>
        /// Branch off the given artifact and add it to the generations list as a new generations.
        /// </summary>
        /// <param name="artifact">The artifact to branch off.</param>
        internal void Branch(Artifact artifact)
        {
            var clone = artifact.Clone(artifact.mode);
            assetsData.Add(clone);
            clone.history.Clear();
            clone.history = new() { clone };
            RefineArtifact(clone, true);
        }

        internal static void SendAnalytics(IAnalytic analytic)
        {
            AnalyticsManager.SendAnalytics(analytic);
        }

        internal void ServerError(long objResponseCode, string objRequestError)
        {
            OnServerError?.Invoke(objResponseCode, objRequestError);
        }

        /// <summary>
        /// Force updating the available tools state
        /// </summary>
        internal void UpdateToolState()
        {
            OnUpdateToolState?.Invoke();
        }

        ///<inheritdoc/>
        public bool Equals(Model other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) || guid == other.guid;
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Model)obj);
        }

        /// <summary>
        /// Equality operator between Models
        /// </summary>
        /// <param name="lhs">first model</param>
        /// <param name="rhs">second model</param>
        /// <returns> true if models are equal</returns>
        public static bool operator ==(Model lhs, Model rhs)
        {
            return ReferenceEquals(lhs, rhs) || (lhs != null) && lhs.Equals(rhs);
        }

        /// <summary>
        /// Inequality operator between Models
        /// </summary>
        /// <param name="lhs">first model</param>
        /// <param name="rhs">second model</param>
        /// <returns> true if models are not equal</returns>
        public static bool operator !=(Model lhs, Model rhs)
        {
            return ((object)lhs != null) && !lhs.Equals(rhs);
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), guid);
        }

        internal void AddToToolbar(VisualElement element, int index, ToolbarPosition position)
        {
            OnAddToolbar?.Invoke(element, index, position);
        }

        internal void RemoveToolbar(VisualElement element)
        {
            OnRemoveToolbar?.Invoke(element);
        }

        internal bool CheckForUpgrade()
        {
            if (m_Version < k_LatestVersion)
            {
                UpgradeFromVersion(m_Version); //upgrade derived classes
                m_Version = k_LatestVersion;
                return true;
            }

            return false;
        }

        void UpgradeFromVersion(int oldVersion)
        {
            if (oldVersion < (int)Versions.MergedPrompts)
                ConvertToMergedPromptsVersion();
        }

        private void ConvertToMergedPromptsVersion()
        {
            foreach (var artifact in assetsData)
            {
                var negPromptOp = artifact.GetOperator<NegativePromptOperator>();
                var promptOp = artifact.GetOperator<PromptOperator>();
                if (promptOp != null)
                {
                    promptOp.UpgradeVersion();
                    if (negPromptOp != null)
                        promptOp.SetNegativePrompt(negPromptOp.GetOperatorData().settings[0]);
                }
                artifact.RemoveOperator<NegativePromptOperator>();

                if (artifact == null || artifact.history == null)
                    return;
                foreach (var subArtifact in artifact.history)
                {
                    var negPrompts = subArtifact.GetOperator<NegativePromptOperator>();
                    var prompts = subArtifact.GetOperator<PromptOperator>();
                    if (prompts != null)
                    {
                        prompts.UpgradeVersion();
                        if (negPrompts != null)
                            prompts.SetNegativePrompt(negPrompts.GetOperatorData().settings[0]);
                    }
                    subArtifact.RemoveOperator<NegativePromptOperator>();
                }
            }
            m_Operators.Remove( m_Operators.GetOperator<NegativePromptOperator>());
            m_PreRefineOperators.Remove(m_PreRefineOperators.GetOperator<NegativePromptOperator>());
        }

        // Debouncing the usage update to avoid spamming the server since we send multiple requests at once
        // when generating batches of artifacts.
        Action m_UpdateUsage;
        Action m_UpdateExperimentalProgramUsage;

        internal void UpdateAnyUsages()
        {
            m_UpdateUsage ??= EventServices.IntervalDebounce(AccountInfo.Instance.UpdateUsage, 4f);
            m_UpdateUsage();

            if (ExperimentalProgram.IsConfigured)
                ExperimentalProgram.Refresh();
        }

        internal void ArtifactGenerationDone(Artifact _)
        {
            UpdateAnyUsages();
        }

        internal void AddExportedArtifact(string unityGuid, string artifactGuid)
        {
            m_ExportedArtifacts ??= new List<ExportedArtifact>();
            m_ExportedArtifacts.Add(new ExportedArtifact(unityGuid, artifactGuid));

            OnModified?.Invoke();
        }

        internal string GetExportedArtifact(string unityGuid)
        {
            m_ExportedArtifacts ??= new List<ExportedArtifact>();
            return m_ExportedArtifacts.FirstOrDefault(e => e.UnityGuid == unityGuid)?.MuseGuid;
        }

        internal Artifact GetArtifactByGuid(string guid)
        {
            return assetsData.FirstOrDefault(a => a.Guid == guid);
        }

        internal void ResetCost()
        {
            CostInMusePoints = null;
        }

        internal void CloseWindowRequested()
        {
            OnCloseWindowRequested?.Invoke();
        }

        internal void NotifyWindowLostFocus()
        {
            OnWindowLostFocus?.Invoke();
        }

        internal static bool TryGetProjectRelativePath(string path, out string relativePath)
        {
            relativePath = Path.GetRelativePath(Application.dataPath[..Application.dataPath.LastIndexOf("/")], path);

            if (!relativePath.StartsWith("Assets"))
            {
                relativePath = path;
                return false;
            }

            return true;
        }
    }
}
