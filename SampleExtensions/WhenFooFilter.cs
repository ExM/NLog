using NLog;
using NLog.Filters;
	
namespace MyExtensionNamespace
{
	[Filter("whenFoo")]
	public class WhenFooFilter : Filter
	{
		private int x;

		public int X
		{
			get { return this.x; }
			set { this.x = value; }
		}

		protected override FilterResult Check(LogEventInfo logEvent)
		{
			if (X == 42)
			{
				return this.Action;
			}

			return FilterResult.Neutral;
		}
	}
}