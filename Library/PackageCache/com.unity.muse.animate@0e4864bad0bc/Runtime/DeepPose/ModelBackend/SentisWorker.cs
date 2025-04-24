using System;
using System.Collections;
using Unity.Sentis;

namespace Unity.DeepPose.ModelBackend
{
    class SentisWorker : IWorker
    {
        Worker m_Worker;

        public SentisWorker(Model model, BackendType backendType)
        {
            m_Worker = new Worker(model, backendType);
        }

        public void SetInput(string inputName, Tensor inputTensor)
        {
            m_Worker.SetInput(inputName, inputTensor);
        }

        public void Execute()
        {
            m_Worker.Schedule();
        }

        public Tensor PeekOutput(string outputName)
        {
            return m_Worker.PeekOutput(outputName);
        }

        public IEnumerator ExecuteLayerByLayer()
        {
            return m_Worker.ScheduleIterable();
        }

        public float scheduleProgress => throw new NotImplementedException();

        public void Dispose()
        {
            m_Worker.Dispose();
        }
    }
}