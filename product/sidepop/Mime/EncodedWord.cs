using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Net.Mime;

namespace sidepop.Mime
{
    /// <summary>
    /// Holds information about an encoded word
    /// </summary>
    internal class EncodedWord
    {
        /// <summary>
        /// Regex for extracting text encoded in QuotedPrintable or Base64 string such as: =?iso-8859-1?Q?Fr=E9d=E9ric_Vandal?=
        /// </summary>
        private static Regex _singleEncodedWordsPattern = new Regex("(\\s*)=\\?([^\\?]+)\\?([BbQq])\\?([^\\?]+)\\?=", RegexOptions.Compiled);
        private static Regex _multipleEncodedWordsPattern = new Regex("(\\s*=\\?[^\\?]+\\?[BbQq]\\?[^\\?]+\\?=)", RegexOptions.Compiled);

        /// <summary>
        /// The value
        /// </summary>
        private string Value { get; set; }

        /// <summary>
        /// The match
        /// </summary>
        private Match Match { get; set; }

        /// <summary>
        /// Determines if the current part is encoded
        /// </summary>
        public bool IsEncoded
        {
            get
            {
                return Match.Success;
            }
        }

        /// <summary>
        /// Prefix of the match (white space before token)
        /// </summary>
        public string Prefix
        {
            get
            {
                return Match.Groups[1].Value;
            }
        }

        /// <summary>
        /// Encoding name : e.g. ISO-8859-1, ISO-2022-JP, UTF-8, etc.
        /// </summary>
        private string EncodingName
        {
            get
            {
                return Match.Groups[2].Value;
            }
        }

        /// <summary>
        /// Encoding type : B or Q
        /// </summary>
        private string EncodingType
        {
            get
            {
                return Match.Groups[3].Value.ToUpper();
            }
        }

        /// <summary>
        /// The encoded data
        /// </summary>
        private string EncodedData
        {
            get
            {
                return Match.Groups[4].Value;
            }
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return string.Format("IsMatch: {0}: {1}", Match.Success, Value);
        }

        /// <summary>
        /// Runs the multiple regex pattern and returns a list of EncodedWord
        /// </summary>
        public static List<EncodedWord> Parse(string value)
        {
            return _multipleEncodedWordsPattern
              .Split(value)
              .Where(s => !string.IsNullOrEmpty(s))
              .Select(s => new EncodedWord { Value = s, Match = _singleEncodedWordsPattern.Match(s) })
              .ToList();
        }

        /// <summary>
        /// Merges the specified encoded word into this instance.
        /// 
        /// This method assumes the encoded words are compatible
        /// </summary>
        public void Merge(EncodedWord encodedWord)
        {
            Value = MergeEncodedValues(encodedWord);
            Match = _singleEncodedWordsPattern.Match(Value);
        }

        /// <summary>
        /// Determines if the current encoded word has the same encoding name and encoding type
        /// </summary>
        public bool IsCompatibleWith(EncodedWord other)
        {
            if (!IsEncoded || !other.IsEncoded)
            {
                return false;
            }

            return string.Equals(EncodingName, other.EncodingName, StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(EncodingType, other.EncodingType, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Merges together two encoded words and returns the correct encoded word string that will match the regular expression
        /// </summary>
        private string MergeEncodedValues(EncodedWord encodedWord)
        {
            string encodedData1 = EncodedData;
            string encodedData2 = encodedWord.EncodedData;
            string encodingType = EncodingType;

            string encodedData;
            if (encodingType == "B")
            {
                byte[] decodedData1 = Convert.FromBase64String(encodedData1);
                byte[] decodedData2 = Convert.FromBase64String(encodedData2);

                encodedData = Convert.ToBase64String(decodedData1.Concat(decodedData2).ToArray());
            }
            else
            {
                encodedData = encodedData1 + encodedData2;
            }

            return string.Format("=?{0}?{1}?{2}?=", EncodingName, encodingType, encodedData);
        }

        /// <summary>
        /// The human readable value. If encoded we decode, if not encoded we return the Value directly.
        /// </summary>
        public string ReadableValue
        {
            get
            {
                if (IsEncoded)
                {
                    string encodedData = EncodedData;

                    TransferEncoding encoding;
                    if (EncodingType == "B")
                    {
                        encoding = TransferEncoding.Base64;
                    }
                    else
                    {
                        // replace "_" by "=20"
                        encoding = TransferEncoding.QuotedPrintable;
                        encodedData = Regex.Replace(encodedData, "_", "=20");
                    }

                    return ContentDecoder.DecodeSingleLineString(encodedData, encoding, EncodingName);
                }
                else
                {
                    return Value;
                }
            }
        }
    }
}
