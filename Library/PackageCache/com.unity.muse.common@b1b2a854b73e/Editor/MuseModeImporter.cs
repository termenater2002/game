using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    [ScriptedImporter(1, new []{"musemode"},new [] {"json"})]
    internal class MuseModeImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<MuseModeAsset>();
            var json = File.ReadAllText(ctx.assetPath);
            asset.modes = JsonUtility.FromJson<Modes>(json);
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            ctx.AddObjectToAsset("MuseModeAsset", asset);
            ctx.SetMainObject(asset);
        }

        [MenuItem("internal:Muse/Internals/Update Modes _F6", false, 111)]
        static void LoadMuseMode()
        {
            ModesFactory.ModesChanged += ModesFactoryOnModesChanged;
            ModesFactory.LoadMuseModes();
        }
        
        private static void ModesFactoryOnModesChanged() {
            ModesFactory.ModesChanged -= ModesFactoryOnModesChanged;
            foreach (var museEditor in EditorModelAssetEditor.GetAllInstances<MuseEditor>())
            {
                var m = museEditor.CurrentModel;
                Debug.Log("reload " + museEditor);
                museEditor.Close();
                EditorApplication.delayCall += () => EditorModelAssetEditor.OpenEditorTo(m);
                // museEditor.Show();
            }
        }
    }
}
