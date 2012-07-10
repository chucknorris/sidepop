namespace sidepop.Mail.Commands
{
	/// <summary>
	/// This class contains a string representation of Pop3 commands
	/// that can be executed.
	/// </summary>
	internal static class Pop3Commands
	{
        /// <summary>
        /// Carriage return in hexadecimal
        /// </summary>
        public const byte Cr = 0xD;

        /// <summary>
        /// Line feed in hexadecimal
        /// </summary>
        public const byte Lf = 0xA;

		/// <summary>
		/// The CRLF escape sequence.
		/// </summary>
		public const string Crlf = "\r\n";

		/// <summary>
		/// The DELE command followed by a space.
		/// </summary>
		public const string Dele = "DELE ";

		/// <summary>
		/// The LIST command followed by a space.
		/// </summary>
		public const string List = "LIST ";

		/// <summary>
		/// The NOOP command followed by a CRLF.
		/// </summary>
		public const string Noop = "NOOP\r\n";

		/// <summary>
		/// The PASS command followed by a space.
		/// </summary>
		public const string Pass = "PASS ";

		/// <summary>
		/// The QUIT command followed by a CRLF.
		/// </summary>
		public const string Quit = "QUIT\r\n";

		/// <summary>
		/// The RETR command followed by a space.
		/// </summary>
		public const string Retr = "RETR ";

		/// <summary>
		/// The RSET command followed by a CRLF.
		/// </summary>
		public const string Rset = "RSET\r\n";

		/// <summary>
		/// The STAT command followed by a CRLF.
		/// </summary>
		public const string Stat = "STAT\r\n";

		/// <summary>
		/// The TOP command followed by a space.
		/// </summary>
		public const string Top = "TOP ";

		/// <summary>
		/// The USER command followed by a space.
		/// </summary>
		public const string User = "USER ";
	}
}