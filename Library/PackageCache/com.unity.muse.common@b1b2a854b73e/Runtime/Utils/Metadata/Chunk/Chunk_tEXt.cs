using System.IO;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Represents tEXt chunk.
    /// </summary>
    internal class Chunk_tEXt : Chunk
    {
        /// <summary>
        /// Chunk type name.
        /// </summary>
        public const string typeString = "tEXt";

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
        /// Creates an instance of Chunk_tEXt and initializes chunk based on data.
        /// </summary>
        /// <param name="data">PNG data.</param>
        /// <param name="hash">CRC hash.</param>
        public Chunk_tEXt(byte[] data, uint hash)
            : base(typeString, data, hash)
        {
            InitializeFromData();
        }

        /// <summary>
        /// Creates an instance of Chunk_tEXt and initializes it with a key-value pair.
        /// </summary>
        /// <param name="keyword">Keyword string.</param>
        /// <param name="text">Text content.</param>
        public Chunk_tEXt(string keyword, string text)
            : base(typeString, null, 0)
        {
            InitializeFromKeyValue(keyword, text);
        }

        void InitializeFromData()
        {
            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);

            var keyBytes = ReadNullTerminatedBytes(binaryReader);
            m_Key = PngMetadataEncoder.latin1Encoding.GetString(keyBytes);

            var textBytes = binaryReader.ReadBytes(data.Length - keyBytes.Length - 1);
            m_Value = PngMetadataEncoder.latin1Encoding.GetString(textBytes);
        }

        void InitializeFromKeyValue(string keyword, string text)
        {
            m_Key = keyword;
            m_Value = text;

            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(PngMetadataEncoder.latin1Encoding.GetBytes(m_Key));
                    binaryWriter.Write(nullByte);

                    binaryWriter.Write(PngMetadataEncoder.latin1Encoding.GetBytes(m_Value));

                    binaryWriter.Flush();
                    data = stream.ToArray();
                }
            }

            RecalculateHash();
        }
    }
}
