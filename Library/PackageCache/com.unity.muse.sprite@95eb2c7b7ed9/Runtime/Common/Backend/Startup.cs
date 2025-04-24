using System;
using Unity.Muse.Common.Account;
using UnityEngine;

namespace Unity.Muse.Sprite.Common.Backend
{
    internal static class Startup
    {
        private const int maxVersionToCheck = 2;
        private const int minVersionToCheck = 1;

        [RuntimeInitializeOnLoadMethod]
        public static void VersionCheck()
        {
            // we don't need version check anymore
            //RegisterVersionCheck();
        }

        public static void RegisterVersionCheck()
        {
            AccountInfo.Instance.OnOrganizationChanged += DoVersionCheck;
            DoVersionCheck();
        }

        public static void DoVersionCheck()
        {
            if (ServerConfig.serverConfig != null &&
                ServerConfig.serverConfig.callApiVersion &&
                ServerConfig.serverConfig.serverList != null &&
                ServerConfig.serverConfig.lastApiServer.Equals(ServerConfig.serverConfig.serverURL))
                return;

            var clientUsable = false;
            try
            {
                clientUsable = SessionStatus.IsSessionUsable;
            }
            catch (Exception)
            {
                // ignored
            }

            if (!clientUsable)
                return;

            ServerConfig.serverConfig.callApiVersion = true;
            ServerConfig.serverConfig.apiVersion = minVersionToCheck;
            ServerConfig.serverConfig.lastApiServer = ServerConfig.serverConfig.serverURL;

            for (var version = maxVersionToCheck; version >= minVersionToCheck; --version)
            {
                var versionCheckTask = new VersionCheckTask(version);
                versionCheckTask.Execute(OnVersionCheckDone);
            }
        }

        private static void OnVersionCheckDone(bool success, int version)
        {
            if (success)
                ServerConfig.serverConfig.apiVersion = Math.Max(version, ServerConfig.serverConfig.apiVersion);
        }
    }
}