#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UltraLiteDB;
using Unity.Muse.Common.Cache.LiteDb;
using BsonMapper = UltraLiteDB.BsonMapper;
using Object = UnityEngine.Object;
using Query = UltraLiteDB.Query;

namespace Unity.Muse.Common
{
    internal class WebArtifactCache : BaseArtifactCache
    {
        readonly string k_FileStreamPath = $"{Application.persistentDataPath}/{k_DatabaseName}";
        const string k_DatabaseName = "ArtifactCache.db";
        const string k_ArtifactCollectionName = "artifacts";

        FileStream m_Fs;
        string m_DatabasePath;

        UltraLiteDatabase m_Database;
        UltraLiteDatabase db => m_Database ??= InitDb();
        UltraLiteCollection<ArtifactDatabaseObject> m_Collection;
        UltraLiteCollection<ArtifactDatabaseObject> collection => m_Collection ??= InitCollection();

        internal WebArtifactCache(string path)
        {
            if (string.IsNullOrEmpty(path))
                m_DatabasePath = k_FileStreamPath;
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

        UltraLiteDatabase InitDb()
        {
            m_Fs = new FileStream(m_DatabasePath, FileMode.OpenOrCreate);
            m_Database = new UltraLiteDatabase(m_Fs);
            return m_Database;
        }

        UltraLiteCollection<ArtifactDatabaseObject> InitCollection()
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
            m_Fs?.Dispose();

            m_Database = null;
            m_Collection = null;
        }

        public override void Clear()
        {
            if (db.CollectionExists(k_ArtifactCollectionName))
            {
                db.DropCollection(k_ArtifactCollectionName);
                m_Collection = null;
            }
        }

        public override bool IsInCache(Artifact artifact)
        {
            try
            {
                var query = Query.Where("Guid", value => value.AsString == artifact.Guid);
                return collection.FindOne(query) != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error accessing cache, dropping collection to try to save issues.\nerror: " + e.Message);
                Clear();
                return false;
            }
        }

        public override void Write(Artifact artifact, byte[] value)
        {
            try
            {
                var query = Query.Where("Guid", value => value.AsString == artifact.Guid);
                var artifactObject = collection.FindOne(query) ?? new ArtifactDatabaseObject(artifact, value);

                collection.Upsert(artifactObject);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error writing cache, dropping collection to try to save issues.\nerror: " + e.Message);
                Clear();
            }
        }

        public override object Read(Artifact artifact)
        {
            try
            {
                var dbObject = GetArtifactObject(artifact);
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
                Clear();
                return null;
            }
        }

        public override byte[] ReadRawData(Artifact artifact)
        {
            return GetArtifactObject(artifact)?.RawData;
        }

        public override void Prune()
        {
            try
            {
                var query = Query.Where("CreatedDate", value => DateTime.Now - value.AsDateTime > TimeSpan.FromDays(10));
                var colArtifact = collection.Find(query);

                foreach (var artifact in colArtifact)
                {
                    var deleteQuery = Query.Where("Guid", value => value.AsString == artifact.Guid);
                    collection.Delete(deleteQuery);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error pruning cache, dropping collection to try to save issues.\nerror: " + e.Message);
                Clear();
            }
        }

        public override void Delete(Artifact artifact)
        {
           DeleteMany(new []{artifact});
        }

        public override void DeleteMany(IEnumerable<Artifact> artifacts)
        {
            try
            {
                var guids = artifacts.Where(artifact => artifact != null).Select(artifact => artifact.Guid);
                var query = Query.Where("Guid", value => guids.Any(guid => guid == value.AsString));
                collection.Delete(query);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error deleting element(s) in cache, dropping collection to try to save issues.\nerror: " + e.Message);
                Clear();
            }
        }

        ArtifactDatabaseObject GetArtifactObject(Artifact artifact)
        {
            try
            {
                var query = Query.Where("Guid", value => value.AsString == artifact.Guid);
                var colArtifact = collection.FindOne(query);

                return colArtifact;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error reading cache, dropping collection to try to save issues.\nerror: " + e.Message);
                Clear();
                return null;
            }
        }
    }
}
#endif