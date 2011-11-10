
namespace NLog.Targets.Wrappers
{
	using NLog.Conditions;
	using NLog.Config;
	using System.ComponentModel;
	using NLog.Internal;

	/// <summary>
	/// Filtering rule for <see cref="PostFilteringTargetWrapper"/>.
	/// </summary>
	[NLogConfigurationItem]
	public class FilteringRule : ISupportsInitialize
	{
		private bool isInitialized;

		/// <summary>
		/// Gets the logging configuration this target is part of.
		/// </summary>
		protected LoggingConfiguration LoggingConfiguration { get; private set; }

		/// <summary>
		/// Initializes a new instance of the FilteringRule class.
		/// </summary>
		public FilteringRule()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FilteringRule class.
		/// </summary>
		/// <param name="whenExistsExpression">Condition to be tested against all events.</param>
		/// <param name="filterToApply">Filter to apply to all log events when the first condition matches any of them.</param>
		public FilteringRule(ConditionExpression whenExistsExpression, ConditionExpression filterToApply)
		{
			this.Exists = whenExistsExpression;
			this.Filter = filterToApply;
		}

		/// <summary>
		/// Gets or sets the condition to be tested.
		/// </summary>
		/// <docgen category='Filtering Options' order='10' />
		[RequiredParameter]
		public ConditionExpression Exists { get; set; }

		/// <summary>
		/// Gets or sets the resulting filter to be applied when the condition matches.
		/// </summary>
		/// <docgen category='Filtering Options' order='10' />
		[RequiredParameter]
		public ConditionExpression Filter { get; set; }

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public void Initialize(LoggingConfiguration cfg)
		{ // TODO: need refactoring
			if (isInitialized)
				return;

			LoggingConfiguration = cfg;
			isInitialized = true;
			InitializeRule();
			
			foreach(var item in ObjectGraph.OneLevelChilds<ISupportsInitialize>(this))
				item.Initialize(cfg);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			if (isInitialized)
			{
				LoggingConfiguration = null;
				isInitialized = false;
				CloseRule();
			}
		}

		/// <summary>
		/// Initializes the condition.
		/// </summary>
		protected virtual void InitializeRule()
		{
			if (Exists != null)
				Exists.Initialize(LoggingConfiguration);
			if (Filter != null)
				Filter.Initialize(LoggingConfiguration);
		}

		/// <summary>
		/// Closes the condition.
		/// </summary>
		protected virtual void CloseRule()
		{
		}

	}
}
