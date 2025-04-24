#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Muse.Common
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    class LinuxNativePluginImpl : INativePluginImpl
    {
        const char k_PathSeparator = (char)28;

        static LinuxNativePluginImpl()
        {
            NativePlugin.RegisterNativePluginImpl(new LinuxNativePluginImpl());
        }
        
        public string[] OpenFilePanel(string title, string directory, IEnumerable<ExtensionFilter> extensions, bool multiselect)
        {
            var command = new StringBuilder();
            command.Append("zenity --file-selection --title=\\\"");
            command.Append(title);
            command.Append("\\\" ");
            if (!string.IsNullOrEmpty(directory))
            {
                command.Append("--filename=\\\"");
                command.Append(directory);
                command.Append("\\\" ");
            }
            var filters = GetFilterFromFileExtensionList(extensions);
            command.Append(!string.IsNullOrEmpty(filters) ? filters : "--file-filter=\\\"All files|*\\\" ");
            if (multiselect)
                command.Append("--multiple ");
            command.Append("--separator=");
            command.Append(k_PathSeparator);
            
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            process.WaitForExit();

            string output = "";
            try 
            {
                output = process.StandardOutput.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return GetPaths(output);
        }

        public void OpenFilePanelAsync(string title, string directory, IEnumerable<ExtensionFilter> extensions, bool multiselect, Action<string[]> cb)
        {
            Task.Run(() =>
            {
                var paths = OpenFilePanel(title, directory, extensions, multiselect);
                MainThreadDispatcher.Invoke(() => cb.Invoke(paths));
            });
        }
        
        static string GetFilterFromFileExtensionList(IEnumerable<ExtensionFilter> extensions)
        {
            var filterString = new StringBuilder();
            if (extensions != null)
            {
                foreach (var filter in extensions)
                {
                    filterString.Append("--file-filter=\\\"");
                    filterString.Append(filter.name);
                    filterString.Append(" (");
                    filterString.Append(string.Join(", ", filter.extensions));
                    filterString.Append(") | ");
                    filterString.Append(string.Join(" ", filter.extensions.Select(ext => $"*.{ext}")));
                    filterString.Append("\\\" ");
                }
            }

            return filterString.ToString();
        }
        
        static string[] GetPaths(string result)
        {
            var ret = new List<string>();
            if (!string.IsNullOrEmpty(result))
            {
                result = result.TrimEnd('\n');
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