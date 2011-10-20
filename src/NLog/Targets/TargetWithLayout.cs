
namespace NLog.Targets
{
	using System.ComponentModel;
	using NLog.Config;
	using NLog.Layouts;

	/// <summary>
	/// Represents target that supports string formatting using layouts.
	/// </summary>
	public abstract class TargetWithLayout : Target
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TargetWithLayout" /> class.
		/// </summary>
		/// <remarks>
		/// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This one is safe.")]
		protected TargetWithLayout()
		{
			this.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}";
		}

		/// <summary>
		/// Gets or sets the layout used to format log messages.
		/// </summary>
		/// <docgen category='Layout Options' order='1' />
		[RequiredParameter]
		[DefaultValue("${longdate}|${level:uppercase=true}|${logger}|${message}")]
		public virtual Layout Layout { get; set; }
   }
}
