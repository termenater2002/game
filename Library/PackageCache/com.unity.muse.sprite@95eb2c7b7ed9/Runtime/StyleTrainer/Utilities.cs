using System;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Common.Backend;
using UnityEngine;

namespace Unity.Muse.StyleTrainer
{
    static class Utilities
    {
        public static readonly string emptyGUID = Guid.Empty.ToString();
        const string k_TempGUIDPrefix = "StyleTrainerTempGUID-";

        public static bool ValidStringGUID(string s)
        {
            return !string.IsNullOrEmpty(s) && s != emptyGUID;
        }

        public static string CreateTempGuid()
        {
            return $"{k_TempGUIDPrefix}{Guid.NewGuid()}";
        }

        public static bool IsTempGuid(string guid)
        {
            return guid.StartsWith(k_TempGUIDPrefix);
        }

        public static Texture2D placeHolderTexture => DuplicateResourceTexture(PackageResources.placeholderTexture);
        public static Texture2D errorTexture => DuplicateResourceTexture(Sprite.PackageResources.generateErrorTexture);
        public static Texture2D forbiddenTexture => DuplicateResourceTexture(PackageResources.forbiddenTexture);

        static Texture2D DuplicateResourceTexture(string path)
        {
            var t = ResourceManager.Load<Texture2D>(path);
            return BackendUtilities.CreateTemporaryDuplicate(t, t.width, t.height);
        }

        public static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}