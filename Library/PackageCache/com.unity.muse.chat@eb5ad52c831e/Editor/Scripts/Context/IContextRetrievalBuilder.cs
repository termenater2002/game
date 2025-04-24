using System.Collections.Generic;

namespace Unity.Muse.Chat
{
    interface IContextRetrievalBuilder
    {
        IEnumerable<IContextSelection> GetSelectors();
    }
}
