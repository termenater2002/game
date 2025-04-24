using UnityEngine;

namespace Unity.Muse.Animate
{
    class RuntimeRootObjectSpawner : IRootObjectSpawner<GameObject>
    {
        Transform m_Root;
        public RuntimeRootObjectSpawner(Transform root)
        {
            m_Root = root;
        }
        
        public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) =>
            Object.Instantiate(prefab, position, rotation, m_Root);

        public GameObject CreateGameObject(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(m_Root, true);
            return go;
        }
    }
}
