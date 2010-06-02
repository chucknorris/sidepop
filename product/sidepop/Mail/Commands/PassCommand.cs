using System;
using System.IO;
using sidepop.Mail.Responses;

namespace sidepop.Mail.Commands
{
    using System.Text;
    using infrastructure.logging;

    /// <summary>
	/// This class represents the Pop3 PASS command.
	/// </summary>
	internal sealed class PassCommand : Pop3Command<Pop3Response>
	{
		private readonly string _password;

		/// <summary>
		/// Initializes a new instance of the <see cref="PassCommand"/> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="password">The password.</param>
		public PassCommand(Stream stream, string password)
			: base(stream, false, Pop3State.Authorization)
		{
			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentNullException("password");
			}

			_password = password;
		}

        /// <summary>
        /// Gets the request message.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>A byte[] request message to send to the host.</returns>
        protected override byte[] GetRequestMessage(params string[] args)
        {
            string message = string.Join(string.Empty, args);

            return Encoding.ASCII.GetBytes(message);
        }

		/// <summary>
		/// Creates the PASS request message.
		/// </summary>
		/// <returns>
		/// The byte[] containing the PASS request message.
		/// </returns>
		protected override byte[] CreateRequestMessage()
		{
			return GetRequestMessage(Pop3Commands.Pass, _password, Pop3Commands.Crlf);
		}
	}
}