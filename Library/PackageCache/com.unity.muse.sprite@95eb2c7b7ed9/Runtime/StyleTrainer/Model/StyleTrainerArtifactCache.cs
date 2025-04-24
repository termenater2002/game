using System;
using System.IO;
using Unity.Muse.Common;
using UnityEngine;
using MuseArtifact = Unity.Muse.Common.Artifact;

namespace Unity.Muse.StyleTrainer
{
    class StyleTrainerArtifactCache
    {
        BaseArtifactCache m_ArtifactCache;
        string m_ArtifactCachePath;
        readonly object m_Mutex;

        public StyleTrainerArtifactCache(string artifactCachePath)
        {
            m_ArtifactCachePath = artifactCachePath;

            m_Mutex = new object();

            CreateCache();
        }

        void CreateCache()
        {
            if (m_ArtifactCache == null)
            {
#if UNITY_EDITOR
                if (!Directory.Exists(Path.GetDirectoryName(m_ArtifactCachePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(m_ArtifactCachePath));
#endif
                m_ArtifactCache = ArtifactCache.CreateCacheInstanceForPlatform(Application.platform, m_ArtifactCachePath);
                CacheOperation((db) =>
                {
                    db.Initialize();
                    db.Prune();
                });
            }
        }

        void CacheOperation(Action<BaseArtifactCache> operation)
        {
            bool failedToRun = false;
            lock (m_Mutex)
            {
                try
                {
                    operation(m_ArtifactCache);
                }
                catch (Exception)
                {
                    failedToRun = true;
                    try
                    {
                        m_ArtifactCache.Dispose();
                    }
                    finally
                    {
                        DeleteCacheFile();
                        m_ArtifactCache = null;
                        CreateCache();
                    }
                }
                finally
                {
                    if (failedToRun)
                        operation(m_ArtifactCache);
                }
            }
        }

        void DeleteCacheFile()
        {
            try
            {
                File.Delete(m_ArtifactCachePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error deleting {m_ArtifactCachePath} file. A temporary cache file is created. Try manually deleting the file.  {e.Message}");
                var fileName = Path.GetFileNameWithoutExtension(m_ArtifactCachePath);
                fileName = $"{fileName}-{Guid.NewGuid().ToString()}.db";
                m_ArtifactCachePath = Path.Combine(Path.GetDirectoryName(m_ArtifactCachePath), fileName);
            }
        }

        public void Dispose()
        {
            CacheOperation((db) => db.Dispose());
        }

        public void Prune()
        {
            CacheOperation((db) => db.Prune());
        }

        public void Clear()
        {
            CacheOperation((db) => db.Clear());
        }

        public bool IsInCache(MuseArtifact artifact)
        {
            var result = false;
            CacheOperation((db) =>
            {
                result = db.IsInCache(artifact);
            });
            return result;
        }

        public void Delete(MuseArtifact artifact)
        {
            CacheOperation((db) => db.Delete(artifact));
        }

        public void Write(MuseArtifact artifact, byte[] value)
        {
            CacheOperation((db) => db.Write(artifact, value));
        }

        public byte[] ReadRawData(MuseArtifact artifact)
        {
            byte[] result = null;
            CacheOperation((db) => result = db.ReadRawData(artifact));
            return result;
        }
    }
}