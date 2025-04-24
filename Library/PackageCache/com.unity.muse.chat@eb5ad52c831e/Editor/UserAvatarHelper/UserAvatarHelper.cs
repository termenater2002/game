using System;
using UnityEngine;

#if IDENTITY_PACKAGE_INSTALLED
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.Identity;

namespace Unity.Muse.Chat
{
    internal class UserAvatarHelper
    {
        static string k_userProfileURL = "https://services.unity.com/api/unity/account/v1/users";

        // Store downloaded avatars for reuse:
        static readonly Dictionary<string, Texture2D> cachedIcons = new();

        // Callbacks to notify when an avatar for a user id has been downloaded:
        static Dictionary<string, List<Action<Texture2D>>> callbacks = new();

        /// <summary>
        /// Downloads the user's avatar asynchronously and calls back when done.
        /// </summary>
        /// <param name="userId">The user ID to get the avatar for</param>
        /// <param name="userAvatarLoaded">The callback to call when the avatar is ready</param>
        public static async void GetUserAvatar(string userId, Action<Texture2D> userAvatarLoaded)
        {
            if (cachedIcons.TryGetValue(userId, out var avatarTex) && avatarTex != null)
            {
                userAvatarLoaded(avatarTex);
                return;
            }

            lock (callbacks)
            {
                if (!callbacks.TryGetValue(userId, out var callbacksForUserId))
                {
                    callbacksForUserId = new List<Action<Texture2D>>();
                    callbacks[userId] = callbacksForUserId;
                }

                callbacksForUserId.Add(userAvatarLoaded);
            }

            var prov = new UserAccountProvider(userId);
            await prov.GetUserAccountIcon();
        }

        class UserAccountProvider
        {
            readonly IServiceHttpClient m_ServiceHttpClient;

            readonly string m_UserId;

            public UserAccountProvider(string userId)
            {
                m_ServiceHttpClient = PlatformServices.serviceHttpClient;
                m_UserId = userId;
            }

            // For easier parsing of the user profile, we just need the avatar:
            class UserAccountJson
            {
                public string avatar;
            }

            public async Task GetUserAccountIcon()
            {
                await PlatformServices.InitializeAsync();

                try
                {
                    var url = $"{k_userProfileURL}/{m_UserId}";
                    var response = await m_ServiceHttpClient.GetAsync(url);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var account = JsonSerialization.Deserialize<UserAccountJson>(responseContent);

                    if (account.avatar != null)
                    {
                        // Got the avatar URL, download it and create a texture from it.
                        // Then call back all observers with the texture:

                        using (var client = new WebClient())
                        {
                            var rawData = client.DownloadData(account.avatar);

                            var tex = new Texture2D(2, 2);
                            tex.LoadImage(rawData);

                            cachedIcons[m_UserId] = tex;

                            lock (callbacks)
                            {
                                if (callbacks.TryGetValue(m_UserId, out var callbacksForUserId))
                                {
                                    foreach (var callback in callbacksForUserId)
                                    {
                                        callback(tex);
                                    }

                                    // Once the avatar is cached, we will never load it and
                                    // never call these observers again, so we can remove them:
                                    callbacks.Remove(m_UserId);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
#else
// Missing package:
// com.unity.cloud.identity
// Stub implementation for avatars:
internal class UserAvatarHelper
{
    public static void GetUserAvatar(string userId, Action<Texture2D> userAvatarLoaded) { }
}
#endif
