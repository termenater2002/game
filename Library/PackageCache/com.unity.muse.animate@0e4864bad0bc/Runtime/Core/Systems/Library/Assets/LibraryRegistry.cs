using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Responsible for listing all available LibraryItemAsset assets available in the project.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    static class LibraryRegistry
    {
        const string k_RootFolder = "Assets";
        static Action<LibraryItemAsset> s_OnProcessItem;

        public static event Action OnChanged;
        public static event Action<LibraryItemAsset, LibraryItemModel.Property> OnItemPropertyChanged;
        public static event Action<LibraryItemAsset, string, string> OnItemMoved;
        public static event Action<LibraryItemAsset> OnItemAdded;
        public static event Action<LibraryItemAsset> OnItemRemoved;

        public static int ItemCount => k_Items.Count;
        public static IReadOnlyDictionary<string, LibraryItemAsset> Items => k_Items;
        public static IReadOnlyList<LibraryItemAsset> ItemsList => k_ItemsList;

        static readonly Dictionary<string, LibraryItemAsset> k_Items = new();

        // Note: Using a dictionary is more performant but I had to keep the List around for compatibility issues.
        // We should get rid of this list IF we remove references to the list indexes in the undo / redo events.
        // Although if we do this we also need to make sure we track all undo-redo events on asset path changes.
        static readonly List<LibraryItemAsset> k_ItemsList = new();

        static readonly Queue<LibraryItemAsset> k_ItemsToProcess = new();

        static Dictionary<string, int> s_NextPrefixIds = new();
        static bool s_IsInitialized;

        static LibraryRegistry()
        {
            if (s_IsInitialized)
                return;

            s_IsInitialized = true;

#if UNITY_EDITOR

            // Subscribe to the asset post processor to stay in sync.
            LibraryItemAssetPostProcessor.OnLibraryItemAssetsDeleted += OnLibraryItemAssetsDeleted;
            LibraryItemAssetPostProcessor.OnLibraryItemAssetsMoved += OnLibraryItemAssetsMoved;
            LibraryItemAssetPostProcessor.OnLibraryItemAssetsImported += OnLibraryItemAssetsImported;

            // Load all LibraryItemAssets available with AssetDatabase.
            RefreshAssets($"LibraryRegistry Constructor");

            UnityEditor.EditorApplication.update += Update;
#endif
        }

        static void Update()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isUpdating)
                return;

            while (k_ItemsToProcess.TryDequeue(out var item))
            {
                ProcessItem(item);
            }
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("internal:Muse/Internals/Muse Animate/Refresh LibraryRegistry Assets")]
        static void RefreshAssetsButton()
        {
            RefreshAssets("Editor Button");
        }
#endif
        /// <summary>
        /// Rebuilds the registry of all available LibraryItemAsset assets within the project.
        /// </summary>
        /// <param name="author">A description used to easily identify where the LibraryRegistry was initialized from.</param>
        public static void RefreshAssets(string author = "Unspecified")
        {
            Log($"RefreshAssets({author})");
#if UNITY_EDITOR
            Clear();
            UnityEditor.AssetDatabase.Refresh();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LibraryItemAsset");

            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                AddItemInternal(assetPath);
            }

            InvokeChanged();
#else
            Debug.LogError("LibraryRegistry cannot be refreshed while not in the Editor.");
#endif
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("internal:Muse/Internals/Muse Animate/Log LibraryRegistry Status")]
        public static void ConsoleLogStatus()
        {
            var log = "[Library Registry Status]";

            foreach (var asset in k_Items)
            {
                log += $"\n{GetAssetDebugLabel(asset.Value)}";
            }

            // Note: Not using DevLogger here because if I can access the button,
            // I want to see this log regardless of any other setting.
            UnityEngine.Debug.Log(log);
        }
