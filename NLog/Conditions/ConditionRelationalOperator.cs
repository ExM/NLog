
namespace NLog.Conditions
{
	/// <summary>
	/// Relational operators used in conditions.
	/// </summary>
	public enum ConditionRelationalOperator
	{
		/// <summary>
		/// Equality (==).
		/// </summary>
		Equal,

		/// <summary>
		/// Inequality (!=).
		/// </summary>
		NotEqual,

		/// <summary>
		/// Less than (&lt;).
		/// </summary>
		Less,

		/// <summary>
		/// Greater than (&gt;).
		/// </summary>
		Greater,

		/// <summary>
		/// Less than or equal (&lt;=).
		/// </summary>
		LessOrEqual,

		/// <summary>
		/// Greater than or equal (&gt;=).
		/// </summary>
		GreaterOrEqual,
	}
}
