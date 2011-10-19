
namespace NLog.LogReceiverService
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
#if WCF_SUPPORTED
	using System.Runtime.Serialization;
#endif

	/// <summary>
	/// List of strings annotated for more terse serialization.
	/// </summary>
#if WCF_SUPPORTED
	[CollectionDataContract(ItemName = "l", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
#endif
	public class StringCollection : Collection<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StringCollection"/> class.
		/// </summary>
		public StringCollection()
		{
		}
	}
}
