using System;
using System.Collections.Generic;
using System.Text;

namespace YASRL
{
	public interface ILogger
	{
		void Debug(string message);
		void Info(string message);
		void Warning(string message);
		void Error(string message);
	}

	public class CoreConfig
	{
		public int TcpServerPort { get; }
		public string RconPassword { get; }
		public Func<string, string> CommandRunner { get; }

		public CoreConfig(int tcpServerPort, string rconPassword, Func<string, string> commandRunner)
		{
			TcpServerPort = tcpServerPort;
			RconPassword = rconPassword;
			CommandRunner = commandRunner;
		}
	}

	public class LogConfig
	{
		public bool EnableTcpPacketLog { get; }
		public bool EnableTcpConnectionLog { get; }
		public bool EnableTcpAuthLog { get; }
		public bool EnableTcpCommandLog { get; }
		public bool DisablePasswordRedaction { get; }
		public ILogger Logger { get; }

		public LogConfig(bool enableTcpPacketLog, bool enableTcpConnectionLog, bool enableTcpAuthLog, bool enableTcpCommandLog, bool disablePasswordRedaction, ILogger logger)
		{
			EnableTcpPacketLog = enableTcpPacketLog;
			EnableTcpConnectionLog = enableTcpConnectionLog;
			EnableTcpAuthLog = enableTcpAuthLog;
			EnableTcpCommandLog = enableTcpCommandLog;
			DisablePasswordRedaction = disablePasswordRedaction;
			Logger = logger;
		}
	}

	public class Config
    {
		public CoreConfig CoreConfig { get; }
		public LogConfig LogConfig { get; }

		public Config(CoreConfig coreConfig, LogConfig logConfig)
		{
			CoreConfig = coreConfig;
			LogConfig = logConfig;
		}
	}
}
