using System;
using System.Collections.Generic;
using NLog.Common;

namespace NLog.Internal
{
	/// <summary>
	/// Provides helpers to sort log events and associated continuations.
	/// </summary>
	internal static class SortHelpers
	{
		/// <summary>
		/// Performs bucket sort (group by) on an array of items and returns a dictionary for easy traversal of the result set.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="inputs">The inputs.</param>
		/// <param name="keySelector">The key selector function.</param>
		/// <returns>
		/// Dictonary where keys are unique input keys, and values are lists of <see cref="AsyncLogEventInfo"/>.
		/// </returns>
		public static Dictionary<TKey, List<TValue>> BucketSort<TValue, TKey>(this IEnumerable<TValue> inputs, Func<TValue, TKey> keySelector)
		{
			var buckets = new Dictionary<TKey, List<TValue>>();

			foreach (var input in inputs)
			{
				var keyValue = keySelector(input);
				List<TValue> eventsInBucket;
				if (!buckets.TryGetValue(keyValue, out eventsInBucket))
				{
					eventsInBucket = new List<TValue>();
					buckets.Add(keyValue, eventsInBucket);
				}

				eventsInBucket.Add(input);
			}

			return buckets;
		}
	}
}
