using NLog.Layouts;
using NLog.Config;
using NLog.Common;
using System;
using NLog.Internal;

namespace NLog.Conditions
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ConditionLazyExpression : ConditionExpression, ISupportsLazyParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionLazyExpression" /> class.
		/// </summary>
		/// <param name="text">text of expression.</param>
		public ConditionLazyExpression(string text)
		{
			Text = text;
			Inner = null;
		}

		/// <summary>
		/// Text of expression.
		/// </summary>
		public string Text { get; private set; }
		
		/// <summary>
		/// Parced expression
		/// </summary>
		public ConditionExpression Inner {get; private set;}

		/// <summary>
		/// Returns a string representation of this expression.
		/// </summary>
		/// <returns>String literal in single quotes.</returns>
		public override string ToString()
		{
			if (Inner != null)
				return Inner.ToString();
			return Text;
		}

		/// <summary>
		/// Evaluates the expression
		/// </summary>
		protected override object EvaluateNode(LogEventInfo context)
		{
			if (Inner == null)
				throw new InvalidOperationException("required run CreateParameters method");

			return Inner.Evaluate(context);
		}

		public void CreateParameters(LoggingConfiguration cfg)
		{
			Inner = ConditionParser.ParseExpression(Text, cfg);
		}
	}
}