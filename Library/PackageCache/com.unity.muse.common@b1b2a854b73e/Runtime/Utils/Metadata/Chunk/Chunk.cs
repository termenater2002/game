using System;
using System.IO;
using System.Text;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Represents a PNG chunk.
    /// </summary>
    internal class Chunk
    {
        /// <summary>
        /// Null byte '\0'.
        /// </summary>
        public static readonly byte nullByte = BitConverter.GetBytes('\0')[0];

        /// <summary>
        /// Chunk type name (4 characters).
        /// </summary>
        public readonly string type;

        /// <summary>
        /// Chunk data.
        /// </summary>
        public byte[] data
        {
            get => m_Data;
            protected set => m_Data = value;
        }

        /// <summary>
        /// CRC hash.
        /// </summary>
        public uint hash => m_Hash;

        byte[] m_Data;
        uint m_Hash;

        /// <summary>
        /// Creates a Chunk instance.
        /// </summary>
        /// <param name="type">Chunk type.</param>
        /// <param name="data">Chunk data.</param>
        /// <param name="hash">CRC hash.</param>
        public Chunk(string type, byte[] data, uint hash)
        {
            this.type = type;

            m_Data = data;
            m_Hash = hash;
        }

        /// <summary>
        /// Recalculates CRC hash based on the chunk contents.
        /// </summary>
        public void RecalculateHash()
        {
            var bytes = new byte[type.Length + data.Length];
            Array.Copy(Encoding.UTF8.GetBytes(type), bytes, 4);
            Array.Copy(data, 0, bytes, 4, data.Length);

            m_Hash = Crc.Calculate(bytes);
        }

        /// <summary>
        /// Reads bytes until null byte is found.
        /// </summary>
        /// <param name="binaryReader">Binary reader.</param>
        /// <returns>A string without null byte.</returns>
        public static byte[] ReadNullTerminatedBytes(BinaryReader binaryReader)
        {
            using var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                byte currentByte;
                while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length && (currentByte = binaryReader.ReadByte()) != nullByte)
                    binaryWriter.Write(currentByte);

                binaryWriter.Flush();
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// CRC Calculator according to PNG specification.
        /// </summary>
        /// <remarks>https://www.w3.org/TR/PNG-CRCAppendix.html</remarks>
        internal static class Crc
        {
            static uint[] s_CrcTable;
            static bool s_IsCrcTableComputed = false;

            static void MakeCrcTable()
            {
                s_CrcTable = new uint[256];
                for (uint n = 0; n < 256; n++)
                {
                    var c = n;
                    for (var k = 0; k < 8; k++)
                    {
                        if ((c & 1) == 1)
                            c = 0xEDB88320 ^ (c >> 1);
                        else
                            c = c >> 1;
                    }

                    s_CrcTable[n] = c;
                }

                s_IsCrcTableComputed = true;
            }

            static uint UpdateCrc(uint crc, byte[] buf, int len)
            {
                var c = crc;
                if (!s_IsCrcTableComputed)
                    MakeCrcTable();
                for (var n = 0; n < len; n++)
                {
                    c = s_CrcTable[(c ^ buf[n]) & 0xFF] ^ (c >> 8);
                }

                return c;
            }

            /// <summary>
            /// Calculates CRC hash.
            /// </summary>
            /// <param name="buffer">Byte array to hash.</param>
            /// <returns>CRC hash.</returns>
            public static uint Calculate(byte[] buffer)
            {
                return UpdateCrc(0xFFFFFFFF, buffer, buffer.Length) ^ 0xFFFFFFFF;
            }
        }
    }
}
