using UnityEngine;

namespace Unity.Muse.Animate
{
    static class GameObjectUtils
    {
        public static GameObject CreateChild(this GameObject parent, string objectName, bool sameLayer = true)
        {
            var parentTransform = parent != null ? parent.transform : null;
            var go = CreateChild(parentTransform, objectName, sameLayer);
            return go;
        }

        public static GameObject CreateChild(this Transform parent, string objectName, bool sameLayer = true)
        {
            var createdObject = new GameObject(objectName);
            createdObject.transform.SetParent(parent, false);

            if (sameLayer)
                createdObject.layer = parent.gameObject.layer;

            return createdObject;
        }

        public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren)
        {
            gameObject.layer = layer;

            if (includeChildren)
            {
                var transform = gameObject.transform;
                foreach (Transform childTransform in transform)
                {
                    SetLayer(childTransform.gameObject, layer, true);
                }
            }
        }

        public static Bounds GetRenderersWorldBounds(this GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                var rendererBounds = renderers[i].bounds;
                bounds.Encapsulate(rendererBounds);
            }

            return bounds;
        }

        public static void Destroy<T>(T go) where T : Object
        {
            if (go == null)
                return;
            
            if (UnityEngine.Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }
    }
}
