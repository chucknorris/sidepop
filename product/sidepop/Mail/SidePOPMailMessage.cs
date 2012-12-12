using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using sidepop.Mime;
using System.Text;

namespace sidepop.Mail
{
    /// <summary>
    /// This class adds a few internet mail headers not already exposed by the 
    /// System.Net.MailMessage.  It also provides support to encapsulate the
    /// nested mail attachments in the Children collection.
    /// </summary>
    public class SidePOPMailMessage : MailMessage
    {
        public const string EmailRegexPattern = "(?:['\"]{1,}(.+?)['\"]{1,}\\s+)?(<?[\\w\\.\\-%\\s]+@[^\\.][\\w\\.\\-]+(\\.[a-zA-Z0-9]{2,})?>?)";
        //private static readonly char[] AddressDelimiters = new char[] {',', ';'};

        public const string InvalidEmailAddress = "invalid@email.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="SidePOPMailMessage"/> class.
        /// </summary>
        public SidePOPMailMessage()
        {
            Children = new List<SidePOPMailMessage>();
        }

        public long Octets { get; set; }

        /// <summary>
        /// Exposes the raw bytes for this mail message
        /// </summary>
        public byte[] RawLines { get; set; }

        /// <summary>
        /// Gets or sets the message number of the MailMessage on the POP3 server.
        /// </summary>
        /// <value>The message number.</value>
        public int MessageNumber { get; internal set; }

        /// <summary>
        /// Gets the children MailMessage attachments.
        /// </summary>
        /// <value>The children MailMessage attachments.</value>
        public List<SidePOPMailMessage> Children { get; private set; }

        /// <summary>
        /// Gets the delivery date.
        /// </summary>
        /// <value>The delivery date.</value>
        public DateTime? DeliveryDate
        {
            get
            {
                string date = GetHeader(MailHeaders.Date);
                if (string.IsNullOrEmpty(date))
                {
                    return null;
                }

                DateTime returnDate;
                DateTime.TryParse(date, out returnDate);

                return returnDate == DateTime.MinValue ? (DateTime?)null : returnDate;
            }
        }

        /// <summary>
        /// Gets the return address.
        /// </summary>
        /// <value>The return address.</value>
        public MailAddress ReturnAddress
        {
            get
            {
                string replyTo = GetHeader(MailHeaders.ReplyTo);
                if (string.IsNullOrEmpty(replyTo))
                {
                    return null;
                }

                return CreateMailAddress(replyTo);
            }
        }

        /// <summary>
        /// Gets the routing.
        /// </summary>
        /// <value>The routing.</value>
        public string Routing
        {
            get { return GetHeader(MailHeaders.Received); }
        }

        /// <summary>
        /// Gets the message id.
        /// </summary>
        /// <value>The message id.</value>
        public string MessageId
        {
            get { return GetHeader(MailHeaders.MessageId); }
        }

        public string ReplyToMessageId
        {
            get { return GetHeader(MailHeaders.InReplyTo, true); }
        }

        /// <summary>
        /// Gets the MIME version.
        /// </summary>
        /// <value>The MIME version.</value>
        public string MimeVersion
        {
            get { return GetHeader(MimeHeaders.MimeVersion); }
        }

        /// <summary>
        /// Gets the content id.
        /// </summary>
        /// <value>The content id.</value>
        public string ContentId
        {
            get { return GetHeader(MimeHeaders.ContentId); }
        }

        /// <summary>
        /// Gets the content description.
        /// </summary>
        /// <value>The content description.</value>
        public string ContentDescription
        {
            get { return GetHeader(MimeHeaders.ContentDescription); }
        }

