using System;
using System.Collections;
using Unity.Sentis;

#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
using Unity.ONNXRuntime;
#endif

namespace Unity.DeepPose.ModelBackend
{
    struct ModelBackend : IModelBackend
    {
        struct EmptyEnumerator : IEnumerator
        {
            public void Reset() { }
            public object Current  => throw new InvalidOperationException();
            public bool MoveNext() { return false; }
        }

        static EmptyEnumerator s_EmptyEnumerator;

        public ModelDefinition ModelDefinition => m_ModelDefinition;

        IWorker Worker
        {
            get
            {
                var worker = m_BarracudaWorker;
#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
                if (m_OnnxBackend != null)
                    worker = m_OnnxBackend.worker;
#endif
                return worker;
            }
        }

        ModelDefinition m_ModelDefinition;
        IWorker m_BarracudaWorker;

#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
        IBackend m_OnnxBackend;
        float m_OnnxScheduleProgress;
#endif

        public ModelBackend(ModelDefinition modelDefinition, BackendType barracudaWorkerType = BackendType.CPU)
        {
            m_ModelDefinition = modelDefinition;

#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
            m_OnnxBackend = null;
            m_OnnxScheduleProgress = 0f;
            if (m_ModelDefinition.IsOnnxRuntime)
            {
                m_OnnxBackend = NNInferenceSession.CreateBackend(modelDefinition.Model);
                m_BarracudaWorker = null;
            }
            else
#endif
            {
                var runtimeModel = ModelLoader.Load(modelDefinition.Model);
                m_BarracudaWorker = WorkerFactory.CreateWorker(barracudaWorkerType, runtimeModel);
            }
        }

        public void SetInput(string inputName, Tensor inputTensor)
        {
            Worker.SetInput(inputName, inputTensor);
        }

        public void Execute()
        {
            Worker.Execute();
        }

        public Tensor<T> PeekOutput<T>(string outputName) where T : unmanaged
        {
            return Worker.PeekOutput(outputName) as Tensor<T>;
        }

        public IEnumerator StartManualSchedule()
        {
#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
            if (m_ModelDefinition.IsOnnxRuntime)
            {
                Execute();
                m_OnnxScheduleProgress = 1f;
                return s_EmptyEnumerator;
            }
            else
#endif
            {
                return m_BarracudaWorker.ExecuteLayerByLayer();
            }
        }

        public float scheduleProgress
        {
            get
            {
#if UNITY_STANDALONE_WIN && DEEPPOSE_ONNXRUNTIME
                if (m_ModelDefinition.IsOnnxRuntime)
                {
                    return m_OnnxScheduleProgress;
                }
                else
#endif
                {
                    return m_BarracudaWorker.scheduleProgress;
                }
            }
        }

        public void Dispose()
        {
            m_BarracudaWorker?.Dispose();
        }
    }
}
