using System;
using NLog.Config;
using NLog.Internal;
using NLog.Common;

namespace NLog.Conditions
{
	/// <summary>
	/// Base class for representing nodes in condition expression trees.
	/// </summary>
	[NLogConfigurationItem]
	[ThreadAgnostic]
	public abstract class ConditionExpression : ISupportsInitialize
	{
		private bool _isInitialized;

		/// <summary>
		/// Converts condition text to a condition expression tree.
		/// </summary>
		/// <param name="conditionExpressionText">Condition text to be converted.</param>
		/// <returns>Condition expression tree.</returns>
		public static implicit operator ConditionExpression(string conditionExpressionText)
		{
			return new ConditionLazyExpression(conditionExpressionText); // ConditionParser.ParseExpression(conditionExpressionText);
		}

		/// <summary>
		/// Evaluates the expression.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>Expression result.</returns>
		public object Evaluate(LogEventInfo context)
		{
			try
			{
				return this.EvaluateNode(context);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				throw new ConditionEvaluationException("Exception occurred when evaluating condition", exception);
			}
		}

		/// <summary>
		/// Returns a string representation of the expression.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the condition expression.
		/// </returns>
		public abstract override string ToString();

		/// <summary>
		/// Evaluates the expression.
		/// </summary>
		/// <param name="context">Evaluation context.</param>
		/// <returns>Expression result.</returns>
		protected abstract object EvaluateNode(LogEventInfo context);

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public void Initialize(LoggingConfiguration cfg)
		{
			if (_isInitialized)
				return;

			_isInitialized = true;
			InternalInit(cfg);
			
			foreach(var item in ObjectGraph.OneLevelChilds<ISupportsInitialize>(this)) //HACK: cached to close?
				item.Initialize(cfg);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			if(!_isInitialized)

			_isInitialized = false;
			InternalClose();

			foreach (var item in ObjectGraph.OneLevelChilds<ISupportsInitialize>(this))
				item.Close();
		}

		/// <summary>
		/// Initializes the condition.
		/// </summary>
		protected virtual void InternalInit(LoggingConfiguration cfg)
		{
		}

		/// <summary>
		/// Closes the condition.
		/// </summary>
		protected virtual void InternalClose()
		{
		}
	}
}
