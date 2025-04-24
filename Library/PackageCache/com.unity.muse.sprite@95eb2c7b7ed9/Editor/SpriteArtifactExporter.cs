using System;
using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using Unity.Muse.Sprite.Artifacts;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Editor
{
    internal class SpriteArtifactExporter : IArtifactExporter
    {
        [InitializeOnLoadMethod]
        static void Register()
        {
            ArtifactExporterFactory.instance.SetExporterForType<SpriteMuseArtifact>(new SpriteArtifactExporter());
        }

        public const string defaultArtifactName = "Artifact";

        public string Extension => "png";

        public string GetSaveFileName(Artifact artifact)
        {
            var prompt = artifact.GetOperator<PromptOperator>()?.GetPrompt();
            var name = ExporterHelpers.RemoveSpecialCharacters(prompt);
            name = !string.IsNullOrWhiteSpace(name) ? name : defaultArtifactName;
            return name;
        }

        public void ExportToDirectory(Artifact artifact, string directory, bool uniquePath, ExportedArtifactDelegate onExport = null, object optionalData = null, object metadata = null)
        {
            if (artifact == null)
                throw new ArgumentNullException(nameof(artifact));

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("Invalid save directory.");

            var fileName = GetSaveFileName(artifact);
            var extension = Extension;
            var path = uniquePath ? ExporterHelpers.GetUniquePath(directory, fileName, extension) : ExporterHelpers.GetPath(directory, fileName, extension);
            Export(artifact, path, onExport, (TextureImporterSettings)optionalData, metadata);
        }

        public void Export(Artifact artifact, string path, ExportedArtifactDelegate onExport, object optionalData = null, object metadata = null)
        {
            if(artifact is SpriteMuseArtifact spriteMuseArtifact)
                Export(spriteMuseArtifact, path, onExport, (TextureImporterSettings)optionalData, (PngMetadata)metadata);
            else
                throw new InvalidOperationException($"Artifact must be a subclass of '{nameof(SpriteMuseArtifact)}'.");
        }

        public void Export(SpriteMuseArtifact artifact, string path, ExportedArtifactDelegate onExport, TextureImporterSettings textureImporterSettings = null, PngMetadata metadata = null)
        {
            if (artifact == null)
                throw new ArgumentNullException(nameof(artifact));

            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Invalid save path.");

            artifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                if (artifactInstance == null || rawData == null)
                {
                    onExport?.Invoke(null);
                    return;
                }

                path = path.Replace('\\', '/');

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
                        textureImporter.textureType = TextureImporterType.Sprite;
                        textureImporter.spriteImportMode = SpriteImportMode.Single;
                    }

                    textureImporter.SaveAndReimport();
                }

                onExport?.Invoke(path);
            }, true);
        }
    }
}
