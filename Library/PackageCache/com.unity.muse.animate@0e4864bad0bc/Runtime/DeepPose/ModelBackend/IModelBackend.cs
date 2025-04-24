using System;
using System.Collections;
using Unity.Sentis;

namespace Unity.DeepPose.ModelBackend
{
    interface IModelBackend: IDisposable
    {
        ModelDefinition ModelDefinition { get; }
        void SetInput(string inputName, Tensor inputTensor);
        void Execute();
        Tensor<T> PeekOutput<T>(string outputName) where T: unmanaged;
        IEnumerator StartManualSchedule();
        float scheduleProgress { get; }
    }
}
