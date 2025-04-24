using System;
using System.Collections;
using Unity.Sentis;

namespace Unity.DeepPose.ModelBackend
{
    interface IWorker : IDisposable
    {
        public void SetInput(string inputName, Tensor inputTensor);
        public void Execute();
        public Tensor PeekOutput(string outputName);
        public IEnumerator ExecuteLayerByLayer();

        public float scheduleProgress { get; }
    }

    static class WorkerFactory
    {
        public static IWorker CreateWorker(BackendType backendType, Model model)
        {
            return new SentisWorker(model, backendType);
        }
    }
}
