using UnityEngine;

namespace Unity.Muse.Common
{
    internal static class IconHelper
    {
        static Texture2D s_WindowIcon;
        static Texture2D s_AssetIcon;

        public const string assetIconPath = "Packages/com.unity.muse.common/Editor/Resources/Icons/MuseAssetIcon.png";

        public static Texture2D windowIcon
        {
            get
            {
                #if UNITY_EDITOR
                if (!s_WindowIcon)
                    s_WindowIcon = UnityEditor.EditorGUIUtility.isProSkin 
                        ? ResourceManager.Load<Texture2D>(PackageResources.darkMuseIcon) 
                        : ResourceManager.Load<Texture2D>(PackageResources.museIcon);
                #endif
                return s_WindowIcon;
            }
        }

        public static Texture2D assetIcon
        {
            get
            {
                if (!s_AssetIcon)
                    s_AssetIcon = ResourceManager.Load<Texture2D>(PackageResources.museAssetIcon);
                return s_AssetIcon;
            }
        }
    }
}
