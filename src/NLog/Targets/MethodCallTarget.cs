
namespace NLog.Targets
{
	using System;
	using System.Reflection;

	/// <summary>
	/// Calls the specified static method on each log message and passes contextual parameters to it.
	/// </summary>
	/// <seealso href="http://nlog-project.org/wiki/MethodCall_target">Documentation on NLog Wiki</seealso>
	/// <example>
	/// <p>
	/// To set up the target in the <a href="config.html">configuration file</a>, 
	/// use the following syntax:
	/// </p>
	/// <code lang="XML" source="examples/targets/Configuration File/MethodCall/NLog.config" />
	/// <p>
	/// This assumes just one target and a single rule. More configuration
	/// options are described <a href="config.html">here</a>.
	/// </p>
	/// <p>
	/// To set up the log target programmatically use code like this:
	/// </p>
	/// <code lang="C#" source="examples/targets/Configuration API/MethodCall/Simple/Example.cs" />
	/// </example>
	[Target("MethodCall")]
	public sealed class MethodCallTarget : MethodCallTargetBase
	{
		/// <summary>
		/// Gets or sets the class name.
		/// </summary>
		/// <docgen category='Invocation Options' order='10' />
		public string ClassName { get; set; }

		/// <summary>
		/// Gets or sets the method name. The method must be public and static.
		/// </summary>
		/// <docgen category='Invocation Options' order='10' />
		public string MethodName { get; set; }

		private MethodInfo Method { get; set; }

		/// <summary>
		/// Initializes the target.
		/// </summary>
		protected override void InitializeTarget()
		{
			base.InitializeTarget();

			if (this.ClassName != null && this.MethodName != null)
			{
				Type targetType = Type.GetType(this.ClassName);
				this.Method = targetType.GetMethod(this.MethodName);
			}
			else
			{
				this.Method = null;
			}
		}

		/// <summary>
		/// Calls the specified Method.
		/// </summary>
		/// <param name="parameters">Method parameters.</param>
		protected override void DoInvoke(object[] parameters)
		{
			if (this.Method != null)
			{
				this.Method.Invoke(null, parameters);
			}
		}
	}
}
