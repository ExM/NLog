
namespace NLog.LayoutRenderers
{
	using System;
	using System.Globalization;
	using System.Text;
	using Microsoft.Win32;
	using NLog.Config;
	using NLog.Internal;

	/// <summary>
	/// A value from the Registry.
	/// </summary>
	[LayoutRenderer("registry")]
	public class RegistryLayoutRenderer : LayoutRenderer
	{
		private string key;
		private RegistryKey rootKey = Registry.LocalMachine;
		private string subKey;

		/// <summary>
		/// Gets or sets the registry value name.
		/// </summary>
		/// <docgen category='Registry Options' order='10' />
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the value to be output when the specified registry key or value is not found.
		/// </summary>
		/// <docgen category='Registry Options' order='10' />
		public string DefaultValue { get; set; }

		/// <summary>
		/// Gets or sets the registry key.
		/// </summary>
		/// <remarks>
		/// Must have one of the forms:
		/// <ul>
		/// <li>HKLM\Key\Full\Name</li>
		/// <li>HKEY_LOCAL_MACHINE\Key\Full\Name</li>
		/// <li>HKCU\Key\Full\Name</li>
		/// <li>HKEY_CURRENT_USER\Key\Full\Name</li>
		/// </ul>
		/// </remarks>
		/// <docgen category='Registry Options' order='10' />
		[RequiredParameter]
		public string Key
		{
			get
			{
				return this.key;
			}

			set
			{
				this.key = value;
				int pos = this.key.IndexOfAny(new char[] { '\\', '/' });

				if (pos >= 0)
				{
					string root = this.key.Substring(0, pos);
					switch (root.ToUpper(CultureInfo.InvariantCulture))
					{
						case "HKEY_LOCAL_MACHINE":
						case "HKLM":
							this.rootKey = Registry.LocalMachine;
							break;

						case "HKEY_CURRENT_USER":
						case "HKCU":
							this.rootKey = Registry.CurrentUser;
							break;

						default:
							throw new ArgumentException("Key name is invalid. Root hive not recognized.");
					}

					this.subKey = this.key.Substring(pos + 1).Replace('/', '\\');
				}
				else
				{
					throw new ArgumentException("Key name is invalid");
				}
			}
		}

		/// <summary>
		/// Reads the specified registry key and value and appends it to
		/// the passed <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event. Ignored.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			string value;

			try
			{
				using (RegistryKey registryKey = this.rootKey.OpenSubKey(this.subKey))
				{
					value = Convert.ToString(registryKey.GetValue(this.Value, this.DefaultValue), CultureInfo.InvariantCulture);
				}
			}
			catch (Exception ex)
			{
				if (ex.MustBeRethrown())
				{
					throw;
				}

				value = this.DefaultValue;
			}

			builder.Append(value);
		}
	}
}