        /// <summary>
        /// Gets the content disposition.
        /// </summary>
        /// <value>The content disposition.</value>
        public ContentDisposition ContentDisposition
        {
            get
            {
                string contentDisposition = GetHeader(MimeHeaders.ContentDisposition);
                if (string.IsNullOrEmpty(contentDisposition))
                {
                    return null;
                }

                return new ContentDisposition(contentDisposition);
            }
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public ContentType ContentType
        {
            get
            {
                string contentType = GetHeader(MimeHeaders.ContentType);
                if (string.IsNullOrEmpty(contentType))
                {
                    return null;
                }

                return MimeReader.GetContentType(contentType);
            }
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns></returns>
        private string GetHeader(string header)
        {
            return GetHeader(header, false);
        }

        private string GetHeader(string header, bool stripBrackets)
        {
            if (stripBrackets)
            {
                return MimeEntity.TrimBrackets(Headers[header]);
            }

            return Headers[header];
        }

        /// <summary>
        /// Creates the mail message from entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static SidePOPMailMessage CreateMailMessageFromEntity(MimeEntity entity)
        {
            SidePOPMailMessage message = new SidePOPMailMessage();

            string value;
            foreach (string key in entity.Headers.AllKeys)
            {
                value = entity.Headers[key];
                if (value.Equals(string.Empty))
                {
                    value = " ";
                }

                message.Headers.Add(key.ToLowerInvariant(), value);

                switch (key.ToLowerInvariant())
                {
                    case MailHeaders.Bcc:
                        PopulateAddressList(value, message.Bcc);
                        break;
                    case MailHeaders.Cc:
                        PopulateAddressList(value, message.CC);
                        break;
                    case MailHeaders.From:
                        message.From = CreateMailAddress(value);
                        break;
                    case MailHeaders.ReplyTo:
                        message.ReplyToList.Add(CreateMailAddress(value));
                        break;
                    case MailHeaders.Subject:
                        message.Subject = value;
                        break;
                    case MailHeaders.To:
                        PopulateAddressList(value, message.To);
                        break;
                }
            }

            return message;
        }

        /// <summary>
        /// Creates the mail address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public static MailAddress CreateMailAddress(string address)
        {
            Match match = Regex.Match(address, EmailRegexPattern);

            if (match.Success)
            {
                return CreateMailAddress(match.Groups[1].Value, match.Groups[2].Value);
            }
            else
            {
                ArgumentException ex = new ArgumentException("The received mail address is not valid", "address");
                ex.Data.Add("address", address);
                throw ex;
            }
        }

        /// <summary>
        /// Creates the mail address.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public static MailAddress CreateMailAddress(string displayName, string address)
        {
            string addressToUse = address;

            if (string.IsNullOrEmpty(addressToUse))
            {
                addressToUse = InvalidEmailAddress;
            }

            // Our regular expression may have captured an invalid email address
            // according to the MailAddress class. It could for example contain
            // spaces or other invalid characters. If that is the case, we
            // at least keep the display name and use a constant address.
            try
            {
                return new MailAddress(addressToUse.Trim('\t'), displayName);
            }
            catch (FormatException)
            {
                return new MailAddress(InvalidEmailAddress, displayName);
            }
        }

        /// <summary>
        /// Populates the address list.
        /// </summary>
        /// <param name="addressList">The address list.</param>
        /// <param name="recipients">The recipients.</param>
        public static void PopulateAddressList(string addressList, MailAddressCollection recipients)
        {
            foreach (MailAddress address in GetMailAddresses(addressList))
            {
                recipients.Add(address);
            }
        }

        /// <summary>
        /// Gets the mail addresses.
        /// </summary>
        /// <param name="addressList">The address list.</param>
        /// <returns></returns>
        private static IEnumerable<MailAddress> GetMailAddresses(string addressList)
        {
            Regex email = new Regex(EmailRegexPattern);

            foreach (Match match in email.Matches(addressList))
            {
                yield return CreateMailAddress(match.Groups[1].Value, match.Groups[2].Value);
            }


            /*
            string[] addresses = addressList.Split(AddressDelimiters);
            foreach (string address in addresses)
            {
                yield return CreateMailAddress(address);
            }*/
        }
	}
}