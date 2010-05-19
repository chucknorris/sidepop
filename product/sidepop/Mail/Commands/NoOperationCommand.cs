using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 NOOP command.
	/// The server takes no action in response to a NOOP command except to send a positive response. (NOOP means "no operation".) A POP3 client may send the NOOP command to reset a connection timeout timer at the server. 
	///  The server always responds with an OK reply code to the NOOP command
	/// </summary>
	internal sealed class NoOperationCommand : Pop3Command<Pop3Response>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NoOperationCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public NoOperationCommand(Stream stream)
			: base(stream, false, Pop3State.Transaction)
		{
		}

		/// <summary>
		/// Creates the NOOP request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the NOOP request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Noop);
		}
	}
}