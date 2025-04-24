using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Muse.Animate
{
#if UNITY_EDITOR
    /// <summary>
    /// Tracks changes to Assets and emits similar events only for the LibraryItemAssets.
    /// </summary>
    class LibraryItemAssetPostProcessor : UnityEditor.AssetPostprocessor
    {
        /// <summary>
        /// Invoked when OnPostprocessAllAssets detects imported <see cref="LibraryItemAsset"/> assets.
        /// </summary>
        /// <remarks>Returns only the <see cref="LibraryItemAsset"/> assets paths.</remarks>
        public static event Action<List<string>> OnLibraryItemAssetsImported;

        /// <summary>
        /// Invoked when OnPostprocessAllAssets detects deleted <see cref="LibraryItemAsset"/> assets.
        /// </summary>
        /// <remarks>Returns only the <see cref="LibraryItemAsset"/> assets paths.</remarks>
        public static event Action<List<string>> OnLibraryItemAssetsDeleted;

        /// <summary>
        /// Invoked when OnPostprocessAllAssets detects moved <see cref="LibraryItemAsset"/> assets.
        /// </summary>
        /// <remarks>Returns only the <see cref="LibraryItemAsset"/> assets paths.</remarks>
        public static event Action<Dictionary<string, string>> OnLibraryItemAssetsMoved;

        /// <summary>
        /// Called at the start of the execution cycle, then every time AssetDatabase detects a change in any asset.
        /// </summary>
        /// <param name="importedAssets">The paths of all imported assets.</param>
        /// <param name="deletedAssets">The paths of all deleted assets.</param>
        /// <param name="movedAssetNewPaths">The paths of all moved assets, after they were moved.</param>
        /// <param name="movedAssetPreviousPaths">The paths of all moved assets, before they were moved.</param>
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssetNewPaths,
            string[] movedAssetPreviousPaths)
        {
            PostprocessMovedAssets(movedAssetNewPaths, movedAssetPreviousPaths);
            PostprocessImportedAssets(importedAssets);
            PostprocessDeletedAssets(deletedAssets);
        }

        /// <summary>
        /// Filters out the moved assets paths to keep only the <see cref="LibraryItemAsset"/> assets paths.
        /// </summary>
        /// <param name="newPaths">The paths of all moved assets, after they were moved.</param>
        /// <param name="previousPaths">The paths of all moved assets, before they were moved.</param>
        static void PostprocessMovedAssets(string[] newPaths, string[] previousPaths)
        {
            if (newPaths.Length == 0)
                return;

            using var filtered = TempDictionary<string, string>.Allocate();

            for (var i = 0; i < newPaths.Length; i++)
            {
                var movedFromPath = previousPaths[i];
                var movedToPath = newPaths[i];

                if (!IsPathALibraryItemAsset(movedToPath))
                    continue;

                filtered.Add(movedFromPath, movedToPath);
                Log($"LibraryItemPostProcessor -> Moved from ({movedFromPath}) to ({movedToPath})");
            }

            if (filtered.Count == 0)
                return;

            OnLibraryItemAssetsMoved?.Invoke(filtered.Dictionary);
        }

        /// <summary>
        /// Filters out the imported assets paths to keep only the <see cref="LibraryItemAsset"/> assets paths.
        /// </summary>
        /// <param name="importedAssets">The paths of all imported assets.</param>
        static void PostprocessImportedAssets(string[] importedAssets)
        {
            if (importedAssets.Length == 0)
                return;

            using var filtered = TempList<string>.Allocate();

            for (var i = 0; i < importedAssets.Length; i++)
            {
                if (!IsPathALibraryItemAsset(importedAssets[i]))
                    continue;

                if (LibraryRegistry.IsPathListed(importedAssets[i]))
                    continue;

                filtered.Add(importedAssets[i]);
                Log($"LibraryItemPostProcessor -> Imported: ({importedAssets[i]})");
            }

            if (filtered.Count == 0)
                return;

            OnLibraryItemAssetsImported?.Invoke(filtered.List);
        }

        /// <summary>
        /// Filters out the deleted assets paths to keep only the <see cref="LibraryItemAsset"/> assets paths.
        /// </summary>
        /// <param name="deletedAssets">The paths of all deleted assets.</param>
        static void PostprocessDeletedAssets(string[] deletedAssets)
        {
            if (deletedAssets.Length == 0)
                return;

            using var filtered = TempList<string>.Allocate();

            for (var i = 0; i < deletedAssets.Length; i++)
            {
                var deletedAssetPath = deletedAssets[i];

                if (!LibraryRegistry.IsPathListed(deletedAssetPath))
                    continue;

                filtered.Add(deletedAssetPath);
                Log($"LibraryItemPostProcessor -> Deleted: ({deletedAssetPath})");
            }

            if (filtered.Count == 0)
                return;

            OnLibraryItemAssetsDeleted?.Invoke(filtered.List);
        }

        /// <summary>
        /// Checks if the specified path is currently pointing to an existing asset loaded in
        /// AssetDatabase, with a <see cref="LibraryItemAsset"/> main asset type.
        /// </summary>
        /// <remarks>
        /// Do not use to check the path of an asset that just got deleted.<br/>
        /// Do not use to check the previous path of an asset that just got moved / renamed.<br/>
        /// For these cases, use the <see cref="LibraryRegistry"/>.IsPathListed() method;
        /// </remarks>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path points to a <see cref="LibraryItemAsset"/> asset. False otherwise.</returns>
        static bool IsPathALibraryItemAsset(string path)
        {
            var assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
            return assetType == typeof(LibraryItemAsset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("UNITY_MUSE_DEV")]
        static void Log(string msg)
        {
            DevLogger.LogInfo($"{nameof(LibraryItemAssetPostProcessor)} -> {msg}");
        }
    }
#endif
}
