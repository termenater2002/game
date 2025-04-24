using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    /// <summary>
    /// Default artifact exporter for texture artifacts.
    /// </summary>
    internal class DefaultTextureExporter : IArtifactExporter
    {
        [InitializeOnLoadMethod]
        static void Register()
        {
            ArtifactExporterFactory.instance.SetExporterForType<Artifact<Texture2D>>(new DefaultTextureExporter());
        }

        public string Extension => "png";
        const int k_FileNameMaxLength = 36;

        public string GetSaveFileName(Artifact artifact)
        {
            var prompt = artifact.GetOperator<PromptOperator>()?.GetPrompt();
            var fileName = ExporterHelpers.RemoveSpecialCharacters(prompt);

            if (string.IsNullOrWhiteSpace(fileName))
                return artifact?.Guid;

            if (fileName.Length > k_FileNameMaxLength)
            {
                fileName = fileName[..k_FileNameMaxLength];
            }
            return fileName;
        }

        public void Export(Artifact artifact, string path, ExportedArtifactDelegate onExport, object optionalData, object metadata)
        {
            Export(artifact, path, onExport, (TextureImporterSettings)optionalData, (PngMetadata)metadata);
        }

        public void Export(Artifact artifact, string path, ExportedArtifactDelegate onExport, TextureImporterSettings textureImporterSettings, PngMetadata metadata)
        {
            if (artifact == null)
                throw new ArgumentNullException(nameof(artifact));

            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Invalid save path.");

            if (artifact is Artifact<Texture2D> textureArtifact)
            {
                textureArtifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
                {
                    if (artifactInstance == null || rawData == null)
                    {
                        onExport?.Invoke(null);
                        return;
                    }

                    if(metadata != null)
                        File.WriteAllBytes(path, ExporterHelpers.CombinePngWithMetadata(rawData, metadata));
                    else
                        File.WriteAllBytes(path, rawData);

                    if (ExporterHelpers.IsInAssets(path, out var relativePath))
                    {
                        AssetDatabase.Refresh();

                        var textureImporter = AssetImporter.GetAtPath(relativePath) as TextureImporter;

                        Debug.Assert(textureImporter != null);

                        if (textureImporterSettings != null)
                        {
                            textureImporter.SetTextureSettings(textureImporterSettings);
                        }
                        else
                        {
                            textureImporter.isReadable = true;
                            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                        }

                        textureImporter.SaveAndReimport();
                    }

                    onExport?.Invoke(path);
                }, true);
            }
            else
            {
                throw new InvalidOperationException($"Argument '{nameof(artifact)}' must be a subclass of '{nameof(Artifact<Texture2D>)}'.");
            }
        }
    }
}