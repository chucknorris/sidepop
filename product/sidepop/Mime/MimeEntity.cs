using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using sidepop.Mail;

namespace sidepop.Mime
{
	/// <summary>
	/// This class represents a Mime entity.
	/// </summary>
	public class MimeEntity
	{
	    private readonly StringBuilder _encodedMessage;
	    private readonly string _startBoundary;

        /// <summary>
        /// Exposes raw lines for the mime part and its children
        /// </summary>
        public string[] RawLines { get; set; }

	    /// <summary>
		/// Initializes a new instance of the <see cref="MimeEntity"/> class.
		/// </summary>
		public MimeEntity()
		{
			Children = new List<MimeEntity>();
			Headers = new NameValueCollection();
			ContentType = MimeReader.GetContentType(string.Empty);
			Parent = null;
			_encodedMessage = new StringBuilder();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeEntity"/> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public MimeEntity(MimeEntity parent) : this()
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}

			Parent = parent;
			_startBoundary = parent.StartBoundary;
		}

		/// <summary>
		/// Gets the encoded message.
		/// </summary>
		/// <value>The encoded message.</value>
		public StringBuilder EncodedMessage
		{
			get { return _encodedMessage; }
		}

	    /// <summary>
	    /// Gets the children.
	    /// </summary>
	    /// <value>The children.</value>
	    public List<MimeEntity> Children { get; private set; }

	    /// <summary>
	    /// Gets the type of the content.
	    /// </summary>
	    /// <value>The type of the content.</value>
	    public ContentType ContentType { get; private set; }

	    /// <summary>
	    /// Gets the type of the media sub.
	    /// </summary>
	    /// <value>The type of the media sub.</value>
	    public string MediaSubType { get; private set; }

	    /// <summary>
	    /// Gets the type of the media main.
	    /// </summary>
	    /// <value>The type of the media main.</value>
	    public string MediaMainType { get; private set; }

	    /// <summary>
	    /// Gets the headers.
	    /// </summary>
	    /// <value>The headers.</value>
	    public NameValueCollection Headers { get; private set; }

	    /// <summary>
	    /// Gets or sets the MIME version.
	    /// </summary>
	    /// <value>The MIME version.</value>
	    public string MimeVersion { get; set; }

	    /// <summary>
	    /// Gets or sets the content id.
	    /// </summary>
	    /// <value>The content id.</value>
	    public string ContentId { get; set; }

	    /// <summary>
	    /// Gets or sets the content description.
	    /// </summary>
	    /// <value>The content description.</value>
	    public string ContentDescription { get; set; }

	    /// <summary>
	    /// Gets or sets the content disposition.
	    /// </summary>
	    /// <value>The content disposition.</value>
	    public ContentDisposition ContentDisposition { get; set; }

	    /// <summary>
	    /// Gets or sets the transfer encoding.
	    /// </summary>
	    /// <value>The transfer encoding.</value>
	    public string TransferEncoding { get; set; }

	    /// <summary>
	    /// Gets or sets the content transfer encoding.
	    /// </summary>
	    /// <value>The content transfer encoding.</value>
	    public TransferEncoding ContentTransferEncoding { get; set; }

	    /// <summary>
		/// Gets a value indicating whether this instance has boundary.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has boundary; otherwise, <c>false</c>.
		/// </value>
		internal bool HasBoundary
		{
			get
			{
				return (!string.IsNullOrEmpty(ContentType.Boundary))
				       || (!string.IsNullOrEmpty(_startBoundary));
			}
		}

		/// <summary>
		/// Gets the start boundary.
		/// </summary>
		/// <value>The start boundary.</value>
		public string StartBoundary
		{
			get
			{
				if (string.IsNullOrEmpty(_startBoundary) || !string.IsNullOrEmpty(ContentType.Boundary))
				{
					return string.Concat("--", ContentType.Boundary);
				}

				return _startBoundary;
			}
		}

		/// <summary>
		/// Gets the end boundary.
		/// </summary>
		/// <value>The end boundary.</value>
		public string EndBoundary
		{
			get { return string.Concat(StartBoundary, "--"); }
		}

