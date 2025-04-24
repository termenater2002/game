using System;
using Unity.Muse.Common;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class UploadResponse : Response
    {
        public string guid;
        public string upload_url;

        internal UploadResponse(bool success, string guid, string upload_url, string error = null)
        {
            this.success = success;
            this.guid = guid;
            this.upload_url = upload_url;
            this.error = error;
        }
    }
}
