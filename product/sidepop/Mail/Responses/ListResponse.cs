using System;
using System.Collections.Generic;
using sidepop.Mail.Results;

namespace sidepop.Mail.Responses
{
	/// <summary>
	/// This class represents the response message 
	/// returned from both a single line and multi line 
	/// Pop3 LIST Command.
	/// </summary>
	internal sealed class ListResponse : Pop3Response
	{
	    /// <summary>
		/// Initializes a new instance of the <see cref="ListResponse"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		/// <param name="items">The items.</param>
		public ListResponse(Pop3Response response, List<Pop3ListItemResult> items)
			: base(response.ResponseContents, response.HostMessage, response.StatusIndicator)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			Items = items;
		}

	    /// <summary>
	    /// Gets or sets the items.
	    /// </summary>
	    /// <value>The items.</value>
	    public List<Pop3ListItemResult> Items { get; set; }

	    /// <summary>
		/// Gets the message number.
		/// </summary>
		/// <value>The message number.</value>
		public int MessageNumber
		{
			get { return Items[0].MessageId; }
		}

		/// <summary>
		/// Gets number of octets.
		/// </summary>
		/// <value>The number of octets.</value>
		public long Octets 
		{
			get { return Items[0].Octets; }
		}
	}
}