using System;

namespace NLog.WinForm.RtfParsing
{
	public class RtfReader
	{
		private string _text;
		private int _pos;
		
		public RtfReader(string text)
		{
			_text = text;
			_pos = 0;
		}
		
		public string Text {get; private set;}
		
		public string Tag {get; private set;}
		
		public bool Next()
		{
			if(_text.Length <= _pos)
				return false;
			
			if(_text[_pos] == '\r' || _text[_pos] == '\n')
			{
				_pos++;
				return Next();
			}
			
			if(_text[_pos] == '\\')
			{
				if(_pos + 1 >= _text.Length)
					throw new FormatException("unexpected end");
				
				char nextCh = _text[_pos + 1];
				if(nextCh == '\\' || nextCh == '{' || nextCh == '}')
				{
					Tag = null;
					Text = nextCh.ToString();
					_pos += 2;
					return true;
				}
				
				Text = null;
				int n = _text.IndexOfAny(new char[]{'\\', ' ', '\r', '\n'}, _pos + 1);
				if(n == -1)
				{
					Tag = _text.Substring(_pos + 1);
					_pos = _text.Length;
				}
				else
				{
					Tag = _text.Substring(_pos + 1, n - _pos - 1);
					_pos = n;
					if(_text[n] == ' ')
						_pos++;
				}
			}
			else
			{
				Tag = null;
				int n = _text.IndexOfAny(new char[]{'\\', '\r', '\n'}, _pos);
				if(n == -1)
				{
					Text = _text.Substring(_pos);
					_pos = _text.Length;
				}
				else
				{
					Text = _text.Substring(_pos, n - _pos);
					_pos = n;
				}
			}
			
			return true;
		}
		
	}
}

