using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Supported chunk types.
    /// </summary>
    internal enum PngChunkType
    {
        tEXt,
        iTXt,
        eXIf
    }

    /// <summary>
    /// Custom PNG metadata.
    /// </summary>
    internal class PngMetadata
    {
        /// <summary>
        /// Max number of bytes allowed to be used to encode the key (tEXt, iTXt) with Latin-1 encoding.
        /// </summary>
        public const int maxKeyBytes = 79;

        List<Dictionary<string, string>> m_Data;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public PngMetadata()
        {
            m_Data = new List<Dictionary<string, string>>();
            foreach (PngChunkType type in Enum.GetValues(typeof(PngChunkType)))
                m_Data.Add(new Dictionary<string, string>());
        }

        /// <summary>
        /// Sets the value of an entry at given key. If the value is null, the entry is removed.
        /// </summary>
        /// <param name="entryKey">Entry key.</param>
        /// <param name="entryValue">Entry value.</param>
        /// <param name="type">Chunk type.</param>
        /// <returns>True if set successfully.</returns>
        public bool SetEntry(string entryKey, string entryValue, PngChunkType type)
        {
            if (string.IsNullOrWhiteSpace(entryKey) || entryKey.Length <= 1)
            {
                Debug.Log($"The given key '{entryKey}' cannot be null or shorter than 1 character.");
                return false;
            }

            if (entryKey.Length > maxKeyBytes)
            {
                Debug.Log($"The given key '{entryKey}' cannot be longer than {maxKeyBytes} characters.");
                return false;
            }

            if (!ValidateKeyEncoding(entryKey, out var incorrectKeyChar))
            {
                Debug.Log($"The given key '{entryKey}' contains unsupported character '{incorrectKeyChar}'. Make sure to use Latin-1 encoding.");
                return false;
            }

            if (!ValidateValueEncoding(entryValue, type, out var incorrectValueChar))
            {
                Debug.Log($"The given value '{entryValue}' contains unsupported character '{incorrectValueChar}'. Make sure to use Latin-1 encoding for tEXt and UTF-8 for iTXt.");
                return false;
            }

            if (GetEntries(type) is not Dictionary<string, string> entries)
                return false;

            if (!string.IsNullOrEmpty(entryValue))
                entries[entryKey] = entryValue;
            else if (entries.ContainsKey(entryKey))
                entries.Remove(entryKey);

            return true;
        }

        /// <summary>
        /// Sets multiple key-value pair entries.
        /// </summary>
        /// <param name="newEntries">New entries.</param>
        /// <param name="type">Chunk type.</param>
        public void SetEntries(IEnumerable<KeyValuePair<string, string>> newEntries, PngChunkType type)
        {
            if (GetEntries(type) is not Dictionary<string, string> entries)
                return;

            entries.Clear();
            if (newEntries != null)
            {
                foreach (var (entryKey, entryValue) in newEntries)
                    SetEntry(entryKey, entryValue, type);
            }
        }

        /// <summary>
        /// Gets key-value entries for a given Chunk type.
        /// </summary>
        /// <param name="type">Chunk type.</param>
        /// <returns>Entries dictionary (read-only).</returns>
        public IReadOnlyDictionary<string, string> GetEntries(PngChunkType type) => m_Data[(int)type];

        static bool ValidateKeyEncoding(string keyString, out char invalidCharacter)
        {
            return ValidateStringEncoding(keyString, PngMetadataEncoder.latin1Encoding, out invalidCharacter);
        }

        static bool ValidateValueEncoding(string valueString, PngChunkType type, out char invalidCharacter)
        {
            var encoding = type == PngChunkType.tEXt ? PngMetadataEncoder.latin1Encoding : Encoding.UTF8;
            return ValidateStringEncoding(valueString, encoding, out invalidCharacter);
        }

        static bool ValidateStringEncoding(string stringToTest, Encoding encoding, out char invalidCharacter)
        {
            invalidCharacter = char.MinValue;
            if (stringToTest == null)
                return true;

            if (encoding.GetString(encoding.GetBytes(stringToTest)) != stringToTest)
            {
                foreach (var character in stringToTest)
                {
                    if (encoding.GetString(encoding.GetBytes(character.ToString())) != character.ToString())
                    {
                        invalidCharacter = character;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
