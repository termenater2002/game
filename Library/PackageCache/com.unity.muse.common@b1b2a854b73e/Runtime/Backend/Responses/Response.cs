using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class Response
    {
        public bool success;
        public string error;

        public virtual bool HasErrors()
        {
            return !success;
        }
    }
}
