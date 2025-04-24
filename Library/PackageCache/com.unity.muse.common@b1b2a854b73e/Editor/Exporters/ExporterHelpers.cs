using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    /// <summary>
    /// Exporter helpers class.
    /// </summary>
    internal static class ExporterHelpers
    {
        /// <summary>
        /// Assets folder root path.
        /// </summary>
        public const string assetsRoot = "Assets";

        /// <summary>
        /// Max allowed system path length.
        /// </summary>
        public const int maxSystemPathLength = 259;

        /// <summary>
        /// Number of characters in path reserved for unique path enumeration.
        /// </summary>
        public const int charactersReservedForUniquePath = 8;

        /// <summary>
        /// Exports artifact to a specified path.
        /// </summary>
        /// <param name="artifact">Artifact to export.</param>
        /// <param name="path">Export path.</param>
        /// <param name="onExport">Callback invoked on export.</param>
        public static void ExportToPath(this Artifact artifact, string path, ExportedArtifactDelegate onExport = null)
        {
            var exporter = ArtifactExporterFactory.instance.GetExporterForType(artifact.GetType());
            path = GetAbsolutePath(path);
            if (path.Length > maxSystemPathLength)
                Debug.Log($"Specified path '{path}' is too long. Use GetPath or GetUniquePath helper methods to create a valid path.");

            exporter?.Export(artifact, path, onExport);
        }

        /// <summary>
        /// Exports artifact to a specified directory.
        /// </summary>
        /// <param name="artifact">Artifact to export.</param>
        /// <param name="directory">Export directory.</param>
        /// <param name="unique">Should the save be unique.</param>
        /// <param name="onExport">Callback invoked on export.</param>
        public static void ExportToDirectory(this Artifact artifact, string directory, bool unique = true, ExportedArtifactDelegate onExport = null)
        {
            var exporter = ArtifactExporterFactory.instance.GetExporterForType(artifact.GetType());

            if (exporter == null)
            {
                onExport?.Invoke(string.Empty);
                return;
            }

            var fileName = exporter.GetSaveFileName(artifact);
            var ext = exporter.Extension;
            var path = unique ? GetUniquePath(directory, fileName, ext) : GetPath(directory, fileName, ext);
            exporter.Export(artifact, path, onExport);
        }

        /// <summary>
        /// Combines PNG image data with metadata.
        /// </summary>
        /// <param name="pngData">PNG data.</param>
        /// <param name="metadata">PNG metadata.</param>
        public static byte[] CombinePngWithMetadata(byte[] pngData, PngMetadata metadata)
        {
            if (pngData == null)
                return null;

            if (metadata == null)
                return pngData;

            var encoder = new PngMetadataEncoder(pngData);
            encoder.SetMetadata(metadata);
            return encoder.GetData();
        }

        /// <summary>
        /// Checks whether the specified path is within the Assets folder.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="relativePath">Path relative to the Assets folder.</param>
        /// <returns>True if the path is within the Assets folder.</returns>
        public static bool IsInAssets(string path, out string relativePath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                relativePath = null;
                return false;
            }

            relativePath = GetPathRelativeToRoot(path);
            return !string.IsNullOrWhiteSpace(relativePath) && relativePath.StartsWith(assetsRoot);
        }

        /// <summary>
        /// Gets a path of with a specified directory, file name, and extension.
        /// </summary>
        /// <param name="directory">Directory.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="extension">File extension.</param>
        /// <param name="maxPathLength">Max allowed length of the path.</param>
        /// <returns>Full save path for a file.</returns>
        public static string GetPath(string directory, string fileName, string extension, int maxPathLength = maxSystemPathLength)
        {
            if (string.IsNullOrEmpty(directory))
            {
                Debug.Log($"Incorrect directory: {directory}.");
                return string.Empty;
            }

            directory = GetAbsolutePath(directory);

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.Log($"Incorrect file name: {fileName}.");
                return string.Empty;
            }

            if (string.IsNullOrEmpty(extension))
            {
                Debug.Log($"Incorrect extension: {extension}.");
                return string.Empty;
            }

            var path = Path.Combine(directory, $"{fileName}.{extension}");
            var characters = path.Length;
            if (characters <= maxPathLength)
                return path;

            var exceedingCharacters = characters - maxPathLength;
            if (exceedingCharacters < fileName.Length)
            {
                fileName = fileName.Substring(0, fileName.Length - exceedingCharacters);
                return Path.Combine(directory, $"{fileName}.{extension}");
            }

            Debug.Log($"The specified path is too long: {path}.");
            return string.Empty;
        }

        /// <summary>
        /// Gets a unique path of with a specified directory, file name, and extension.
        /// </summary>
        /// <param name="directory">Directory.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="extension">File extension.</param>
        /// <returns>Unique full save path for a file.</returns>
        public static string GetUniquePath(string directory, string fileName, string extension)
        {
            var path = GetPath(directory, fileName, extension, maxSystemPathLength - charactersReservedForUniquePath);
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (!File.Exists(path))
                return path;

            directory = Path.GetDirectoryName(path);
            fileName = Path.GetFileNameWithoutExtension(path);

            Debug.Assert(directory != null);
            Debug.Assert(fileName != null);

            var uniqueFileName = FindUniqueFileName(directory, fileName, extension);
            var uniquePath = Path.Combine(directory, $"{uniqueFileName}.{extension}");

            return uniquePath;
        }

        public static string GetAbsolutePath(string path)
        {
            if (path.StartsWith(assetsRoot))
            {
                //Only replace the first occurence of Assets
                path = path.Remove(0, assetsRoot.Length);
                path = path.Insert(0, Application.dataPath);
            }

            path = path.Replace("\\", "/");
            return path;
        }

        static string FindUniqueFileName(string directory, string fileName, string extension)
        {
            var runningNumber = 1;

            while (File.Exists(GetPath(directory, $"{fileName} {runningNumber}", extension)))
                runningNumber++;

            return $"{fileName} {runningNumber}";
        }

        public static string GetPathRelativeToRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path.StartsWith(assetsRoot) ? path : FileUtil.GetProjectRelativePath(path);
        }

        public static void EnsureDirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static string RemoveSpecialCharacters(string stringValue, char replaceCharacter = '_')
        {
            if (stringValue == null)
                return null;

            foreach (var c in Path.GetInvalidFileNameChars())
                stringValue = stringValue.Replace(c, replaceCharacter);

            // Remove additional special characters that Unity doesn't like
            foreach (var c in "/:?<>*|\\~")
                stringValue = stringValue.Replace(c, replaceCharacter);
            return stringValue.Trim();
        }

        public static string[] OpenFilesPanel(string title, string directory, IEnumerable<ExtensionFilter> extensions)
        {
            return NativePlugin.OpenFilePanel(title, directory, extensions, true);
        }

        public static void OpenFilesPanelAsync(string title, string directory, IEnumerable<ExtensionFilter> extensions, Action<string[]> cb)
        {
            NativePlugin.OpenFilePanelAsync(title, directory, extensions, true, cb);
        }
    }
}