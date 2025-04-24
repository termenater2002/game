using System;
using Unity.Collections;

namespace Unity.DeepPose.Cloud
{
    static class ByteUtils
    {
        public static PooledArray<byte> AsBytePooledArray(this NativeArray<float> source)
        {
            var byteSlice = new NativeSlice<float>(source).SliceConvert<byte>();
            var pooledByteArray = new PooledArray<byte>(byteSlice.Length);
            byteSlice.CopyTo(pooledByteArray.Array);

            return pooledByteArray;
        }

        public static string ToDataString(this byte[] bytes, bool forceLittleEndianInPlace = false)
        {
            // Note: swapping happens in-place
            if (forceLittleEndianInPlace && !BitConverter.IsLittleEndian)
                FlipBytes4(bytes);

            return bytes != null ? System.Convert.ToBase64String(bytes) : null;
        }

        public static byte[] BytesFromDataString(this string base64, bool forceLittleEndianInPlace = false)
        {
            var bytes = Convert.FromBase64String(base64);

            // Note: swapping happens in-place
            if (forceLittleEndianInPlace && !BitConverter.IsLittleEndian)
                FlipBytes4(bytes);

            return bytes;
        }

        static void FlipBytes4(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i += 4)
            {
                Swap(ref bytes[i + 0], ref bytes[i + 2]);
                Swap(ref bytes[i + 1], ref bytes[i + 3]);
            }
        }

        static void Swap(ref byte a, ref byte b)
        {
            (a, b) = (b, a);
        }
    }
}
