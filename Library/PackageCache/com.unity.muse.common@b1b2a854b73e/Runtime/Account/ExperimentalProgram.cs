using System;
using System.Collections.Generic;

namespace Unity.Muse.Common.Account
{
    /// <summary>
    /// Interface for an experimental program instance.
    /// You can implement this interface to create a new experimental program.
    /// </summary>
    interface IExperimentalProgramInstance : IDisposable
    {
        /// <summary>
        /// Callback that is invoked when the balance of the authenticated user for the experimental program is retrieved.
        /// </summary>
        /// <param name="balance"> The balance of the authenticated user for the experimental program (if any). </param>
        /// <param name="errorMessage"> An error message if the balance could not be retrieved. </param>
        delegate void GetBalanceCallback(int? balance, string errorMessage);
        
        /// <summary>
        /// Event that is invoked when the experimental program instance changes.
        /// </summary>
        event Action Changed;

        /// <summary>
        /// Whether the current user is authorized to use the experimental program.
        /// </summary>
        void IsUserAuthorized(Action<bool> callback);

        /// <summary>
        /// Initialize the experimental program instance.
        /// </summary>
        void Init();
        
        /// <summary>
        /// Get the current points balance of the authenticated user for the experimental program.
        /// </summary>
        /// <param name="callback"> Callback that will be invoked with the balance, or null if the balance could not be retrieved. </param>
        void GetBalance(GetBalanceCallback callback);
        
        /// <summary>
        /// Poll for any changes on the experimental program instance.
        /// </summary>
        void RequestUpdate();
    }
    
    /// <summary>
    /// Class that manages the current experimental program instance (if any).
    /// </summary>
    /// <remarks>
    /// Only a single experimental program instance can be configured at a time.
    /// To know if an experimental program instance is currently configured, check <see cref="IsConfigured"/>.
    /// </remarks>
    static class ExperimentalProgram
    {
        static IExperimentalProgramInstance s_Instance;
        
        delegate void GetBalanceDelegate(IExperimentalProgramInstance.GetBalanceCallback callback);
        
        delegate void IsAuthorizedDelegate(Action<bool> callback);
        
        static GetBalanceDelegate s_GetBalanceFunc;
        
        static IsAuthorizedDelegate s_IsAuthorizedFunc;

        static readonly Dictionary<string, bool> k_BalanceCheckPerMode = new Dictionary<string, bool>();

        static readonly Dictionary<string, bool> k_EnabledPerMode = new Dictionary<string, bool>();

        /// <summary>
        /// Event that is invoked when the current experimental program instance changes.
        /// </summary>
        public static event Action Changed;

        /// <summary>
        /// Whether an experimental program instance is currently configured.
        /// If there is no instance, this will be false.
        /// </summary>
        public static bool IsConfigured => s_Instance != null;

        /// <summary>
        /// Get the current points balance of the authenticated use for the experimental program.
        /// </summary>
        /// <param name="callback"> Callback that will be invoked with the balance, or null if the balance could not be retrieved. </param>
        public static void GetBalance(IExperimentalProgramInstance.GetBalanceCallback callback)
        {
            if (!IsConfigured || s_GetBalanceFunc == null)
                callback?.Invoke(null, null);
            else
                s_GetBalanceFunc?.Invoke(callback);
        }

        /// <summary>
        /// Whether the current user is authorized to use the experimental program.
        /// </summary>
        public static void IsUserAuthorized(Action<bool> callback)
        {
            if (!IsConfigured || s_IsAuthorizedFunc == null)
                callback?.Invoke(true);
            else
                s_IsAuthorizedFunc?.Invoke(callback);
        }

        internal static void SetBalanceOverride(int points, bool isOverride)
        {
            s_GetBalanceFunc = isOverride 
                ? new GetBalanceDelegate(callback => callback?.Invoke(points, null)) 
                : s_Instance.GetBalance;
            
            InvokeChanged();
        }
        
        internal static void SetAuthorizedOverride(bool isAuthorized, bool isOverride)
        {
            s_IsAuthorizedFunc = isOverride 
                ? new IsAuthorizedDelegate(callback => callback?.Invoke(isAuthorized)) 
                : s_Instance.IsUserAuthorized;
            
            InvokeChanged();
        }
        
        /// <summary>
        /// Register a new experimental program instance.
        /// </summary>
        /// <remarks>
        /// If the given type is already registered, this method will do nothing.
        /// </remarks>
        /// <typeparam name="TProgramBackendType"> The type of the experimental program instance to register. </typeparam>
        public static void RegisterProgram<TProgramBackendType>()
            where TProgramBackendType : IExperimentalProgramInstance, new()
        {
            if (s_Instance != null && s_Instance.GetType() == typeof(TProgramBackendType))
                return;

            if (s_Instance != null)
            {
                s_GetBalanceFunc = null;
                s_IsAuthorizedFunc = null;
                s_Instance.Changed -= InvokeChanged;
                s_Instance.Dispose();
                s_Instance = null;
            }

            s_Instance = new TProgramBackendType();
            s_Instance.Init();
            s_GetBalanceFunc = s_Instance.GetBalance;
            s_IsAuthorizedFunc = s_Instance.IsUserAuthorized;
            s_Instance.Changed += InvokeChanged;
        }
        
        /// <summary>
        /// Set whether to check the balance for the given mode type.
        /// </summary>
        /// <param name="mppModeType"> The mode type to set the check balance for. </param>
        /// <param name="enable"> Whether to check the balance for the given mode type. </param>
        public static void EnableCheckBalanceForMode(string mppModeType, bool enable)
        {
            k_BalanceCheckPerMode[mppModeType] = enable;
        }
        
        /// <summary>
        /// Whether to check the balance for the given mode type.
        /// </summary>
        /// <param name="mppModeType"> The mode type to check the balance for. </param>
        /// <returns> Whether to check the balance for the given mode type. </returns>
        public static bool ShouldCheckBalanceForMode(string mppModeType)
        {
            return k_BalanceCheckPerMode.TryGetValue(mppModeType, out var shouldCheck) && shouldCheck;
        }

        /// <summary>
        /// Whether the experimental program is enabled for the given mode type.
        /// </summary>
        /// <param name="mppModeType"> The mode type to check the enabled state for. </param>
        /// <param name="enable"> Whether the experimental program is enabled for the given mode type. </param>
        public static void EnableForMode(string mppModeType, bool enable)
        {
            k_EnabledPerMode[mppModeType] = enable;
        }
        
        /// <summary>
        /// Whether the experimental program is enabled for the given mode type.
        /// </summary>
        /// <param name="mppModeType"> The mode type to check the enabled state for. </param>
        /// <returns> Whether the experimental program is enabled for the given mode type. </returns>
        public static bool IsEnabledForMode(string mppModeType)
        {
            return k_EnabledPerMode.TryGetValue(mppModeType, out var enabled) && enabled;
        }
        
        /// <summary>
        /// Poll for any changes on the current experimental program instance.
        /// </summary>
        public static void Refresh()
        {
            if (!IsConfigured)
                return;

            s_Instance.RequestUpdate();
        }
        
        static void InvokeChanged()
        {
            Changed?.Invoke();
        }
    }
}
