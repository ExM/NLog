
using NUnit.Framework;

#if !NUNIT
	using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
	using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
	using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
	using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests
{
	[TestFixture]
	public class GetLoggerTests : NLogTestBase
	{
#if !NET_CF
		[Test]
		public void GetCurrentClassLoggerTest()
		{
			Logger logger = LogManager.GetCurrentClassLogger();

			Assert.AreEqual("NLog.UnitTests.GetLoggerTests", logger.Name);
		}
#endif

		[Test]
		public void TypedGetLoggerTest()
		{
			LogFactory lf = new LogFactory();

			MyLogger l1 = (MyLogger)lf.GetLogger("AAA", typeof(MyLogger));
			MyLogger l2 = (MyLogger)lf.GetLogger("AAA", typeof(MyLogger));
			Logger l3 = lf.GetLogger("AAA", typeof(Logger));
			Logger l4 = lf.GetLogger("AAA", typeof(Logger));
			Logger l5 = lf.GetLogger("AAA");
			Logger l6 = lf.GetLogger("AAA");

			Assert.AreSame(l1, l2);
			Assert.AreSame(l3, l4);
			Assert.AreSame(l5, l6);
			Assert.AreSame(l3, l5);

			Assert.AreNotSame(l1, l3);

			Assert.AreEqual("AAA", l1.Name);
			Assert.AreEqual("AAA", l3.Name);
		}

#if !NET_CF
		[Test]
		public void TypedGetCurrentClassLoggerTest()
		{
			LogFactory lf = new LogFactory();

			MyLogger l1 = (MyLogger)lf.GetCurrentClassLogger(typeof(MyLogger));
			MyLogger l2 = (MyLogger)lf.GetCurrentClassLogger(typeof(MyLogger));
			Logger l3 = lf.GetCurrentClassLogger(typeof(Logger));
			Logger l4 = lf.GetCurrentClassLogger(typeof(Logger));
			Logger l5 = lf.GetCurrentClassLogger();
			Logger l6 = lf.GetCurrentClassLogger();

			Assert.AreSame(l1, l2);
			Assert.AreSame(l3, l4);
			Assert.AreSame(l5, l6);
			Assert.AreSame(l3, l5);

			Assert.AreNotSame(l1, l3);

			Assert.AreEqual("NLog.UnitTests.GetLoggerTests", l1.Name);
			Assert.AreEqual("NLog.UnitTests.GetLoggerTests", l3.Name);
		}
#endif

		[Test]
		public void GenericGetLoggerTest()
		{
			LogFactory<MyLogger> lf = new LogFactory<MyLogger>();

			MyLogger l1 = lf.GetLogger("AAA");
			MyLogger l2 = lf.GetLogger("AAA");
			MyLogger l3 = lf.GetLogger("BBB");

			Assert.AreSame(l1, l2);
			Assert.AreNotSame(l1, l3);

			Assert.AreEqual("AAA", l1.Name);
			Assert.AreEqual("BBB", l3.Name);
		}

#if !NET_CF
		[Test]
		public void GenericGetCurrentClassLoggerTest()
		{
			LogFactory<MyLogger> lf = new LogFactory<MyLogger>();

			MyLogger l1 = lf.GetCurrentClassLogger();
			MyLogger l2 = lf.GetCurrentClassLogger();

			Assert.AreSame(l1, l2);
			Assert.AreEqual("NLog.UnitTests.GetLoggerTests", l1.Name);
		}
#endif

		public class MyLogger : Logger
		{
			public MyLogger()
			{
			}

			public void LogWithEventID(int eventID, string message, object[] par)
			{
				LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, this.Name, null, message, par);
				lei.Properties["EventId"] = eventID;
				base.Log(typeof(MyLogger), lei);
			}
		}
	}
}
