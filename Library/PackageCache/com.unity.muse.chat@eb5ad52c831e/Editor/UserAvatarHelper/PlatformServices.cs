#if IDENTITY_PACKAGE_INSTALLED

using System;
using System.Threading.Tasks;
using Unity.Cloud.AppLinking.Runtime;
using Unity.Cloud.Common;
using Unity.Cloud.Common.Runtime;
using Unity.Cloud.Identity;
using Unity.Cloud.Identity.Runtime;


namespace Unity.Muse.Chat
{
    #region PlatformServices

    static class PlatformServices
    {
        /// <summary>
        /// Returns a <see cref="ICompositeAuthenticator"/>.
        /// </summary>
        public static ICompositeAuthenticator Authenticator { get; private set; }

        public static IServiceHostResolver serviceHostResolver;

        public static ServiceHttpClient serviceHttpClient;

        static PlatformServices()
        {
            var httpClient = new UnityHttpClient();
            serviceHostResolver = UnityRuntimeServiceHostResolverFactory.Create();
            var playerSettings = UnityCloudPlayerSettings.Instance;
            var platformSupport = PlatformSupportFactory.GetAuthenticationPlatformSupport();

            var compositeAuthenticatorSettings = new CompositeAuthenticatorSettingsBuilder(httpClient, platformSupport, serviceHostResolver, playerSettings)
                .AddDefaultPkceAuthenticator(playerSettings)
                .Build();

            Authenticator = new CompositeAuthenticator(compositeAuthenticatorSettings);

            serviceHttpClient = new ServiceHttpClient(httpClient, Authenticator, playerSettings);
        }

        /// <summary>
        /// A Task that initializes all platform services.
        /// </summary>
        /// <returns>A Task.</returns>
        public static async Task InitializeAsync()
        {
            if (Authenticator.AuthenticationState == AuthenticationState.LoggedIn)
            {
                return;
            }

            await Authenticator.InitializeAsync();

            if (Authenticator.AuthenticationState == AuthenticationState.LoggedOut)
            {
                await Authenticator.LoginAsync();
            }
        }

        /// <summary>
        /// Shuts down all platform services.
        /// </summary>
        public static void ShutDownServices()
        {
            (Authenticator as IDisposable)?.Dispose();
            Authenticator = null;
        }
    }

    #endregion
}
#endif
