using System;
using Unity.Muse.Common;
using Unity.Muse.StyleTrainer.Debug;
using UnityEngine;
using MuseArtifact = Unity.Muse.Common.Artifact;

namespace Unity.Muse.StyleTrainer
{
    [Serializable]
    class StyleTrainerImageArtifactCache : MuseArtifact, IDisposable
    {
        Texture2D m_Texture;
        byte[] m_RawData;

        public StyleTrainerImageArtifactCache(string guid, uint seed)
            : base(guid, seed) { }

        public void ChangeCacheKey(string guid)
        {
            if (guid != Guid)
            {
                // If it's a temp guid  we need to update the cache
                if (Utilities.IsTempGuid(Guid))
                {
                    m_RawData = GetRawDataDirect();
                    if (m_RawData != null)
                    {
                        StyleTrainerConfig.config.artifactCache.Delete(this);
                        Guid = guid;
                        WriteToCache(m_RawData);
                        if (m_Texture != null)
                            m_Texture.name = $"StyleTrainer-{Guid}";
                    }
                }

                Guid = guid;
            }
        }

        public Texture2D GetTexture2D()
        {
            using (Debug.Profiling.styleTrainerImageArtifactCache_GetTexture2D.Auto())
            {
                if (m_Texture == null)
                {
                    m_RawData = GetRawDataDirect();
                    if (m_RawData?.Length > 0)
                    {
                        m_Texture = TextureUtils.Create();
                        m_Texture.LoadImage(m_RawData);
                        m_Texture.name = $"StyleTrainer-{Guid}";
                    }
                }
            }

            return m_Texture;
        }

        public override void GetPreview(ArtifactPreviewDelegate onDoneCallback, bool useCache)
        {
            StyleTrainerDebug.LogWarning("StyleTrainerImageArtifactCache.GetPreview() is not implemented");
        }

        public void WriteToCache(byte[] value)
        {
            if (m_Texture == null)
            {
                m_Texture = TextureUtils.Create();
            }

            m_Texture.LoadImage(value);
            m_RawData = value;
            using (Debug.Profiling.styleTrainerImageArtifactCache_WriteCache.Auto())
            {
                if (StyleTrainerConfig.config.artifactCache.IsInCache(this))
                    StyleTrainerConfig.config.artifactCache.Delete(this);
                StyleTrainerConfig.config.artifactCache.Write(this, value);
            }
        }

        public override void Generate(Model model)
        {
            StyleTrainerDebug.LogWarning("StyleTrainerImageArtifactCache.Generate() is not implemented");
        }

        public override void RetryGenerate(Model model)
        {
           Generate(model);
        }

        public override ArtifactView CreateView()
        {
            StyleTrainerDebug.LogWarning("StyleTrainerImageArtifactCache.CreateView() is not implemented");
            return null;
        }

        public bool IsLoaded()
        {
            return m_Texture != null && m_RawData != null && m_RawData.Length > 0;
        }

        public byte[] GetRawDataDirect()
        {
            using (Debug.Profiling.styleTrainerImageArtifactCache_ReadCache.Auto())
            {
                if ((m_RawData == null || m_RawData.Length == 0) && StyleTrainerConfig.config.artifactCache.IsInCache(this)) m_RawData = StyleTrainerConfig.config.artifactCache.ReadRawData(this);
            }

            return m_RawData;
        }

        public void Dispose()
        {
            if (m_Texture != null)
                UnityEngine.Object.DestroyImmediate(m_Texture);
            m_Texture = null;
            m_RawData = null;
        }

        public void DeleteCache()
        {
            if (StyleTrainerConfig.config.artifactCache.IsInCache(this))
                StyleTrainerConfig.config.artifactCache.Delete(this);
        }

        public void ChangeGUID(string guid)
        {
            Guid = guid;
            m_Texture = null;
            m_RawData = null;
        }
    }
}