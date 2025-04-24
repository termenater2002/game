using System.Collections.Generic;

namespace Unity.DeepPose.ModelBackend
{
    interface IModelDefinition
    {
        public bool IsOnnxRuntime { get; }
        public bool TryReadMetaData<T>(out T metadata);
        public List<string> InputNames { get; }
    }
}