using System;
using System.Collections.Generic;

namespace Unity.Muse.Common
{
    struct ExtensionFilter
    {
        internal string name;
        internal string[] extensions;

        public ExtensionFilter(string filterName, params string[] filterExtensions)
        {
            name = filterName;
            extensions = filterExtensions;
        }
    }
    
    interface INativePluginImpl
    {
        string[] OpenFilePanel(string title, string directory, IEnumerable<ExtensionFilter> filters, bool multiselect);
        void OpenFilePanelAsync(string title, string directory, IEnumerable<ExtensionFilter> extensions, bool multiselect, Action<string[]> cb);
    }
}
