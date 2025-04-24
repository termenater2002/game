using System.Collections.Generic;

namespace Unity.Muse.Common
{
    internal interface IProxyOperator: IOperator
    {
        public IEnumerable<IOperator> CloneProxyOperators();
    }
}