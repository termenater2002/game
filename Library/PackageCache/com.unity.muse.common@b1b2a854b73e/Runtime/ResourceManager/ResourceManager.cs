using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// This class is an abstraction over the Unity AssetDatabase and Resources API.
    /// </summary>
    class ResourceManager
    {
        /// <summary>
        /// Loads an asset.
        /// </summary>
        /// <param name="path"> The path to the asset, relative to the project. </param>
        /// <typeparam name="T"> The type of the asset. </typeparam>
        /// <returns> The loaded asset. </returns>
        public static T Load<T>(string path) 
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);            
#elif UNITY_MUSE_ADDRESSABLES_PRESENT
            return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
#else 
            return null;
#endif
        }
        
        /// <summary>
        /// Loads all assets of a given type.
        /// </summary>
        /// <param name="path"> The path to the assets, relative to the project. </param>
        /// <typeparam name="T"> The type of the assets. </typeparam>
        /// <returns> The loaded assets. </returns>
        public static IEnumerable<T> LoadAll<T>(string path)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase
                .FindAssets($"t:{typeof(T).Name}", string.IsNullOrEmpty(path) ? null : new[] {path})
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<T>);
#elif UNITY_MUSE_ADDRESSABLES_PRESENT
            var assetsToLoad = new List<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>();
            var assetType = typeof(T);
            foreach (var loc in UnityEngine.AddressableAssets.Addressables.ResourceLocators)
            {
                if (loc is UnityEngine.AddressableAssets.ResourceLocators.ResourceLocationMap map)
                {
                    foreach (var location in map.Locations)
                    {
                        if ((string.IsNullOrEmpty(path) || location.Key.ToString().StartsWith(path)) && 
                            location.Value.Count > 0 && 
                            location.Value[0].ResourceType == assetType)
                            assetsToLoad.Add(location.Value[0]);
                    }
                }
            }

            Debug.Log($"Loading {assetsToLoad.Count} assets of type {typeof(T).Name} from path {path}");
            
            return assetsToLoad.Count > 0 
                ? UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<T>(assetsToLoad, null).WaitForCompletion() 
                : Enumerable.Empty<T>();
#else
            yield break;
#endif
        }
    }
}
