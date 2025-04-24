using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    internal interface IInpaintArtifact
    {
        public void GenerateInpaint(string prompt,
            string sourceGuid,
            Texture2D mask,
            MaskType maskType,
            TextToImageRequest settings,
            Action<TextToImageResponse, string> onDone);
    }
}
