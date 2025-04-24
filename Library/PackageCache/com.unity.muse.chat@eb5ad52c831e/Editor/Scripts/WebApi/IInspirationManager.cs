using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Muse.Chat.BackendApi.Model;

namespace Unity.Muse.Chat.WebApi
{
    interface IInspirationManager
    {
        /// <summary>
        /// Starts a task to return inspirations.
        /// </summary>
        Task<IEnumerable<Inspiration>> GetInspirations(CancellationToken ct = default);

        /// <summary>
        /// Starts a task to add a new inspiration.
        /// This is used to provide data that is relevant to the conversation scope and return a
        /// conversation in response.
        /// </summary>
        Task<Inspiration> AddInspiration(Inspiration inspiration, CancellationToken ct = default);

        /// <summary>
        /// Starts a task to update an inspiration.
        /// This is used to provide data that is relevant to the conversation scope and return a
        /// conversation in response.
        /// </summary>
        Task<Inspiration> UpdateInspiration(
            Inspiration inspiration,
            CancellationToken ct = default);

        /// <summary>
        /// Starts a task to update an inspiration.
        /// This is used to provide data that is relevant to the conversation scope and return a
        /// conversation in response.
        /// </summary>
        Task DeleteInspiration(Inspiration inspiration, CancellationToken ct = default);
    }
}
