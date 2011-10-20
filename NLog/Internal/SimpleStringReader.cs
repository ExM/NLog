
namespace NLog.Internal
{
	/// <summary>
	/// Simple character tokenizer.
	/// </summary>
	public class SimpleStringReader
	{
		private readonly string text;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleStringReader" /> class.
		/// </summary>
		/// <param name="text">The text to be tokenized.</param>
		public SimpleStringReader(string text)
		{
			this.text = text;
			this.Position = 0;
		}

		internal int Position { get; set; }

		internal string Text
		{
			get { return this.text; }
		}

		internal int Peek()
		{
			if (this.Position < this.text.Length)
			{
				return this.text[this.Position];
			}

			return -1;
		}

		internal int Read()
		{
			if (this.Position < this.text.Length)
			{
				return this.text[this.Position++];
			}

			return -1;
		}

		internal string Substring(int p0, int p1)
		{
			return this.text.Substring(p0, p1 - p0);
		}
	}
}