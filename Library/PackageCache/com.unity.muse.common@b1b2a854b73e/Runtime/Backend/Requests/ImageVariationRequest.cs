using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class ImageVariationRequest: TextToImageItemRequest
    {
        public string guid;

        public ImageVariationRequest(string sourceGuid, string prompt, ImageVariationSettingsRequest settings)
            : base(prompt, (TextToImageRequest)settings)
        {
            guid = sourceGuid;
        }
    }
}