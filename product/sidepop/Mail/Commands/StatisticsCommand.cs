using System;
using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class represents the Pop3 STAT command.
	/// Returns the count of a messages available on a POP3 mail server
	/// </summary>
	internal sealed class StatisticsCommand : Pop3Command<StatResponse>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StatisticsCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public StatisticsCommand(Stream stream)
			: base(stream, false, Pop3State.Transaction)
		{
		}

		/// <summary>
		/// Creates the STAT request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the STAT request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Stat);
		}

		/// <summary>
		/// Creates the response.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns>
		/// The <c>Pop3Response</c> containing the results of the
		/// Pop3 command execution.
		/// </returns>
		protected override StatResponse CreateResponse(byte[] buffer)
		{
			Pop3Response response = Pop3Response.CreateResponse(buffer);
			string[] values = response.HostMessage.Split(' ');

			//should consist of '+OK', 'messagecount', 'octets'
			if (values.Length < 3)
			{
				throw new Pop3Exception(string.Concat("Invalid response message: ", response.HostMessage));
			}

			int messageCount = Convert.ToInt32(values[1]);
			long octets = Convert.ToInt64(values[2]);

			return new StatResponse(response, messageCount, octets);
		}
	}
}