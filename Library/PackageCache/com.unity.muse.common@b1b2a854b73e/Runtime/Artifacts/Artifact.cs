using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Muse.Common
{
    [Serializable]
    internal abstract class Artifact<T> : Artifact
    {
        protected Artifact(string guid, uint seed)
            : base(guid, seed) { }

        public ArtifactData GetArtifactData()
        {
            OperatorData[] resultArray = m_Operators.Select(obj => obj.GetOperatorData()).ToArray();
            return new ArtifactData(Guid, resultArray);
        }

        public void SetArtifactData(ArtifactData data)
        {
            Guid = data.guid;

            SetOperators(ModesFactory.GetOperators(data.operators));
        }

        public delegate void ArtifactCreationDelegate(T artifactInstance, byte[] rawData, string errorMessage);

        public abstract string FileExtension { get; }

        // TODO: Cloudlab
        public bool IsCached => ArtifactCache.IsInCache(this);


        /// <summary>
        /// Will download the artifact, or load it from the cache on your current machine, if it exists.
        /// </summary>
        /// <param name="onReceived">Callback for when the artifact was successfully loaded and created. <b>T</b> will be <b>default</b> on error, and the error message will be in the 2nd parameter detailing the reason</param>
        /// <param name="useCache">When <b>true</b> will attempt to load the cached representation of this artifact from the local machine's GeneratedArtifactCache, and if not found will download it from the client. <b>false</b> will always reach out to the client and update the cache</param>
        public abstract void GetArtifact(ArtifactCreationDelegate onReceived, bool useCache);

        //TODO: Wants to be a static constructor/factory
        public abstract T ConstructFromData(byte[] data);

        /// <summary>
        /// Takes binary data and will attempt to deserialize it into its concrete Unity representation. Concrete implementations will throw exceptions or handle malformed incoming data
        /// </summary>
        /// <param name="data">Binary data from backend, file, etc, to be deserialized</param>
        /// <param name="updateCache">When <b>true</b> update the local system's cached state of the artifact. <b>false</b> skips the update</param>
        /// <returns></returns>
        protected abstract T CreateFromData(byte[] data, bool updateCache);

        /// <summary>
        /// Writes the serialized Unity representation of this artifact to the GeneratedArtifactCache on the local machine.
        /// </summary>
        /// <param name="value"></param>
        protected abstract void WriteToCache(byte[] value);

        /// <summary>
        /// Will return the deserialized Unity representation of the cached artifact from the local machine.
        /// </summary>
        /// <returns>A fully constructed artifact of type T, <b>default(T)</b> otherwise.</returns>
        protected abstract T ReadFromCache(out byte[] rawData);

        protected abstract byte[] ReadFromCacheRaw();
    }
}
