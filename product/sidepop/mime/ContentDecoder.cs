using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace sidepop.Mime
{

    /// <summary>
    /// Utility class that decodes string
    /// </summary>
    public static class ContentDecoder
    {
        /// <summary>
        /// Sets the decoded content stream by decoding the EncodedMessage 
        /// and writing it to the entity content stream.
        /// </summary>
        public static byte[] DecodeBytes(MimeEntity entity)
        {
            switch (entity.ContentTransferEncoding)
            {
                case TransferEncoding.Base64:
                    byte[] decodedBytes = Convert.FromBase64String(Encoding.ASCII.GetString(entity.ContentBytes));
                    return decodedBytes;

                case TransferEncoding.QuotedPrintable:
                    return QuotedPrintableEncoding.Decode(entity.ContentLines);

                case TransferEncoding.SevenBit:
                default:
                    return entity.ContentBytes;
            }
        }

        /// <summary>
        /// Decode the recieved content
        /// </summary>
        /// <returns></returns>
        public static string DecodeString(MimeEntity entity)
        {
            switch (entity.ContentTransferEncoding)
            {
                case TransferEncoding.Base64:
                    return DecodeBase64(entity.ContentLines, entity.ContentType.CharSet);

                case TransferEncoding.QuotedPrintable:
                    return DecodeQuotedPrintable(entity);

                case TransferEncoding.SevenBit:
                default:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (byte[] line in entity.ContentLines)
                        {
                            if (sb.Length > 0)
                            {
                                sb.AppendLine();
                            }

                            sb.Append(DecodeBytesWithSpecificCharset(line, entity.ContentType.CharSet));
                        }

                        return sb.ToString();
                    }

            }
        }

        /// <summary>
        /// Decode the received single line using the correct decoder
        /// </summary>
        internal static string DecodeSingleLineString(string line, TransferEncoding encoding, string charSet)
        {
            switch (encoding)
            {
                case TransferEncoding.Base64:
                    {
                        byte[] decodedBytes = Encoding.ASCII.GetBytes(line);
                        return DecodeBase64(new byte[][] { decodedBytes }, charSet);
                    }
                case TransferEncoding.QuotedPrintable:
                    {
                        byte[] decodedBytes = QuotedPrintableEncoding.DecodeSingleLine(line);
                        return DecodeBytesWithSpecificCharset(decodedBytes, charSet);
                    }
                case TransferEncoding.SevenBit:
                default:
                    return line;
            }
        }

        /// <summary>
        /// Decode the content into the given charset
        /// </summary>
        private static string DecodeQuotedPrintable(MimeEntity entity)
        {
            byte[] decodedBytes = QuotedPrintableEncoding.Decode(entity.ContentLines);

            if (entity.ContentType.CharSet != null)
            {
                return DecodeBytesWithSpecificCharset(decodedBytes, entity.ContentType.CharSet);
            }
            else
            {
                // by default, a text/plain ContentType without Specific Charset must default to ISO-8859-1.
                // Other text/* ContentType without Specific charset must default to utf-8
                // See http://tools.ietf.org/html/rfc6657#page-3

                string encodingName = "utf-8";

                if (entity.ContentType != null && string.Compare(entity.ContentType.MediaType, "text/plain", true) == 0)
                {
                    encodingName = "ISO-8859-1";
                }

                string decodedBytesString = Encoding.GetEncoding(encodingName).GetString(decodedBytes);
                return decodedBytesString;
            }
        }

        /// <summary>
        /// Decodes a Base64 string and returns a string intepreted with the specified charset.
        /// </summary>
        private static string DecodeBase64(byte[][] content, string charSet)
        {
            byte[] allBytes = content.SelectMany(b => b).ToArray();
            byte[] decodedBytes = Convert.FromBase64String(Encoding.ASCII.GetString(allBytes));

            if (charSet != null)
            {
                return DecodeBytesWithSpecificCharset(decodedBytes, charSet);
            }
            else
            {
                return ByteArrayToString(decodedBytes);
            }
        }

        /// <summary>
        /// Converts a string to a byte array without using any encoding.
        /// If we deal with a character that is unicode : ie not fitting into a single byte, we will lose information
        /// </summary>
        public static byte[] StringToByteArray(string decoded)
        {
            byte[] byteArray = decoded.ToCharArray().Select(c => (byte)c).ToArray();
            return byteArray;
        }

        /// <summary>
        /// Converts a string to a byte array without using any encoding.
        /// If we deal with a character that is unicode : ie not fitting into a single byte, we will lose information
        /// </summary>
        public static string ByteArrayToString(byte[] encoded)
        {
            string decoded = new string(encoded.Select(b => (char)b).ToArray());
            return decoded;
        }

        /// <summary>
        /// Use the specified charset to decode the given bytes.
        /// </summary>
        private static string DecodeBytesWithSpecificCharset(byte[] decodedBytes, string charSet)
        {
            Encoding encoding = MimeEntity.GetEncoding(charSet);
            encoding = DetectRealEncoding(decodedBytes, encoding);

            string decodedBytesString = encoding.GetString(decodedBytes);
            return decodedBytesString;
        }

        /// <summary>
        /// Some email client may specify an encoding but it is not the correct encoding. Try to prevent this situation
        /// </summary>
        private static Encoding DetectRealEncoding(byte[] decodedBytes, Encoding encoding)
        {
            //It happens that a MimePart says it is encoded with ISO-8859-1 but it is really Windows-1252
            //Convention is that when any char in the range 0x80 through 0x92 is present, chances are that the encoding is Windows-1252
            if (encoding.HeaderName.Equals("ISO-8859-1", StringComparison.InvariantCultureIgnoreCase) && decodedBytes.Any(b => b >= 0x80 && b <= 0x92))
            {
                //See thread: http://stackoverflow.com/questions/3714061/php-problems-converting-character-from-iso-8859-1-to-utf-8
                encoding = Encoding.GetEncoding("Windows-1252");
            }

            return encoding;
        }

        /// <summary>
        /// replaces all the occurences of : =?charset?Q|B?string?= into the specified value.
        /// </summary>
        public static string DecodeEncodedWords(string value)
        {           
            EncodedWords encodedWords = new EncodedWords(value);
            return encodedWords.Decoded;
        }
    }
}
