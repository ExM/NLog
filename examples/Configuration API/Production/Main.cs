using System;
using NLog;
using System.Threading;

namespace Production
{
	class MainClass
	{
		private static object _sync = new object();
		private static int count = 1;
		
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += HandleAppDomainCurrentDomainUnhandledException;
			
			try
			{
				Log.Info("Main begin");
			
				for(int i = 0; i<10; i++)
				{
					LogOn();
					Thread th = new Thread(Work);
					th.Start(i);
				}
				
				//Thread.Sleep(500);
			
				//throw new InvalidOperationException("My EX");
			
				Log.Info("Main end");
			}
			finally
			{
				LogOff();
			}
		}
		
		private static void Work(object arg)
		{
			Random rnd = new Random();
			
			try
			{
				Log.Info("Work {0} begin", arg);
			
				Thread.Sleep(rnd.Next(1, 1000));
				
				//throw new InvalidOperationException("My EX");
				
				Log.Info("Work {0} end", arg);
			}
			catch(Exception ex)
			{
				Log.Fatal("Fatal exception: {0}", ex);
				AppDomain.CurrentDomain.UnhandledException -= HandleAppDomainCurrentDomainUnhandledException;
				throw;
			}
			finally
			{
				LogOff();
			}
		}
		
		private static void LogOn()
		{
			lock(_sync)
				count++;
		}
		
		private static void LogOff()
		{
			lock(_sync)
			{
				count--;
				if(count > 0)
					return;
			}
			
			LogManager.TurnOff();
		}

		static void HandleAppDomainCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Fatal("Unhandled exception: {0}", e.ExceptionObject);
			LogManager.TurnOff();
		}
	}
}
