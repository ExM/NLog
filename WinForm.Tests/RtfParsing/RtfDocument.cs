using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NLog.WinForm.RtfParsing
{
	public class RtfDocument
	{
		public List<RtfParagraph> Pars = new List<RtfParagraph>();


		public static List<RtfParagraph> Load(string text)
		{
			int end;
			Token body = Split(0, out end, text);

			if (body.Text != null)
				throw new FormatException("no RTF format");

			Color[] table = ExtractColorTable(body.Inner);
			string parsText = body.Inner[body.Inner.Count - 1].Text;
			if(parsText == null)
				throw new FormatException("text not found");

			return ExtractParagraphs(parsText, table);
		}

		private static List<RtfParagraph> ExtractParagraphs(string text, Color[] table)
		{
			RtfParagraph last = null;
			List<RtfParagraph> result = new List<RtfParagraph>();

			int start = 0;
			while(true)
			{
				int pos = text.IndexOf("\\par", start);
				if(pos == -1)
				{
					result.Add(RtfParagraph.Parse(last, table, text.Substring(start)));
					return result;
				}

				int offset = "\\par".Length;
				if (text.IndexOf("\\pard", pos) == pos)
				{
					last = null;
					offset = "\\pard".Length;
				}

				var p = RtfParagraph.Parse(last, table, text.Substring(start, pos - start));

				last = p;
				result.Add(p);
				start = pos + offset;
			}
		}

		private static Color[] ExtractColorTable(List<Token> list)
		{
			foreach (var t in list)
			{
				if (t.Text == null)
					continue;
				if (t.Text.StartsWith("\\colortbl"))
					return ParseColorTable(t.Text.Substring("\\colortbl".Length + 1));
			}

			return new Color[0];
		}

		private static Color[] ParseColorTable(string text)
		{
			List<Color> result = new List<Color>();
			// \red0\green0\blue0;\red128\green0\blue0;
			foreach (string item in text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				result.Add(ParseColor(item));

			return result.ToArray();
		}

		private static Color ParseColor(string item)
		{
			int[] nums = item
				.Replace("\\red", "")
				.Replace("\\green", ",")
				.Replace("\\blue", ",")
				.Split(',').Select(txt => int.Parse(txt)).ToArray();
			
			if(nums.Length != 3)
				throw new FormatException("no Color format");

			return Color.FromArgb(nums[0], nums[1], nums[2]);
		}

		public static Token Split(int start, out int end, string text)
		{
			List<Token> tokens = new List<Token>();

			while (true)
			{
				int pos = FindNextBracket(start, text);
				if (pos == -1)
				{
					end = text.Length;
					tokens.Add(new Token() { Text = text.Substring(start) });
					tokens = RemoveEmpty(tokens);

					if (tokens.Count == 0)
						return new Token() { Text = string.Empty };
					if (tokens.Count == 1)
						return tokens[0];

					return new Token() { Inner = tokens };
				}

				if (text[pos] == '{')
				{
					tokens.Add(new Token() { Text = text.Substring(start, pos - start) });
					int upEnd;
					tokens.Add(Split(pos + 1, out upEnd, text));
					start = upEnd + 1;
				}
				else // '}'
				{
					if (tokens.Count == 0)
					{
						end = pos;
						return new Token() { Text = text.Substring(start, pos - start) };
					}
					else
					{
						end = pos;
						tokens.Add(new Token() { Text = text.Substring(start, pos - start) });
						tokens = RemoveEmpty(tokens);

						if (tokens.Count == 0)
							return new Token() { Text = string.Empty };
						if (tokens.Count == 1)
							return tokens[0];
						
						return new Token() { Inner = tokens };
					}
				}
			}
		}

		private static List<Token> RemoveEmpty(List<Token> tokens)
		{
			List<Token> result = new List<Token>();
			foreach (var t in tokens)
			{
				if (t.Text != null && IsEmpty(t.Text))
					continue;
				result.Add(t);
			}

			return result;
		}

		private static bool IsEmpty(string text)
		{
			text = text.Replace('\r', ' ').Replace('\n', ' ').Replace('\0', ' ');
			return string.IsNullOrWhiteSpace(text);
		}

		private static int FindNextBracket(int start, string text)
		{
			while (true)
			{
				if (text.Length <= start)
					return -1;

				int pos = text.IndexOfAny(new char[] { '{', '}' }, start);
				if (pos == -1)
					return -1;

				if (pos - 1 < 0)
					return 0;

				if (text[pos - 1] == '\\')
					start = pos + 1;
				else
					return pos;
			}
		}
	}
}
