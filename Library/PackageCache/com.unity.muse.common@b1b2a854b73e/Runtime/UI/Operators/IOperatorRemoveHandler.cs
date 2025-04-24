using System.Collections.Generic;

namespace Unity.Muse.Common
{
    internal interface IOperatorRemoveHandler
    {
        void OnOperatorRemoved(List<IOperator> removedInOperators);
    }
}