using UnityEngine;
#if !UNITY_EDITOR
namespace Unity.Muse.StyleTrainer.EditorMockClass
{
    class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        static T s_Instance;

        public static T instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = CreateInstance<T>();
                return s_Instance;
            }
        }

        public void Save(bool asText)
        {
            // Not used for now.
        }
    }
}
#endif