using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Editor
{
    class MuseAssetCreation : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            pathName = Path.ChangeExtension(pathName, ".asset");

            var asset = CreateInstance<Model>();
            asset.Initialize();
            int mode = ModesFactory.GetModeIndexFromKey(UIMode.UIMode.modeKey);
            if (mode < 0)
                Debug.LogError($"Mode {UIMode.UIMode.modeKey} not found");
            asset.ModeChanged(mode);

            AssetDatabase.CreateAsset(asset, pathName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/Muse/Sprite Generator", false, 0)]
        static void CreateSpriteLibrarySourceAssetMenu()
        {
            var action = CreateInstance<MuseAssetCreation>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, "Muse Sprite.asset", IconHelper.assetIcon, null);
        }
    }
}
