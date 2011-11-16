using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NLog.WinForm.RtfParsing
{
	public class RtfParagraph
	{
		public Color? Color;
		public FontStyle? FontStyle;

		public List<RtfText> Items = new List<RtfText>();

		internal static RtfParagraph Parse(RtfParagraph last, Color[] table, string text)
		{
			RtfParagraph result = new RtfParagraph();
			if (last != null)
			{
				result.Color = last.Color;
				result.FontStyle = last.FontStyle;
			}



			return result;
		}
	}
}
