using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace sidepop.Mime
{
    /// <summary>
    /// Handle complexity of parsing a string with multiple encoded word: See RFC2047
    /// </summary>
    internal class EncodedWords
    {
        /// <summary>
        /// Parsed encoded words
        /// </summary>
        private List<EncodedWord> _encodedWords;

        /// <summary>
        /// Decoded string
        /// </summary>
        private string _decoded;

        /// <summary>
        /// Ctor
        /// </summary>
        public EncodedWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _decoded = null;
            }
            else
            {
                _encodedWords = EncodedWord.Parse(value);

                MergeContiguousCompatibleEncodedTokens();

                Decode();
            }
        }
        
        /// <summary>
        /// The decoded string
        /// </summary>
        public string Decoded
        {
            get
            {
                return _decoded;
            }            
        }

        /// <summary>
        /// If happens that when having multiple contiguous encoded words  (specially binary, an encoded word is splitted on a byte boundary example : C3 A9
        /// represents the à character and the first encoded word can contain the C3 and the second encoded word contains A0. This method with modify 
        /// the list of tokens so when the same encoding and charset are used for multiple tokens, they will be merge together
        /// </summary>
        private void MergeContiguousCompatibleEncodedTokens()
        {
            bool doneSomeThing = false;

            do
            {
                doneSomeThing = false;

                for (int i = 0; i < _encodedWords.Count; i++)
                {
                    var encodedWord = _encodedWords[i];
                    if (NextTokenIsCompatible(encodedWord, i))
                    {
                        EncodedWord nextEncodedWord = _encodedWords[i + 1];
                        encodedWord.Merge(nextEncodedWord);

                        _encodedWords.RemoveAt(i + 1);
                        doneSomeThing = true;
                        break;
                    }
                }
            } while (doneSomeThing);
        }

        /// <summary>
        /// Determines if the next token in the list (if any) match the encoded word regular expression and has the same encoding and charset values
        /// </summary>
        private bool NextTokenIsCompatible(EncodedWord encodedWord, int currentIndex)
        {
            if (currentIndex + 1 >= _encodedWords.Count)
            {
                return false;
            }

            var nextEncodedWord = _encodedWords[currentIndex + 1];
            return encodedWord.IsCompatibleWith(nextEncodedWord);
        }

        /// <summary>
        /// Decoded and merges all the token from the specified list.
        /// </summary>
        public void Decode()
        {
            StringBuilder sb = new StringBuilder();

            EncodedWord previousEncodedWord = null;
            foreach (EncodedWord encodedWord in _encodedWords)
            {
                string value = encodedWord.ReadableValue;

                if (encodedWord.IsEncoded)
                {
                    if (previousEncodedWord != null)
                    {
                        if (!previousEncodedWord.IsEncoded)
                        {
                            value = encodedWord.Prefix + value;
                        }
                    }
                }

                sb.Append(value);

                previousEncodedWord = encodedWord;
            }

            _decoded = sb.ToString();
        }
    }
}
