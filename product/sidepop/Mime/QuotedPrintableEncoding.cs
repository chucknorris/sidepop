using System;
using System.IO;
using System.Text.RegularExpressions;

namespace sidepop.Mime
{
	/// <summary>
	/// This class is based on the QuotedPrintable class written by Bill Gearhart
	/// found at http://www.aspemporium.com/classes.aspx?cid=6
	/// </summary>
	public static class QuotedPrintableEncoding
	{
		private const string Equal = "=";

		private const string HexPattern = "(\\=([0-9A-F][0-9A-F]))";

		public static string Decode(string contents)
		{
			if (contents == null)
			{
				throw new ArgumentNullException("contents");
			}

			using (StringWriter writer = new StringWriter())
			{
				using (StringReader reader = new StringReader(contents))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						/*remove trailing line whitespace that may have
                         been added by a mail transfer agent per rule
                         #3 of the Quoted Printable section of RFC 1521.*/
						line.TrimEnd();

						if (line.EndsWith(Equal))
						{
							//Don't include the Equal character itself because it is not part of the line content
							line = line.Substring(0, line.Length - 1);

							writer.Write(DecodeLine(line));
						} //handle soft line breaks for lines that end with an "="
						else
						{
							writer.Write(DecodeLine(line));

							//Avoid extra line break on last line of the message
							if (reader.Peek() != -1)
							{
								writer.WriteLine();
							}
						}
					}
				}
				writer.Flush();

				return writer.ToString();
			}
		}

		private static string DecodeLine(string line)
		{
			if (line == null)
			{
				throw new ArgumentNullException("line");
			}

			Regex hexRegex = new Regex(HexPattern, RegexOptions.IgnoreCase);

			return hexRegex.Replace(line, new MatchEvaluator(HexMatchEvaluator));
		}

		private static string HexMatchEvaluator(Match m)
		{
			int dec = Convert.ToInt32(m.Groups[2].Value, 16);
			char character = Convert.ToChar(dec);
			return character.ToString();
		}
	}
}