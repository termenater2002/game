using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Encoder for PNG metadata.
    /// </summary>
    internal class PngMetadataEncoder
    {
        byte[] m_Header;
        List<Chunk> m_Chunks;
        PngMetadata m_Metadata;

        /// <summary>
        /// Returns metadata.
        /// </summary>
        public PngMetadata data => m_Metadata;

        /// <summary>
        /// Returns available chunks in the PNG data.
        /// </summary>
        internal IReadOnlyList<Chunk> chunks => m_Chunks;

        /// <summary>
        /// Latin-1 character encoding (ISO-8859-1).
        /// </summary>
        public static readonly Encoding latin1Encoding = Encoding.GetEncoding("ISO-8859-1");

        /// <summary>
        /// Creates a PngMetadataEncoder instance.
        /// </summary>
        /// <param name="pngData">Raw PNG data.</param>
        public PngMetadataEncoder(byte[] pngData)
        {
            m_Chunks = new List<Chunk>();

            using (var memoryStream = new MemoryStream(pngData))
            {
                InitializeFromStream(memoryStream);
            }

            InitializeMetadataFromChunks();
        }

        /// <summary>
        /// Clears and sets metadata.
        /// </summary>
        /// <param name="newMetadata">New metadata</param>
        public void SetMetadata(PngMetadata newMetadata)
        {
            ClearMetadata();

            if (newMetadata != null)
            {
                var newChunks = new List<Chunk>();

                foreach (var (key, value) in newMetadata.GetEntries(PngChunkType.tEXt))
                    newChunks.Add(new Chunk_tEXt(key, value));
                foreach (var (key, value) in newMetadata.GetEntries(PngChunkType.iTXt))
                    newChunks.Add(new Chunk_iTXt(key, value));
                var exif = newMetadata.GetEntries(PngChunkType.eXIf);
                if (exif.Count > 0)
                    newChunks.Add(new Chunk_eXIf(exif));

                m_Chunks.InsertRange(1, newChunks);
            }

            InitializeMetadataFromChunks();
        }

        /// <summary>
        /// Clears metadata.
        /// </summary>
        public void ClearMetadata()
        {
            for (var i = 0; i < m_Chunks.Count; i++)
            {
                var chunk = m_Chunks[i];
                if (chunk is Chunk_iTXt or Chunk_tEXt or Chunk_eXIf)
                {
                    m_Chunks.RemoveAt(i);
                    i--;
                }
            }

            m_Metadata = new PngMetadata();
        }

        /// <summary>
        /// Generates PNG data.
        /// </summary>
        /// <returns>Byte array of PNG data with metadata chunks.</returns>
        public byte[] GetData()
        {
            using var stream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write(m_Header);
                foreach (var chunk in m_Chunks)
                    WriteChunk(binaryWriter, chunk);

                binaryWriter.Flush();
            }

            return stream.ToArray();
        }

        void InitializeFromStream(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream))
            {
                m_Header = binaryReader.ReadBytes(8);

                if (Encoding.ASCII.GetString(m_Header).Substring(1, 3) != "PNG")
                    throw new Exception("Input data is not in a PNG format.");

                while (stream.Position != stream.Length)
                {
                    var chunk = ReadChunk(binaryReader);
                    if (chunk != null)
                        m_Chunks.Add(chunk);
                }
            }
        }

        void InitializeMetadataFromChunks()
        {
            m_Metadata = new PngMetadata();
            foreach (var chunk in m_Chunks)
            {
                switch (chunk)
                {
                    case Chunk_iTXt iTXt:
                        m_Metadata.SetEntry(iTXt.keyString, iTXt.valueString, PngChunkType.iTXt);
                        break;
                    case Chunk_tEXt tEXt:
                        m_Metadata.SetEntry(tEXt.keyString, tEXt.valueString, PngChunkType.tEXt);
                        break;
                    case Chunk_eXIf eXIf:
                    {
                        foreach (var (tagType, tag) in eXIf.tags)
                            m_Metadata.SetEntry(tagType.ToString(), Encoding.ASCII.GetString(tag.data), PngChunkType.eXIf);
                        break;
                    }
                }
            }
        }

        static Chunk ReadChunk(BinaryReader binaryReader)
        {
            var length = (int)ReadPngUInt32(binaryReader.ReadBytes(4));
            var type = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
            var data = binaryReader.ReadBytes(length);
            var hash = ReadPngUInt32(binaryReader.ReadBytes(4));

            switch (type)
            {
                case Chunk_tEXt.typeString:
                    return new Chunk_tEXt(data, hash);
                case Chunk_iTXt.typeString:
                    return new Chunk_iTXt(data, hash);
                case Chunk_eXIf.typeString:
                    return new Chunk_eXIf(data, hash);
                default:
                    return new Chunk(type, data, hash);
            }
        }

        static void WriteChunk(BinaryWriter binaryWriter, Chunk chunk)
        {
            binaryWriter.Write(WritePngUInt32((uint)chunk.data.Length));
            binaryWriter.Write(Encoding.UTF8.GetBytes(chunk.type));
            binaryWriter.Write(chunk.data);
            binaryWriter.Write(WritePngUInt32(chunk.hash));
        }

        /// <summary>
        /// Reads PNG unsigned 32-bit integer.
        /// </summary>
        /// <param name="pngUInt32">Byte representation of an unsigned 32-bit integer in PNG format.</param>
        /// <returns>Unsigned 32-bit integer.</returns>
        /// <remarks>PNG is using a big-endian convention.</remarks>
        static uint ReadPngUInt32(byte[] pngUInt32) => BitConverter.ToUInt32(CorrectEndianness(pngUInt32, false, BitConverter.IsLittleEndian));

        /// <summary>
        /// Writes an unsigned int 32 to PNG format.
        /// </summary>
        /// <param name="uInt32">Unsigned 32-bit integer.</param>
        /// <returns>Byte representation of an unsigned 32-bit integer int PNG format.</returns>
        /// <remarks>PNG is a using big-endian convention.</remarks>
        static byte[] WritePngUInt32(uint uInt32) => CorrectEndianness(BitConverter.GetBytes(uInt32), BitConverter.IsLittleEndian, false);

        /// <summary>
        /// Corrects the endianness.
        /// </summary>
        /// <param name="number">Byte representation of a number.</param>
        /// <param name="isSourceLittleEndian">Is the source little-endian.</param>
        /// <param name="isTargetLittleEndian">Is the target little-endian.</param>
        /// <returns>Byte representation of a number with correct endianness.</returns>
        public static byte[] CorrectEndianness(byte[] number, bool isSourceLittleEndian, bool isTargetLittleEndian)
        {
            if (isSourceLittleEndian != isTargetLittleEndian)
                Array.Reverse(number);
            return number;
        }
    }
}
