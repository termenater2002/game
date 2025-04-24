using System;
#if !UNITY_EDITOR
namespace Unity.Muse.StyleTrainer.EditorMockClass
{
    [AttributeUsage(AttributeTargets.Class)]
    class FilePathAttribute : Attribute
    {
        public FilePathAttribute(string relativePath, Location location) { }

        internal enum Location
        {
            PreferencesFolder,
            ProjectFolder
        }
    }
}
#endif