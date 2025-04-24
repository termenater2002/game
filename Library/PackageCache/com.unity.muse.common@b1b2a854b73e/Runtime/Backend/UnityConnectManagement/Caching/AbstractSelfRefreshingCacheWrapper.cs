using System.Threading.Tasks;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Class used to coordinate caching of data and automatic refreshing/fetching of cached data.
    /// </summary>
    /// <typeparam name="TCachedType"> Data type cached by CacheWrapper </typeparam>
    abstract class AbstractSelfRefreshingCacheWrapper<TCachedType>
        where TCachedType : class
    {
        /// <summary>
        /// Cache wrapper used to hold cached value.
        /// </summary>
        protected CacheWrapper<TCachedType> m_Cache = new CacheWrapper<TCachedType>();

        /// <summary>
        /// Get the value stored by this cache and refresh it if necessary.
        /// </summary>
        /// <returns> Value stored by this cache </returns>
        public async Task<TCachedType> GetCachedValueAsync()
        {
            if (await GetShouldRefreshCacheAsync())
            {
                await m_Cache.ReplaceCachedValueAsync(GetRefreshedValueAsync());
            }

            return await m_Cache.GetCachedValueAsync();
        }

        /// <summary>
        /// Force a refresh of the cached value.
        /// </summary>
        public async Task<TCachedType> ForceCacheRefreshAsync()
        {
            return await m_Cache.ReplaceCachedValueAsync(GetRefreshedValueAsync());
        }

        /// <summary>
        /// Get whether or not cache is out of date
        /// </summary>
        /// <returns> Whether or not cache is out of date </returns>
        protected abstract Task<bool> GetShouldRefreshCacheAsync();

        /// <summary>
        /// Get up to date entry for cache
        /// </summary>
        /// <returns> Up to date entry </returns>
        protected abstract Task<TCachedType> GetRefreshedValueAsync();
    }
}
