
using System.Collections.Generic;
using System.Threading;
using Unity.Muse.Common;
using UnityEditor;
using UnityEngine;

namespace Unity.GenerativeAI.Editor
{
    class EditorCloudContext : ICloudContext
    {
        [InitializeOnLoadMethod]
        static void InjectEditorContext()
        {
            s_UnitySynchronizationContext = SynchronizationContext.Current;
            CloudContextFactory.InjectCloudContextType<EditorCloudContext>();
        }

        static Dictionary<ICloudContext.Callback, EditorApplication.CallbackFunction> s_CallbackDelegateTrackingTable = new();
        static SynchronizationContext s_UnitySynchronizationContext;

        void ICloudContext.RegisterNextFrameCallback(ICloudContext.Callback cb)
        {
            s_UnitySynchronizationContext.Post(SynchronizationContextPostCallback, cb);
        }

        static void SynchronizationContextPostCallback(object cb)
        {
            ((ICloudContext.Callback)cb)();
        }
    }
}
