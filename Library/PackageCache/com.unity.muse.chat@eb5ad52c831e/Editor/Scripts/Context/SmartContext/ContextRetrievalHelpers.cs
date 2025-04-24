using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal static class ContextRetrievalHelpers
    {
        internal struct ObjectAndScore
        {
            public Object Object;
            public long Score;

            public bool IsDefault => Object == null && Score == 0;
        }

        internal struct PathAndScore
        {
            public string Path;
            public long Score;

            public bool IsDefault => string.IsNullOrEmpty(Path) && Score == 0;
        }

        /// <summary>
        /// Returns the GameObject or prefab that most closely matches the given name.
        /// </summary>
        /// <param name="gameObjectName">Name to fuzzy search for</param>
        /// <returns>Matching GameObject</returns>
        internal static T FindObject<T>(string gameObjectName) where T : Object
        {
            var assetPathsToSearch =
                        AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .Where(path => path.StartsWith("Assets")).ToList();

            var sceneObjectsToSearch =
                Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID).ToList<Object>();

            var bestSceneObj = FuzzyObjectSearchWithScore(gameObjectName, sceneObjectsToSearch).FirstOrDefault();
            var bestAssetPath = FuzzyObjectSearchByPathWithScore(gameObjectName, assetPathsToSearch).FirstOrDefault();

            if (!bestSceneObj.IsDefault && (bestAssetPath.IsDefault || bestSceneObj.Score > bestAssetPath.Score))
            {
                return bestSceneObj.Object as T;
            }

            if (!bestAssetPath.IsDefault)
            {
                return AssetDatabase.LoadAssetAtPath<T>(bestAssetPath.Path);
            }

            return null;
        }

        /// <summary>
        /// Returns the asset Object that most closely matches the given name.
        /// </summary>
        /// <param name="assetName">Name to fuzzy search for</param>
        /// <returns>Matching asset Object</returns>
        internal static T FindAsset<T>(string assetName) where T : Object
        {
            var objectsToSearch =
                AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.StartsWith("Assets"));

            var res = FuzzyObjectSearchByPathWithScore(assetName, objectsToSearch).FirstOrDefault();

            if (!res.IsDefault)
            {
                return AssetDatabase.LoadAssetAtPath<T>(res.Path);
            }

            return null;
        }

        /// <summary>
        /// Returns the asset Object that most closely matches the given name.
        /// Having a type name as a parameter allows using non-class names the AssetDatabase supports like model or script.
        /// </summary>
        /// <param name="assetName">Name to fuzzy search for</param>
        /// <param name="typeName">Type of asset to search for</param>
        /// <returns>Matching asset Object</returns>
        internal static Object FindAsset(string assetName, string typeName)
        {
            var objectsToSearch =
                AssetDatabase.FindAssets($"t:{typeName}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.StartsWith("Assets"));

            var res = FuzzyObjectSearchByPathWithScore(assetName, objectsToSearch).FirstOrDefault();

            if (!res.IsDefault)
            {
                return AssetDatabase.LoadAssetAtPath<Object>(res.Path);
            }

            return null;
        }

        internal static IEnumerable<Component> FindComponents(GameObject gameObject, string componentName)
        {
            if (componentName.ToLower() == "script")
            {
                componentName = nameof(MonoBehaviour);
            }

            long outScore = 0;
            var result = gameObject.GetComponents<Component>()
                .Where(comp => comp != null)
                .Where(comp => FuzzySearch.FuzzyMatch(componentName, comp.GetType().Name, ref outScore))
                .Select(comp => new Tuple<Component, long>(comp, outScore))
                .ToList();

            // If there are no matching components, do a fuzzy search the other way around:
            if (!result.Any())
            {
                result = gameObject.GetComponents<Component>()
                    .Where(comp => comp != null)
                    .Where(comp => FuzzySearch.FuzzyMatch(comp.GetType().Name, componentName, ref outScore))
                    .Select(comp => new Tuple<Component, long>(comp, outScore))
                    .ToList();
            }

            // If there are still no matching components, do a fuzzy search on the base class:
            if (!result.Any())
            {
                result = gameObject.GetComponents<Component>()
                    .Where(comp => comp != null)
                    .Where(comp => FuzzySearch.FuzzyMatch(comp.GetType().BaseType?.Name, componentName, ref outScore))
                    .Select(comp => new Tuple<Component, long>(comp, outScore))
                    .ToList();
            }

            return result
                .OrderByDescending(a => a.Item2)
                .Select(a => a.Item1);
        }

        internal static IEnumerable<T> FuzzyObjectSearch<T>(string pattern, IEnumerable<T> objectsToSearch)
            where T : Object
        {
            var finalResult = FuzzyObjectSearchWithScore(pattern, objectsToSearch)
                .Select(x => x.Object as T);

            return finalResult;
        }

        internal static IEnumerable<ObjectAndScore> FuzzyObjectSearchWithScore(string pattern,
            IEnumerable<Object> objectsToSearch, Func<Object, string> customNameFunction = null)
        {
            pattern ??= string.Empty;

            pattern = pattern.ToLowerInvariant();

            var objects = objectsToSearch.ToArray();

            var includePathSearch = pattern.Contains("."); // If the search pattern contains a dot, it might be a path.

            // Search by entire string first:
            var results =
                objects
                    .Select(obj =>
                    {
                        long outScore = 0;
                        var isMatch = FuzzySearch.FuzzyMatch(pattern, GetName(obj), ref outScore);

                        // If the object name does not match, try searching by path:
                        if (includePathSearch && !isMatch)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            if (!string.IsNullOrEmpty(path))
                            {
                                isMatch = FuzzySearch.FuzzyMatch(pattern, path, ref outScore);
                            }
                        }

                        return new { obj, outScore, isMatch };
                    })
                    .Where(x => x.isMatch).ToList();

            // Also search by parts of the string.
            // Separate search pattern to find matches containing parts of the pattern:
            var splitSearchPatterns = pattern.Split(" ");
            if (splitSearchPatterns.Length > 1)
            {
                var splitSearchResults = splitSearchPatterns
                    .SelectMany(splitSearchPattern =>
                        objects
                            .Select(obj =>
                            {
                                long outScore = 0;
                                var isMatch = FuzzySearch.FuzzyMatch(splitSearchPattern, GetName(obj), ref outScore);
                                return new { obj, outScore, isMatch };
                            })
                            .Where(x => x.isMatch));

                // Add items from splitSearchResults that do not already have an obj in results.
                // If the object exists in the splitSearchResults multiple times, sum up the scores:
                foreach (var splitSearchResult in splitSearchResults.GroupBy(x => x.obj))
                {
                    var existingResult = results.FirstOrDefault(x => x.obj == splitSearchResult.Key);
                    if (existingResult == null)
                    {
                        results.Add(new
                        {
                            splitSearchResult.First().obj,
                            outScore = splitSearchResult.Sum(x => x.outScore),
                            isMatch = true
                        });
                    }
                }
            }

            var finalResult = results
                .OrderByDescending(x => x.outScore)
                .ThenBy(x => x.obj.name.Length)
                .ThenBy(x => x.obj.name.ToLowerInvariant() != pattern) // Prefer objects that have an exact name match
                .Select(x => new ObjectAndScore { Object = x.obj, Score = x.outScore });

            string GetName(Object obj)
            {
                var name = customNameFunction != null ? customNameFunction(obj) : obj.name;
                return name;
            }

            return finalResult;
        }

        internal static IEnumerable<PathAndScore> FuzzyObjectSearchByPathWithScore(string pattern, IEnumerable<string> pathsToSearch)
        {
            pattern ??= string.Empty;

            pattern = pattern.ToLowerInvariant();

            var paths = pathsToSearch.ToArray();

            var includePathSearch = pattern.Contains("."); // If the search pattern contains a dot, it might be a path.

            // Search by entire string first:
            var results =
                paths
                    .Select(path =>
                    {
                        long outScore = 0;
                        var objName = Path.GetFileNameWithoutExtension(path);
                        var isMatch = FuzzySearch.FuzzyMatch(pattern, objName, ref outScore);

                        // If the object name does not match, try searching by path:
                        if (includePathSearch && !isMatch)
                        {
                            if (!string.IsNullOrEmpty(path))
                            {
                                isMatch = FuzzySearch.FuzzyMatch(pattern, path, ref outScore);
                            }
                        }

                        return new { objPath = path, outScore, isMatch };
                    })
                    .Where(x => x.isMatch).ToList();

            // Also search by parts of the string.
            // Separate search pattern to find matches containing parts of the pattern:
            var splitSearchPatterns = pattern.Split(" ");
            if (splitSearchPatterns.Length > 1)
            {
                var splitSearchResults = splitSearchPatterns
                    .SelectMany(splitSearchPattern =>
                        paths
                            .Select(path =>
                            {
                                long outScore = 0;
                                var objName = Path.GetFileNameWithoutExtension(path);
                                var isMatch = FuzzySearch.FuzzyMatch(splitSearchPattern, objName, ref outScore);
                                return new { objPath = path, outScore, isMatch };
                            })
                            .Where(x => x.isMatch));

                // Add items from splitSearchResults that do not already have an obj in results.
                // If the object exists in the splitSearchResults multiple times, sum up the scores:
                foreach (var splitSearchResult in splitSearchResults.GroupBy(x => x.objPath))
                {
                    var existingResult = results.FirstOrDefault(x => x.objPath == splitSearchResult.Key);
                    if (existingResult == null)
                    {
                        results.Add(new
                        {
                            splitSearchResult.First().objPath,
                            outScore = splitSearchResult.Sum(x => x.outScore),
                            isMatch = true
                        });
                    }
                }
            }


            var finalResult = results
                .OrderByDescending(x => x.outScore)
                .ThenBy(x => x.objPath.Length)
                .ThenBy(x => x.objPath.ToLowerInvariant() != pattern) // Prefer objects that have an exact name match
                .Select(x => new PathAndScore { Path = x.objPath, Score = x.outScore });

            return finalResult;
        }

        // When we receive a search for certain strings, they may not be in the asset name but in the extension, for example: "material" would be found if we search for the "mat" extension.
        private static readonly Dictionary<string, string> k_AlternateNameLookup = new() { { "material", "mat" } };

        internal static IEnumerable<string> FuzzySearchAssetsByName(string pattern, IEnumerable<string> namesToSearch)
        {
            long outScore = 0;

            var names = namesToSearch.ToArray();

            var results = DoSearch(pattern);

            if (results.Any())
                return results;

            // If no results were found, try to find an alternative name:
            var splitSearchPatterns = pattern.ToLowerInvariant().Split(" ");

            // Try looking up by alternative names instead:
            foreach (var splitSearchPattern in splitSearchPatterns)
            {
                if (k_AlternateNameLookup.TryGetValue(splitSearchPattern, out var alternativeName))
                {
                    var alternativeSearchPattern = pattern.Replace(splitSearchPattern, alternativeName);

                    results = DoSearch(alternativeSearchPattern);

                    if (results.Any())
                        return results;
                }
            }

            // There are no results, search by parts of the string:
            return splitSearchPatterns
                .SelectMany(splitSearchPattern =>
                    names
                        .Where(name => FuzzySearch.FuzzyMatch(splitSearchPattern,
                            Path.GetFileName(name) + " " + Path.GetExtension(name),
                            ref outScore))
                        .Select(name => new Tuple<string, long>(name, outScore))
                )
                .OrderByDescending(a => a.Item2)
                .Select(t => t.Item1).ToArray();


            string[] DoSearch(string searchPattern)
            {
                return names
                    .Where(name => FuzzySearch.FuzzyMatch(searchPattern,
                        Path.GetFileName(name) + " " + Path.GetExtension(name),
                        ref outScore))
                    .Select(name => new Tuple<string, long>(name, outScore))
                    .OrderByDescending(a => a.Item2)
                    .Select(t => t.Item1).ToArray();
            }
        }
    }
}
