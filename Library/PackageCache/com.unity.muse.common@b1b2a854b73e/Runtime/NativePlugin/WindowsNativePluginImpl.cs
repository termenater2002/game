#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Common
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    class WindowsNativePluginImpl : INativePluginImpl
    {
        static WindowsNativePluginImpl()
        {
            NativePlugin.RegisterNativePluginImpl(new WindowsNativePluginImpl());
        }
        
        const int k_MaxFileLength = 2048;
        
        // ReSharper disable InconsistentNaming
        const char NULL_CHAR = '\0';
        
        const int OFN_NOCHANGEDIR = 0x00000008;

        const int OFN_ALLOWMULTISELECT = 0x00000200;

        const int OFN_PATHMUSTEXIST = 0x00000800;

        const int OFN_FILEMUSTEXIST = 0x00001000;

        const int OFN_EXPLORER = 0x00080000;

        const int WM_USER = 0x400;

        const int BFFM_INITIALIZED = 1;

        const int BFFM_SELCHANGED = 2;

        const int BFFM_SETSELECTIONW = WM_USER + 103;

        const int BFFM_SETSTATUSTEXTW = WM_USER + 104;

        const int BFFM_SETEXPANDED = WM_USER + 106;

        const int BIF_USENEWUI = 0x0040 + 0x0010;

        const int BIF_SHAREABLE = 0x8000;

        const int MAX_LENGTH = 256;
        // ReSharper restore InconsistentNaming
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class OpenFileName
        {
            internal int structSize = 0;

            internal IntPtr dlgOwner = IntPtr.Zero;

            internal IntPtr instance = IntPtr.Zero;

            internal string filter = null;

            internal string customFilter = null;

            internal int maxCustFilter = 0;

            internal int filterIndex = 0;

            internal IntPtr file;

            internal int maxFile = 0;

            internal string fileTitle = null;

            internal int maxFileTitle = 0;

            internal string initialDir = null;

            internal string title = null;

            internal int flags = 0;

            internal short fileOffset = 0;

            internal short fileExtension = 0;

            internal string defExt = null;

            internal IntPtr custData = IntPtr.Zero;

            internal IntPtr hook = IntPtr.Zero;

            internal string templateName = null;

            internal IntPtr reservedPtr = IntPtr.Zero;

            internal int reservedInt = 0;

            internal int flagsEx = 0;

            ~OpenFileName()
            {
                Marshal.FreeHGlobal(file);
            }

            public OpenFileName() { }

            internal OpenFileName(string title, IEnumerable<ExtensionFilter> extensions, bool allowMultiSelect = true,
                string directory = null, string defaultName = null)
            {
                structSize = Marshal.SizeOf(typeof(OpenFileName));
                filter = GetRawExtensions(extensions);
                file = Marshal.AllocHGlobal(k_MaxFileLength * Marshal.SystemDefaultCharSize);
                var nameArr = string.IsNullOrEmpty(defaultName)
                    ? new[] {NULL_CHAR, NULL_CHAR}
                    : (defaultName + NULL_CHAR).ToCharArray();
                Marshal.Copy(nameArr, 0, file, Mathf.Min(nameArr.Length, k_MaxFileLength));
                maxFile = k_MaxFileLength;
                fileTitle = new string(new char[64]);
                maxFileTitle = fileTitle.Length;
                initialDir = directory ?? Application.dataPath;
                this.title = title;
                flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;
                if (allowMultiSelect)
                    flags |= OFN_ALLOWMULTISELECT;
                dlgOwner = GetForegroundWindow();
            }

            internal List<string> GetFiles()
            {
                var selectedFilesList = new List<string>();

                var filePointer = file;
                var pointer = (long) filePointer;
                var name = Marshal.PtrToStringAuto(filePointer);

                // Retrieve file names
                while (!string.IsNullOrEmpty(name))
                {
                    selectedFilesList.Add(name);

                    pointer += name.Length * Marshal.SystemDefaultCharSize + Marshal.SystemDefaultCharSize;
                    filePointer = (IntPtr) pointer;
                    name = Marshal.PtrToStringAuto(filePointer);
                    if ((flags & OFN_ALLOWMULTISELECT) ==
                        0) // if multiselect is false, we force to stop here because Windows doesn't clear next bytes
                        break;
                }

                // Multiple files selected, add directory
                return selectedFilesList.Count > 1
                    ? selectedFilesList.GetRange(1, selectedFilesList.Count - 1)
                        .Select(f => Path.Combine(selectedFilesList[0], f)).ToList()
                    : selectedFilesList;
            }
            
            static string GetRawExtensions(IEnumerable<ExtensionFilter> extensions)
            {
                var rawExtensions = extensions.Select(ext =>
                {
                    var rgxExt = ext.extensions.Select(e => $"*{e}").ToArray();
                    return $"{ext.name} ({string.Join(",", rgxExt)})\0{string.Join(";", rgxExt)}\0";
                });
                return $"{string.Join("", rawExtensions)}\0";
            }

            internal static string GetSelectedFilter(string rawFilter, int filterIndex)
            {
                if (string.IsNullOrEmpty(rawFilter) || filterIndex <= 0)
                    return string.Empty;

                var lastZeroIndex = rawFilter.IndexOfNth(NULL_CHAR, filterIndex * 2) + 1;
                if (lastZeroIndex == 0)
                    return string.Empty;

                var firstZeroIndex = rawFilter.IndexOfNth(NULL_CHAR, (filterIndex - 1) * 2) + 1;

                return rawFilter.Substring(firstZeroIndex, lastZeroIndex - firstZeroIndex);
            }
        }
        
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        public string[] OpenFilePanel(string title, string directory, IEnumerable<ExtensionFilter> filters, bool multiselect)
        {
            var data = new OpenFileName(title, filters, multiselect, directory);
            return GetOpenFileName(data) ? data.GetFiles().ToArray() : new string[] {};
        }

        public void OpenFilePanelAsync(string title, string directory, IEnumerable<ExtensionFilter> filters, bool multiselect, Action<string[]> cb)
        {
            Task.Run(() =>
            {
                var files = OpenFilePanel(title, directory, filters, multiselect);
                MainThreadDispatcher.Invoke(() => cb?.Invoke(files));
            });
        }
    }
}

#endif