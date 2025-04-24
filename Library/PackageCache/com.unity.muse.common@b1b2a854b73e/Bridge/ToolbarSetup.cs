#if MUSE_TOOLBAR_BUTTON_ENABLED
using System.Reflection;
using Unity.Muse.Common.Account.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Bridge
{
    [InitializeOnLoad]
    public class ToolbarSetup
    {
        static ToolbarSetup()
        {
            EditorApplication.delayCall += () =>
            {
                var toolbar = Toolbar.get;
                var rootField = typeof(Toolbar).GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rootField != null && rootField.GetValue(toolbar) is VisualElement root)
                {
                    AccountDropdownWindow.toolbarButton = root.Q<UnityEditor.Toolbars.MuseDropdown>();
                    AccountDropdownWindow.toolbarPosition = () => Toolbar.get.screenPosition;
                }
            };
        }
    }
}
#endif
