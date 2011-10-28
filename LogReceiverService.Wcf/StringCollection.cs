using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NLog.LogReceiverService
{
	/// <summary>
	/// List of strings annotated for more terse serialization.
	/// </summary>
	[CollectionDataContract(ItemName = "l", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
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
