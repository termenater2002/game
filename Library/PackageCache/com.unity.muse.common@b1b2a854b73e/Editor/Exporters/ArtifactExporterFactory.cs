using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    /// <summary>
    /// Delegate informs of the path the artifact was exported to (null if export failed).
    /// </summary>
    internal delegate void ExportedArtifactDelegate(string exportPath);

    /// <summary>
    /// Interface for creating custom artifact exporters.
    /// </summary>
    internal interface IArtifactExporter
    {
        /// <summary>
        /// File extension of the exported artifact.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Gets the file save name.
        /// </summary>
        /// <param name="artifact">Artifact instance.</param>
        /// <returns>File save name.</returns>
        string GetSaveFileName(Artifact artifact);

        /// <summary>
        /// Exports the artifact to a specified path.
        /// </summary>
        /// <param name="artifact">Artifact instance.</param>
        /// <param name="path">Save path.</param>
        /// <param name="onExport">Delegate invoked when the artifact is exported to a path. Null if the export failed.</param>
        /// <param name="optionalData">Optional data used by the exporter.</param>
        /// <param name="metadata">Optional metadata added to saved artifact.</param>
        void Export(Artifact artifact, string path, ExportedArtifactDelegate onExport = null, object optionalData = null, object metadata = null);
    }

    /// <summary>
    /// Class used to register and get artifact exporters.
    /// </summary>
    internal class ArtifactExporterFactory : ScriptableSingleton<ArtifactExporterFactory>
    {
        Dictionary<Type, IArtifactExporter> m_ArtifactTypes = new Dictionary<Type, IArtifactExporter>();

        /// <summary>
        /// Sets the exporter instance used for exporting an artifact type.
        /// </summary>
        /// <param name="exporter">Exporter instance.</param>
        /// <typeparam name="T">Artifact type.</typeparam>
        /// <returns>True if set successfully.</returns>
        public bool SetExporterForType<T>(IArtifactExporter exporter) where T : Artifact
        {
            return SetExporterForType(typeof(T), exporter);
        }

        /// <summary>
        /// Sets the exporter instance used for exporting an artifact type.
        /// </summary>
        /// <param name="artifactType">Artifact type.</param>
        /// <param name="exporter">Exporter instance.</param>
        /// <returns>True if set successfully.</returns>
        public bool SetExporterForType(Type artifactType, IArtifactExporter exporter)
        {
            if (artifactType == null)
                throw new ArgumentNullException(nameof(artifactType));

            if (artifactType.IsAssignableFrom(typeof(Artifact)))
                return false;

            m_ArtifactTypes[artifactType] = exporter;
            return true;
        }

        /// <summary>
        /// Gets the exporter instance for a specified type.
        /// If no exporter is found for a specified type, a closest type will be attempted.
        /// </summary>
        /// <typeparam name="T">Artifact type.</typeparam>
        /// <returns>Exporter instance.</returns>
        public IArtifactExporter GetExporterForType<T>()
        {
            return GetExporterForType(typeof(T));
        }

        /// <summary>
        /// Gets the exporter instance for a specified type.
        /// If no exporter is found for a specified type, a closest type will be attempted.
        /// </summary>
        /// <param name="artifactType">Artifact type.</param>
        /// <returns>Exporter instance.</returns>
        public IArtifactExporter GetExporterForType(Type artifactType)
        {
            if (artifactType == null)
                throw new ArgumentNullException(nameof(artifactType));

            while (artifactType != null && !m_ArtifactTypes.ContainsKey(artifactType))
            {
                if (artifactType.IsSubclassOf(typeof(Artifact)))
                    artifactType = artifactType.BaseType;
                else
                    break;
            }

            if (artifactType == null)
                return null;

            return m_ArtifactTypes.TryGetValue(artifactType, out var exporter) ? exporter : null;
        }
    }
}
