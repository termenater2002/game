using System;
using Newtonsoft.Json;
using Unity.Muse.Animate;
using Unity.Sentis;
using UnityEditor;
using UnityEngine;

namespace Unity.DeepPose.ModelBackend
{
    static partial class MetadataUtils
    {
        public static bool TryReadMetaData<T>(this ModelAsset model, out T metadata)
        {
            var assetPath = AssetDatabase.GetAssetPath(model);
            var metadataData = AssetDatabase.LoadAssetAtPath<SerializedOnnxMetadata>(assetPath);
            
            if (metadataData != null && metadataData.Metadata.MetadataProps.TryGetValue("json", out var json))
            {
                metadata = JsonConvert.DeserializeObject<T>(json);
                return true;
            }

            metadata = default;
            return false;
        }
    }
}
