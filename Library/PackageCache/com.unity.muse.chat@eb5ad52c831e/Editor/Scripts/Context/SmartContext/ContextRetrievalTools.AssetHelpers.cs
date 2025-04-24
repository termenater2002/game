using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static partial class ContextRetrievalTools
    {
        private static readonly string[] k_TypesWithModelMainAssets = { "mesh", "animationclip", "animation", "avatar" };
        private static readonly string[] k_TypesWithTextureMainAssets = { "sprite" };

        private static Object TryFindAssets(string objectName, string assetType)
        {
            Object matchingAsset = null;

            assetType = assetType.ToLower();

            // Types that are potentially sub assets inside models
            if (k_TypesWithModelMainAssets.Contains(assetType))
            {
                matchingAsset = TryFindAssetOrSubAssetOfType(objectName, assetType, "model");
                if (matchingAsset != null) return matchingAsset;
            }

            // Types that are potentially sub assets inside texture2D
            if (k_TypesWithTextureMainAssets.Contains(assetType))
            {
                matchingAsset = TryFindAssetOrSubAssetOfType(objectName, assetType, "texture2d");
                if (matchingAsset != null) return matchingAsset;
            }

            // Various main asset types (not sub assets inside other assets)
            matchingAsset = TryFindAssetOfType(objectName, assetType);
            if (matchingAsset != null) return matchingAsset;

            return matchingAsset;
        }

        private static Object TryFindAssetOrSubAssetOfType(string objectName, string assetType, string mainAssetType)
        {
            return FindAssetOrSubAssetOfType(objectName, assetType, mainAssetType);
        }

        private static Object TryFindAssetOfType(string objectName, string assetType)
        {
            return ContextRetrievalHelpers.FindAsset(objectName, assetType);
        }

        private static Object FindAssetOrSubAssetOfType(string objectName, string assetType, string mainAssetType)
        {
            var matchingAsset = ContextRetrievalHelpers.FindAsset(objectName, assetType);

            // may be a sub asset
            if (matchingAsset == null)
            {
                matchingAsset = FindSubAssetOfType(objectName, assetType, mainAssetType);
            }

            return matchingAsset;
        }

        private static Object FindSubAssetOfType(string objectName, string assetType, string mainAssetType)
        {
            List<Object> subAssetCandidatesWithMainAssetMatch = new List<Object>();

            var mainAsset  = ContextRetrievalHelpers.FindAsset(objectName, mainAssetType);
            if (mainAsset != null)
            {
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mainAsset));
                foreach (var subAsset in subAssets)
                {
                    var subAssetType = subAsset.GetType().ToString().ToLower();
                    var dotIdx = subAssetType.LastIndexOf('.');
                    if (dotIdx >= 0)
                        subAssetType = subAssetType.Substring(dotIdx + 1);

                    if (subAssetType == assetType)
                    {
                        // Perfect match
                        if (objectName == subAsset.name)
                            return subAsset;

                        subAssetCandidatesWithMainAssetMatch.Add(subAsset);
                    }
                }
            }

            // Go wider, this may be a sub asset name, not an main asset name
            var assetsOfMainType =
                AssetDatabase.FindAssets($"t:{mainAssetType}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.StartsWith("Assets"));

            var subAssetCandidates = new List<Object>();

            bool isTextureType = mainAssetType == "texture2d";

            foreach (var path in assetsOfMainType)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (isTextureType && texImporter != null)
                    {
                        // Early-out: skip non-sprite textures
                        if (texImporter.textureType != TextureImporterType.Sprite)
                            continue;
                    }

                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                    var subAssets = allAssets.Where(asset =>
                    {
                        var subAssetType = asset.GetType().ToString().ToLower();
                        var dotIdx = subAssetType.LastIndexOf('.');
                        if (dotIdx >= 0)
                            subAssetType = subAssetType.Substring(dotIdx + 1);

                        return subAssetType == assetType;
                    }).ToList();

                    var perfectMatch = subAssets.FirstOrDefault(e => e.name == objectName);
                    if (perfectMatch)
                    {
                        return perfectMatch;
                    }

                    var fuzzyMatch = ContextRetrievalHelpers.FuzzyObjectSearch(objectName, subAssets).FirstOrDefault();

                    if (fuzzyMatch != null && !subAssetCandidates.Any(e => e.name == fuzzyMatch.name))
                    {
                        subAssetCandidates.Add(fuzzyMatch);
                    }
                }
            }

            // TODO: Revisit the fallbacks here; instead of just matching once more we'd like the best "score" from all the candidates!?

            // Best name match inside all main assets, that may not have any good name matches themselves
            var fuzzyMatchCandidate = ContextRetrievalHelpers.FuzzyObjectSearch(objectName, subAssetCandidates).FirstOrDefault();
            if (fuzzyMatchCandidate != null)
                return fuzzyMatchCandidate;

            // Best name match inside all main assets that had fuzzy matched names (the main assets, not the actual sub asset we search)
            fuzzyMatchCandidate = ContextRetrievalHelpers.FuzzyObjectSearch(objectName, subAssetCandidatesWithMainAssetMatch).FirstOrDefault();
            if (fuzzyMatchCandidate != null)
                return fuzzyMatchCandidate;

            // At least return the first sub asset that was found (main asset name match, just the sub asset didn't match)
            if (subAssetCandidatesWithMainAssetMatch.Count > 0)
                return subAssetCandidatesWithMainAssetMatch[0];

            return null;
        }
    }
}
