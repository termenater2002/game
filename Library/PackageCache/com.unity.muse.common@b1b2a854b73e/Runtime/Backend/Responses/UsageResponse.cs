using System;
using System.Globalization;

namespace Unity.Muse.Common
{
    [Serializable]
    class UsageResponse : Response
    {
        public int points_used;
        public int points_balance;
    }
}
