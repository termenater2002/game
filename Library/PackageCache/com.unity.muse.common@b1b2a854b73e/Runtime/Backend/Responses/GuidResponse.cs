using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class GuidResponse : Response
    {
        public string guid;
        public uint seed;
    }
}
