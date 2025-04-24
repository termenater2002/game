using System;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    internal class AssetMoveWatcher : AssetModificationProcessor
    {
        static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            var model = AssetDatabase.LoadAssetAtPath<Model>(sourcePath);
            if (model)
            {
                foreach (var editor in Resources.FindObjectsOfTypeAll<MuseEditor>())
                {
                    if (editor.CurrentModel == model)
                        editor.AssetMoved(destinationPath);
                }
            }

            return AssetMoveResult.DidNotMove;
        }
    }
}
