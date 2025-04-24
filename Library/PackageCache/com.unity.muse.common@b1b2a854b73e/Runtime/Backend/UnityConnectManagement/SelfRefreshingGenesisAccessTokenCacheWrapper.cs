#if UNITY_EDITOR

using System;
using System.Threading.Tasks;
using UnityEditor;

namespace Unity.Muse.Common
{
    class SelfRefreshingGenesisAccessTokenCacheWrapper : AbstractSelfRefreshingCacheWrapper<string>
    {
        /// <inheritdoc/>>
        protected override async Task<bool> GetShouldRefreshCacheAsync()
        {
            if (!m_Cache.IsValid())
            {
                return true;
            }
            if (!m_Cache.IsRefreshing())
            {
                if (string.IsNullOrEmpty(CloudProjectSettings.accessToken))
                {
                    return true;
                }
                if (!CloudProjectSettings.accessToken.Equals(await m_Cache.GetCachedValueAsync()))
                {
                    return true;
                }
            }
            return false;
        }
        /// <inheritdoc/>>
        protected override async Task<string> GetRefreshedValueAsync()
        {
            TaskCompletionSource<string> getNewAccessTokenTask = new TaskCompletionSource<string>();
            // If the access token is ready and simply hasn't been cached yet, cache it now
            if (!string.IsNullOrEmpty(CloudProjectSettings.accessToken))
            {
                getNewAccessTokenTask.SetResult(CloudProjectSettings.accessToken);
                return await getNewAccessTokenTask.Task;
            }
            // If the access token is not ready, refresh and then cache it
            try
            {
                CloudProjectSettings.RefreshAccessToken(wasRefreshSuccessful =>
                {
                    if (wasRefreshSuccessful)
                    {
                        getNewAccessTokenTask.SetResult(CloudProjectSettings.accessToken);
                    }
                    else
                    {
                        Exception e = new Exception("Error getting access token. Access token could not be refreshed.");
                        getNewAccessTokenTask.SetException(e);
                    }
                });
            }
            catch (Exception e)
            {
                getNewAccessTokenTask.SetException(e);
            }
            return await getNewAccessTokenTask.Task;
        }
    }
}
#endif