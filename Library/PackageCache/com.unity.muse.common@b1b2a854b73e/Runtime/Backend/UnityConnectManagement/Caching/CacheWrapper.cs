using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Class used to coordinate caching of data and refreshing/fetching of cached data.
    /// </summary>
    /// <typeparam name="TCachedType"> Data type cached by CacheWrapper </typeparam>
    [Serializable]
    class CacheWrapper<TCachedType>
        where TCachedType : class
    {
        [SerializeField] TCachedType m_Data;

        /// <summary>
        /// Event fired when cache is invalidated.
        /// </summary>
        public event Action OnCacheInvalidated = null;

        /// <summary>
        /// The task to monitor for current Get operations. Will only be completed with the results of the most recent refresh operation.
        /// </summary>
        TaskCompletionSource<TCachedType> m_SetCachedValueTask = null;
        /// <summary>
        /// Last cache refresh operation.
        /// </summary>
        Task<TCachedType> m_LastReplaceCachedValueTask = null;

        /// <summary>
        /// Create cache wrapper
        /// </summary>
        public CacheWrapper()
        {
            m_Data = null;
        }

        /// <summary>
        /// Refresh the value currently being cached by CacheWrapper. If another refresh task is currently in progress, replace it with this one.
        /// </summary>
        /// <param name="refreshTask"> Task used to refresh cached value </param>
        public async Task<TCachedType> ReplaceCachedValueAsync(Task<TCachedType> refreshTask)
        {
            // If there is no ongoing replace task, create one
            if (m_SetCachedValueTask == null || m_SetCachedValueTask.Task.IsCompleted)
            {
                m_SetCachedValueTask = new TaskCompletionSource<TCachedType>();
            }
            m_LastReplaceCachedValueTask = refreshTask;

            try
            {
                m_Data = await refreshTask;

                // We only want to update cache with the result of the final refresh
                if (refreshTask == m_LastReplaceCachedValueTask)
                {
                    m_SetCachedValueTask.SetResult(m_Data);
                }
                return m_Data;
            }
            catch (Exception e)
            {
                m_Data = null;
                m_SetCachedValueTask.SetException(e);
                throw e;
            }
        }
        /// <summary>
        /// Value currently cached by wrapper or result of current refresh operation.
        /// </summary>
        /// <returns> Value currently cached by wrapper </returns>
        public async Task<TCachedType> GetCachedValueAsync()
        {
            if (!IsValid())
            {
                throw new Exception("Error fetching from cache. Cache is currently empty.");
            }
            if (m_Data == null)
                m_Data = await m_SetCachedValueTask.Task;
            return m_Data;
        }
        /// <summary>
        /// Clear cache. Will invalidate the cache too.
        /// </summary>
        public void Clear()
        {
            OnCacheInvalidated?.Invoke();
            m_Data = null;
            m_SetCachedValueTask = null;
            m_LastReplaceCachedValueTask = null;
        }
        /// <summary>
        /// Does cache have valid contents.
        /// </summary>
        /// <returns> Does cache have valid contents </returns>
        public bool IsValid()
        {
            return m_Data != null ||
                (m_SetCachedValueTask != null
                && m_SetCachedValueTask.Task.Status != TaskStatus.Canceled
                && m_SetCachedValueTask.Task.Status != TaskStatus.Faulted);
        }
        /// <summary>
        /// Is cache currently being refreshed.
        /// </summary>
        /// <returns> True if cache is currently being refreshed. False otherwise. </returns>
        public bool IsRefreshing()
        {
            return m_SetCachedValueTask != null && !m_SetCachedValueTask.Task.IsCompleted;
        }
    }
}
