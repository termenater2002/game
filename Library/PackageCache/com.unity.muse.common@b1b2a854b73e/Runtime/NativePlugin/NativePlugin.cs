using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Common
{
    static class NativePlugin
    {
        static INativePluginImpl s_Impl;
        
        /// <summary>
        /// Register the native plugin implementation.
        /// </summary>
        /// <remarks>
        /// This should not be called by user code or by others Muse packages.
        /// </remarks>
        /// <param name="impl"> The native plugin implementation. </param>
        internal static void RegisterNativePluginImpl(INativePluginImpl impl)
        {
            Assert.IsNull(s_Impl, "NativePlugin.RegisterNativePluginImpl: Native plugin implementation already registered.");
            s_Impl = impl;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureImpl()
        {
            if (s_Impl == null)
                throw new InvalidOperationException("NativePlugin: No native plugin implementation registered.");
        }
        
        internal static string[] OpenFilePanel(string title, string directory, IEnumerable<ExtensionFilter> filters, bool multiselect)
        {
            EnsureImpl();
            return s_Impl.OpenFilePanel(title, directory, filters, multiselect);
        }
        
        internal static void OpenFilePanelAsync(string title, string directory, IEnumerable<ExtensionFilter> filters, bool multiselect, Action<string[]> cb)
        {
            EnsureImpl();
            s_Impl.OpenFilePanelAsync(title, directory, filters, multiselect, cb);
        }
    }
}
