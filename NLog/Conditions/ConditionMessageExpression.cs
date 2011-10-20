
namespace NLog.Conditions
{
	/// <summary>
	/// Condition message expression (represented by the <b>message</b> keyword).
	/// </summary>
	public sealed class ConditionMessageExpression : ConditionExpression
	{
		/// <summary>
		/// Returns a string representation of this expression.
		/// </summary>
		/// <returns>The '<b>message</b>' string.</returns>
		public override string ToString()
		{
			return "message";
		}

		/// <summary>
		/// Evaluates to the logger message.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>The logger message.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			return context.FormattedMessage;
		}
	}
}