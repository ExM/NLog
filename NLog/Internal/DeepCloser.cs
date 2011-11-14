using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Common;

namespace NLog.Internal
{
	internal class DeepCloser: IDisposable
	{
		private ISupportsInitialize[] _initializedItems;

		internal DeepCloser(ISupportsInitialize[] initialized)
		{
			_initializedItems = initialized;
		}

		public void Dispose()
		{
			if(_initializedItems == null)
				return;
			
			foreach (var initialize in _initializedItems)
			{
				InternalLogger.Trace("Closing {0}", initialize);
				try
				{
					initialize.Close();
				}
				catch (Exception exception)
				{
					if (exception.MustBeRethrown())
						throw;

					InternalLogger.Warn("Exception while closing {0}", exception);
				}
			}
			_initializedItems = null;
		}
	}
}
