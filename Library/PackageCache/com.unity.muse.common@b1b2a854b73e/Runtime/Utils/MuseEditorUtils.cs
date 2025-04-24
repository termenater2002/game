using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Muse.Common
{
    static class MuseEditorUtils
    {
        internal static void SetLabelOnExportedArtifact(string relativePath)
        {
#if UNITY_EDITOR
            const string unityMuseAI = "Unity Muse AI";
            var asset = AssetDatabase.LoadMainAssetAtPath(relativePath);
            var labelList = new List<string>(AssetDatabase.GetLabels(asset));
            if (!labelList.Contains(unityMuseAI))
            {
                labelList.Add(unityMuseAI);
                AssetDatabase.SetLabels(asset, labelList.ToArray());
            }
#endif
        }
    }
}
