using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common.Cache;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Unity.Muse.Common
{
    internal static class ArtifactCache
    {
        static BaseArtifactCache s_ArtifactCache;

        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        #else
        [RuntimeInitializeOnLoadMethod]
        #endif
        [Preserve]
        public static void Initialize()
        {
            s_ArtifactCache = GetCacheInstanceForPlatform(Application.platform);
            s_ArtifactCache.Initialize();
            s_ArtifactCache.Prune();
        }

        static BaseArtifactCache GetCacheInstanceForPlatform(RuntimePlatform runtimePlatform)
        {
#if UNITY_WEBGL
            if (runtimePlatform == RuntimePlatform.WebGLPlayer)
                return new WebArtifactCache(null);
#endif

            return new LiteDbArtifactCache(null);
        }

        internal static BaseArtifactCache CreateCacheInstanceForPlatform(RuntimePlatform runtimePlatform, string assetPath)
        {
#if UNITY_WEBGL
            if (runtimePlatform == RuntimePlatform.WebGLPlayer)
                return new WebArtifactCache(assetPath);
#endif
            return new LiteDbArtifactCache(assetPath);
        }

        /// <summary>
        /// Dispose of the database so that it's content is saved to disk.
        /// </summary>
        public static void Dispose() => s_ArtifactCache.Dispose();
        public static void Clear() => s_ArtifactCache.Clear();

        public static bool IsInCache(Artifact artifact) => s_ArtifactCache.IsInCache(artifact);

        public static void Write(Artifact artifact, byte[] value) => s_ArtifactCache.Write(artifact, value);

        public static object Read(Artifact artifact) => s_ArtifactCache.Read(artifact);

        public static byte[] ReadRawData(Artifact artifact) => s_ArtifactCache.ReadRawData(artifact);

        /// <summary>
        /// Delete the given artifacts from the cache.
        /// </summary>
        /// <param name="artifacts">The artifacts to delete.</param>
        public static void Delete(params Artifact[] artifacts) => s_ArtifactCache.DeleteMany(artifacts);
        /// <summary>
        /// Delete the given artifacts from the cache.
        /// </summary>
        /// <param name="artifacts">The artifacts to delete.</param>
        public static void Delete(IEnumerable<Artifact> artifacts) => Delete(artifacts?.ToArray());

        internal static bool ReadAsTexture2D( Artifact artifact, out Texture2D texture)
        {
            var data = s_ArtifactCache.ReadRawData(artifact);
            texture = new Texture2D(2, 2);

            return data != null && texture.LoadImage(data);
        }
    }
}
