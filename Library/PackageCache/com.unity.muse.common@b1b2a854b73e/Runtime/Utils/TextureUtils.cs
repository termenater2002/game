using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.Muse.Common
{
    internal static class TextureUtils
    {
        /// <summary>
        /// Creates a texture that will remain in memory even after a scene is unloaded.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Texture2D Create(object context = null)
        {
            var texture = new Texture2D(2, 2);
            ObjectUtils.Retain(texture, context);
            return texture;
        }

        public static void SafeDestroy(this RenderTexture texture)
        {
            if(texture == null)
                return;

            texture.Release();

            ((Object)texture).SafeDestroy();
        }

        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            // Save current RenderTexture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rTex;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rTex.width, rTex.height, rTex.graphicsFormat, TextureCreationFlags.None);

            // Copy pixels to texture
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            // Restore previously active RenderTexture
            RenderTexture.active = currentActiveRT;

            return tex;
        }

        public static Texture2D CopyTexture2D(Texture2D src)
        {
            var dest = new Texture2D(src.width, src.height);
            if (src.isReadable)
            {
                dest.SetPixels(src.GetPixels());
                dest.Apply();
            }
            else
            {
                var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(src, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                dest.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
                dest.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
            return dest;
        }
    }
}
