using System;
using System.Collections.Generic;
using System.Linq;
#if ENABLE_UNITYENGINE_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.Muse.Common.Analytics
{
    static class AnalyticsManager
    {
        internal const int maxEventsPerHour = 500;
        internal const string vendorKey = "unity.muse";

        static Dictionary<Type, Action<IAnalytic>> s_Registry = new();

        internal static void RegisterEvent<T>(Action<IAnalytic> action) where T : IAnalytic
        {
            s_Registry.TryAdd(typeof(T), action);
        }

        internal static void SendAnalytics(IAnalytic analytic)
        {
            var eventType = s_Registry.Keys.FirstOrDefault(t => analytic != null && t.IsInstanceOfType(analytic));
            if (eventType == null)
                throw new Exception("Event type not registered.");
            var callback = s_Registry[eventType];
            if (callback == null)
                throw new Exception("Event type registration no longer valid.");
            callback.Invoke(analytic);
        }
    }
}
