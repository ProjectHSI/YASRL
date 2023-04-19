using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using YASRL;

namespace YASRL.Server
{
    internal class TCPServerHost
    {
        private static readonly Config Config = YASRL.Config;
		private static readonly ILogger Log = YASRL.Log;

		private static Thread ServerHostThread;

		// Referenced by threads to indicate if it's time for them to close
		// Done around the destruction of the TCPServerHost.
		internal static bool ShouldRunOnNextCycle = true;

        public Thread CreateServer()
        {
            Log.Info("Starting Remote Console Server...");

            Thread currentThread = new Thread(Server)
            {
                IsBackground = true
            };

            currentThread.Start(Config.CoreConfig.TcpServerPort);

			ServerHostThread = currentThread;

            Log.Info($"Server started.\nPort: {Config.CoreConfig.TcpServerPort}.");

            return currentThread;
        }

		private static void Server(object port)
		{
            int intPort;

            try
            {
                intPort = (int)port;
            }
            catch
            {
                throw new Exception("The TCP Server Port is not an integer, please restart the server after fixing the TCP Server Port in SCP:SL RCon's \"CoreConfig.yml\" file.");
            }

            TcpListener listener = new TcpListener(IPAddress.Any, intPort);

            listener.Start();

			while (ShouldRunOnNextCycle)
			{
				Log.Info("Waiting for a new connection.");

				Socket currentSocket = listener.AcceptSocket();

				string Ip = $"{((IPEndPoint)currentSocket.RemoteEndPoint).Address}:";
				string Port = $"{((IPEndPoint)currentSocket.RemoteEndPoint).Port}:";

				Log.Info($"Accepted connection from {Ip}:{Port}.");

				// If ShouldRunOnNextCycle is false after accepting the socket (Likely is.) we close it here
				if (!ShouldRunOnNextCycle)
				{
					currentSocket.Dispose();
					return;
				}

				Thread slaveThread = new Thread(new TCPServerSlave().Slave)
				{
					IsBackground = true
				};
				slaveThread.Start(currentSocket);
            }
        }

        public TCPServerHost()
        {}

        public void DestroyServer() {
			ShouldRunOnNextCycle = false;
        }
    }
}
