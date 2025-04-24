using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Muse.Common.Editor.Integration;
using UnityEditor;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static partial class ContextRetrievalTools
    {
        internal class ProjectHierarchyInfo : IParentable<ProjectHierarchyInfo>
        {
            public readonly string FileInfoName;
            public ProjectHierarchyInfo Parent { get; }

            public ProjectHierarchyInfo(string fileInfoName, ProjectHierarchyInfo parent)
            {
                FileInfoName = fileInfoName;
                Parent = parent;
            }

            public override bool Equals(object obj)
            {
                if (obj is not ProjectHierarchyInfo other)
                {
                    return false;
                }

                return FileInfoName == other.FileInfoName && Parent == other.Parent;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FileInfoName, Parent);
            }
        }

        internal class ProjectHierarchyMapEntry : HierarchyMapEntry<ProjectHierarchyInfo>
        {
            public ProjectHierarchyMapEntry(ProjectHierarchyInfo obj) : base(obj)
            {
            }

            public override string ObjectName => k_ObjectRef?.FileInfoName;

            protected override HierarchyMapEntry<ProjectHierarchyInfo> CreateInstance(ProjectHierarchyInfo obj, HierarchyMapEntry<ProjectHierarchyInfo> parent)
            {
                return new ProjectHierarchyMapEntry(obj);
            }
        }

        [ContextProvider("Returns the file structure under the Assets/ folder.")]
        internal static SmartContextToolbox.ExtractedContext ProjectStructureExtractor(
            [Parameter("Filter to specify which files or assets to include. Use an empty string if the full project hierarchy is needed.")]
            string assetNameFilter = null)
        {
            var resultPrefix = string.Empty;
            var result = new SmartContextToolbox.ExtractedContext();

            ProjectHierarchyMapEntry.SmartContextLimit = SmartContextToolbox.SmartContextLimit;

            // Store all objects in a tree structure first, then serialize it:
            var hierarchyMap = new ProjectHierarchyMapEntry(null);

            var assetPath = new DirectoryInfo(Path.GetFullPath(Application.dataPath));

            Dictionary<string, ProjectHierarchyInfo> parentMap = new();

            ProjectHierarchyMapEntry.Reset();

            // If there is no filter given, extract everything:
            if (string.IsNullOrEmpty(assetNameFilter))
            {
                var directoriesToProcess = new Queue<DirectoryInfo>(new[] { assetPath });

                while (directoriesToProcess.Count > 0)
                {
                    var subDir = directoriesToProcess.Dequeue();
                    ProcessDirectory(subDir);

                    if (ProjectHierarchyMapEntry.EstimatedSerializedLength > ProjectHierarchyMapEntry.SmartContextLimit)
                    {
                        result.Truncated = true;
                        break;
                    }

                    continue;

                    void ProcessDirectory(DirectoryInfo dir)
                    {
                        foreach (var file in dir.GetFiles())
                        {
                            // Ignore meta files:
                            if (file.Extension == ".meta")
                                continue;

                            var info = new ProjectHierarchyInfo(file.Name, GetParentInfo(file.Directory));

                            hierarchyMap.Insert(info);

                            if (ProjectHierarchyMapEntry.EstimatedSerializedLength > ProjectHierarchyMapEntry.SmartContextLimit)
                            {
                                result.Truncated = true;
                                break;
                            }
                        }

                        foreach (var subDir in dir.GetDirectories())
                        {
                            var info = new ProjectHierarchyInfo(subDir.Name, GetParentInfo(subDir.Parent));

                            hierarchyMap.Insert(info);

                            directoriesToProcess.Enqueue(subDir);

                            if (ProjectHierarchyMapEntry.EstimatedSerializedLength > ProjectHierarchyMapEntry.SmartContextLimit)
                            {
                                result.Truncated = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (assetNameFilter.StartsWith("*."))
                {
                    assetNameFilter = assetNameFilter[2..];
                }

                // For specific searches, include entire project directory.
                // We'll need to think about this more, it leads to a lof of false positives:
                const bool includeOnlyAssetPath = true;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (!includeOnlyAssetPath)
#pragma warning disable CS0162 // Unreachable code detected
                {
                    assetPath = assetPath.Parent;
                }
#pragma warning restore CS0162 // Unreachable code detected

                result.Truncated = true;

                resultPrefix = $"Filter:'{assetNameFilter}':\n";

                // Add all paths that fuzzy match the pattern:
                var assetPathsToAdd =
                    ContextRetrievalHelpers.FuzzySearchAssetsByName(
                            assetNameFilter,
                            AssetDatabase.GetAllAssetPaths()
                                .Where(path => path.StartsWith("Assets") || (!includeOnlyAssetPath && path.StartsWith("Packages"))))
                        .ToList();

                // Also add all asset db matches for searches by type, duplicates are handled by the hierarchyMap:
                assetPathsToAdd.AddRange(
                    AssetDatabase.FindAssets(assetNameFilter)
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Where(path => path.StartsWith("Assets") || (!includeOnlyAssetPath && path.StartsWith("Packages"))));

                foreach (var file in assetPathsToAdd)
                {
                    var fileInfo = new FileInfo(Path.GetFullPath(file));
                    var info = new ProjectHierarchyInfo(fileInfo.Name, GetParentInfo(fileInfo.Directory));

                    hierarchyMap.Insert(info);

                    if (ProjectHierarchyMapEntry.EstimatedSerializedLength > ProjectHierarchyMapEntry.SmartContextLimit)
                    {
                        break;
                    }
                }
            }

            if (hierarchyMap.Children.Count == 0)
            {
                return null;
            }

            result.ContextType = "project structure";
            result.Payload = resultPrefix + hierarchyMap.Serialized();

            return result;

            bool IsDirectoryInside(DirectoryInfo dir1, DirectoryInfo dir2)
            {
                var relativePath = Path.GetRelativePath(dir2.FullName, dir1.FullName);
                return relativePath.StartsWith(".");
            }

            ProjectHierarchyInfo GetParentInfo(DirectoryInfo directoryInfo)
            {
                // Don't go above assetPath:
                if (directoryInfo == null || IsDirectoryInside(directoryInfo, assetPath))
                    return null;

                if (parentMap.TryGetValue(directoryInfo.FullName, out var parentInfo))
                    return parentInfo;

                parentInfo = new ProjectHierarchyInfo(directoryInfo.Name, GetParentInfo(directoryInfo.Parent));

                parentMap[directoryInfo.FullName] = parentInfo;

                return parentInfo;
            }
        }
    }
}
