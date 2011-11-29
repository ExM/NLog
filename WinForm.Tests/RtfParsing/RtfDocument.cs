using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NLog.WinForm.RtfParsing
{
	public class RtfDocument
	{
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
			List<RtfParagraph> result = new List<RtfParagraph>();
			Color curFColor = Color.FromKnownColor(KnownColor.WindowText);
			Color curBColor = Color.FromKnownColor(KnownColor.WindowFrame);
			FontStyle curFontStyle = FontStyle.Regular;
			RtfReader reader = new RtfReader(text);
			
			bool newText = false;
			RtfParagraph par = new RtfParagraph();
			
			while(reader.Next())
			{
				if(reader.Text != null)
				{
					//Console.WriteLine("text: {0}", reader.Text);
					
					if(newText || par.Count == 0)
					{
						newText = false;
						par.Add(new RtfText() { FColor = curFColor, BColor = curBColor, FontStyle = curFontStyle, Text = reader.Text });
						continue;
					}
					else
					{
						var t = par[par.Count - 1];
						t.Text += reader.Text;
						continue;
					}
				}
				// reader.Tag != null
				//Console.WriteLine("tag: {0}", reader.Tag);
				
				if(reader.Tag == "par")
				{
					newText = true;
					result.Add(par);
					par = new RtfParagraph();
					continue;
				}

				if(reader.Tag == "pard")
				{
					newText = true;
					curBColor = table[0];
					curFColor = table[0];
					curFontStyle = FontStyle.Regular;
					result.Add(par);
					par = new RtfParagraph();
					continue;
				}
				
				if(reader.Tag == "f")
					continue;
				
				if(reader.Tag == "b")
				{
					newText = true;
					curFontStyle |= FontStyle.Bold;
					continue;
				}
				
				if(reader.Tag == "b0")
				{
					newText = true;
					curFontStyle &= ~FontStyle.Bold;
					continue;
				}
				
				if(reader.Tag == "i")
				{
					newText = true;
					curFontStyle |= FontStyle.Italic;
					continue;
				}
				
				if(reader.Tag == "i0")
				{
					newText = true;
					curFontStyle &= ~FontStyle.Italic;
					continue;
				}
				
				if(reader.Tag == "ul")
				{
					newText = true;
					curFontStyle |= FontStyle.Underline;
					continue;
				}
				
				if(reader.Tag == "ul0" || reader.Tag == "ulnone")
				{
					newText = true;
					curFontStyle &= ~FontStyle.Underline;
					continue;
				}
				
				int num;
				num = NumTag("cf", reader.Tag);
				if(num != -1)
				{
					newText = true;
					curFColor = table[num];
					continue;
				}

				num = NumTag("cb", reader.Tag);
				if(num != -1)
				{
					newText = true;
					curBColor = table[num];
					continue;
				}

				num = NumTag("highlight", reader.Tag);
				if (num != -1)
				{
					newText = true;
					curBColor = table[num];
					continue;
				}
				
				num = NumTag("f", reader.Tag);
				if(num != -1)
					continue;

				num = NumTag("fs", reader.Tag);
				if(num != -1)
					continue;

				num = NumTag("viewkind", reader.Tag);
				if(num != -1)
					continue;

				num = NumTag("uc", reader.Tag);
				if(num != -1)
					continue;
				
				num = NumTag("cf", reader.Tag);
				if(num != -1)
				{
					curFColor = table[num];
					continue;
				}
				
				Console.WriteLine("unknown rtf tag: {0}", reader.Tag);
			}
			
			result.RemoveAll(p => p.Count == 0);
			return result;
		}
		
		private static int NumTag(string tag, string text)
		{
			if(tag.Length >= text.Length)
				return -1;
			
			if(!text.StartsWith(tag))
				return -1;
			
			int num;
			if(int.TryParse(text.Substring(tag.Length), out num))
				return num;
			
			return -1;
		}

		private static Color[] ExtractColorTable(List<Token> list)
		{
			foreach(var t in list)
			{
				if(t.Text == null)
					continue;
				if(t.Text.StartsWith("\\colortbl"))
					return ParseColorTable(t.Text.Substring("\\colortbl".Length + 1));
			}

			return new Color[]{ Color.FromKnownColor(KnownColor.WindowText)};
		}

		private static Color[] ParseColorTable(string text)
		{
			List<Color> result = new List<Color>();
			// \red0\green0\blue0;\red128\green0\blue0;
			foreach (string item in text.Split(new char[] { ';' }, StringSplitOptions.None))
			{
				if(item != string.Empty)
					result.Add(ParseColor(item));
				else if(result.Count == 0)
					result.Add(Color.FromArgb(0,0,0));
			}

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
