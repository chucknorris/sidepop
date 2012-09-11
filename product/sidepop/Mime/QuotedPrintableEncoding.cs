
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace sidepop.Mime
{
    /// <summary>
    /// This class is based on the QuotedPrintable class written by Bill Gearhart
    /// found at http://www.aspemporium.com/classes.aspx?cid=6
    /// </summary>
    public static class QuotedPrintableEncoding
    {
        private const string Equal = "=";

        /// <summary>
        /// A quoted printable string is composed only of the ASCII characters 0 to 9, A to F and =.
        /// But in fact it represents an array of bytes from the range 0 to 255. These bytes will later
        /// be converted to a string using the character set specified in the Content-Type header.
        /// </summary>
        public static byte[] Decode(string contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            List<byte> decodedBytes = new List<byte>();

            using (StringReader reader = new StringReader(contents))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    /*remove trailing line whitespace that may have
                        been added by a mail transfer agent per rule
                        #3 of the Quoted Printable section of RFC 1521.*/
                    line.TrimEnd();

                    if (line.EndsWith(Equal))
                    {
                        //Don't include the Equal character itself because it is not part of the line content
                        line = line.Substring(0, line.Length - 1);

                        decodedBytes.AddRange(DecodeLine(line));
                    } //handle soft line breaks for lines that end with an "="
                    else
                    {
                        decodedBytes.AddRange(DecodeLine(line));

                        //Avoid extra line break on last line of the message
                        if (reader.Peek() != -1)
                        {
                            decodedBytes.AddRange(DecodeLine(Environment.NewLine));
                        }
                    }
                }
            }

            return decodedBytes.ToArray();
        }

        /// <summary>
        /// Only a subset of the byte range from 0 to 255 could be represented as ASCII.
        /// The others (like 195) have been encoded using the following syntax =C3 (= followed by 2 hex characters).
        /// To decode a quoted printable string is to restore the original bytes by undoing that syntax.
        /// Example: A=C3BC will become [65,195,66,67]
        /// </summary>
        private static byte[] DecodeLine(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            byte[] encodedBytes = Encoding.ASCII.GetBytes(line);

            List<byte> decodedBytes = new List<byte>();

            int encodedByteIndex = 0;
            while (encodedByteIndex < encodedBytes.Length)
            {
                if (encodedBytes[encodedByteIndex] == (byte)'=' &&
                    Regex.IsMatch(new String((char)(encodedBytes[encodedByteIndex + 1]), 1), @"[0-9A-F]") &&
                    Regex.IsMatch(new String((char)(encodedBytes[encodedByteIndex + 2]), 1), @"[0-9A-F]"))
                {
                    string hexadecimalString = new String(new char[] { (char)(encodedBytes[encodedByteIndex + 1]), (char)(encodedBytes[encodedByteIndex + 2]) });
                    int hexadecimal = Int32.Parse(hexadecimalString, NumberStyles.HexNumber);
                    decodedBytes.Add((byte)hexadecimal);
                    encodedByteIndex += 3;
                }
                else
                {
                    decodedBytes.Add(encodedBytes[encodedByteIndex]);
                    encodedByteIndex += 1;
                }
            }

            return decodedBytes.ToArray();
        }
    }
}
