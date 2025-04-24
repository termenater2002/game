using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Sentis;
using UnityEngine;

namespace Unity.DeepPose.Internal
{
    /// <summary>
    /// An implementation of OpenAI's SimpleTokenizer for CLIP.
    /// See https://github.com/openai/CLIP/blob/main/clip/simple_tokenizer.py
    /// </summary>
    class Tokenizer
    {
        static readonly Dictionary<int, char> k_ByteEncoder = BytesToUnicode();
        static readonly Regex k_WordRegex = new(
            @"<\|startoftext\|>|<\|endoftext\|>|'s|'t|'re|'ve|'m|'ll|'d|[\p{L}]+|[\p{N}]|[^\s\p{L}\p{N}]+",
            RegexOptions.IgnoreCase);
        
        const string k_DefaultBpeVocab = "bpe_simple_vocab_16e6";
        const int k_DefaultBpeLineCount = 49152 - 256 - 2; // See https://github.com/openai/CLIP/blob/main/clip/simple_tokenizer.py#L67

        readonly Dictionary<(string, string), int> m_BpeRanks;
        readonly Dictionary<string, int> m_Encoder;
        
        const string k_StartOfText = "<|startoftext|>";
        const string k_EndOfText = "<|endoftext|>";
        
        static readonly string[] k_LineSeparators = {"\n", "\r\n"};

        public Tokenizer(TextAsset bpeFile = null)
        {
            if (bpeFile == null)
            {
                bpeFile = Resources.Load<TextAsset>(k_DefaultBpeVocab);
            }

            m_BpeRanks = new Dictionary<(string, string), int>();
            var lines = bpeFile.text.Split(k_LineSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Take(k_DefaultBpeLineCount).ToArray();
            List<string> vocab = k_ByteEncoder.Values.Select(c => c.ToString()).ToList();
            var n = vocab.Count;
            for (var i=0; i<n; i++)
            {
                vocab.Add($"{vocab[i]}</w>");
            }
            
            var merges = new List<(string, string)>();
            foreach (var line in lines)
            {
                merges.Add((line.Split(' ')[0], line.Split(' ')[1]));
                vocab.Add(line.Split(' ')[0] + line.Split(' ')[1]);
            }
            
            vocab.AddRange(new[] {k_StartOfText, k_EndOfText});
            m_Encoder = vocab.Zip(Enumerable.Range(0, vocab.Count), (k, v) => new KeyValuePair<string, int>(k, v))
                .ToDictionary(x => x.Key, x => x.Value);
            
            m_BpeRanks = merges.Select((item, index) => (item, index))
                .ToDictionary(p => p.item, p => p.index);
        }
        
        /// <summary>
        /// Create the BPE tokens for the given text.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The generated tokens are longer than the output length.
        /// </exception>
        /// <remarks>
        /// The caller is responsible for disposing the returned tensor.
        /// </remarks>
        public Tensor<int> Encode(string text, int outputLength)
        {
            int[] bpeTokens = new int[outputLength];
            text = WhitespaceClean(BasicClean(text)).ToLower();
            
            var count = 0;
            
            if (!text.StartsWith(k_StartOfText))
            {
                bpeTokens[count] = m_Encoder[k_StartOfText];
                count++;
            }

            foreach (Match match in k_WordRegex.Matches(text))
            {
                foreach (var bpeToken in CreateBpeTokens(match.Captures[0].Value))
                {
                    if (count >= outputLength - 1)
                    {
                        throw new IndexOutOfRangeException("The prompt is too long");
                    }
                    bpeTokens[count] = m_Encoder[bpeToken];
                    count++;
                }
            }
            
            if (!text.EndsWith(k_EndOfText))
            {
                bpeTokens[count] = m_Encoder[k_EndOfText];
            }

            return new Tensor<int>(new TensorShape(1, outputLength), bpeTokens);
        }

        static string WhitespaceClean(string text) => Regex.Replace(text, @"\s+", " ").Trim();

        static string BasicClean(string text)
        {
            // TODO: Fix encoding issues
            // TODO: Strip HTML markup
            return text;
        }

        IEnumerable<string> CreateBpeTokens(string token)
        {
            var word= token.Select(t => t.ToString()).ToList();
            word[^1] += "</w>";
            
            var pairs = GetPairs(word).ToList();

            if (!pairs.Any())
            {
                return new List<string>{token + "</w>"};
            }

            while (true)
            {
                var x = pairs.OrderBy(p => m_BpeRanks.TryGetValue(p, out var rank) ? rank: float.PositiveInfinity);
                var bigram = x.FirstOrDefault();
                if (!m_BpeRanks.ContainsKey(bigram))
                {
                    break;
                }

                var (first, second) = bigram;
                var newWord = new List<string>();
                var i = 0;
                while (i < word.Count)
                {
                    int j = word.IndexOf(first, i);
                    if (j == -1)
                    {
                        newWord.AddRange(word.Skip(i).ToList());
                        break;
                    }
                    newWord.AddRange(word.Skip(i).Take(j - i).ToList());
                    i = j;
                    if (i < word.Count - 1 && word[i] == first && word[i + 1] == second)
                    {
                        newWord.Add(first + second);
                        i += 2;
                    }
                    else
                    {
                        newWord.Add(word[i]);
                        i += 1;
                    }
                }

                word = newWord;
                if (word.Count == 1)
                {
                    break;
                }

                pairs = GetPairs(word).ToList();
            }
            return word;
        }
        
        static IEnumerable<(string, string)> GetPairs(IReadOnlyList<string> word)
        {
            var pairs = new HashSet<(string, string)>();
            string prevChar = word[0];
            for (var i = 1; i < word.Count; i++)
            {
                var ch = word[i];
                pairs.Add((prevChar, ch));
                prevChar = ch;
            }
            return pairs;
        }
        
        static Dictionary<int, char> BytesToUnicode()
        {
            var bs = Enumerable.Range(33, 94).Concat(Enumerable.Range(161, 94))
                .Concat(Enumerable.Range(174, 255 - 174 + 1)).ToList();
            var cs = bs.ToList();
            var n = 0;
            for (var b = 0; b <= 255; b++)
            {
                if (!bs.Contains(b))
                {
                    bs.Add(b);
                    cs.Add(256 + n);
                    n++;
                }
            }

            var result = new Dictionary<int, char>();
            for (int i = 0; i < bs.Count; i++)
            {
                result[bs[i]] = (char)cs[i];
            }

            return result;
        }
    }
}