#endif

        static void OnLibraryItemAssetsImported(List<string> paths)
        {
            foreach (var path in paths)
            {
                AddItemInternal(path);
            }

            InvokeChanged();
        }

        static void OnLibraryItemAssetsMoved(Dictionary<string, string> movedAssets)
        {
#if UNITY_EDITOR

            foreach (var entry in movedAssets)
            {
                var movedFromPath = entry.Key;
                var movedToPath = entry.Value;

                var changedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<LibraryItemAsset>(movedToPath);
                if (changedAsset != null)
                {
                    var previousListIndex = IndexOf(k_Items[movedFromPath]);
                    var previousItem = k_Items[movedFromPath];
                    k_Items.Remove(movedFromPath);
                    k_Items.Add(movedToPath, changedAsset);
                    k_ItemsList[previousListIndex] = changedAsset;

                    OnItemMoved?.Invoke(previousItem, movedFromPath, movedToPath);
                    QueueProcessItem(changedAsset);
                }
            }

            InvokeChanged();
#endif
        }

        static void OnLibraryItemAssetsDeleted(List<string> paths)
        {
            foreach (var path in paths)
            {
                RemoveItemInternal(path);
            }

            InvokeChanged();
        }

        public static LibraryItemAsset CreateNewAsset<T>(T model, StageModel stageModel) where T : LibraryItemModel
        {
#if UNITY_EDITOR
            var folderPath = GetAndEnsureAssetFolder();
            var uniqueAssetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{model.Title}.asset");

            try
            {
                // Create the main object and sub-objects.
                var libraryItem = ScriptableObject.CreateInstance<LibraryItemAsset>();
                var preview = ScriptableObject.CreateInstance<LibraryItemAssetPreview>();
                var data = ScriptableObject.CreateInstance<LibraryItemAssetData>();

                preview.hideFlags = data.hideFlags = HideFlags.HideInHierarchy;

                data.SetupNewAsset(model);
                // Setup the preview and data sub-objects
                preview.SetupNewAsset(model);
                // Setup the main object last
                libraryItem.SetupNewAsset(uniqueAssetPath, model, stageModel, preview, data);

                UnityEditor.AssetDatabase.CreateAsset(libraryItem, uniqueAssetPath);
                UnityEditor.AssetDatabase.AddObjectToAsset(data, libraryItem);
                UnityEditor.AssetDatabase.AddObjectToAsset(preview, libraryItem);
                UnityEditor.AssetDatabase.AddObjectToAsset(preview.Texture, libraryItem);

                // Save the item
                libraryItem.Save(stageModel);
                return libraryItem;
            }
            catch (Exception exception)
            {
                LogError($"CreateNewAsset<{typeof(T).Name}>({model}) Failed with exception: " + exception);
                return null;
            }
#else
            DevLogger.LogError("Creating a library item outside of the editor is not supported.");
#endif
        }

        internal static string GetAndEnsureAssetFolder()
        {
            var pathFromPreference = AnimatePreferences.AssetGeneratedFolderPath;
            pathFromPreference = pathFromPreference.Replace('\\', '/').TrimEnd('/');

            if (!pathFromPreference.StartsWith($"{k_RootFolder}/") && pathFromPreference != k_RootFolder)
            {
                throw new DirectoryNotFoundException("The path must be a relative path in the Assets folder");
            }

            // Recursively create the folder if it doesn't exist
            var components = pathFromPreference.Split('/');
            var currentPath = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                var folderPath = $"{currentPath}/{components[i]}";
                if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder(currentPath, components[i]);
                }

                currentPath = folderPath;
            }

            return currentPath;
        }

        static string GetAssetDebugLabel(LibraryItemAsset asset)
        {
#if UNITY_EDITOR
            return $"{GetAssetStatusLabel(asset)} | AssetDatabase Path: {UnityEditor.AssetDatabase.GetAssetPath(asset)}";
#else
            return $"{GetAssetStatus(asset)} | Path: (Error: Can't find path of asset if not in editor)";
#endif
        }

        static string GetAssetStatusLabel(LibraryItemAsset asset)
        {
            return $"asset.name: {asset.name} | asset.Title: {asset.Title} | asset.AssetName: {asset.AssetName} | asset.Data.Model.Title: {asset.Data.Model.Title} ";
        }

        static void AddItemInternal(string path)
        {
            var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) as LibraryItemAsset;

            if (asset == null)
            {
                DevLogger.LogError("AddItemInternal() -> Could not load LibraryItemAsset from path: " + path);
                return;
            }

            if (asset.Version < ApplicationConstants.MinimumAssetVersion)
            {
                DevLogger.LogError(
                    $"AddItemInternal() -> Could not load {path} because it is not supported. " +
                    $"Detected version: {asset.Version} | Minimum supported version: {ApplicationConstants.MinimumAssetVersion}");
                return;
            }

            var status = GetAssetDebugLabel(asset);
            Log($"AddItemInternal() -> {status}");

            SubscribeToAssetModel(asset);

            k_ItemsList.Add(asset);
            k_Items.Add(path, asset);

            OnItemAdded?.Invoke(asset);
            QueueProcessItem(asset);
        }

        internal static void QueueProcessItem(LibraryItemAsset item)
        {
            k_ItemsToProcess.Enqueue(item);
        }

        /// <summary>
        /// Processes an asset when AssetDatabase is done importing or moving them.
        /// </summary>
        /// <remarks>Reads the current asset's path from the <see cref="UnityEditor.AssetDatabase"/>
        /// and changes properties to match if needed.</remarks>
        /// <remarks>Is called when:<br/></remarks>
        /// <remarks>- The user renames or moves an asset outside of the Muse Animate window.<br/></remarks>
        /// <remarks>- Duplicates an asset, both outside and inside of the Muse Animate window.</remarks>
        static void ProcessItem(LibraryItemAsset item)
        {
#if UNITY_EDITOR
            var newPath = UnityEditor.AssetDatabase.GetAssetPath(item);
            var newTitle = System.IO.Path.GetFileNameWithoutExtension(newPath);
            var previousPath = item.Path;
            var previousTitle = item.Data.Model.Title;

            // If the asset has changed path
            if (!newPath.Equals(previousPath))
            {
                Log($"ProcessItem() -> Changed path since last processing!\n" +
                    $"newPath: {newPath}\n" +
                    $"newTitle: {newTitle}\n" +
                    $"previousPath: {previousPath}\n" +
                    $"previousTitle: {previousTitle}\n");

                UnityEditor.AssetDatabase.StartAssetEditing();

                // Set properties affected by the path
                item.Path = newPath;
                item.AssetName = newTitle;
                item.Title = newTitle;
                item.Data.Model.Title = newTitle;
                item.RefreshTooltip();

                try
                {
                    // Rename the sub-asset animation clip, if any
                    var existingClipAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(item.Path);

                    if (existingClipAsset != null)
                    {
                        existingClipAsset.name = item.AssetName;
                        UnityEditor.EditorUtility.SetDirty(existingClipAsset);
                    }

                    UnityEditor.EditorUtility.SetDirty(item);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ApplyPathToTitle() -> Error: {e.Message}");
                }
                finally
                {
                    UnityEditor.AssetDatabase.StopAssetEditing();
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            else if (!newTitle.Equals(previousTitle))
            {
                // If paths are the same but title is not, that means the change in title didn't trigger a rename of the asset
                // Note: This is a fix for an issue I couldn't fix yet
                // TODO: Fix renaming only working once in the window
                RenameFileToMatchTitle(item);
            }
            else
            {
                Log($"ProcessItem() -> No changes.\n" +
                    $"newPath: {newPath}\n" +
                    $"newTitle: {newTitle}\n" +
                    $"previousPath: {previousPath}\n" +
                    $"previousTitle: {previousTitle}\n");
            }
#endif
        }

        /// <summary>
        /// Renames the asset to fit the Title.
        /// </summary>
        /// <remarks>
        /// This is called when the Title is changed from within the Data.Model itself.<br/>
        /// For now this happens when the user renames a <see cref="TextToMotionTake"/> from the UI in the ConvertToFrames workflow.
        /// </remarks>
        static void RenameFileToMatchTitle(LibraryItemAsset asset)
        {
#if UNITY_EDITOR
            var sourcePath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            var sourceFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            var sourceDirectory = System.IO.Path.GetDirectoryName(sourcePath);

            // The file name already matches the title
            if (sourceFileNameWithoutExtension == asset.Data.Model.Title)
            {
                return;
            }

            var newPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{sourceDirectory}/{asset.Data.Model.Title}.asset");
            var newFileName = System.IO.Path.GetFileName(newPath);
            var newFileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(newPath);

            Log($"RenameFileToMatchTitle() -> \n" +
                $"Data.Model.Title: {asset.Data.Model.Title}\n" +
                $"sourceFileNameWithoutExtension: {sourceFileNameWithoutExtension}\n" +
                $"sourceDirectory: {sourceDirectory}\n" +
                $"sourcePath: {sourcePath}\n" +
                $"newPath: {newPath}\n" +
                $"newFileName: {newFileName}\n" +
                $"newFileNameWithoutExtension: {newFileNameWithoutExtension}\n");

            // Check if the new title would result in a different unique path vs. what is wanted, if so, abort changing the path right away
            if (asset.Data.Model.Title != newFileNameWithoutExtension)
            {
                Log($"ApplyTitleToPath() -> Title doesnt generate unique asset path, overwriting Title to {newFileNameWithoutExtension}.");

                // Change the Model Title again to what the unique path will be
                asset.Data.Model.Title = newFileNameWithoutExtension;
                return;
            }

            if (newFileNameWithoutExtension != sourceFileNameWithoutExtension)
            {
                Log("ApplyTitleToPath() -> Renaming asset to fit Model Title! ");

                var result = UnityEditor.AssetDatabase.RenameAsset(sourcePath, newFileName);

                if (result != "")
                {
                    UnityEngine.Debug.LogWarning($"Renaming the asset has Failed! Error Message: {result} | Old Path: {sourcePath} | New Path: {newPath})");
                    return;
                }

                UnityEditor.AssetDatabase.SaveAssetIfDirty(asset.Data);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(asset);
            }
#endif
        }

        static void SubscribeToAssetModel(LibraryItemAsset asset)
        {
            asset.OnSubAssetPropertyChanged += OnItemDataModelChanged;
        }

        static void UnsubscribeFromAssetModel(LibraryItemAsset asset)
        {
            asset.OnSubAssetPropertyChanged -= OnItemDataModelChanged;
        }

        static void OnItemDataModelChanged(LibraryItemAsset asset, LibraryItemModel.Property property)
        {
            InvokeItemPropertyChanged(asset, property);
        }

        static void RemoveItemInternal(LibraryItemAsset item)
        {
            OnItemRemoved?.Invoke(item);
            UnsubscribeFromAssetModel(item);
            k_Items.Remove(item.Path);
            k_ItemsList.Remove(item);
        }

        static void RemoveItemInternal(string path)
        {
            RemoveItemInternal(GetItemByPath(path));
        }

        static LibraryItemAsset GetItemByPath(string path)
        {
            return k_Items[path];
        }

        public static int IndexOf(LibraryItemAsset item)
        {
            return k_ItemsList.IndexOf(item);
        }

        static void InvokeChanged()
        {
            LogVerbose("LibraryRegistry -> InvokeChanged()");
            OnChanged?.Invoke();
        }

        static void InvokeItemPropertyChanged(LibraryItemAsset asset, LibraryItemModel.Property property)
        {
            OnItemPropertyChanged?.Invoke(asset, property);
        }

        static void Clear()
        {
            foreach (var entry in k_Items)
            {
                UnsubscribeFromAssetModel(entry.Value);
            }

            k_ItemsList.Clear();
            k_Items.Clear();
        }

        public static void Delete(LibraryItemAsset item)
        {
#if UNITY_EDITOR
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(item);
            var success = UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            Log(success ? $"Delete({item}) was a Success! | Path: {assetPath})" : $"Delete({item}) has Failed! | Path: {assetPath})");
#else
            LogError("Can't delete if not in the editor.")
#endif
        }

        public static bool IsPathListed(string assetPath)
        {
            return Items.ContainsKey(assetPath);
        }

        public static void Duplicate(LibraryItemAsset item)
        {
            Log($"Duplicate({item})");
#if UNITY_EDITOR

            // Copy the asset to a new unique path
            var sourcePath = UnityEditor.AssetDatabase.GetAssetPath(item);
            var newPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(sourcePath);

            if (!UnityEditor.AssetDatabase.CopyAsset(sourcePath, newPath))
            {
                Log($"Duplicate({item}) CopyAsset has Failed! | Source Path: {sourcePath} | New Path: {newPath})");
                return;
            }

            // Note: Note handling the duplicated item here
            // We will receive an event from LibraryItemAssetPostProcessor
            Log($"Duplicate({item}) Complete! Source Path: {sourcePath} | New Path: {newPath})");
#else
            LogError("Can't duplicate an item asset if not in the editor.")
#endif
        }

        public static bool TryGetOwnerOf(LibraryItemModel model, out LibraryItemAsset asset)
        {
            asset = null;

            foreach (var pair in k_Items)
            {
                if (pair.Value.Data == null)
                    continue;

                if (pair.Value.Data.Model == model)
                {
                    asset = pair.Value;
                    break;
                }
            }

            return asset != null;
        }

        public static void RequestAllThumbnails(ThumbnailsService thumbnailsService, CameraModel cameraModel)
        {
            foreach (var item in k_Items)
            {
                item.Value.RequestThumbnailUpdate(thumbnailsService, cameraModel);
            }
        }

        // [Section] Takes Names Helpers

        static string GetNewItemTitle(string prefix)
        {
            if (!s_NextPrefixIds.ContainsKey(prefix))
                s_NextPrefixIds[prefix] = 0;

            while (true)
            {
                var nextTitle = $"{prefix} {s_NextPrefixIds[prefix]}";

                if (!IsTitleUsed(nextTitle))
                {
                    return nextTitle;
                }

                s_NextPrefixIds[prefix] += 1;
            }
        }

        public static string GetNewM2KTakeTitle()
        {
            return GetNewItemTitle("Editable Take");
        }

        public static string GetNewT2MTakeTitle()
        {
            return GetNewItemTitle("Generated Take");
        }

        public static string GetNewV2MTakeTitle()
        {
            return GetNewItemTitle("Video Take");
        }

        static bool IsTitleUsed(string name)
        {
            foreach (var item in Items)
            {
                if (item.Value == null)
                    continue;

                if (item.Value.Title.Equals(name))
                    return true;
            }

            return false;
        }

        // [Section] Debugging

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        static void Log(string msg)
        {
            DevLogger.LogInfo($"LibraryRegistry -> {msg}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        static void LogVerbose(string msg)
        {
            DevLogger.LogSeverity(TraceLevel.Verbose, $"LibraryRegistry -> {msg}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        static void LogError(string msg)
        {
            DevLogger.LogError($"LibraryRegistry -> {msg}");
        }
    }
}
