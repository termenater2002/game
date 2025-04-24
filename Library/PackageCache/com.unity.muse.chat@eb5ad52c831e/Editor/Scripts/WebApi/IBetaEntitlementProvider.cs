using System.Threading;
using System.Threading.Tasks;

namespace Unity.Muse.Chat.WebApi
{
    interface IBetaEntitlementProvider
    {
        Task<bool> CheckBetaEntitlement(CancellationToken ct = default);
    }
}
