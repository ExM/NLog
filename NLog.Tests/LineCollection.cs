using System.Xml;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.UnitTests
{
	public class LineCollection
	{
		private object _sync = new object();
		
		LinkedList<string> _lines = new LinkedList<string>();
		
		public void AppendLine(string frm, params object[] args)
		{
			string line = string.Format(frm, args);
			lock(_sync)
				_lines.AddLast(line);
		}
		
		public string BuildLines()
		{
			StringBuilder result = new StringBuilder();
			lock(_sync)
				foreach(string line in _lines)
					result.AppendLine(line);
			
			return result.ToString();
		}
	}
}
