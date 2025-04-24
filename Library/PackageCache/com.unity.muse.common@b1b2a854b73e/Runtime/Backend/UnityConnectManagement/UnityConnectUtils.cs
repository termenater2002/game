#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;


namespace Unity.Muse.Common
{
    internal class ChangeInfo
    {
        public bool registered;
        public Func<Task> onChange;
        public Delegate eventDelegate;
    }

    internal enum UnityConnectEvents
    {
        StateChanged,
        ProjectRefreshed,
        ProjectStateChanged,
        UserStateChanged
    }

    /// <summary>
    /// Caches UnityConnect reflection methods
    /// </summary>
    internal static class UnityConnectUtils
    {
        /// <summary>
        /// Method used to get organization's foreign key
        /// </summary>
        static MethodInfo GetOrganizationForeignKey => s_GetOrganizationForeignKey ??=
            k_UnityConnectType.GetMethod("GetOrganizationForeignKey");
        static MethodInfo s_GetOrganizationForeignKey;

        static MethodInfo GetShowLogin => s_ShowLogin ??=
            k_UnityConnectType.GetMethod("ShowLogin");
        static MethodInfo s_ShowLogin;
        /// <summary>
        /// Property used to get whether or not user is logged in
        /// </summary>
        static PropertyInfo LoggedInProperty => s_LoggedInProperty ??=
            k_UnityConnectType.GetProperty("loggedIn");
        static PropertyInfo s_LoggedInProperty;
        /// <summary>
        /// Property used to know if user info is ready to be considered
        /// </summary>
        static PropertyInfo isUserInfoReady => s_IsUserInfoReady ??=
            k_UnityConnectType.GetProperty("isUserInfoReady");
        static PropertyInfo s_IsUserInfoReady;

        /// <summary>
        /// Property used to get CloudProjectSettings configuration
        /// </summary>
        static PropertyInfo ConfigurationProperty => s_ConfigurationProperty ??=
            k_UnityConnectType.GetProperty("configuration");
        static PropertyInfo s_ConfigurationProperty;

        static readonly Assembly k_ConnectAssembly = typeof(CloudProjectSettings).Assembly;
        static readonly Type k_UnityConnectType = k_ConnectAssembly.GetType("UnityEditor.Connect.UnityConnect");

        static object Instance => s_Instance ??=
            k_UnityConnectType
                .GetProperty("instance", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null, null);
        static object s_Instance;
        static Dictionary<UnityConnectEvents, EventInfo> s_Events = new Dictionary<UnityConnectEvents, EventInfo>();

        static SelfRefreshingGenesisAccessTokenCacheWrapper m_AccessTokenCache = new SelfRefreshingGenesisAccessTokenCacheWrapper();

        /// <summary>
        /// Get event info for event with passed id
        /// </summary>
        /// <param name="eventId"> ID of event to get info for </param>
        /// <returns> Requested event info </returns>
        public static EventInfo GetEventInfo(UnityConnectEvents eventId)
        {
            if (!s_Events.ContainsKey(eventId))
                s_Events[eventId] = k_UnityConnectType.GetEvent(eventId.ToString());
            return s_Events[eventId];
        }
        /// <summary>
        /// Get the current user's organization ID
        /// </summary>
        /// <returns> Current user's organization ID </returns>
        /// <exception cref="Exception"> Throws exception when organization ID cannot be fetched </exception>
        public static string GetUserOrganizationId()
        {
            try
            {
                return (string) GetOrganizationForeignKey.Invoke(Instance, null);
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not fetch CloudProjectSettings organization ID: {exception.Message}");
            }
        }
        /// <summary>
        /// Get the current user's access token
        /// </summary>
        /// <returns> Current user's access token </returns>
        public static async Task<string> GetUserAccessTokenAsync()
        {
            return await m_AccessTokenCache.GetCachedValueAsync();
        }

        /// <summary>
        /// Clear the current user's access token
        /// </summary>
        /// <exception cref="Exception"> Throws exception when access token cannot be cleared </exception>
        public static void ClearAccessToken()
        {
            try
            {
                k_UnityConnectType.GetMethod("ClearAccessToken")?.Invoke(Instance, null);
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not clear access token: {exception.Message}");
            }
        }

