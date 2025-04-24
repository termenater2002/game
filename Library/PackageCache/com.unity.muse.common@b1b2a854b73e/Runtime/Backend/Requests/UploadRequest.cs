using System;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class UploadRequest
    {
        public string extension;
        public int size;

        internal UploadRequest(string extension, int size)
        {
            this.extension = extension;
            this.size = size;
        }
    }
}
