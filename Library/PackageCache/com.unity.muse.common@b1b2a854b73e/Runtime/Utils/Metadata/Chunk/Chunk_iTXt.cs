using System;
using System.IO;
using System.Text;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Represents iTXt chunk.
    /// </summary>
    internal class Chunk_iTXt : Chunk
    {
        /// <summary>
        /// Chunk type name.
        /// </summary>
        public const string typeString = "iTXt";

        /// <summary>
        /// Key string.
        /// </summary>
        public string keyString => m_Key;

        /// <summary>
        /// Value string.
        /// </summary>
        public string valueString => m_Value;

        string m_Key;
        string m_Value;

        /// <summary>
        /// Creates an instance of Chunk_iTXt and initializes it with a key-value pair.
        /// </summary>
        /// <param name="keyString">Key string.</param>
        /// <param name="valueString">Value string.</param>
        public Chunk_iTXt(string keyString, string valueString)
            : base(typeString, null, 0)
        {
            InitializeFromKeyValue(keyString, valueString);
        }

        /// <summary>
        /// Creates an instance of Chunk_iTXt and initializes it with a key-value pair.
        /// </summary>
        /// <param name="data">PNG data.</param>
        /// <param name="hash">CRC hash.</param>
        public Chunk_iTXt(byte[] data, uint hash)
            : base(typeString, data, hash)
        {
            InitializeFromData();
        }

        void InitializeFromKeyValue(string key, string val)
        {
            m_Key = key;
            m_Value = val;

            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(PngMetadataEncoder.latin1Encoding.GetBytes(m_Key));
                    binaryWriter.Write(nullByte);

                    // Compression Flag
                    binaryWriter.Write((byte)0);

                    // Compression Method
                    binaryWriter.Write((byte)0);

                    // Language Bytes
                    binaryWriter.Write(nullByte);

                    // Translation Bytes
                    binaryWriter.Write(nullByte);

                    binaryWriter.Write(Encoding.UTF8.GetBytes(m_Value));

                    binaryWriter.Flush();
                    data = stream.ToArray();
                }
            }

            RecalculateHash();
        }

        void InitializeFromData()
        {
            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);

            m_Key = PngMetadataEncoder.latin1Encoding.GetString(ReadNullTerminatedBytes(binaryReader));

            var compressionFlag = binaryReader.ReadByte();
            var compressionMethod = binaryReader.ReadByte();

            if (compressionFlag != 0)
                throw new Exception($"{nameof(PngMetadataEncoder)} does not support compression.");

            var compressionDataBytes = 2;

            var languageBytes = Encoding.ASCII.GetString(ReadNullTerminatedBytes(binaryReader));
            var translationBytes = Encoding.UTF8.GetString(ReadNullTerminatedBytes(binaryReader));

            m_Value = Encoding.UTF8.GetString(binaryReader.ReadBytes(data.Length - compressionDataBytes - languageBytes.Length - 1 - translationBytes.Length - 1));
        }
    }
}
