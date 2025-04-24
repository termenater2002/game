using System;
using System.Threading.Tasks;
using Unity.Sentis;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class TensorUtils
    {
        public static Tensor<T> Alloc<T>(int d0, int d1 = -1, int d2 = -1, int d3 = -1) where T : unmanaged
        {
            var shape = CreateShape(d0, d1, d2, d3);
            return new Tensor<T>(shape, clearOnInit: true);
        }

        static TensorShape CreateShape(int d0, int d1, int d2, int d3)
        {
            if (d3 >= 0)
            {
                return new TensorShape( d0, d1, d2, d3 );
            }

            if (d2 >= 0)
            {
                return new TensorShape(d0, d1, d2);
            }

            if (d1 >= 0)
            {
                return new TensorShape(d0, d1 );
            }

            if (d0 >= 0)
            {
                return new TensorShape(d0);
            }

            Debug.LogError($"Invalid buffer shape: ({d0}, {d1}, {d2})");

            return new TensorShape(0);
        }

        public static Tensor<float> NewTensorFloat(TensorShape shape)
        {
            var tensor = new Tensor<float>(shape, new float[shape.length]);
            return tensor;
        }

        public static Tensor<int> NewTensorInt(TensorShape shape)
        {
            var tensor = new Tensor<int>(shape, new int[shape.length]);
            return tensor;
        }

        public static void Fill(Tensor<float> t, float value)
        {
            for (var i = 0; i < t.shape.length; ++i)
            {
                t[i] = value;
            }
        }

        public static void Fill(Tensor<int> t, int value)
        {
            for (var i = 0; i < t.shape.length; ++i)
            {
                t[i] = value;
            }
        }
    }
}