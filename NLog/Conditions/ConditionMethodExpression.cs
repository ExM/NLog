using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NLog.Common;

namespace NLog.Conditions
{
	/// <summary>
	/// Condition method invocation expression (represented by <b>method(p1,p2,p3)</b> syntax).
	/// </summary>
	public sealed class ConditionMethodExpression : ConditionExpression
	{
		private readonly bool acceptsLogEvent;
		private readonly string conditionMethodName;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionMethodExpression" /> class.
		/// </summary>
		/// <param name="conditionMethodName">Name of the condition method.</param>
		/// <param name="methodInfo"><see cref="MethodInfo"/> of the condition method.</param>
		/// <param name="methodParameters">The method parameters.</param>
		public ConditionMethodExpression(string conditionMethodName, MethodInfo methodInfo, IEnumerable<ConditionExpression> methodParameters)
		{
			this.MethodInfo = methodInfo;
			this.conditionMethodName = conditionMethodName;
			this.MethodParameters = new List<ConditionExpression>(methodParameters).AsReadOnly();

			ParameterInfo[] formalParameters = this.MethodInfo.GetParameters();
			if (formalParameters.Length > 0 && formalParameters[0].ParameterType == typeof(LogEventInfo))
			{
				this.acceptsLogEvent = true;
			}

			int actualParameterCount = this.MethodParameters.Count;
			if (this.acceptsLogEvent)
			{
				actualParameterCount++;
			}

			if (formalParameters.Length != actualParameterCount)
			{
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Condition method '{0}' expects {1} parameters, but passed {2}.",
					conditionMethodName,
					formalParameters.Length,
					actualParameterCount);

				InternalLogger.Error(message);
				throw new ConditionParseException(message);
			}
		}

		/// <summary>
		/// Gets the method info.
		/// </summary>
		public MethodInfo MethodInfo { get; private set; }

		/// <summary>
		/// Gets the method parameters.
		/// </summary>
		/// <value>The method parameters.</value>
		public IList<ConditionExpression> MethodParameters { get; private set; }

		/// <summary>
		/// Returns a string representation of the expression.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the condition expression.
		/// </returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(this.conditionMethodName);
			sb.Append("(");

			string separator = string.Empty;
			foreach (ConditionExpression expr in this.MethodParameters)
			{
				sb.Append(separator);
				sb.Append(expr);
				separator = ", ";
			}

			sb.Append(")");
			return sb.ToString();
		}

		/// <summary>
		/// Evaluates the expression.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>Expression result.</returns>
		protected override object EvaluateNode(LogEventInfo context)
		{
			int parameterOffset = this.acceptsLogEvent ? 1 : 0;

			var callParameters = new object[this.MethodParameters.Count + parameterOffset];
			int i = 0;
			foreach (ConditionExpression ce in this.MethodParameters)
			{
				callParameters[i++ + parameterOffset] = ce.Evaluate(context);
			}

			if (this.acceptsLogEvent)
			{
				callParameters[0] = context;
			}

			return this.MethodInfo.Invoke(null, callParameters);
		}
	}
}