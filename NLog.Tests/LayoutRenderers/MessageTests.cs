using System;
using System.Xml;
using System.Reflection;
using System.Globalization;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
	[TestFixture]
	public class MessageTests : NLogTestBase
	{
		[Test]
		public void MessageWithoutPaddingTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "a");
			logger.Debug("a{0}", 1);
			AssertDebugLastMessage("debug", "a1");
			logger.Debug("a{0}{1}", 1, "2");
			AssertDebugLastMessage("debug", "a12");
			logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
			AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
		}

		[Test]
		public void MessageRightPaddingTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:padding=3}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "  a");
			logger.Debug("a{0}", 1);
			AssertDebugLastMessage("debug", " a1");
			logger.Debug("a{0}{1}", 1, "2");
			AssertDebugLastMessage("debug", "a12");
			logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
			AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
		}


		[Test]
		public void MessageFixedLengthRightPaddingTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:padding=3:fixedlength=true}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "  a");
			logger.Debug("a{0}", 1);
			AssertDebugLastMessage("debug", " a1");
			logger.Debug("a{0}{1}", 1, "2");
			AssertDebugLastMessage("debug", "a12");
			logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
			AssertDebugLastMessage("debug", "a01");
		}

		[Test]
		public void MessageLeftPaddingTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "axx");
			logger.Debug("a{0}", 1);
			AssertDebugLastMessage("debug", "a1x");
			logger.Debug("a{0}{1}", 1, "2");
			AssertDebugLastMessage("debug", "a12");
			logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
			AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
		}

		[Test]
		public void MessageFixedLengthLeftPaddingTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x:fixedlength=true}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "axx");
			logger.Debug("a{0}", 1);
			AssertDebugLastMessage("debug", "a1x");
			logger.Debug("a{0}{1}", 1, "2");
			AssertDebugLastMessage("debug", "a12");
			logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
			AssertDebugLastMessage("debug", "a01");
		}

		[Test]
		public void MessageWithExceptionTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:withException=true}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "a");

			var ex = new InvalidOperationException("Exception message.");
			logger.DebugException("Foo", ex);
			string newline = Environment.NewLine;

			AssertDebugLastMessage("debug", "Foo" + newline + ex.ToString());
		}

		[Test]
		public void MessageWithExceptionAndCustomSeparatorTest()
		{
			LogManager.Configuration = CreateConfigurationFromString(@"
			<nlog>
				<targets><target name='debug' type='Debug' layout='${message:withException=true:exceptionSeparator=,}' /></targets>
				<rules>
					<logger name='*' minlevel='Debug' writeTo='debug' />
				</rules>
			</nlog>");

			Logger logger = LogManager.GetLogger("A");
			logger.Debug("a");
			AssertDebugLastMessage("debug", "a");

			var ex = new InvalidOperationException("Exception message.");
			logger.DebugException("Foo", ex);
			AssertDebugLastMessage("debug", "Foo," + ex.ToString());
		}
	}
}