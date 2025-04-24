using System;
using System.Collections.Generic;
using UnityEditor;

namespace Unity.Muse.Common.Editor
{
    static class MuseProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Muse", SettingsScope.Project)
            {
                label = "Muse",
                activateHandler = (_, root) =>
                {
                    root.Add(new MuseProjectSettings());
                },
                keywords = new HashSet<string>(new[] { "Muse" })
            };

            return provider;
        }
    }
}
