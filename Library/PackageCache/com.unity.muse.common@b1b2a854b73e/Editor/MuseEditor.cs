using System;
using System.Collections.Generic;
using System.IO;
using Unity.Muse.AppUI.UI;
using Unity.Muse.Common.Account;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Editor
{
    class MuseEditor : EditorWindow, IHasCustomMenu
    {
        public MainUI mainUI;
        IPanel m_Panel;

        public Model CurrentModel;
        public Model DiscardModel;

        [SerializeField]
        string m_AssetPath;
        [SerializeField]
        string m_Mode;

        MuseShortcut m_SaveShortcut;
        Vector2 m_MinSizeWindow = new(500f, 350f);

        string defaultWindowTitle => TextContent.DefaultAssetName(ModesFactory.GetModeData(m_Mode)?.title ?? "Muse Generator");

        bool m_Maximized;

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Muse/Project Settings"), false, () => SettingsService.OpenProjectSettings("Project/Muse"));
        }

        void OnEnable()
        {
            m_SaveShortcut = new MuseShortcut("Save Changes", SaveChanges, KeyCode.S, KeyModifier.Action, source: rootVisualElement);
            MuseShortcuts.AddShortcut(m_SaveShortcut);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            minSize = m_MinSizeWindow;
            saveChangesMessage = TextContent.savePopupMessage;
        }

        void OnDisable()
        {
            MuseShortcuts.RemoveShortcut(m_SaveShortcut);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (CurrentModel)
                CurrentModel.Dispose();
        }

        void CreateGUI()
        {
            if (!CurrentModel && !string.IsNullOrEmpty(m_AssetPath))
                CurrentModel = AssetDatabase.LoadAssetAtPath<Model>(m_AssetPath);

            if (!CurrentModel)
            {
                if (!string.IsNullOrEmpty(m_Mode))
                {
                    CurrentModel = CreateInstance<Model>();
                    CurrentModel.Initialize();
                    CurrentModel.ModeChanged(ModesFactory.GetModeIndexFromKey(m_Mode));
                }

                if (!CurrentModel)
                {
                    Close();
                    return;
                }
            }

            // If the model has been upgraded to a new version, we need to save it.
            if (CurrentModel.CheckForUpgrade())
            {
                EditorUtility.SetDirty(CurrentModel);
                AssetDatabase.SaveAssetIfDirty(CurrentModel);
            }

            m_Mode = CurrentModel.CurrentMode;
            m_Panel = rootVisualElement.panel;

            if (ModesFactory.GetModeData(m_Mode).GetValueOrDefault().type == null)
            {
                ModesFactory.ModesChanged += Init;
                return;
            }

            Init();
        }

        void Init() {
            ModesFactory.ModesChanged -= Init;
            DiscardModel = Instantiate(CurrentModel);

            CurrentModel.OnEditorDragStart += EditorDragStart;
            CurrentModel.OnEditorMultiDragStart += EditorMultiDragStart;
            CurrentModel.OnExportArtifact += OnExportArtifact;
            CurrentModel.OnMultiExport += OnMultiExport;
            CurrentModel.OnModified += OnModelDataModified;
            CurrentModel.OnGenerateButtonClicked += OnGenerateButtonClicked;
            CurrentModel.OnCloseWindowRequested += Close;
            EditorApplication.quitting += Close;

            rootVisualElement.ProvideContext(CurrentModel);
            var mainui = ResourceManager.Load<VisualTreeAsset>(PackageResources.mainUITemplate);
            mainui.CloneTree(rootVisualElement);
            mainUI = rootVisualElement.Q<MainUI>();
            rootVisualElement.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));
            var museRoot = rootVisualElement.Q<Panel>("muse-root");
            museRoot.theme = EditorGUIUtility.isProSkin ? "editor-dark" : "editor-light";
            museRoot.AddToClassList("unity-editor");
            AccountController.Register(this);

            if (!string.IsNullOrEmpty(m_Mode))
            {
                int mode = ModesFactory.GetModeIndexFromKey(m_Mode);
                CurrentModel.ModeChanged(mode);
            }

            CurrentModel.OnModeChanged += OnModeChanged;
            UpdateTitle();
        }

        internal void OnModelDataModified()
        {
            if (Preferences.autoSave)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(CurrentModel)))
                    hasUnsavedChanges = true;
                else
                    SaveChanges();
            }
            else
                hasUnsavedChanges = true;
        }

        void OnModeChanged(int obj)
        {
            m_Mode = ModesFactory.GetModeKeyFromIndex(obj);
        }

        void OnDestroy()
        {
            if (!CurrentModel)
                return;

            CurrentModel.Dispose();
            EditorUtility.SetDirty(CurrentModel);
            AssetDatabase.SaveAssetIfDirty(CurrentModel);
            ArtifactCache.Dispose();
            ReleaseTextures();

            CurrentModel.OnEditorDragStart -= EditorDragStart;
            CurrentModel.OnEditorMultiDragStart -= EditorMultiDragStart;
            CurrentModel.OnExportArtifact -= OnExportArtifact;
            CurrentModel.OnMultiExport -= OnMultiExport;
            CurrentModel.OnModified -= OnModelDataModified;
            CurrentModel.OnGenerateButtonClicked -= OnGenerateButtonClicked;
            CurrentModel.OnCloseWindowRequested -= Close;
            EditorApplication.quitting -= Close;
        }

        void ReleaseTextures()
        {
            ObjectUtils.Release(m_Panel);
            m_Panel = null;
        }

        void EditorDragStart(string type, IList<Artifact> artifacts)
        {
            if (artifacts == null)
                return;

            foreach (var artifact in artifacts)
            {
                if (!ArtifactCache.IsInCache(artifact))
                    return;
            }

            var handler = DragAndDropFactory.CreateHandler(type, artifacts);
            if (handler == null)
                return;

            handler.ArtifactDropped += (path, artifact) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                AddExportedArtifact(path, artifact.Guid);
            };

            ArtifactDragAndDropHandler.StartDrag(handler, type);
        }

        void EditorMultiDragStart(IList<(string name, IList<Artifact> artifacts)> items)
        {
            if (items == null || items.Count == 0)
                return;

            var handlers = new List<IArtifactDragAndDropHandler>();
            foreach (var item in items)
            {
                foreach (var artifact in item.artifacts)
                {
                    if (!ArtifactCache.IsInCache(artifact))
                        return;
                }

                var h = DragAndDropFactory.CreateHandler(item.name, item.artifacts);
                if (h != null)
                {
                    h.ArtifactDropped += (path, artifact) =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            return;
                        }

                        AddExportedArtifact(path, artifact.Guid);
                    };

                    handlers.Add(h);
                }
            }

            var handler = DragAndDropFactory.CreateMultiHandler(handlers);
            ArtifactDragAndDropHandler.StartDrag(handler, "Multiple Elements");
        }

        public void SetContext(Model model)
        {
            m_AssetPath = AssetDatabase.GetAssetPath(model);
            CurrentModel = model;

            if (string.IsNullOrEmpty(m_AssetPath))
                hasUnsavedChanges = true;
        }

        void OnExportArtifact(Artifact artifact)
        {
            if (artifact == null)
                return;

            var exporter = ArtifactExporterFactory.instance.GetExporterForType(artifact.GetType());
            if (exporter == null)
            {
                Debug.Log($"Couldn't find exporter for {artifact.GetType()} type.");
                return;
            }

            var extension = exporter.Extension;
            var artifactName = exporter.GetSaveFileName(artifact);
            var directory = Application.dataPath;
            var path = ExporterHelpers.GetUniquePath(directory, artifactName, extension);
            path = EditorUtility.SaveFilePanel("Save Artifact", Application.dataPath, Path.GetFileNameWithoutExtension(path), extension);
            if (string.IsNullOrEmpty(path))
                return;

            artifact.ExportToPath(path, (exportedPath) =>
            {
                AddExportedArtifact(exportedPath, artifact.Guid);
            });
        }

        void OnMultiExport(IList<ArtifactView> artifactViews)
        {
            if (artifactViews == null)
                return;

            if (artifactViews.Count == 1)
            {
                OnExportArtifact(artifactViews[0].Artifact);
                return;
            }

            var directory = EditorUtility.SaveFolderPanel("Save Generator Assets", Application.dataPath, "");
            if (string.IsNullOrEmpty(directory))
                return;

            foreach (var artifactView in artifactViews)
            {
                if (!artifactView.TrySaveAsset(directory, (path) =>
                {
                    AddExportedArtifact(path, artifactView.Artifact.Guid);
                }))
                {
                    ExporterHelpers.ExportToDirectory(artifactView.Artifact, directory, true, (path) =>
                    {
                        AddExportedArtifact(path, artifactView.Artifact.Guid);
                    });
                }
            }
        }

        void AddExportedArtifact(string exportedPath, string artifactGuid)
        {
            if (!TryGetProjectRelativePath(exportedPath, out var relativePath))
                return;

            AssetDatabase.Refresh();
            var unityGuid = AssetDatabase.AssetPathToGUID(relativePath);
            if (!string.IsNullOrEmpty(unityGuid) && !string.IsNullOrEmpty(artifactGuid))
            {
                CurrentModel.AddExportedArtifact(unityGuid, artifactGuid);

                MuseEditorUtils.SetLabelOnExportedArtifact(relativePath);
            }
        }

        static bool TryGetProjectRelativePath(string path, out string relativePath)
        {
            return Model.TryGetProjectRelativePath(path, out relativePath);
        }

        public override void SaveChanges()
        {
            if (string.IsNullOrEmpty(m_AssetPath))
            {
                var path = EditorModelAssetEditor.GetSavePath(CurrentModel, true);
                if (string.IsNullOrEmpty(path))
                {
                    base.SaveChanges();
                    return;
                }

                var relativePath = ExporterHelpers.GetPathRelativeToRoot(path);
                var absolutePath = Path.GetFullPath(ExporterHelpers.GetAbsolutePath(relativePath));
                ExporterHelpers.EnsureDirectoryExists(Path.GetDirectoryName(absolutePath));
                AssetDatabase.CreateAsset(CurrentModel, relativePath);
            }

            EditorUtility.SetDirty(CurrentModel);
            AssetDatabase.SaveAssetIfDirty(CurrentModel);
            UpdateTitle();

            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            m_AssetPath = AssetDatabase.GetAssetPath(CurrentModel);

            if (!string.IsNullOrEmpty(m_AssetPath))
            {
                List<Artifact> deltaArtifacts;
                if (!string.IsNullOrEmpty(m_AssetPath))
                {
                    deltaArtifacts = CurrentModel.AssetsData.FindAll(x => !DiscardModel.AssetsData.Contains(x));

                    AssetDatabase.DeleteAsset(m_AssetPath);
                    AssetDatabase.CreateAsset(DiscardModel, m_AssetPath);
                }
                else
                {
                    deltaArtifacts = CurrentModel.AssetsData;
                }

                ArtifactCache.Delete(deltaArtifacts);
            }

            base.DiscardChanges();
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                HandleUnsavedChanges();
        }

        void HandleUnsavedChanges()
        {
            if (hasUnsavedChanges)
            {
                if (EditorUtility.DisplayDialog(TextContent.savePopupTitle, TextContent.savePopupMessage, TextContent.yes, TextContent.no))
                    SaveChanges();
                else
                    DiscardChanges();
            }
        }

        internal void AssetMoved(string destinationPath)
        {
            m_AssetPath = destinationPath;

            UpdateTitle();
            SaveChanges();
        }

        void OnGenerateButtonClicked()
        {
            if (!string.IsNullOrEmpty(m_AssetPath))
                return;

            var path = EditorModelAssetEditor.GetSavePath(CurrentModel, false);
            path = path.Replace("\\", "/");
            m_AssetPath = ExporterHelpers.GetPathRelativeToRoot(path);
            var absolutePath = Path.GetFullPath(ExporterHelpers.GetAbsolutePath(m_AssetPath));
            ExporterHelpers.EnsureDirectoryExists(Path.GetDirectoryName(absolutePath));
            AssetDatabase.CreateAsset(CurrentModel, m_AssetPath);
            CurrentModel = AssetDatabase.LoadAssetAtPath<Model>(m_AssetPath);

            SaveChanges();

            EditorGUIUtility.PingObject(CurrentModel);
        }

        void UpdateTitle()
        {
            if (string.IsNullOrEmpty(m_AssetPath))
                m_AssetPath = AssetDatabase.GetAssetPath(CurrentModel);

            var titleString = Path.GetFileNameWithoutExtension(m_AssetPath);
            if (string.IsNullOrEmpty(titleString))
                titleString = defaultWindowTitle;
            titleContent = new GUIContent(titleString, IconHelper.windowIcon);
        }

        void OnGUI()
        {
            CheckMaximized();
        }

        void CheckMaximized()
        {
            if (maximized == m_Maximized) return;

            m_Maximized = maximized;
            mainUI?.UpdateView();
            if (!maximized)
            {
                rootVisualElement.ProvideContext(CurrentModel);
            }
        }

        void OnLostFocus()
        {
            if (!CurrentModel)
                return;

            CurrentModel.NotifyWindowLostFocus();
        }
    }
}