	    /// <summary>
	    /// Gets or sets the parent.
	    /// </summary>
	    /// <value>The parent.</value>
	    public MimeEntity Parent { get; set; }

	    /// <summary>
	    /// Gets or sets the content.
	    /// </summary>
	    /// <value>The content.</value>
	    public MemoryStream Content { get; internal set; }
        
	    /// <summary>
		/// Sets the type of the content.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		internal void SetContentType(ContentType contentType)
		{
			ContentType = contentType;
			ContentType.MediaType = MimeReader.GetMediaType(contentType.MediaType);
			MediaMainType = MimeReader.GetMediaMainType(contentType.MediaType);
			MediaSubType = MimeReader.GetMediaSubType(contentType.MediaType);
		}

		/// <summary>
		/// Toes the mail message ex.
		/// </summary>
		/// <returns></returns>
		public SidePOPMailMessage ToMailMessageEx()
		{
			return ToMailMessageEx(this);
		}

		/// <summary>
		/// Toes the mail message ex.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private SidePOPMailMessage ToMailMessageEx(MimeEntity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			//parse standard headers and create base email.
			SidePOPMailMessage message = SidePOPMailMessage.CreateMailMessageFromEntity(entity);

			if (!string.IsNullOrEmpty(entity.ContentType.Boundary))
			{
				message = SidePOPMailMessage.CreateMailMessageFromEntity(entity);
				BuildMultiPartMessage(entity, message);
			} //parse multipart message into sub parts.
			else if (string.Equals(entity.ContentType.MediaType, MediaTypes.MessageRfc822,
			                       StringComparison.InvariantCultureIgnoreCase))
			{
				//use the first child to create the multipart message.
				if (entity.Children.Count < 0)
				{
					throw new Pop3Exception("Invalid child count on message/rfc822 entity.");
				}

				//create the mail message from the first child because it will
				//contain all of the mail headers.  The entity in this state
				//only contains simple content type headers indicating, disposition, type and description.
				//This means we can't create the mail message from this type as there is no 
				//internet mail headers attached to this entity.
				message = SidePOPMailMessage.CreateMailMessageFromEntity(entity.Children[0]);
				BuildMultiPartMessage(entity, message);
			} //parse nested message.
			else
			{
				message = SidePOPMailMessage.CreateMailMessageFromEntity(entity);
				BuildSinglePartMessage(entity, message);
			} //Create single part message.

