using Unity.Profiling;

namespace Unity.Muse.StyleTrainer.Debug
{
    static class Profiling
    {
        public static readonly ProfilerMarker styleTrainerImageArtifactCache_GetTexture2D = new("StyleTrainerImageArtifactCache.GetTexture2D");
        public static readonly ProfilerMarker styleTrainerImageArtifactCache_WriteCache = new("StyleTrainerImageArtifactCache.WriteCache");
        public static readonly ProfilerMarker styleTrainerImageArtifactCache_ReadCache = new("StyleTrainerImageArtifactCache.ReadCache");
    }
}