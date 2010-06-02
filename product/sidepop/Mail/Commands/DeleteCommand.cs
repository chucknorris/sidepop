using System;
using System.IO;
using sidepop.Mail.Responses;

//using System.Net.Sockets;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 DELE command
	/// Receives a number (number of the message) as an argument and deletes the specified message, located on a server by it's number
	/// </summary>
	internal sealed class DeleteCommand : Pop3Command<Pop3Response>
	{
		private readonly int _messageId = int.MinValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeleteCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="messageId">The message id.</param>
		public DeleteCommand(Stream stream, int messageId)
			: base(stream, false, Pop3State.Transaction)
		{
			if (messageId < 0)
			{
				throw new ArgumentOutOfRangeException("messageId");
			}
			_messageId = messageId;
		}

		/// <summary>
		/// Creates the DELE request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the DELE request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(string.Concat(Pop3Commands.Dele, _messageId.ToString(), Pop3Commands.Crlf));
		}
	}
}