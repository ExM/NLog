using System;
using System.ComponentModel;
using System.Security.Principal;
using System.Text;

namespace NLog.LayoutRenderers
{
	/// <summary>
	/// Thread identity information (name and authentication information).
	/// </summary>
	[LayoutRenderer("identity")]
	public class IdentityLayoutRenderer : LayoutRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityLayoutRenderer" /> class.
		/// </summary>
		public IdentityLayoutRenderer()
		{
			this.Name = true;
			this.AuthType = true;
			this.IsAuthenticated = true;
			this.Separator = ":";
		}

		/// <summary>
		/// Gets or sets the separator to be used when concatenating 
		/// parts of identity information.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(":")]
		public string Separator { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.Name.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.AuthenticationType.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool AuthType { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to render Thread.CurrentPrincipal.Identity.IsAuthenticated.
		/// </summary>
		/// <docgen category='Rendering Options' order='10' />
		[DefaultValue(true)]
		public bool IsAuthenticated { get; set; }

		/// <summary>
		/// Renders the specified identity information and appends it to the specified <see cref="StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			IPrincipal principal = System.Threading.Thread.CurrentPrincipal;
			if (principal != null)
			{
				IIdentity identity = principal.Identity;
				if (identity != null)
				{
					string separator = string.Empty;

					if (this.IsAuthenticated)
					{
						builder.Append(separator);
						separator = this.Separator;

						if (identity.IsAuthenticated)
						{
							builder.Append("auth");
						}
						else
						{
							builder.Append("notauth");
						}
					}

					if (this.AuthType)
					{
						builder.Append(separator);
						separator = this.Separator;
						builder.Append(identity.AuthenticationType);
					}

					if (this.Name)
					{
						builder.Append(separator);
						builder.Append(identity.Name);
					}
				}
			}
		}
	}
}

