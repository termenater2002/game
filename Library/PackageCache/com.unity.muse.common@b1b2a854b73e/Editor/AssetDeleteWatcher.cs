using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    internal class AssetDeleteWatcher : AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Model>(assetPath);
            if (asset != null)
            {
                var editors = Resources.FindObjectsOfTypeAll<MuseEditor>().Where(w => w.CurrentModel == asset).ToArray();
                if (editors.Length > 0)
                {
                    if (!EditorUtility.DisplayDialog(TextContent.assetRemovedFromProjectTitle, string.Format(TextContent.assetRemovedFromProjectMessage, asset.name), TextContent.deleteSingle, TextContent.cancel))
                        return AssetDeleteResult.DidDelete;
                }
                foreach (var museEditor in editors)
                    museEditor.Close();

                ArtifactCache.Delete(asset.AssetsData);
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