        /// <summary>
        /// Is the user currently logged in?
        /// </summary>
        /// <returns> True if the user is currently logged in </returns>
        /// <exception cref="Exception"> Throws exception when user's login state cannot be fetched </exception>
        public static bool GetIsLoggedIn()
        {
            try
            {
                return (bool) LoggedInProperty.GetValue(Instance);
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not fetch CloudProjectSettings log-in state: {exception.Message}");
            }
        }
        /// <summary>
        /// Is the user info ready?
        /// </summary>
        /// <returns> True if the user is currently logged in </returns>
        /// <exception cref="Exception"> Throws exception when user's login state cannot be fetched </exception>
        public static bool GetIsUserInfoReady()
        {
            try
            {
                return (bool) isUserInfoReady.GetValue(Instance);
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not fetch CloudProjectSettings is user ready state: {exception.Message}");
            }
        }

        /// <summary>
        /// Get whether or not the editor is currently in staging
        /// </summary>
        /// <returns> True if the editor is currently in staging </returns>
        /// <exception cref="Exception"> Throws exception when editor's current environment cannot be fetched </exception>
        public static bool GetIsStaging()
        {
            try
            {
                string environment = (string)ConfigurationProperty.GetValue(Instance);
                return string.Equals(environment, "staging");
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not fetch CloudProjectSettings environment: {exception.Message}");
            }
        }

        public static void ShowLogin()
        {
            GetShowLogin.Invoke(Instance, null);
        }


        /// <summary>
        /// Register to user state changed events
        /// </summary>
        public static Delegate RegisterUserStateChangedEvent(Action<object> changed) =>
            RegisterUnityConnectChangedEvent(changed, UnityConnectEvents.UserStateChanged);
        /// <summary>
        /// Register to project state changed events
        /// </summary>
        public static Delegate RegisterProjectStateChangedEvent(Action<object> changed) =>
            RegisterUnityConnectChangedEvent(changed, UnityConnectEvents.ProjectStateChanged);
        /// <summary>
        /// Register to connect state changed events
        /// </summary>
        public static Delegate RegisterConnectStateChangedEvent(Action<object> changed) =>
            RegisterUnityConnectChangedEvent(changed, UnityConnectEvents.StateChanged);
        /// <summary>
        /// Unregister from user state changed events
        /// </summary>
        public static void UnregisterUserStateChangedEvent(Delegate attachedDelegate) =>
            UnregisterUnityConnectStateChangedEvent(attachedDelegate, UnityConnectEvents.UserStateChanged);
        /// <summary>
        /// Unregister from project state changed events
        /// </summary>
        public static void UnregisterProjectStateChangedEvent(Delegate attachedDelegate) =>
            UnregisterUnityConnectStateChangedEvent(attachedDelegate, UnityConnectEvents.ProjectStateChanged);
        /// <summary>
        /// Unregister from connect state changed events
        /// </summary>
        public static void UnregisterConnectStateChangedEvent(Delegate attachedDelegate) =>
            UnregisterUnityConnectStateChangedEvent(attachedDelegate, UnityConnectEvents.StateChanged);

        /// <summary>
        /// Register callback to UnityConnectEvent
        /// </summary>
        /// <param name="callback"> Callback to register </param>
        /// <param name="eventId"> Event to register to </param>
        /// <returns></returns>
        /// <exception cref="Exception"> Event cannot be registered to </exception>
        static Delegate RegisterUnityConnectChangedEvent(Action<object> callback, UnityConnectEvents eventId)
        {
            try
            {
                var userChangedEvent = GetEventInfo(eventId);
                var convertedHandler = Delegate.CreateDelegate(
                    userChangedEvent.EventHandlerType,
                    callback.Target,
                    callback.Method);
                userChangedEvent.AddEventHandler(Instance, convertedHandler);
                return convertedHandler;
            }
            catch(Exception exception)
            {
                throw new Exception($"Could not register to change event: {exception.Message}");
            }
        }
        /// <summary>
        /// Unregister callback from UnityConnectEvent
        /// </summary>
        /// <param name="attachedDelegate"> Callback to unregister </param>
        /// <param name="eventId"> Event to unregister from </param>
        /// <returns></returns>
        /// <exception cref="Exception"> Event cannot be unregistered </exception>
        static void UnregisterUnityConnectStateChangedEvent(Delegate attachedDelegate, UnityConnectEvents eventId)
        {
            try
            {
                var userChangedEvent = GetEventInfo(eventId);

                userChangedEvent.RemoveEventHandler(Instance, attachedDelegate);
            }
            catch(Exception exception)
            {
                throw new Exception($"Could not de-register to change event: {exception.Message}");
            }
        }

    }
}

#endif