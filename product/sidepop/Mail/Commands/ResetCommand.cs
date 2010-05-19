using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This command represents the Pop3 RSET command.
	/// This resets (unmarks) any messages previously marked for deletion in this session so that the QUIT command will not delete them.
	/// </summary>
	internal sealed class ResetCommand : Pop3Command<Pop3Response>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResetCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public ResetCommand(Stream stream)
			: base(stream, false, Pop3State.Transaction)
		{
		}

		/// <summary>
		/// Creates the RSET request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the RSET request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Rset);
		}
	}
}