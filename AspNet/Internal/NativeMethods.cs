using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
	
namespace NLog.Internal
{
	internal static class NativeMethods
	{
		[DllImport("ole32.dll")]
		internal static extern int CoGetObjectContext(ref Guid iid, out AspHelper.IObjectContext g);
	}
}
