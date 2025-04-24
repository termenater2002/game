using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class TextToImageItemRequest : ItemRequest
    {
        public string prompt;

        public TextToImageRequest settings;

        public TextToImageItemRequest(string prompt, TextToImageRequest settings)
        {
            this.prompt = prompt;
            this.settings = settings;
        }
    }
}
