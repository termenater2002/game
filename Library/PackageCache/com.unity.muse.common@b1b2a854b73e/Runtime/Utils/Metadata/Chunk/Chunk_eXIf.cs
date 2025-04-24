using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Represents eXIf chunk.
    /// </summary>
    internal class Chunk_eXIf : Chunk
    {
        internal enum IFDTagType : int
        {
            /// <summary>
            /// File change date and time.
            /// </summary>
            ModifyData = 0x0132,
            /// <summary>
            /// Image title.
            /// </summary>
            ImageDescription = 0x010E,
            /// <summary>
            /// Image input equipment manufacturer.
            /// </summary>
            Make = 0x010F,
            /// <summary>
            /// Image input equipment model.
            /// </summary>
            Model = 0x0110,
            /// <summary>
            /// Software used.
            /// </summary>
            Software = 0x0131,
            /// <summary>
            /// Person who created the image.
            /// </summary>
            Artist = 0x13B,
            /// <summary>
            /// Copyright holder.
            /// </summary>
            Copyright = 0x8298
        }

        internal enum DataType
        {
            /// <summary>
            /// Unsigned Byte (1 byte).
            /// </summary>
            UnsignedByte = 1,
            /// <summary>
            /// ASCII String (1 byte).
            /// </summary>
            String = 2,
            /// <summary>
            /// Unsigned Short (2 bytes).
            /// </summary>
            UnsignedShort = 3,
            /// <summary>
            /// Unsigned Long (4 bytes).
            /// </summary>
            UnsignedLong = 4,
            /// <summary>
            /// Unsigned Rational (8 bytes).
            /// </summary>
            UnsignedRational = 5,
            /// <summary>
            /// Signed Byte (1 byte).
            /// </summary>
            SignedByte = 6,
            /// <summary>
            /// Undefined (1 byte).
            /// </summary>
            Undefined = 7,
            /// <summary>
            /// Signed Short (2 bytes).
            /// </summary>
            SignedShort = 8,
            /// <summary>
            /// Singed Long (4 bytes).
            /// </summary>
            SignedLong = 9,
            /// <summary>
            /// Signed Rational (8 bytes).
            /// </summary>
            SignedRational = 10,
            /// <summary>
            /// Single Float (4 bytes).
            /// </summary>
            SingleFloat = 11,
            /// <summary>
            /// Double Float (8 bytes).
            /// </summary>
            DoubleFloat = 12
        }

        static readonly Dictionary<DataType, int> k_Bytes = new()
        {
            { DataType.UnsignedByte, 1 },
            { DataType.String, 1 },
            { DataType.UnsignedShort, 2 },
            { DataType.UnsignedLong, 4 },
            { DataType.UnsignedRational, 8 },
            { DataType.SignedByte, 1 },
            { DataType.Undefined, 1 },
            { DataType.SignedShort, 2 },
            { DataType.SignedLong, 4 },
            { DataType.SignedRational, 8 },
            { DataType.SingleFloat, 4 },
            { DataType.DoubleFloat, 8 },
        };

        static readonly Dictionary<int, DataType> k_TagDataType = new()
        {
            { (int)IFDTagType.ModifyData, DataType.String },
            { (int)IFDTagType.ImageDescription, DataType.String },
            { (int)IFDTagType.Make, DataType.String },
            { (int)IFDTagType.Model, DataType.String },
            { (int)IFDTagType.Software, DataType.String },
            { (int)IFDTagType.Artist, DataType.String },
            { (int)IFDTagType.Copyright, DataType.String }
        };

        internal class Tag
        {
            public readonly short tagType;
            public readonly DataType dataType;
            public readonly byte[] data;

            public Tag(short tagType, DataType dataType, byte[] data)
            {
                this.tagType = tagType;
                this.dataType = dataType;
                this.data = data;
            }
        }

        /// <summary>
        /// Chunk type name.
        /// </summary>
        public const string typeString = "eXIf";

        public bool isLittleEndian => m_ByteOrder == k_LittleEndianOrder;

        public IReadOnlyDictionary<short, Tag> tags => m_Tags;
        Dictionary<short, Tag> m_Tags;

        string m_ByteOrder;

        const string k_LittleEndianOrder = "II";
        const string k_BigEndianOrder = "MM";

        /// <summary>
        /// Creates an instance of ChunkExif and initializes it with an Exif tag dictionary.
        /// </summary>
        public Chunk_eXIf(IReadOnlyDictionary<string, string> keyValuePairs, bool isLittleEndian = false)
            : base(typeString, null, 0)
        {
            m_ByteOrder = isLittleEndian ? k_LittleEndianOrder : k_BigEndianOrder;

            m_Tags = new Dictionary<short, Tag>();
            foreach (var (key, value) in keyValuePairs)
            {
                if (!short.TryParse(key, out var tagType))
                {
                    Debug.Log($"Incorrect tag type {key}.");
                    continue;
                }

                if (!k_TagDataType.TryGetValue(tagType, out var dataType))
                {
                    Debug.Log($"Unsupported tag type {tagType}. Cannot find data type.");
                    continue;
                }

                m_Tags[tagType] = new Tag(tagType, dataType, Encoding.ASCII.GetBytes(value));
            }

            InitializeFromDictionary(m_Tags);
        }

        /// <summary>
        /// Creates an instance of ChunkExif and initializes it with raw PNG data and CRC hash.
        /// </summary>
        /// <param name="data">PNG data.</param>
        /// <param name="hash">CRC hash.</param>
        public Chunk_eXIf(byte[] data, uint hash)
            : base(typeString, data, hash)
        {
            InitializeFromData();
        }

        void InitializeFromDictionary(Dictionary<short, Tag> tags)
        {
            m_Tags = tags;

            using (var stream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(Encoding.ASCII.GetBytes(k_BigEndianOrder));

                    binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes((short)42), BitConverter.IsLittleEndian, isLittleEndian));

                    binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes(8), BitConverter.IsLittleEndian, isLittleEndian));

                    binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes((short)m_Tags.Count), BitConverter.IsLittleEndian, isLittleEndian));

                    var dataOffset = m_Tags.Count * 12 + 14;

                    foreach (var (tagType, tag) in m_Tags)
                    {
                        binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes(tagType), BitConverter.IsLittleEndian, isLittleEndian));
                        binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes((short)tag.dataType), BitConverter.IsLittleEndian, isLittleEndian));
                        var componentCount = tag.data.Length / k_Bytes[tag.dataType];
                        binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes(componentCount), BitConverter.IsLittleEndian, isLittleEndian));
                        if (tag.data.Length == 4)
                            binaryWriter.Write(tag.data);
                        else
                        {
                            binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes(dataOffset), BitConverter.IsLittleEndian, isLittleEndian));
                            dataOffset += tag.data.Length;
                        }
                    }

                    binaryWriter.Write(PngMetadataEncoder.CorrectEndianness(BitConverter.GetBytes(0), BitConverter.IsLittleEndian, isLittleEndian));

                    foreach (var (_, tag) in m_Tags)
                    {
                        if (tag.data.Length > 4)
                            binaryWriter.Write(tag.data);
                    }

                    binaryWriter.Write(nullByte);
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

            m_ByteOrder = Encoding.ASCII.GetString(binaryReader.ReadBytes(2));

            var version = BitConverter.ToInt16(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(2), m_ByteOrder == k_LittleEndianOrder, BitConverter.IsLittleEndian));
            if (version != 42)
            {
                Debug.Log($"Unsupported version {version}. Only TIFF (42) is supported.");
                return;
            }

            var offset = BitConverter.ToInt32(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(4), m_ByteOrder == k_LittleEndianOrder, BitConverter.IsLittleEndian));
            if (offset >= data.Length)
                offset = 8;

            GetIfd(offset);
        }

        void GetIfd(int offset)
        {
            var minBytes = 16;
            if (offset > data.Length - minBytes)
            {
                Debug.Log("Cannot read tags.");
                m_Tags = new Dictionary<short, Tag>();
                return;
            }

            var memoryStream = new MemoryStream(data, offset, data.Length - offset);
            var binaryReader = new BinaryReader(memoryStream);

            var numberOfTags = BitConverter.ToInt16(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(2), isLittleEndian, BitConverter.IsLittleEndian));

            m_Tags = new Dictionary<short, Tag>(numberOfTags);

            for (var t = 0; t < numberOfTags; t++)
            {
                var tagType = BitConverter.ToInt16(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(2), isLittleEndian, BitConverter.IsLittleEndian));
                var formatCode = BitConverter.ToInt16(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(2), isLittleEndian, BitConverter.IsLittleEndian));
                var componentCount = BitConverter.ToInt32(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(4), isLittleEndian, BitConverter.IsLittleEndian));
                var componentLength = k_Bytes[(DataType)formatCode] * componentCount;
                var inlineValue = PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(4), isLittleEndian, BitConverter.IsLittleEndian);

                Debug.Assert(formatCode is >= 1 and <= 12);

                var val = new byte[componentLength];
                if (componentLength == 4)
                    val = PngMetadataEncoder.CorrectEndianness(inlineValue, isLittleEndian, BitConverter.IsLittleEndian);
                else
                {
                    Array.Copy(data, BitConverter.ToInt32(inlineValue), val, 0, componentLength);
                }

                m_Tags[tagType] = new Tag(tagType, (DataType)formatCode, val);
            }

            var offsetToNextIFD = BitConverter.ToInt32(PngMetadataEncoder.CorrectEndianness(binaryReader.ReadBytes(4), isLittleEndian, BitConverter.IsLittleEndian));
            if (offsetToNextIFD != 0)
                Debug.Log($"Other IFDs are not supported");
        }
    }
}
