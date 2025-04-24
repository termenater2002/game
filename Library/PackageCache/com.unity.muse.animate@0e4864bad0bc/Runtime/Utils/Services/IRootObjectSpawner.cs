using UnityEngine;

namespace Unity.Muse.Animate
{
    /// <summary>
    /// Abstracts the instantiation of root objects in the scene.
    /// </summary>
    /// <remarks>
    /// This is useful for instantiating objects in locations other than the main scene, such as in a preview scene,
    /// or for testing.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    interface IRootObjectSpawner<T> where T : Object
    {
        public T Instantiate(T original, Vector3 position, Quaternion rotation);

        public GameObject CreateGameObject(string name);
    }
}
