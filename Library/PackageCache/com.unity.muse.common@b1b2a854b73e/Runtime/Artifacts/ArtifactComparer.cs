using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Muse.Common
{
    class ArtifactComparer : IEqualityComparer<object>
    {
        public new bool Equals(object o1, object o2)
        {
            if (o1 is Artifact a1 && o2 is Artifact a2)
            {
                return ReferenceEquals(a1, a2);
            }

            return o1 == o2;
        }
        public int GetHashCode(object o)
        {
            return RuntimeHelpers.GetHashCode(o);
        }
    }
}
