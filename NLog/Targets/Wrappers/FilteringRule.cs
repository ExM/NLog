using NLog.Conditions;
using NLog.Config;
using System.ComponentModel;
using NLog.Internal;
using NLog.Common;

namespace NLog.Targets.Wrappers
{
	/// <summary>
	/// Filtering rule for <see cref="PostFilteringTargetWrapper"/>.
	/// </summary>
	[NLogConfigurationItem]
	public sealed class FilteringRule : ISupportsInitialize
	{
		private bool _isInitialized;

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
			Exists = whenExistsExpression;
			Filter = filterToApply;
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
		{
			if(_isInitialized)
				return;

			_isInitialized = true;

			if (Exists != null)
				Exists.Initialize(cfg);
			if (Filter != null)
				Filter.Initialize(cfg);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			if(!_isInitialized)
				return;

			_isInitialized = false;

			if (Exists != null)
				Exists.Close();
			if (Filter != null)
				Filter.Close();
		}
	}
}
