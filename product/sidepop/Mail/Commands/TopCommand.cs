using System;
using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 TOP command.
	/// receives two numbers (number of the message and the lines count) as an arguments and returns a string, containing the specified message header and the specified count of the lines of the message body. 
	/// </summary>
	internal sealed class TopCommand : Pop3Command<RetrieveResponse>
	{
		private readonly int _lineCount;
		private readonly int _messageNumber;

		/// <summary>
		/// Initializes a new instance of the <see cref="TopCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="messageNumber">The message number.</param>
		/// <param name="lineCount">The line count.</param>
		internal TopCommand(Stream stream, int messageNumber, int lineCount)
			: base(stream, true, Pop3State.Transaction)
		{
			if (messageNumber < 1)
			{
				throw new ArgumentOutOfRangeException("messageNumber");
			}

			if (lineCount < 0)
			{
				throw new ArgumentOutOfRangeException("lineCount");
			}

			_messageNumber = messageNumber;
			_lineCount = lineCount;
		}

		/// <summary>
		/// Abstract method intended for inheritors to
		/// build out the byte[] request message for
		/// the specific command.
		/// </summary>
		/// <returns>
		/// The byte[] containing the request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Top, _messageNumber.ToString(), " ", _lineCount.ToString(), Pop3Commands.Crlf);
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

			if (response == null)
			{
				return null;
			}

			string[] messageLines = GetResponseLines(StripPop3HostMessage(buffer, response.HostMessage));

			return new RetrieveResponse(response, messageLines);
		}
	}
}