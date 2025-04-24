using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Sentis;
using UnityEngine;

namespace Unity.DeepPose.ModelBackend
{
    class BenchmarkModel : MonoBehaviour
    {
        [Serializable]
        public struct ModelInput
        {
            public DataType DataType;
            public string Name;
            public int[] Shape;

            public Tensor AllocateTensor()
            {
                var shape = new TensorShape(Shape);
                switch (DataType)
                {
                    case DataType.Float:
                        return new Tensor<float>(shape, new float[shape.length]);

                    case DataType.Int:
                        return new Tensor<int>(shape, new int[shape.length]);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public BackendType BarracudaWorkerType = BackendType.CPU;
        public ModelAsset[] Models;
        public ModelInput[] Inputs;
        public int NumRuns = 100;
        public int DefaultUnknownSizeValue = -1;

        public void CreateInputs()
        {
            if (Models == null || Models.Length == 0)
            {
                Inputs = Array.Empty<ModelInput>();
                return;
            }

            var firstModel = Models[0];
            Model loadedModel = default;
            loadedModel = ModelLoader.Load(firstModel);
            var inputDefinitions = loadedModel.inputs;

            Inputs = new ModelInput[inputDefinitions.Count];

            for (var i = 0; i < Inputs.Length; i++)
            {
                var inputDefinition = inputDefinitions[i];

                var input = new ModelInput();
                input.Name = inputDefinition.name;
                input.Shape = new int[inputDefinition.shape.rank];
                for (var j = 0; j < input.Shape.Length; j++)
                {
                    if (inputDefinition.shape.IsStatic())
                    {
                        var shape = inputDefinition.shape.ToTensorShape();
                        input.Shape[j] = shape[j];
                    }
                    else
                    {
                        input.Shape[j] = DefaultUnknownSizeValue;
                    }
                }

                Inputs[i] = input;
            }
        }

        public void Benchmark()
        {
            foreach (var model in Models)
            {
                var time = Benchmark(model);
                UnityEngine.Debug.Log($"Model {model.name} took {time}ms per iteration");
            }
        }

        double Benchmark(ModelAsset model)
        {
            var modelDefinition = new ModelDefinition(model);
            var backend = new ModelBackend(modelDefinition, BarracudaWorkerType);

            var tensors = new Dictionary<string, Tensor>();
            foreach (var input in Inputs)
            {
                tensors[input.Name] = input.AllocateTensor();
                backend.SetInput(input.Name, tensors[input.Name]);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            for (var i = 0; i < NumRuns; i++)
            {
                backend.Execute();
            }
            stopwatch.Stop();

            foreach (var pair in tensors)
            {
                pair.Value.Dispose();
            }

            var elapsed = ((double)stopwatch.ElapsedMilliseconds) / NumRuns;
            return elapsed;
        }
    }
}