using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class PlatformUtils
    {
#if UNITY_WEBGL
        public static bool IsWebGL => true;
#else
        public static bool IsWebGL => false;
#endif
        
#if UNITY_EDITOR
        public static bool IsEditor => true;
#else
        public static bool IsEditor => false;
#endif
        
        /// <summary>
        /// Adds Ctrl+ or ⌘+ before the provided text depending on the platform.
        /// </summary>
        /// <param name="text">The text that follows the added localised command label.</param>
        /// <returns>The provided text with an added Ctrl+ or ⌘+ depending on the platform.</returns>
        public static string GetCommandLabel(string text)
        {
            if (text == "")
            {
                return "";
            }
            
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                return $"⌘+{text}";
            }
            
            return $"Ctrl+{text}";
        }
        
        public static void CopyTexture(Texture2D source, Texture2D destination)
        {
            Assert.IsTrue(source != null &&
                destination != null &&
                source.width == destination.width &&
                source.height == destination.height,
                "Source and destination textures must be non-null and have the same dimensions.");

            if (IsWebGL && !IsEditor)
            {
                // We can't use Graphics.CopyTexture on WebGL. There appears to be a bug in the runtime
                // where frame buffers are created internally, and the cleanup of these buffers
                // crashes the app when the WebGL context is lost during quitting.
                var sourcePixels = source.GetPixels();
                destination.SetPixels(sourcePixels);
                destination.Apply();
            }
            else
            {
                Graphics.CopyTexture(source, destination);
            }
        }
    }
}
