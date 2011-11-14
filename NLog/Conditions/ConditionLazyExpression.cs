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
		private ConditionExpression _condEx;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionLazyExpression" /> class.
		/// </summary>
		/// <param name="text">text of expression.</param>
		public ConditionLazyExpression(string text)
		{
			Text = text;
		}

		/// <summary>
		/// Text of expression.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// Returns a string representation of this expression.
		/// </summary>
		/// <returns>String literal in single quotes.</returns>
		public override string ToString()
		{
			if (_condEx != null)
				return _condEx.ToString();
			return Text;
		}

		/// <summary>
		/// Evaluates the expression
		/// </summary>
		protected override object EvaluateNode(LogEventInfo context)
		{
			if(_condEx == null)
				throw new InvalidOperationException("required run CreateChilds method");
		
			return _condEx.Evaluate(context);
		}

		public void CreateParameters(LoggingConfiguration cfg)
		{
			_condEx = ConditionParser.ParseExpression(Text, cfg);
		}
	}
}