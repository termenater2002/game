using System.Runtime.CompilerServices;
using Unity.Sentis;
using UnityEditor.AssetImporters;
using UnityEngine;

[assembly: InternalsVisibleTo("Unity.Sentis.ONNX")]

namespace Unity.Muse.Animate
{
    class OnnxMetadataSerializer : IONNXMetadataImportCallbackReceiver
    {
        public void OnMetadataImported(AssetImportContext ctx, ONNXModelMetadata onnxModelMetadata)
        {
            var metadata = ScriptableObject.CreateInstance<SerializedOnnxMetadata>();
            metadata.name = "ONNX Metadata";
            metadata.Metadata = onnxModelMetadata;
            ctx.AddObjectToAsset("metadata", metadata);
        }
    }
}
