
namespace NLog.Common
{
	using System;
	using NLog.Common;

	/// <summary>
	/// A cyclic buffer of <see cref="LogEventInfo"/> object.
	/// </summary>
	public class LogEventInfoBuffer
	{
		private readonly bool growAsNeeded;
		private readonly int growLimit;

		private AsyncLogEventInfo[] buffer;
		private int getPointer;
		private int putPointer;
		private int count;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogEventInfoBuffer" /> class.
		/// </summary>
		/// <param name="size">Buffer size.</param>
		/// <param name="growAsNeeded">Whether buffer should grow as it becomes full.</param>
		/// <param name="growLimit">The maximum number of items that the buffer can grow to.</param>
		public LogEventInfoBuffer(int size, bool growAsNeeded, int growLimit)
		{
			this.growAsNeeded = growAsNeeded;
			this.buffer = new AsyncLogEventInfo[size];
			this.growLimit = growLimit;
			this.getPointer = 0;
			this.putPointer = 0;
		}

		/// <summary>
		/// Gets the number of items in the array.
		/// </summary>
		public int Size
		{
			get { return this.buffer.Length; }
		}

		/// <summary>
		/// Adds the specified log event to the buffer.
		/// </summary>
		/// <param name="eventInfo">Log event.</param>
		/// <returns>The number of items in the buffer.</returns>
		public int Append(AsyncLogEventInfo eventInfo)
		{
			lock (this)
			{
				// make room for additional item
				if (this.count >= this.buffer.Length)
				{
					if (this.growAsNeeded && this.buffer.Length < this.growLimit)
					{
						// create a new buffer, copy data from current
						int newLength = this.buffer.Length * 2;
						if (newLength >= this.growLimit)
						{
							newLength = this.growLimit;
						}

						// InternalLogger.Trace("Enlarging LogEventInfoBuffer from {0} to {1}", this.buffer.Length, this.buffer.Length * 2);
						var newBuffer = new AsyncLogEventInfo[newLength];
						Array.Copy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
						this.buffer = newBuffer;
					}
					else
					{
						// lose the oldest item
						this.getPointer = this.getPointer + 1;
					}
				}

				// put the item
				this.putPointer = this.putPointer % this.buffer.Length;
				this.buffer[this.putPointer] = eventInfo;
				this.putPointer = this.putPointer + 1;
				this.count++;
				if (this.count >= this.buffer.Length)
				{
					this.count = this.buffer.Length;
				}

				return this.count;
			}
		}

		/// <summary>
		/// Gets the array of events accumulated in the buffer and clears the buffer as one atomic operation.
		/// </summary>
		/// <returns>Events in the buffer.</returns>
		public AsyncLogEventInfo[] GetEventsAndClear()
		{
			lock (this)
			{
				int cnt = this.count;
				var returnValue = new AsyncLogEventInfo[cnt];

				// InternalLogger.Trace("GetEventsAndClear({0},{1},{2})", this.getPointer, this.putPointer, this.count);
				for (int i = 0; i < cnt; ++i)
				{
					int p = (this.getPointer + i) % this.buffer.Length;
					var e = this.buffer[p];
					this.buffer[p] = default(AsyncLogEventInfo); // we don't want memory leaks
					returnValue[i] = e;
				}

				this.count = 0;
				this.getPointer = 0;
				this.putPointer = 0;

				return returnValue;
			}
		}
	}
}
