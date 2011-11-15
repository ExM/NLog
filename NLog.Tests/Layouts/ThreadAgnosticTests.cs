using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NLog.LayoutRenderers;
using NLog.Internal;
using NLog.Config;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;
using NUnit.Framework;
using NLog.Common;

namespace NLog.UnitTests.Layouts
{
	[TestFixture]
	public class ThreadAgnosticTests : NLogTestBase
	{
		[Test]
		public void ThreadAgnosticAttributeTest()
		{
			foreach (var t in ReflectionHelpers.SafeGetTypes(typeof(Layout).Assembly))
			{
				if (t.Namespace == typeof(WrapperLayoutRendererBase).Namespace)
				{
					if (t.IsAbstract)
					{
						// skip non-concrete types
						continue;
					}

					Assert.IsTrue(t.IsDefined(typeof(ThreadAgnosticAttribute), true), "Type " + t + " is missing [ThreadAgnostic] attribute.");
				}
			}
		}

		[Test]
		public void ThreadAgnosticTest()
		{
			Layout l = new SimpleLayout("${message}");
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void NonThreadAgnosticTest()
		{
			Layout l = new SimpleLayout("${threadname}");
			l.Initialize(CommonCfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void AgnosticPlusNonAgnostic()
		{
			Layout l = new SimpleLayout("${message}${threadname}");
			l.Initialize(CommonCfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void AgnosticPlusAgnostic()
		{
			Layout l = new SimpleLayout("${message}${level}${logger}");
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void WrapperOverAgnostic()
		{
			Layout l = new SimpleLayout("${rot13:${message}}");
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void DoubleWrapperOverAgnostic()
		{
			Layout l = new SimpleLayout("${lowercase:${rot13:${message}}}");
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void TripleWrapperOverAgnostic()
		{
			Layout l = new SimpleLayout("${uppercase:${lowercase:${rot13:${message}}}}");
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void TripleWrapperOverNonAgnostic()
		{
			Layout l = new SimpleLayout("${uppercase:${lowercase:${rot13:${message}${threadname}}}}");
			l.Initialize(CommonCfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void ComplexAgnosticWithCondition()
		{
			Layout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger'}";
			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void ComplexNonAgnosticWithCondition()
		{
			Layout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${threadname}:padding=10:padCharacter=X}'=='XXXXlogger'}";
			l.Initialize(CommonCfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void CsvThreadAgnostic()
		{
			CsvLayout l = new CsvLayout()
			{
				Columns =
				{
					new CsvColumn("name1", "${message}"),
					new CsvColumn("name2", "${level}"),
					new CsvColumn("name3", "${longdate}"),
				},
			};

			l.Initialize(CommonCfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[Test]
		public void CsvNonAgnostic()
		{
			CsvLayout l = new CsvLayout()
			{
				Columns =
				{
					new CsvColumn("name1", "${message}"),
					new CsvColumn("name2", "${threadname}"),
					new CsvColumn("name3", "${longdate}"),
				},
			};

			l.Initialize(CommonCfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void CustomNotAgnosticTests()
		{
			var cif = new ConfigurationItemFactory();
			cif.RegisterType(typeof(CustomRendererNonAgnostic), string.Empty);
			var cfg = new LoggingConfiguration(cif);
			Layout l = new SimpleLayout("${customNotAgnostic}", cfg);

			l.Initialize(cfg);
			Assert.IsFalse(l.IsThreadAgnostic);
		}

		[Test]
		public void CustomAgnosticTests()
		{
			var cif = new ConfigurationItemFactory();
			cif.RegisterType(typeof(CustomRendererAgnostic), string.Empty);
			var cfg = new LoggingConfiguration(cif);
			Layout l = new SimpleLayout("${customAgnostic}", cfg);

			l.Initialize(cfg);
			Assert.IsTrue(l.IsThreadAgnostic);
		}

		[LayoutRenderer("customNotAgnostic")]
		public class CustomRendererNonAgnostic : LayoutRenderer
		{
			protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
			{
				builder.Append("custom");
			}
		}

		[LayoutRenderer("customAgnostic")]
		[ThreadAgnostic]
		public class CustomRendererAgnostic : LayoutRenderer
		{
			protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
			{
				builder.Append("customAgnostic");
			}
		}
	}
}
