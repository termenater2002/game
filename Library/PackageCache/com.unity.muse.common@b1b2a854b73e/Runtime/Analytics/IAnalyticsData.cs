#if !ENABLE_UNITYENGINE_ANALYTICS
using System;

namespace Unity.Muse.Common.Analytics
{
    interface IAnalytic
    {
        interface IData
        {
            string EventName { get; }
            int Version { get; }
        }

        bool TryGatherData(out IData data, out Exception error);
    }
}
#endif
