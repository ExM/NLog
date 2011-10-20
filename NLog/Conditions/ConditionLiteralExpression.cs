
namespace NLog.Conditions
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Condition literal expression (numeric, <b>LogLevel.XXX</b>, <b>true</b> or <b>false</b>).
	/// </summary>
	public sealed class ConditionLiteralExpression : ConditionExpression
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionLiteralExpression" /> class.
		/// </summary>
		/// <param name="literalValue">Literal value.</param>
		public ConditionLiteralExpression(object literalValue)
		{
			this.LiteralValue = literalValue;
		}

		/// <summary>
		/// Gets the literal value.
		/// </summary>
		/// <value>The literal value.</value>
		public object LiteralValue { get; private set; }

		/// <summary>
		/// Returns a string representation of the expression.
		/// </summary>
		/// <returns>The literal value.</returns>
		public override string ToString()
		{
			if (this.LiteralValue == null)
			{
				return "null";
			}

			return Convert.ToString(this.LiteralValue, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Evaluates the expression.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>The literal value as passed in the constructor.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			return this.LiteralValue;
		}
	}
}