using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YASRL;

namespace YASRLServer
{
	internal class Logger : YASRL.ILogger
	{
		public void Debug(string message)
		{
			Console.WriteLine(message);
		}

		public void Info(string message)
		{
			Console.WriteLine(message);
		}
		
		public void Warning(string message)
		{
			Console.WriteLine(message);
		}
		
		public void Error(string message)
		{
			Console.WriteLine(message);
		}
	}

	internal class Program
	{
		internal static YASRL.YASRL YASRLInstance;

		static string Run(string command)
		{
			if (command == "stop")
			{
				YASRLInstance.Stop();
				
			}
			return command;
		}

		static void Main()
		{
			Console.WriteLine("test");

			CoreConfig CoreConfig = new CoreConfig(7778, "SuperSecretPassword", Run);
			LogConfig LogConfig = new LogConfig(true, true, true, true, true, new Logger());

			Config Config = new Config(CoreConfig, LogConfig);

			YASRLInstance = new YASRL.YASRL(Config);

			Console.WriteLine("Ready!");

			YASRLInstance.Start();

			Console.WriteLine("Server started.");

			while (YASRLInstance.IsServerRunning == true)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
