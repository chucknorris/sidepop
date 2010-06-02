using System;
using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 RETR command.
	/// Receives a number (number of the message) as an argument and returns a string with the entire text (including header) of the specified message.
	/// </summary>
	internal sealed class RetrieveCommand : Pop3Command<RetrieveResponse>
	{
		private readonly int _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="RetrieveCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="message">The message.</param>
		public RetrieveCommand(Stream stream, int message)
			: base(stream, true, Pop3State.Transaction)
		{
			if (message < 0)
			{
				throw new ArgumentOutOfRangeException("message");
			}

			_message = message;
		}

		/// <summary>
		/// Creates the RETR request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the RETR request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Retr, _message.ToString(), Pop3Commands.Crlf);
		}

		/// <summary>
		/// Creates the response.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns>
		/// The <c>Pop3Response</c> containing the results of the
		/// Pop3 command execution.
		/// </returns>
		protected override RetrieveResponse CreateResponse(byte[] buffer)
		{
			Pop3Response response = Pop3Response.CreateResponse(buffer);

			string[] messageLines = GetResponseLines(StripPop3HostMessage(buffer, response.HostMessage));

			return new RetrieveResponse(response, messageLines);
		}
	}
}