using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Unity.Muse.Common
{
    internal abstract class BaseArtifactCache
    {
        public abstract void Initialize();
        public abstract void Clear();
        public abstract bool IsInCache(Artifact artifact);
        public abstract void Write(Artifact artifact, byte[] value);
        public abstract object Read(Artifact artifact);
        public abstract byte[] ReadRawData(Artifact artifact);
        public abstract void Prune();
        public abstract void Delete(Artifact artifact);
        public abstract void DeleteMany(IEnumerable<Artifact> artifacts);
        /// <summary>
        /// Dispose of the database so that it's content is saved to disk.
        /// </summary>
        public abstract void Dispose();
    }
}
