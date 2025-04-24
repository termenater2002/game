using System.ComponentModel;
using UnityEditor;
using UnityEngine;

// We need to add this to make the record type work in Unity with the init keyword
// The type System.Runtime.CompilerServices.IsExternalInit is defined in .NET 5 and later, which Unity does not support yet
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace Unity.Muse.Common.Editor
{
    static class MenuItems
    {
        record GuidModelPair(string Guid, Model Model);
        static GuidModelPair s_LatestSelectedGuidModelPair;

        [MenuItem("Assets/Open in Muse", false, 20)]
        public static void OpenMuseAssetWindowMenuItem()
        {
            EditorModelAssetEditor.OpenEditorTo(s_LatestSelectedGuidModelPair?.Model);

            var artifact = s_LatestSelectedGuidModelPair?.Model.GetArtifactByGuid(s_LatestSelectedGuidModelPair?.Guid);
            if (artifact != null)
            {
                s_LatestSelectedGuidModelPair.Model.RefineArtifact(artifact);
            }

            s_LatestSelectedGuidModelPair = null;
        }

        [MenuItem("Assets/Open in Muse", true)]
        public static bool OpenMuseAssetWindowMenuItemValidation()
        {
            s_LatestSelectedGuidModelPair = null;

            if (Selection.activeObject is Model)
            {
                s_LatestSelectedGuidModelPair = new GuidModelPair(string.Empty, (Model)Selection.activeObject);
                return true;
            }

            var guids = AssetDatabase.FindAssets("t:Unity.Muse.Common.Model");
            var assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));

            foreach (var guid in guids)
            {
                var modelInstance = AssetDatabase.LoadAssetAtPath<Model>(AssetDatabase.GUIDToAssetPath(guid));
                if (modelInstance == null)
                {
                    Debug.LogWarning("Model instance is null");
                    continue;
                }

                var museGuid = modelInstance.GetExportedArtifact(assetGUID);

                if (string.IsNullOrEmpty(museGuid))
                {
                    continue;
                }

                s_LatestSelectedGuidModelPair = new GuidModelPair(museGuid, modelInstance);
                return true;
            }
            return false;
        }
    }
}
