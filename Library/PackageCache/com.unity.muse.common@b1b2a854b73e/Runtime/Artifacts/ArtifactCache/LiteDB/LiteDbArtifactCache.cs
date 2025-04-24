using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Unity.Muse.Common.Cache.LiteDb;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Common.Cache
{
    internal class LiteDbArtifactCache : BaseArtifactCache
    {
        const string k_DatabaseName = "ArtifactCache.db";
        const string k_ArtifactCollectionName = "artifacts";
        string m_DatabasePath;

        LiteDatabase m_Database;
        LiteDatabase db => m_Database ??= InitDb();
        ILiteCollection<ArtifactDatabaseObject> m_Collection;
        ILiteCollection<ArtifactDatabaseObject> collection => m_Collection ??= InitCollection();

        internal LiteDbArtifactCache(string path)
        {
            if (string.IsNullOrEmpty(path))
                m_DatabasePath = Path.Combine(ApplicationExtensions.museDbPath, k_DatabaseName);
            else
                m_DatabasePath = path;
        }

        public override void Initialize()
        {
            BsonMapper.Global.RegisterType<Artifact>(
                serialize: (artifact) => JsonUtility.ToJson(artifact),
                deserialize: (artifact) => JsonUtility.FromJson<Artifact<Texture2D>>(artifact.AsString)
            );
        }

        LiteDatabase InitDb()
        {
            m_Database = new LiteDatabase(m_DatabasePath);
            return m_Database;
        }

        ILiteCollection<ArtifactDatabaseObject> InitCollection()
        {
            var result = db.GetCollection<ArtifactDatabaseObject>(k_ArtifactCollectionName);
            result.EnsureIndex("Guid");
            return result;
        }

        /// <summary>
        /// Dispose of the database so that it's content is saved to disk.
        /// </summary>
        public override void Dispose()
        {
            db?.Dispose();

            m_Database = null;
            m_Collection = null;
        }

        public override void Clear()
        {
            db.DropCollection(k_ArtifactCollectionName);
            db.Rebuild();
        }

        public override bool IsInCache(Artifact artifact)
        {
            if (artifact == null)
                return false;

            try
            {
                return collection.FindOne(x => x.Guid == artifact.Guid) != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error accessing cache, dropping collection to try to save issues.\nerror: " + e.Message);
                DropCollection();
                return false;
            }
        }

        public override void Write(Artifact artifact, byte[] value)
        {
            if (artifact == null)
                return;

            try
            {
                var artifactObject = collection.FindOne(x => x.Guid == artifact.Guid) ?? new ArtifactDatabaseObject(artifact, value);
                collection.Upsert(artifactObject);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error writing cache, dropping collection to try to save issues.\nerror: " + e.Message);
                DropCollection();
            }
        }

        public override object Read(Artifact artifact)
        {
            try
            {
                var dbObject = GetArtifactObject(artifact);
                if (dbObject == null)
                    return null;

                switch (dbObject.FileExtension)
                {
                    case "png":
                    {
                        var texture = TextureUtils.Create();
                        texture.LoadImage(dbObject.RawData);

                        return texture;
                    }

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error reading cache, dropping collection to try to save issues.\nerror: " + e.Message);
                DropCollection();
                return null;
            }
        }

        public override byte[] ReadRawData(Artifact artifact)
        {
            return GetArtifactObject(artifact)?.RawData;
        }

        public override void Prune() { }

        public override void Delete(Artifact artifact)
        {
            DeleteMany(new[] { artifact });
        }

        public override void DeleteMany(IEnumerable<Artifact> artifacts)
        {
            if (artifacts == null)
                return;

            try
            {
                var guids = artifacts.Where(artifact => artifact != null)
                    .Select(artifact => artifact.Guid);
                var deleted = collection.DeleteMany(x => guids.Any(guid => guid == x.Guid));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error deleting element(s) in cache, dropping collection to try to save issues.\nerror: " + e.Message);
                DropCollection();
            }
        }

        ArtifactDatabaseObject GetArtifactObject(Artifact artifact)
        {
            if (artifact == null)
                return null;

            try
            {
                var colArtifact = collection.FindOne(x => x.Guid == artifact.Guid);
                return colArtifact;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error deleting element(s) in cache, dropping collection to try to save issues.\nerror: " + e.Message);
                DropCollection();
                return null;
            }
        }

        void DropCollection()
        {
            Clear();
        }
    }
}
