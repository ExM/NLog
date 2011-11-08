
namespace NLog.Conditions
{
	/// <summary>
	/// Condition <b>not</b> expression.
	/// </summary>
	internal sealed class ConditionNotExpression : ConditionExpression
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionNotExpression" /> class.
		/// </summary>
		/// <param name="expression">The expression.</param>
		public ConditionNotExpression(ConditionExpression expression)
		{
			this.Expression = expression;
		}

		/// <summary>
		/// Gets the expression to be negated.
		/// </summary>
		/// <value>The expression.</value>
		public ConditionExpression Expression { get; private set; }

		/// <summary>
		/// Returns a string representation of the expression.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the condition expression.
		/// </returns>
		public override string ToString()
		{
			return "(not " + this.Expression + ")";
		}

		/// <summary>
		/// Evaluates the expression.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>Expression result.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			return !(bool)this.Expression.Evaluate(context);
		}

		protected override void InitializeCondition()
		{
			base.InitializeCondition();
			Expression.Initialize(LoggingConfiguration);
		}
	}
}