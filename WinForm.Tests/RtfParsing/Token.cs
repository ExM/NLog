using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.WinForm.RtfParsing
{
	public class Token
	{
		public string Text;
		public List<Token> Inner;

		public Token()
		{
			Inner = new List<Token>();
		}

		public override string ToString()
		{
			if (Text != null)
				return Text;

			return string.Join("|", Inner.Select(t => t.ToString()).ToArray());
		}
	}
}
