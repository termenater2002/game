using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class StatusResponse : Response
    {
        public StatusResponseItem[] results;
    }

    [Serializable]
    struct StatusResponseItem
    {
        public string guid;
        public string status;
        public float progress;
    }
}
