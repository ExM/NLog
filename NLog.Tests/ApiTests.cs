using System;
using System.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using NLog.Config;

namespace NLog.UnitTests
{
	[TestFixture]
	public class ApiTests : NLogTestBase
	{
		private Type[] allTypes;
		private Assembly nlogAssembly = typeof(LogManager).Assembly;
		private readonly Dictionary<Type, int> typeUsageCount = new Dictionary<Type, int>();

		[SetUp]
		public void Initialize()
		{
			allTypes = typeof(LogManager).Assembly.GetTypes();
		}

		[Test]
		public void PublicEnumsTest()
		{
			foreach (Type type in allTypes)
			{
				if (!type.IsPublic)
				{
					continue;
				}

				if (type.IsEnum || type.IsInterface)
				{
					this.typeUsageCount[type] = 0;
				}
			}

			this.typeUsageCount[typeof(IInstallable)] = 1;

			foreach (Type type in allTypes)
			{
				if (type.IsGenericTypeDefinition)
				{
					continue;
				}

				if (type.BaseType != null)
				{
					this.IncrementUsageCount(type.BaseType);
				}

				foreach (var iface in type.GetInterfaces())
				{
					this.IncrementUsageCount(iface);
				}

				foreach (var method in type.GetMethods())
				{
					if (method.IsGenericMethodDefinition)
					{
						continue;
					}

					// Console.WriteLine("  {0}", method.Name);
					try
					{
						this.IncrementUsageCount(method.ReturnType);

						foreach (var p in method.GetParameters())
						{
							this.IncrementUsageCount(p.ParameterType);
						}
					}
					catch (Exception ex)
					{
						// this sometimes throws on .NET Compact Framework, but is not fatal
						Console.WriteLine("EXCEPTION {0}", ex);
					}
				}
			}

			var unusedTypes = new List<Type>();
			StringBuilder sb = new StringBuilder();

			foreach (var kvp in this.typeUsageCount)
			{
				if (kvp.Value == 0)
				{
					Console.WriteLine("Type '{0}' is not used.", kvp.Key);
					unusedTypes.Add(kvp.Key);
					sb.Append(kvp.Key.FullName).Append("\n");
				}
			}

			Assert.AreEqual(0, unusedTypes.Count, "There are unused public types. " + sb);
		}

		private void IncrementUsageCount(Type type)
		{
			if (type.IsArray)
			{
				type = type.GetElementType();
			}

			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				this.IncrementUsageCount(type.GetGenericTypeDefinition());
				foreach (var parm in type.GetGenericArguments())
				{
					this.IncrementUsageCount(parm);
				}
				return;
			}

			if (type.Assembly != nlogAssembly)
			{
				return;
			}

			if (typeUsageCount.ContainsKey(type))
			{
				typeUsageCount[type]++;
			}
		}
	}
}