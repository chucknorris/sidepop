namespace sidepop.Mime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.IO;
    using System.Net.Mime;
    using System.Text;
    using System.Text.RegularExpressions;
    using infrastructure;
    using infrastructure.logging;
    using Mail.Commands;

    /// <summary>
    /// This class is responsible for parsing a string array of lines
    /// containing a MIME message.
    /// </summary>
    public class MimeReader
    {
        private static readonly char[] HeaderWhitespaceChars = new[] { ' ', '\t' };
        private static Encoding DefaultEncoding;
        private static Regex UnquotedEncodedString = new Regex("(?<!\")=\\?([^\\?]+)\\?([BbQq])\\?([^\\?]+)\\?=(?!\")", RegexOptions.Compiled);
        private readonly MimeEntity _entity;
        private readonly Queue<byte[]> _lines;
        private byte[] _rawBytes;
        private bool _throwOnInvalidContentType;

        /// <summary>
        /// Static Ctor
        /// </summary>
        static MimeReader()
        {
            DefaultEncoding = Encoding.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeReader"/> class.
        /// </summary>
        private MimeReader(bool throwOnInvalidContentType)
        {
            _entity = new MimeEntity();
            _throwOnInvalidContentType = throwOnInvalidContentType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeReader"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="lines">The lines.</param>
        private MimeReader(MimeEntity entity, Queue<byte[]> lines, bool throwOnInvalidContentType)
            : this(throwOnInvalidContentType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (lines == null)
            {
                throw new ArgumentNullException("lines");
            }

            _lines = lines;
            _entity = new MimeEntity(entity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MimeReader"/> class.
        /// </summary>
        /// <param name="rawBytes">The raw bytes of the message.</param>
        /// <param name="throwOnInvalidContentType">Determine if an invalid content type should raise an exception.</param>
        public MimeReader(byte[] rawBytes, bool throwOnInvalidContentType = false)
            : this(throwOnInvalidContentType)
        {
            if (rawBytes == null)
            {
                throw new ArgumentNullException("rawBytes");
            }

            _rawBytes = rawBytes;
            _entity.RawBytes = rawBytes;

            _lines = new Queue<byte[]>(SplitByteArrayWithCrLf(rawBytes));
        }

        /// <summary>
        /// Splits a byte array using CrLf as the line delimiter.
        /// </summary>
        public static byte[][] SplitByteArrayWithCrLf(byte[] byteArray)
        {
            List<byte[]> lines = new List<byte[]>();

            if (byteArray == null)
            {
                throw new ArgumentException("Value cannot be null", "byteArray");
            }

            if (byteArray.Length == 0)
            {
                return lines.ToArray();
            }

            int startIndex = 0;
            for (int i = 0; i < byteArray.Length - 1; i++)
            {
                byte byte1 = byteArray[i];
                byte byte2 = byteArray[i + 1];

                if (byte1 == Pop3Commands.Cr && byte2 == Pop3Commands.Lf)
                {
                    byte[] line = new byte[i - startIndex];
                    Array.Copy(byteArray, startIndex, line, 0, i - startIndex);
                    lines.Add(line);

                    startIndex = i + 2;

                    i++;
                }
            }

            if (startIndex < byteArray.Length)
            {
                byte[] line = new byte[byteArray.Length - startIndex];
                Array.Copy(byteArray, startIndex, line, 0, byteArray.Length - startIndex);
                lines.Add(line);
            }

            return lines.ToArray();
        }

        /// <summary>
        /// Gets the lines.
        /// </summary>
        /// <value>The lines.</value>
        public Queue<byte[]> Lines
        {
            get { return _lines; }
        }

        /// <summary>
        /// Parse headers into _entity.Headers NameValueCollection.
        /// </summary>
        private int ParseHeaders()
        {
            string lastHeader = string.Empty;
            string line = string.Empty;
            byte[] lineBytes = null;

            // the first empty line is the end of the headers.
            while (_lines.Count > 0 && !string.IsNullOrEmpty(ConvertBytesToStringWithDefaultEncoding(_lines.Peek())))
            {
                lineBytes = Dequeue();

                line = ConvertBytesToStringWithDefaultEncoding(lineBytes);

                //if a header line starts with a space or tab then it is a continuation of the
                //previous line.
                if (line.StartsWith(" ") || line.StartsWith(Convert.ToString('\t')))
                {
                    _entity.Headers[lastHeader] = string.Concat(_entity.Headers[lastHeader], line);
                    continue;
                }

                int separatorIndex = line.IndexOf(':');

                if (separatorIndex < 0)
                {
                    Debug.WriteLine(string.Format("Invalid header:{0}", line));
                    continue;
                } //This is an invalid header field.  Ignore this line.

                string headerName = line.Substring(0, separatorIndex);
                string headerValue = line.Substring(separatorIndex + 1).Trim(HeaderWhitespaceChars);

                //if (headerName.ToLower() == "content-type")
                //{
                //    if (_entity.Headers[headerName.ToLower()] == null) // only add the header if it doesn't exists.           
                //    {
                //        _entity.Headers.Add(headerName.ToLower(), headerValue);
                //    }
                //}
                //else
                //{
                _entity.Headers.Add(headerName.ToLower(), headerValue);
                //}

                lastHeader = headerName;
            }

            if (_lines.Count > 0)
            {
                Dequeue();
            } //remove closing header CRLF.

            return _entity.Headers.Count;
        }

        /// <summary>
        /// Consumes a line and add it to the raw content of this mime part.
        /// </summary>
        private byte[] Dequeue()
        {
            byte[] lineBytes = _lines.Dequeue();

            _entity.RawContent.Write(lineBytes, 0, lineBytes.Length);
            _entity.RawContent.WriteByte(Pop3Commands.Cr);
            _entity.RawContent.WriteByte(Pop3Commands.Lf);

            return lineBytes;
        }

        /// <summary>
        /// Processes mime specific headers.
        /// </summary>
        /// <returns>A mime entity with mime specific headers parsed.</returns>
        private void ProcessHeaders()
        {
            foreach (string key in _entity.Headers.AllKeys)
            {
                switch (key)
                {
                    case "content-description":
                        _entity.ContentDescription = _entity.Headers[key];
                        break;
                    case "content-disposition":
                        _entity.ContentDisposition = GetContentDisposition(_entity.Headers[key]);
                        break;
                    case "content-id":
                        _entity.ContentId = _entity.Headers[key];
                        break;
                    case "content-transfer-encoding":
                        _entity.TransferEncoding = _entity.Headers[key];
                        _entity.ContentTransferEncoding = GetTransferEncoding(_entity.Headers[key]);
                        break;
                    case "content-type":
                        _entity.SetContentType(GetContentType(_entity.Headers[key], _throwOnInvalidContentType));
                        break;
                    case "mime-version":
                        _entity.MimeVersion = _entity.Headers[key];
                        break;
                }
            }
        }

        /// <summary>
        /// Creates the MIME entity.
        /// </summary>
        /// <returns>A mime entity containing 0 or more children representing the mime message.</returns>
        public MimeEntity CreateMimeEntity()
        {
            try
            {
                ParseHeaders();

                ProcessHeaders();

                ParseBody();

                SetDecodedContentStream();

                return _entity;
            }
            catch (Exception ex)
            {
                MimeParserException exception = new MimeParserException(_rawBytes, "An error occured while creating the Mime Entity", _entity, ex);
                throw exception;
            }
        }

        /// <summary>
        /// Sets the decoded content stream by decoding the EncodedMessage 
        /// and writing it to the entity content stream.
        /// </summary>
        private void SetDecodedContentStream()
        {
            switch (_entity.ContentTransferEncoding)
            {
                case TransferEncoding.Base64:
                    _entity.Content = new MemoryStream(_entity.ContentBytes, false);
                    break;

                case TransferEncoding.QuotedPrintable:
                    _entity.Content = new MemoryStream(QuotedPrintableEncoding.Decode(_entity.ContentLines), false);
                    break;

                case TransferEncoding.SevenBit:
                default:
                    _entity.Content = new MemoryStream(_entity.ContentBytes.ToArray(), false);
                    break;
            }
        }

        /// <summary>
        /// Gets a byte[] of content for the provided string.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <returns>A byte[] containing content.</returns>
        private byte[] GetBytes(string content)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Parses the body.
        /// </summary>
        private void ParseBody()
        {
            if (_entity.HasBoundary)
            {
                while (!HasReachedEndOfPart())
                {
                    if (string.Equals(ConvertBytesToStringWithDefaultEncoding(_lines.Peek()), _entity.StartBoundary))
                    {
                        AddChildEntity(_entity, _lines);
                    }

                    //Parse a new child mime part.
                    else if (string.Equals(_entity.ContentType.MediaType, MediaTypes.MessageRfc822, StringComparison.InvariantCultureIgnoreCase))
                    {
                        /*If the content type is message/rfc822 the stop condition to parse headers has already been encountered.
                         But, a content type of message/rfc822 would have the message headers immediately following the mime
                         headers so we need to parse the headers for the attached message now.  This is done by creating
                         a new child entity.*/
                        Queue<byte[]> partLines = ReadPart();

                        // The complete rfc822 child entity may be encoded.
                        Queue<byte[]> decodedPartLines = DecodePartIfNeeded(_entity, partLines);
                        AddChildEntity(_entity, decodedPartLines);
                    }
                    else
                    {
                        AppendMessageContent();
                    } //Append the message content.
                }
            } //Parse a multipart message.
            else
            {
                while (_lines.Count > 0)
                {
                    AppendMessageContent();
                }
            } //Parse a single part message.
        }

        /// <summary>
        /// Returns whether the current line matches the parent boundary
        /// </summary>
        private bool HasReachParentBoundary(string line)
        {
            if (_entity.Parent == null)
            {
                return false;
            }

            return string.Equals(_entity.Parent.StartBoundary, line) ||
                string.Equals(_entity.Parent.EndBoundary, line);
        }

        /// <summary>
        /// Returns whether the current line is the end of a part.
        /// </summary>
        private bool HasReachedEndOfPart()
        {
            // No more lines to process,
            // we have reached the end of the current part.
            if (_lines.Count == 0)
            {
                return true;
            }

            string currentLine = ConvertBytesToStringWithDefaultEncoding(_lines.Peek());

            // If the current line is the current entity end boundary, 
            // we have reached the end of the current part.
            if (string.Equals(currentLine, _entity.EndBoundary))
            {
                return true;
            }

            // If the current line is the start or the end of the entity's parent boundary,
            // we have reached the end of the current part.
            return HasReachParentBoundary(currentLine);
        }

        /// <summary>
        /// Reads all the lines of the current part.
        /// </summary>
        private Queue<byte[]> ReadPart()
        {
            Queue<byte[]> partLines = new Queue<byte[]>();

            while (!HasReachedEndOfPart())
            {
                partLines.Enqueue(_lines.Dequeue());
            }

            return partLines;
        }
        
        /// <summary>
        /// Append the current queued line to the entity encoded / decoded message buffers
        /// </summary>
        private void AppendMessageContent()
        {
            byte[] lineBytes = Dequeue();

            _entity.AppendLineContent(lineBytes);
        }

        /// <summary>
        /// Converts bytes using the default encoding
        /// </summary>
        private string ConvertBytesToStringWithDefaultEncoding(byte[] line)
        {
            if (line == null)
            {
                return "";
            }

            return DefaultEncoding.GetString(line);
        }

        /// <summary>
        /// Adds the child entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void AddChildEntity(MimeEntity entity, Queue<byte[]> lines)
        {
            /*if (entity == null)
            {
                return;
            }

            if (lines == null)
            {
                return;
            }*/

            MimeReader reader = new MimeReader(entity, lines, _throwOnInvalidContentType);
            MimeEntity childEntity = reader.CreateMimeEntity();
            entity.Children.Add(childEntity);

            byte[] innerBytes = childEntity.RawContent.ToArray();
            entity.RawContent.Write(innerBytes, 0, innerBytes.Length);
        }

        /// <summary>
        /// Decodes the specified part if needed.
        /// </summary>
        private Queue<byte[]> DecodePartIfNeeded(MimeEntity parentEntity, Queue<byte[]> lines)
        {
            if (parentEntity.ContentTransferEncoding == TransferEncoding.Base64)
            {               
                try
                {
                     byte[] encodedBytes = lines.SelectMany(childEntityLine => childEntityLine).ToArray();
                     string encodedString = ConvertBytesToStringWithDefaultEncoding(encodedBytes);
                     byte[] decodedBytes = Convert.FromBase64String(encodedString);
                     return new Queue<byte[]>(SplitByteArrayWithCrLf(decodedBytes));
                }
                catch
                {
                    //It happens that invalid transfer encoding is specified, just consider that this part is not encoded with Base64
                    return lines;
                }
            }

            return lines;
        }

        /// <summary>
        /// Get the attachments 
        /// </summary>
        /// <param name="contentDisposition">The disposition text</param>
        /// <returns></returns>
        public static ContentDisposition GetContentDisposition(string contentDisposition)
        {
            string epuratedContentDisposition = StripInvalidDateTime(contentDisposition);
            epuratedContentDisposition = FixUnquotedEncodedString(epuratedContentDisposition);

            ContentDisposition result = new ContentDisposition(epuratedContentDisposition);
            return result;
        }

        /// <summary>
        /// Remove any invalid date. 
        /// Some client, like NOVEL_GROUPWISE store invalid date, such as Thu, 19 sep 2012. In 2012, September 19 was a wednesday
        /// </summary>
        private static string StripInvalidDateTime(string contentDisposition)
        {
            // extract the possible dates and ensure they will be parsed correctly
            string result = RemoveIfInvalidDateTime(contentDisposition, "modification-date");
            result = RemoveIfInvalidDateTime(result, "creation-date");
            result = RemoveIfInvalidDateTime(result, "read-date");

            return result;
        }

        /// <summary>
        /// Returns whether the specified string represents a valid date time.
        /// </summary>
        private static bool IsInvalidDateTime(string dateValueString)
        {
            // RFC 2822 states that days can be expressed using one or two digits.
            // But the .NET ContentDisposition class insists on receiving two.
            if (!Regex.IsMatch(dateValueString, @"..., \d\d.*"))
            {
                return false;
            }

            // Make sure the date is parsable.
            DateTime parsedDt;
            if (!DateTime.TryParse(dateValueString, out parsedDt))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure that a value representing a encoded value is surrounded with
        /// double quotes
        /// </summary>
        private static string FixUnquotedEncodedString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return UnquotedEncodedString.Replace(value, (m) => string.Format("\"{0}\"", m.Groups[0].Value));
        }

        /// <summary>
        /// Parse for the received token, and if found, try to parse the date. If it fails, remove the whole token from the line
        /// </summary>
        /// <returns></returns>
        private static string RemoveIfInvalidDateTime(string contentDisposition, string dateTokenName)
        {
            string result = contentDisposition;

            string tokenToLook = dateTokenName + "=\"";

            int dateTimeTokenStartIndex = result.IndexOf(dateTokenName);

            if (dateTimeTokenStartIndex > -1)
            {
                // the DateTime value starts at the tokenIndex + its length
                int dateTimeStartIndex = dateTimeTokenStartIndex + tokenToLook.Length;

                // look for the next "
                int endIndex = result.IndexOf( "\"", dateTimeStartIndex);

                // get the string of the datetime
                string dateValueString = result.Substring(dateTimeStartIndex, endIndex - dateTimeStartIndex);

                if (!IsInvalidDateTime(dateValueString))
                { 
                    //the date is not parsable, remove it from the string
                    result = result.Substring(0, dateTimeTokenStartIndex) + result.Substring(endIndex + 1);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        public static ContentType GetContentType(string contentType, bool throwOnInvalidContentType = false)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "text/plain; charset=us-ascii";
            }

            contentType = contentType.Replace("= ", "=");
            contentType = contentType.Replace(" =", "=");

            try
            {
                return new ContentType(contentType);
            }
            catch (Exception ex)
            {
                if (throwOnInvalidContentType)
                {
                    throw;
                }

                Log.bound_to(typeof(MimeReader)).log_a_warning_event_containing(
                    "{0} was not able to use content type \"{1}\". Defaulting to \"text/plain; charset=us-ascii\".{2}{3}", ApplicationParameters.name, contentType, Environment.NewLine, ex.ToString());
                return new ContentType("text/plain; charset=us-ascii");
            }
        }

        /// <summary>
        /// Gets the type of the media.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns></returns>
        public static string GetMediaType(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                return "text/plain";
            }
            return mediaType.Trim();
        }

        /// <summary>
        /// Gets the type of the media main.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns></returns>
        public static string GetMediaMainType(string mediaType)
        {
            int separatorIndex = mediaType.IndexOf('/');
            if (separatorIndex < 0)
            {
                return mediaType;
            }
            else
            {
                return mediaType.Substring(0, separatorIndex);
            }
        }

        /// <summary>
        /// Gets the type of the media sub.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <returns></returns>
        public static string GetMediaSubType(string mediaType)
        {
            int separatorIndex = mediaType.IndexOf('/');
            if (separatorIndex < 0)
            {
                if (mediaType.Equals("text"))
                {
                    return "plain";
                }
                return string.Empty;
            }
            else
            {
                if (mediaType.Length > separatorIndex)
                {
                    return mediaType.Substring(separatorIndex + 1);
                }
                else
                {
                    string mainType = GetMediaMainType(mediaType);
                    if (mainType.Equals("text"))
                    {
                        return "plain";
                    }
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the transfer encoding.
        /// </summary>
        /// <param name="transferEncoding">The transfer encoding.</param>
        /// <returns></returns>
        /// <remarks>
        /// The transfer encoding determination follows the same rules as 
        /// Peter Huber's article w/ the exception of not throwing exceptions 
        /// when binary is provided as a transferEncoding.  Instead it is left
        /// to the calling code to check for binary.
        /// </remarks>
        public static TransferEncoding GetTransferEncoding(string transferEncoding)
        {
            switch (transferEncoding.Trim().ToLowerInvariant())
            {
                case "7bit":
                case "8bit":
                    return TransferEncoding.SevenBit;
                case "quoted-printable":
                    return TransferEncoding.QuotedPrintable;
                case "base64":
                    return TransferEncoding.Base64;
                case "binary":
                default:
                    return TransferEncoding.Unknown;
            }
        }
    }
}