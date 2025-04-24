using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Client;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    interface IServerCompatibilityProvider
    {
        Task<ApiResponse<List<VersionSupportInfo>>> GetServerCompatibility(
            string version,
            CancellationToken ct = default);
    }
}
