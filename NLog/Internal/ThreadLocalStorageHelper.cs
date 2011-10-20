using System;
using System.Collections.Generic;
using System.Threading;

namespace NLog.Internal
{
	/// <summary>
	/// Helper for dealing with thread-local storage.
	/// </summary>
	internal static class ThreadLocalStorageHelper
	{
		/// <summary>
		/// Allocates the data slot for storing thread-local information.
		/// </summary>
		/// <returns>Allocated slot key.</returns>
		public static object AllocateDataSlot()
		{
			return System.Threading.Thread.AllocateDataSlot();
		}

		/// <summary>
		/// Gets the data for a slot in thread-local storage.
		/// </summary>
		/// <typeparam name="T">Type of the data.</typeparam>
		/// <param name="slot">The slot to get data for.</param>
		/// <returns>
		/// Slot data (will create T if null).
		/// </returns>
		public static T GetDataForSlot<T>(object slot)
			where T : class, new()
		{
			LocalDataStoreSlot localDataStoreSlot = (LocalDataStoreSlot)slot;
			object v = Thread.GetData(localDataStoreSlot);
			if (v == null)
			{
				v = new T();
				Thread.SetData(localDataStoreSlot, v);
			}

			return (T)v;
		}
	}
}
