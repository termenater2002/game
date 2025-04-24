using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Chat
{
    internal static class GameObjectExtensions
    {
        static readonly IList<string> k_TempList = new List<string>();

        public static Texture2D GetTextureForObject(this Object obj)
        {
            if (obj is GameObject go)
            {
                return PrefabUtility.GetIconForGameObject(go);
            }

            var result = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(obj)) as Texture2D;
            if (result != null)
            {
                return result;
            }

            return EditorGUIUtility.GetIconForObject(obj);
        }

        public static Texture2D GetTextureForObjectType(this Object obj)
        {
            return EditorGUIUtility.ObjectContent(null, obj.GetType()).image as Texture2D;
        }

        public static string GetObjectHierarchy(this GameObject source)
        {
            k_TempList.Clear();
            GetObjectHierarchy(source, k_TempList);
            if (k_TempList.Count > 1)
            {
                return $"/{string.Join("/", k_TempList)}/{source.name}";
            }

            if (k_TempList.Count == 1)
            {
                return $"/{k_TempList[0]}/{source.name}";
            }

            return $"/{source.name}";
        }

        static void GetObjectHierarchy(this GameObject source, IList<string> segments)
        {
            if (source.transform.parent == null)
            {
                return;
            }

            var parent = source.transform.parent.gameObject;
            segments.Insert(0, parent.name);
            GetObjectHierarchy(parent, segments);
        }

        public static bool IsPrefabInstance(this GameObject obj)
        {
            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            return root != null;
        }

        public static bool IsPrefabVariant(this GameObject obj)
        {
            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (root == null)
            {
                return false;
            }

            return PrefabUtility.IsPartOfVariantPrefab(root);
        }

        public static bool IsPrefabType(this Object obj)
        {
            if (obj is GameObject go)
            {
                return go.IsPrefabType();
            }

            return false;
        }

        public static bool IsPrefabType(this GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            var isAsset = AssetDatabase.Contains(obj);
            if (!isAsset)
            {
                return obj.IsPrefabInScene();
            }

            return PrefabUtility.IsPartOfAnyPrefab(obj);
        }

        public static bool IsPrefabInScene(this Object obj)
        {
            if (obj is GameObject go)
            {
                return go.IsPrefabInScene();
            }

            return false;
        }

        public static bool IsPrefabInScene(this GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (AssetDatabase.Contains(obj))
            {
                return false;
            }

            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null;
        }
    }
}
