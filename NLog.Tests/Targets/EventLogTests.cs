using System.Diagnostics;
using NUnit.Framework;
using NLog.Config;
using NLog.Targets;

namespace NLog.UnitTests.Targets
{
	[TestFixture]
	public class EventLogTests : NLogTestBase
	{
		[SetUp]
		public void Init()
		{
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void Test1()
		{
#if TODO
			EventLogTarget elt = new EventLogTarget();
			elt.Log = "NLog.UnitTests";
			elt.Source = "NLog.UnitTests";
			elt.EventId = "10";
			elt.Category = "123";
			SimpleConfigurator.ConfigureForTargetLogging(elt);

			LogManager.Configuration = null;

			Logger l = LogManager.GetCurrentClassLogger();
			l.Info("aaa");
#endif
		}
	}
}
