using System;
using System.IO;

namespace sidepop.Mail.Responses
{
	/// <summary>
	/// This class represents a Pop3 response message and
	/// is intended to be used as a base class for all other
	/// Pop3Response types.
	/// </summary>
	internal class Pop3Response : Response
	{
	    /// <summary>
		/// Initializes a new instance of the <see cref="Pop3Response"/> class.
		/// </summary>
		/// <param name="responseContents">The response contents.</param>
		/// <param name="hostMessage">The host message.</param>
		/// <param name="statusIndicator">if set to <c>true</c> [status indicator].</param>
		public Pop3Response(byte[] responseContents, string hostMessage, bool statusIndicator)
		{
			if (responseContents == null)
			{
                throw new ArgumentNullException("responseContents");
			}

			if (string.IsNullOrEmpty(hostMessage))
			{
				throw new ArgumentNullException("hostMessage");
			}

			ResponseContents = responseContents;
			HostMessage = hostMessage;
			StatusIndicator = statusIndicator;
		}

	    /// <summary>
	    /// Gets the response contents.
	    /// </summary>
	    /// <value>The response contents.</value>
	    internal byte[] ResponseContents { get; private set; }

	    /// <summary>
	    /// Gets a value indicating whether message was <c>true</c> +OK or <c>false</c> -ERR
	    /// </summary>
	    /// <value><c>true</c> if [status indicator]; otherwise, <c>false</c>.</value>
	    public bool StatusIndicator { get; private set; }

	    /// <summary>
	    /// Gets the host message.
	    /// </summary>
	    /// <value>The host message.</value>
	    public string HostMessage { get; private set; }

	    /// <summary>
		/// Creates the response.
		/// </summary>
		/// <param name="responseContents">The response contents.</param>
		/// <returns></returns>
		public static Pop3Response CreateResponse(byte[] responseContents)
		{
			string hostMessage;
			MemoryStream stream = new MemoryStream(responseContents);
			using (StreamReader reader = new StreamReader(stream))
			{
				hostMessage = reader.ReadLine();

				if (hostMessage == null)
				{
                    ArgumentException exception = new ArgumentException("responseContents");
                    exception.Data["responseContents"] = responseContents;
                    throw exception;
				}

				bool success = hostMessage.StartsWith(Pop3Responses.Ok);

				return new Pop3Response(responseContents, hostMessage, success);
			}
		}
	}
}