using System;
using System.IO;
using UnityEngine;

namespace Unity.Muse.Common
{
    static class ApplicationExtensions
    {
        internal static string libraryPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library");
        internal static string museDbPath
        {
            get
            {
                var path = Path.Combine(libraryPath, "Muse", "Db");
                Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}
