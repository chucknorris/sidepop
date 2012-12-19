using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sidepop.Mime
{
    /// <summary>
    /// Class that contains information when an exception while parsing the bytes occurs
    /// </summary>
    [Serializable]
    public class MimeParserException : Exception
    {
        /// <summary>
        /// The raw bytes that caused the exception
        /// </summary>
        public byte[] RawBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// The partially created MimeEntity during parsing
        /// </summary>
        public MimeEntity PartialMimeEntity
        {
            get;
            private set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MimeParserException()
            : base()
        {
        }

        /// <summary>
        /// .Ctor
        /// </summary>
        public MimeParserException(byte[] rawBytes) 
            :base()
        {
            RawBytes = rawBytes;
        }

        /// <summary>
        /// .Ctor
        /// </summary>
        public MimeParserException(byte[] rawBytes, string message)
            : base(message)
        {
            RawBytes = rawBytes;
        }

        /// <summary>
        /// .Ctor()
        /// </summary>
        public MimeParserException(byte[] rawBytes, string message, MimeEntity partialMimeEntity, Exception innerException)
            :base(message, innerException)
        {
            PartialMimeEntity = partialMimeEntity;
            RawBytes = rawBytes;
        }
    }
}
