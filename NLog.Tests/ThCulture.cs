using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

namespace NLog.UnitTests
{
	/// <summary>
	/// replace culture in current thread
	/// </summary>
	public class ThCulture: IDisposable
	{
		private readonly CultureInfo _restoreCulture;
		private readonly CultureInfo _restoreUICulture;

		private ThCulture(CultureInfo ci)
		{
			if (ci == null)
				ci = CultureInfo.InvariantCulture;

			_restoreCulture = Thread.CurrentThread.CurrentCulture;
			_restoreUICulture = Thread.CurrentThread.CurrentUICulture;

			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}

		/// <summary>
		/// replace culture in current thread
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>
		public static ThCulture Use(CultureInfo ci = null)
		{
			return new ThCulture(ci);
		}

		/// <summary>
		/// restore culture
		/// </summary>
		public void Dispose()
		{
			Thread.CurrentThread.CurrentCulture = _restoreCulture;
			Thread.CurrentThread.CurrentUICulture = _restoreUICulture;
		}
	}
}
