using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Artifact that represents a simple image (the image doesn't support any operators).
    /// </summary>
    [Serializable]
    internal class SimpleImageArtifact : Artifact<Texture2D>
    {
        public SimpleImageArtifact(string guid, uint seed)
            : base(guid, seed) { }

        public override void GetPreview(ArtifactPreviewDelegate onDoneCallback, bool useCache)
        {
            GetArtifact((instance, data, message) =>
            {
                onDoneCallback?.Invoke(instance, data, message);
            }, true);
        }

        public override void Generate(Model model) { }

        public override void RetryGenerate(Model model) { }

        public override ArtifactView CreateView() { return null; }

        public override string FileExtension => "png";

        public override Texture2D ConstructFromData(byte[] data)
        {
            var tex = TextureUtils.Create();
            tex.LoadImage(data);

            return tex;
        }

        protected override Texture2D CreateFromData(byte[] data, bool updateCache)
        {
            var tex = ConstructFromData(data);

            if (updateCache)
                WriteToCache(data);

            return tex;
        }

        protected override void WriteToCache(byte[] value)
        {
            ArtifactCache.Write(this, value);
        }

        protected override Texture2D ReadFromCache(out byte[] rawData)
        {
            if (ArtifactCache.Read(this) is Texture2D tex)
            {
                rawData = ArtifactCache.ReadRawData(this);
                return tex;
            }
            rawData = null;
            return null;
        }

        protected override byte[] ReadFromCacheRaw()
        {
            ReadFromCache(out var raw);
            return raw;
        }

        public override void GetArtifact(ArtifactCreationDelegate onReceived, bool useCache) {}
    }
}
