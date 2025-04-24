using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class GuidItemRequest : ItemRequest
    {
        public string guid;
        public GuidItemRequest(string guid)
        {
            this.guid = guid;
        }
    }
}
