using System;
using System.IO;

namespace sidepop.Mail.Responses
{
	internal sealed class ConnectResponse : Pop3Response
	{
	    public ConnectResponse(Pop3Response response, Stream networkStream)
			: base(response.ResponseContents, response.HostMessage, response.StatusIndicator)
		{
			if (networkStream == null)
			{
				throw new ArgumentNullException("networkStream");
			}
			NetworkStream = networkStream;
		}

	    public Stream NetworkStream { get; private set; }
	}
}