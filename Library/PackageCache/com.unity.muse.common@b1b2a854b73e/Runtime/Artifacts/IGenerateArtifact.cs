using System;

namespace Unity.Muse.Common
{
    internal interface IGenerateArtifact
    {
        public void Generate(string prompt, TextToImageRequest settings, Action<TextToImageResponse, string> onDone);
    }
}
