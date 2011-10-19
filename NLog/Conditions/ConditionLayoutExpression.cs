
namespace NLog.Conditions
{
	using NLog.Layouts;

	/// <summary>
	/// Condition layout expression (represented by a string literal
	/// with embedded ${}).
	/// </summary>
	internal sealed class ConditionLayoutExpression : ConditionExpression
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionLayoutExpression" /> class.
		/// </summary>
		/// <param name="layout">The layout.</param>
		public ConditionLayoutExpression(Layout layout)
		{
			this.Layout = layout;
		}

		/// <summary>
		/// Gets the layout.
		/// </summary>
		/// <value>The layout.</value>
		public Layout Layout { get; private set; }

		/// <summary>
		/// Returns a string representation of this expression.
		/// </summary>
		/// <returns>String literal in single quotes.</returns>
		public override string ToString()
		{
			return this.Layout.ToString();
		}

		/// <summary>
		/// Evaluates the expression by calculating the value
		/// of the layout in the specified evaluation context.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>The value of the layout.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			return this.Layout.Render(context);
		}
	}
}