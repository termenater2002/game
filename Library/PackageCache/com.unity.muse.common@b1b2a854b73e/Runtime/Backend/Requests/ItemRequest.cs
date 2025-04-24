using System;

namespace Unity.Muse.Common
{
    [Serializable]
    class ItemRequest
    {
        private string _parameters = "";
        public virtual string parameters
        {
            get => _parameters;
            set => _parameters = value;
        }
    }
}
