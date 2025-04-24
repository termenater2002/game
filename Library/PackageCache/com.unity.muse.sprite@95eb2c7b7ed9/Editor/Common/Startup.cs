using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Common.Editor
{
    internal class Startup
    {
        [InitializeOnLoadMethod]
        public static void VersionCheck()
        {
            Backend.Startup.RegisterVersionCheck();
        }
    }
}