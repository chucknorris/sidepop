using System;

namespace sidepop.Mail.Responses
{
	/// <summary>
	/// This class represents a RETR response message resulting
	/// from a Pop3 RETR command execution against a Pop3 Server.
	/// </summary>
	internal sealed class RetrieveResponse : Pop3Response
	{
	    /// <summary>
		/// Initializes a new instance of the <see cref="RetrieveResponse"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="messageLines">The message lines.</param>
		public RetrieveResponse(Pop3Response response, string[] messageLines)
			: base(response.ResponseContents, response.HostMessage, response.StatusIndicator)
		{
			if (messageLines == null)
			{
				throw new ArgumentNullException("messageLines");
			}
			string[] values = response.HostMessage.Split(' ');
			if (values.Length == 2)
			{
				Octets = Convert.ToInt64(values[1]);
			}

			MessageLines = messageLines;
		}

	    /// <summary>
	    /// Gets the message lines.
	    /// </summary>
	    /// <value>The Pop3 message lines.</value>
	    public string[] MessageLines { get; private set; }

	    public long Octets { get; private set; }
	}
}