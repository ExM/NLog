
namespace NLog.Conditions
{
	/// <summary>
	/// Condition logger name expression (represented by the <b>logger</b> keyword).
	/// </summary>
	public sealed class ConditionLoggerNameExpression : ConditionExpression
	{
		/// <summary>
		/// Returns a string representation of this expression.
		/// </summary>
		/// <returns>A <b>logger</b> string.</returns>
		public override string ToString()
		{
			return "logger";
		}

		/// <summary>
		/// Evaluates to the logger name.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>The logger name.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			return context.LoggerName;
		}
	}
}