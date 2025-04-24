using System;
using System.Collections.Generic;

namespace Unity.Muse.Common
{
    internal static class ArtifactFactory
    {
        static Dictionary<string, Type> s_ArtifactTypes;

        public static bool SetArtifactTypeForMode<T> (string mode) where T : Artifact, new()
        {
            s_ArtifactTypes ??= new Dictionary<string, Type>();
            return s_ArtifactTypes.TryAdd(mode, typeof(T));
        }

        public static Type GetTypeForMode(string mode)
        {
            if (string.IsNullOrEmpty(mode) || s_ArtifactTypes == null || !s_ArtifactTypes.TryGetValue(mode, out var type))
            {
                return null;
            }

            return type;
        }

        public static Artifact CreateArtifact(string mode)
        {
            if (s_ArtifactTypes == null || !s_ArtifactTypes.TryGetValue(mode, out var type))
            {
                return null;
            }

            var instance = (Artifact)Activator.CreateInstance(type);
            instance.mode = mode;
            return instance;
        }
    }
}
