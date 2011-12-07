using System;
using NLog;
using System.Threading;

namespace Production
{
	class MainClass
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		
		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += HandleAppDomainCurrentDomainUnhandledException;
			
			Log.Info("Main begin");
			
			for(int i = 0; i<10; i++)
			{
				Thread th = new Thread(Work);
				th.Start(i);
			}
			
			//Thread.Sleep(500);
		
			//throw new InvalidOperationException("My EX");
			
			Log.Info("Main end");

			LogManager.Flush();
		}
		
		private static void Work(object arg)
		{
			Random rnd = new Random();
			
			Log.Info("Work {0} begin", arg);
			
			Thread.Sleep(rnd.Next(1, 1000));
			
			//throw new InvalidOperationException("My EX");
			
			Log.Info("Work {0} end", arg);

			LogManager.Flush();
		}

		static void HandleAppDomainCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Fatal("Unhandled exception: {0}", e.ExceptionObject);
			LogManager.Flush();
		}
	}
}
