
#if !NET_CF && !SILVERLIGHT && !MONO

namespace NLog.UnitTests.Targets.Wrappers
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Security.Principal;
	using NUnit.Framework;
	using NLog.Common;
	using NLog.Targets;
	using NLog.Targets.Wrappers;

	[TestFixture]
	public class ImpersonatingTargetWrapperTests : NLogTestBase
	{
		private const string NLogTestUser = "NLogTestUser";
		private const string NLogTestUserPassword = "BC@57acasd123";

		[Test]
		public void ImpersonatingWrapperTest()
		{
			var wrapped = new MyTarget()
			{
				ExpectedUser = Environment.MachineName + "\\" + NLogTestUser,
			};

			var wrapper = new ImpersonatingTargetWrapper()
			{
				UserName = NLogTestUser,
				Password = NLogTestUserPassword,
				Domain = Environment.MachineName,
				WrappedTarget = wrapped,
			};

			// wrapped.Initialize(null);
			wrapper.Initialize(null);

			var exceptions = new List<Exception>();
			wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			Assert.AreEqual(1, exceptions.Count);
			wrapper.WriteAsyncLogEvents(
				LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
				LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
				LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			Assert.AreEqual(4, exceptions.Count);
			wrapper.Flush(exceptions.Add);
			Assert.AreEqual(5, exceptions.Count);
			foreach (var ex in exceptions)
			{
				Assert.IsNull(ex, Convert.ToString(ex));
			}

			wrapper.Close();
		}

		[Test]
		public void RevertToSelfTest()
		{
			var wrapped = new MyTarget()
			{
				ExpectedUser = Environment.UserDomainName + "\\" + Environment.UserName,
			};

			WindowsIdentity originalIdentity = WindowsIdentity.GetCurrent();

			try
			{
				var id = this.CreateWindowsIdentity(NLogTestUser, Environment.MachineName, NLogTestUserPassword, SecurityLogOnType.Interactive, LogOnProviderType.Default, SecurityImpersonationLevel.Identification);
				id.Impersonate();

				WindowsIdentity changedIdentity = WindowsIdentity.GetCurrent();
				Assert.AreEqual((Environment.MachineName + "\\" + NLogTestUser).ToLowerInvariant(), changedIdentity.Name.ToLowerInvariant());

				var wrapper = new ImpersonatingTargetWrapper()
				{
					WrappedTarget = wrapped,
					RevertToSelf = true,
				};

				// wrapped.Initialize(null);
				wrapper.Initialize(null);

				var exceptions = new List<Exception>();
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
				Assert.AreEqual(1, exceptions.Count);
				wrapper.WriteAsyncLogEvents(
					LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
					LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
					LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
				Assert.AreEqual(4, exceptions.Count);
				wrapper.Flush(exceptions.Add);
				Assert.AreEqual(5, exceptions.Count);
				foreach (var ex in exceptions)
				{
					Assert.IsNull(ex, Convert.ToString(ex));
				}

				wrapper.Close();
			}
			finally
			{
				// revert to self
				NativeMethods.RevertToSelf();

				WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
				Assert.AreEqual(originalIdentity.Name.ToLowerInvariant(), currentIdentity.Name.ToLowerInvariant());
			}
		}

		[Test]
		public void ImpersonatingWrapperNegativeTest()
		{
			var wrapped = new MyTarget()
			{
				ExpectedUser = NLogTestUser,
			};

			var wrapper = new ImpersonatingTargetWrapper()
			{
				UserName = NLogTestUser,
				Password = Guid.NewGuid().ToString("N"), // wrong password
				Domain = Environment.MachineName,
				WrappedTarget = wrapped,
			};

			try
			{
				wrapper.Initialize(null);
				Assert.Fail("Expected exception");
			}
			catch (COMException)
			{
			}

			wrapper.Close(); // will not fail because Initialize() failed
		}

		[Test]
		public void ImpersonatingWrapperNegativeTest2()
		{
			var wrapped = new MyTarget()
			{
				ExpectedUser = NLogTestUser,
			};

			var wrapper = new ImpersonatingTargetWrapper()
			{
				UserName = NLogTestUser,
				Password = NLogTestUserPassword, // wrong password
				Domain = Environment.MachineName,
				ImpersonationLevel = (SecurityImpersonationLevel)1234,
				WrappedTarget = wrapped,
			};

			try
			{
				wrapper.Initialize(null);
				Assert.Fail("Expected exception");
			}
			catch (COMException)
			{
			}

			wrapper.Close(); // will not fail because Initialize() failed
		}

		private WindowsIdentity CreateWindowsIdentity(string username, string domain, string password, SecurityLogOnType logonType, LogOnProviderType logonProviderType, SecurityImpersonationLevel impersonationLevel)
		{
			// initialize tokens
			var existingTokenHandle = IntPtr.Zero;
			var duplicateTokenHandle = IntPtr.Zero;

			if (!NativeMethods.LogonUser(
				username,
				domain,
				password,
				(int)logonType,
				(int)logonProviderType,
				out existingTokenHandle))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			if (!NativeMethods.DuplicateToken(existingTokenHandle, (int)impersonationLevel, out duplicateTokenHandle))
			{
				NativeMethods.CloseHandle(existingTokenHandle);
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			// create new identity using new primary token
			return new WindowsIdentity(duplicateTokenHandle);
		}

		private static class NativeMethods
		{
			// obtains user token
			[DllImport("advapi32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

			// closes open handes returned by LogonUser
			[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool CloseHandle(IntPtr handle);

			// creates duplicate token handle
			[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

			[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool RevertToSelf();
		}

		public class MyTarget : Target
		{
			public MyTarget()
			{
				this.Events = new List<LogEventInfo>();
			}

			public List<LogEventInfo> Events { get; set; }

			public string ExpectedUser { get; set; }

			protected override void InitializeTarget()
			{
				base.InitializeTarget();
				this.AssertExpectedUser();
			}

			protected override void CloseTarget()
			{
				base.CloseTarget();
				this.AssertExpectedUser();
			}

			protected override void Write(LogEventInfo logEvent)
			{
				this.AssertExpectedUser();
				this.Events.Add(logEvent);
			}

			protected override void Write(AsyncLogEventInfo[] logEvents)
			{
				this.AssertExpectedUser();
				base.Write(logEvents);
			}

			protected override void FlushAsync(AsyncContinuation asyncContinuation)
			{
				this.AssertExpectedUser();
				base.FlushAsync(asyncContinuation);
			}

			private void AssertExpectedUser()
			{
				if (this.ExpectedUser != null)
				{
					var windowsIdentity = WindowsIdentity.GetCurrent();
					Assert.IsTrue(windowsIdentity.IsAuthenticated);
					Assert.AreEqual(Environment.MachineName + "\\" + ExpectedUser, windowsIdentity.Name);
				}
			}
		}
	}
}

#endif