namespace sidepop.Mail.Responses
{
	/// <summary>
	/// This class represents the resulting Pop3 response from a STAT command
	/// executed against a Pop3 server.
	/// </summary>
	internal sealed class StatResponse : Pop3Response
	{
	    /// <summary>
		/// Initializes a new instance of the <see cref="StatResponse"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="messageCount">The message count.</param>
		/// <param name="octets">The octets.</param>
		public StatResponse(Pop3Response response, int messageCount, long octets)
			: base(response.ResponseContents, response.HostMessage, response.StatusIndicator)
		{
			MessageCount = messageCount;
			Octets = octets;
		}

	    /// <summary>
	    /// Gets the message count.
	    /// </summary>
	    /// <value>The message count.</value>
	    public int MessageCount { get; private set; }

	    /// <summary>
	    /// Gets the octets.
	    /// </summary>
	    /// <value>The octets.</value>
	    public long Octets { get; private set; }
	}
}