			return message;
		}

		/// <summary>
		/// Builds the single part message.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="message">The message.</param>
		private void BuildSinglePartMessage(MimeEntity entity, SidePOPMailMessage message)
		{
			SetMessageBody(message, entity);
		}
        
		/// <summary>
		/// Gets the body encoding.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		public Encoding GetEncoding()
		{
			if (string.IsNullOrEmpty(ContentType.CharSet))
			{
				return Encoding.ASCII;
			}
			else
			{
				try
				{
					return Encoding.GetEncoding(ContentType.CharSet);
				}
				catch (ArgumentException)
				{
					return Encoding.ASCII;
				}
			}
		}

		/// <summary>
		/// Builds the multi part message.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="message">The message.</param>
		private void BuildMultiPartMessage(MimeEntity entity, SidePOPMailMessage message)
		{
			foreach (MimeEntity child in entity.Children)
			{
				if (string.Equals(child.ContentType.MediaType, MediaTypes.MultipartAlternative,
				                  StringComparison.InvariantCultureIgnoreCase)
				    ||
				    string.Equals(child.ContentType.MediaType, MediaTypes.MultipartMixed,
				                  StringComparison.InvariantCultureIgnoreCase))
				{
					BuildMultiPartMessage(child, message);
				} //if the message is mulitpart/alternative or multipart/mixed then the entity will have children needing parsed.
				else if (!IsAttachment(child) &&
				         (string.Equals(child.ContentType.MediaType, MediaTypes.TextPlain)
				          || string.Equals(child.ContentType.MediaType, MediaTypes.TextHtml)))
				{
					message.AlternateViews.Add(CreateAlternateView(child));
					SetMessageBody(message, child);
				} //add the alternative views.
				else if (string.Equals(child.ContentType.MediaType, MediaTypes.MessageRfc822,
				                       StringComparison.InvariantCultureIgnoreCase)
				         &&
				         string.Equals(child.ContentDisposition.DispositionType, DispositionTypeNames.Attachment,
				                       StringComparison.InvariantCultureIgnoreCase))
				{
					message.Children.Add(ToMailMessageEx(child));
				} //create a child message and 
				else if (IsAttachment(child))
				{
					message.Attachments.Add(CreateAttachment(child));
				}
			}
		}

		private static bool IsAttachment(MimeEntity child)
		{
			return (child.ContentDisposition != null)
			       &&
			       (string.Equals(child.ContentDisposition.DispositionType, DispositionTypeNames.Attachment,
			                      StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Sets the message body.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="child">The child.</param>
		private void SetMessageBody(SidePOPMailMessage message, MimeEntity child)
		{
			Encoding encoding = child.GetEncoding();
			message.Body = DecodeBytes(child.Content.ToArray(), encoding);
			message.BodyEncoding = encoding;
			message.IsBodyHtml = string.Equals(MediaTypes.TextHtml,
			                                   child.ContentType.MediaType, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Decodes the bytes.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		private string DecodeBytes(byte[] buffer, Encoding encoding)
		{
			if (buffer == null)
			{
				return null;
			}

			if (encoding == null)
			{
				encoding = Encoding.UTF7;
			} //email defaults to 7bit.  

			return encoding.GetString(buffer);
		}

		/// <summary>
		/// Creates the alternate view.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <returns></returns>
		private AlternateView CreateAlternateView(MimeEntity view)
		{
			AlternateView alternateView = new AlternateView(view.Content, view.ContentType);
			alternateView.TransferEncoding = view.ContentTransferEncoding;
			alternateView.ContentId = TrimBrackets(view.ContentId);
			return alternateView;
		}

		/// <summary>
		/// Trims the brackets.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string TrimBrackets(string value)
		{
			if (value == null)
			{
				return value;
			}

			if (value.StartsWith("<") && value.EndsWith(">"))
			{
				return value.Trim('<', '>');
			}

			return value;
		}

		/// <summary>
		/// Creates the attachment.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		private Attachment CreateAttachment(MimeEntity entity)
		{
			Attachment attachment = new Attachment(entity.Content, entity.ContentType);

			if (entity.ContentDisposition != null)
			{
				attachment.ContentDisposition.Parameters.Clear();
				foreach (string key in entity.ContentDisposition.Parameters.Keys)
				{
                    //Skip values that will be strongly typed copied after this loop: it happens that date string
                    //will not be parsed correctly when going through Parameters.Add but are already parsed into entity.ContentDisposition
                    if (key == "creation-date" ||
                        key == "modification-date" ||
                        key == "read-date" ||
                        key == "size" ||
                        key == "filename")
                    {
                        continue;
                    }

					attachment.ContentDisposition.Parameters.Add(key, entity.ContentDisposition.Parameters[key]);
				}

				attachment.ContentDisposition.CreationDate = entity.ContentDisposition.CreationDate;
				attachment.ContentDisposition.DispositionType = entity.ContentDisposition.DispositionType;
				attachment.ContentDisposition.FileName = entity.ContentDisposition.FileName;
				attachment.ContentDisposition.Inline = entity.ContentDisposition.Inline;
				attachment.ContentDisposition.ModificationDate = entity.ContentDisposition.ModificationDate;
				attachment.ContentDisposition.ReadDate = entity.ContentDisposition.ReadDate;
				attachment.ContentDisposition.Size = entity.ContentDisposition.Size;
			}

			if (!string.IsNullOrEmpty(entity.ContentId))
			{
				attachment.ContentId = TrimBrackets(entity.ContentId);
			}

			attachment.TransferEncoding = entity.ContentTransferEncoding;

			return attachment;
		}
	}
}