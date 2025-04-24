#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.StyleTrainer
{
    static class EditorUtilities
    {
        public static string OpenFile(string title, string directory, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
#else
            return "";
#endif
        }
    }
}