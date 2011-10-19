
namespace MyExtensionNamespace
{
	using NLog;
	using NLog.Layouts;

	[Layout("FooLayout")]
	public class FooLayout : Layout
	{
		private int x;

		public int X
		{
			get { return this.x; }
			set { this.x = value; }
		}

		protected override string GetFormattedMessage(LogEventInfo logEvent)
		{
			return "FooFoo" + this.X;
		}
	}
}