using System;
using System.Collections.Generic;
using Unity.Muse.Common.Account;

namespace Unity.Muse.Common
{
    [Serializable]
    class SubscriptionResponse : Response
    {
        public List<OrganizationInfo> orgs;
    }
}
