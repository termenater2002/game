#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Muse.Common
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    class MacOSNativePluginImpl : INativePluginImpl
    {
        const char k_PathSeparator = (char)28;
        
        static Action<string[]> s_OpenFileCb;
        
        static MacOSNativePluginImpl()
        {
            NativePlugin.RegisterNativePluginImpl(new MacOSNativePluginImpl());
        }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AsyncCallback(string path);

        [AOT.MonoPInvokeCallback(typeof(AsyncCallback))]
        static void OpenFileCb(string result)
        {
            s_OpenFileCb.Invoke(GetPaths(result));
        }
        
        [DllImport("MuseNativePlugin")]
        static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);
        
        [DllImport("MuseNativePlugin")]
        static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);
        
        public string[] OpenFilePanel(string title, string directory, IEnumerable<ExtensionFilter> extensions, bool multiselect)
        {
            var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(
                title,
                directory,
                GetFilterFromFileExtensionList(extensions),
                multiselect));
            return GetPaths(paths);
        }

        public void OpenFilePanelAsync(string title, string directory, IEnumerable<ExtensionFilter> extensions, bool multiselect, Action<string[]> cb)
        {
            s_OpenFileCb = cb;
            DialogOpenFilePanelAsync(
                title,
                directory,
                GetFilterFromFileExtensionList(extensions),
                multiselect,
                OpenFileCb);
        }
        
        static string GetFilterFromFileExtensionList(IEnumerable<ExtensionFilter> extensions)
        {
            if (extensions == null)
            {
                return "";
            }

            var filterString = "";
            foreach (var filter in extensions)
            {
                filterString += filter.name + ";";

                foreach (var ext in filter.extensions)
                {
                    filterString += ext.TrimStart('.') + ",";
                }

                filterString = filterString.Remove(filterString.Length - 1);
                filterString += "|";
            }
            filterString = filterString.Remove(filterString.Length - 1);
            return filterString;
        }
        
        static string[] GetPaths(string result)
        {
            var ret = new List<string>();
            if (!string.IsNullOrEmpty(result))
            {
                foreach (var p in result.Split(k_PathSeparator))
                {
                    if (!string.IsNullOrEmpty(p))
                        ret.Add(p);
                }
            }
            return ret.ToArray();
        }
    }
}

#endif