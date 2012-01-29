using System;
using System.IO;
using System.Collections.Generic;

namespace NLog.UnitTests
{
	public static class Toolkit
	{
		public static string[] GetLines(this StringWriter swr)
		{
			return swr.ToString().GetLines();
		}
		
		public static string[] GetLines(this string text)
		{
			StringReader reader = new StringReader(text);
			List<string> lines = new List<string>();
			while(true)
			{
				var line = reader.ReadLine();
				if(line != null)
					lines.Add(line);
				else
					break;
			}
			return lines.ToArray();
		}
	
	}
}

