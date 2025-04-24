using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Cloud
{
    static class SerializationUtils
    {
        static Vector3[] s_SingleVector3 = new Vector3[1];
        static Quaternion[] s_SingleQuaternion = new Quaternion[1];

        static void Fill(Quaternion[] rotations, NativeArray<float> a) => rotations.FromFloatArray(a);
        static void Fill(Vector3[] positions, NativeArray<float> a) => positions.FromFloatArray(a);
        
        public static string ToBase64(this float[] floats)
        {
            using var floatArray = new NativeArray<float>(floats, Allocator.Temp);
            using var pooledArray = floatArray.AsBytePooledArray();
            return pooledArray.Array.ToDataString(true);
        }
        public static string ToBase64(this Vector3 position)
        {
            s_SingleVector3[0] = position;
            return s_SingleVector3.ToBase64();
        }

        public static string ToBase64(this Quaternion rotation)
        {
            s_SingleQuaternion[0] = rotation;
            return s_SingleQuaternion.ToBase64();
        }

        public static string ToBase64(this Vector3[] positions)
        {
            using var floatArray = new NativeArray<float>(positions.Length * 3, Allocator.Temp);
            positions.ToFloatArray(floatArray);

            using var pooledArray = floatArray.AsBytePooledArray();
            return pooledArray.Array.ToDataString(true);
        }

        public static string ToBase64(this Quaternion[] rotations)
        {
            using var floatArray = new NativeArray<float>(rotations.Length * 4, Allocator.Temp);
            rotations.ToFloatArray(floatArray);

            using var pooledArray = floatArray.AsBytePooledArray();
            return pooledArray.Array.ToDataString(true);
        }

        static PooledArray<T> Base64ToArray<T>(this string base64, int floatsPerElement, Action<T[], NativeArray<float>> fillElementsFromFloats)
        {
            if (string.IsNullOrEmpty(base64))
                return default;

            var bytes = base64.BytesFromDataString();
            using var floatArray = BytesToNativeArray(bytes, Allocator.Temp);

            Assert.AreEqual(0, floatArray.Length % floatsPerElement);

            var elements = new PooledArray<T>(floatArray.Length / floatsPerElement);
            fillElementsFromFloats(elements.Array, floatArray);

            return elements;
        }

        public static PooledArray<float> Base64ToFloatArray(this string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return default;

            var bytes = base64.BytesFromDataString();
            using var floatArray = BytesToNativeArray(bytes, Allocator.Temp);

            var values = new PooledArray<float>(floatArray.Length);
            floatArray.CopyTo(values.Array);

            return values;
        }

        public static PooledArray<Vector3> Base64ToVector3Array(this string base64) =>
            Base64ToArray<Vector3>(base64, 3, Fill);

        public static PooledArray<Quaternion> Base64ToQuaternionArray(this string base64) =>
            Base64ToArray<Quaternion>(base64, 4, Fill);

        static NativeArray<float> BytesToNativeArray(byte[] bytes, Allocator allocator)
        {
            var length = bytes.Length / sizeof(float);
            var floatArray = new NativeArray<float>(length, allocator);

            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToSingle(bytes, i * 4);
            }

            return floatArray;
        }
        
        static void ToFloatArray(this Vector3[] positions, NativeArray<float> destination)
        {
            Assert.AreEqual(positions.Length * 3, destination.Length);

            var idx = 0;
            for (var i = 0; i < positions.Length; i++)
            {
                var position = positions[i];

                destination[idx++] = position.x;
                destination[idx++] = position.y;
                destination[idx++] = position.z;
            }
        }

        static void ToFloatArray(this Quaternion[] rotations, NativeArray<float> destination)
        {
            Assert.AreEqual(rotations.Length * 4, destination.Length);

            var idx = 0;
            for (var i = 0; i < rotations.Length; i++)
            {
                var rotation = rotations[i];

                destination[idx++] = rotation.w;
                destination[idx++] = rotation.x;
                destination[idx++] = rotation.y;
                destination[idx++] = rotation.z;
            }
        }

        static void FromFloatArray(this Vector3[] destination, NativeArray<float> source)
        {
            Assert.AreEqual(source.Length, destination.Length * 3);

            var idx = 0;
            for (var i = 0; i < destination.Length; i++)
            {
                var x = source[idx++];
                var y = source[idx++];
                var z = source[idx++];

                destination[i] = new Vector3(x, y, z);
            }
        }

        static void FromFloatArray(this Quaternion[] destination, NativeArray<float> source)
        {
            Assert.AreEqual(source.Length, destination.Length * 4);

            var idx = 0;
            for (var i = 0; i < destination.Length; i++)
            {
                var w = source[idx++];
                var x = source[idx++];
                var y = source[idx++];
                var z = source[idx++];

                destination[i] = new Quaternion(x, y, z, w);
            }
        }
    }
}
