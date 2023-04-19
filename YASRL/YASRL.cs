using System;
using System.Threading;
using YASRL.Server;

namespace YASRL
{
	public class YASRL
	{
		internal static YASRL Singleton;
		internal static Config Config;
		internal static ILogger Log;

		internal TCPServerHost TCPServerHost;

		public bool IsServerRunning = false;

		public YASRL(Config config)
		{
			Singleton = this;
			Config = config;
			Log = Config.LogConfig.Logger;

			TCPServerHost = new TCPServerHost();
		}

		public void Start()
		{
			TCPServerHost.CreateServer();
			IsServerRunning = true;
		}

		public void Stop()
		{
			TCPServerHost.DestroyServer();
			IsServerRunning = false;
		}
	}
}
