using System.Collections.Generic;
using Unity.Sentis;

#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
using Unity.ONNXRuntime;
#endif

namespace Unity.DeepPose.ModelBackend
{
    struct ModelDefinition : IModelDefinition
    {
        ModelAsset m_Model;

        public ModelAsset Model => m_Model;

        public List<string> InputNames
        {
            get
            {
                if (m_InputNames == null)
                    RetrieveInputs();

                return m_InputNames;
            }
        }

        List<string> m_InputNames;

        public bool IsOnnxRuntime {
            get
            {
#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
                return m_Model is NNModelONNXRuntime;
#endif
                return false;
            }
        }

        public ModelDefinition(ModelAsset model)
        {
            m_Model = model;
            m_InputNames = null;
        }

        public bool TryReadMetaData<T>(out T metadata)
        {
            return m_Model.TryReadMetaData(out metadata);
        }

        void RetrieveInputs()
        {
            if (m_InputNames == null)
                m_InputNames = new List<string>();
            else
                m_InputNames.Clear();

#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
            if (m_Model is NNModelONNXRuntime)
            {
                var backend = NNInferenceSession.CreateBackend(m_Model);
                foreach (var input in backend.inputs)
                {
                    m_InputNames.Add(input.name);
                }
            }
            else
#endif
            {
                Model loadedModel = default;
                loadedModel = ModelLoader.Load(Model);
                foreach (var input in loadedModel.inputs)
                {
                    m_InputNames.Add(input.name);
                }
            }
        }
    }
}