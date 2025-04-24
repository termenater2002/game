using System;
using UnityEngine;

namespace Unity.Muse.Common.Cache.LiteDb
{
    internal record ArtifactDatabaseObject
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public uint Seed { get; set; }
        public byte[] RawData { get; set; }
        public string FileExtension { get; set; }
        public DateTime CreatedDate { get; set; }
        public ArtifactDatabaseObject() { }
        public ArtifactDatabaseObject(Artifact artifact, byte[] textureData)
        {
            Guid = artifact.Guid;
            Seed = artifact.Seed;
            FileExtension = "png";
            RawData = textureData;
            CreatedDate = DateTime.Now;
        }
    }
}
