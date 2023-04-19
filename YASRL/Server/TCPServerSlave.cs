using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YASRL.Shared;
using System.Threading;
using YASRL;

namespace YASRL.Server
{
    internal class TCPServerSlave
    {
		private Config Config = YASRL.Config;
		private ILogger Log = YASRL.Log;

		private bool Authenticated = false;
		private Socket Socket;

		private static void ConfigureSocket(Socket Socket)
		{
			Socket.Blocking = true;
		}

		private bool IsConnectionClosed(Socket Socket)
		{
			try
			{
				return Socket.Poll(100, SelectMode.SelectRead) && (Socket.Available == 0);
			}
			catch
			{
				return true;
			}
		}
		private (RconPacket, RconPacket) ProcessPacket(RconPacket rconPacket)
		{
			rconPacket.LogPacket();

			RconPacket GoodServerPacket = new RconPacket(
						0x0,
						RconPacket.RconPacketType.RESPONSE_VALUE,
						"An error occurred, please try again."
					);

			RconPacket GoodServerPacket2 = null;

#if DEBUG
			Log.Debug("A slave thread is now processing data.");
#endif

			if (rconPacket.GetPacketType() == RconPacket.RconPacketType.AUTH)
			{
				// potential timing attack
				// right now we don't care

				if (rconPacket.GetBody() == Config.CoreConfig.RconPassword)
				{
					Log.Info("Client logged on successfully.");

					Authenticated = true;
					
					GoodServerPacket2 = new RconPacket(
						GoodServerPacket.GetId(),
						RconPacket.RconPacketType.RESPONSE_VALUE,
						""
					);

					GoodServerPacket = new RconPacket(
						rconPacket.GetId(),
						RconPacket.RconPacketType.AUTH_RESPONSE,
						""
					);
				}
				else
				{
					Log.Warning("Warning; Client failed to authenticate properly.\nThis might just be innocent, but this could also mean that they're trying to gain RCon to your server.");

					GoodServerPacket2 = new RconPacket(
						GoodServerPacket.GetId(),
						RconPacket.RconPacketType.RESPONSE_VALUE,
						""
					);

					GoodServerPacket = new RconPacket(
						-1,
						RconPacket.RconPacketType.AUTH_RESPONSE,
						""
					);
				}
			}
			else if (rconPacket.GetPacketType() == RconPacket.RconPacketType.EXECCOMMAND)
			{
				if (Authenticated)
				{
					string CommandOutput = Config.CoreConfig.CommandRunner(rconPacket.GetBody());

#if DEBUG
					Log.Debug(CommandOutput);
#endif

					GoodServerPacket = new RconPacket(
						rconPacket.GetId(),
						RconPacket.RconPacketType.RESPONSE_VALUE,
						CommandOutput
					);
				}
			}
			else
			{
				throw new Exception("MALFORMED TYPE!?!?!?!?!?!");
			}

			return (GoodServerPacket, GoodServerPacket2);
		}

        public void Slave(object Socket)
        {

#if DEBUG
            Log.Debug("A slave thread has been started.");
#endif

            Socket GoodSocket;

            try
            {
                GoodSocket = (Socket)Socket;
            }
            catch
            {
                throw new Exception("That wasn't a socket.");
            }

			ConfigureSocket(GoodSocket);

#if DEBUG
            Log.Debug("A slave thread has finished it's initalization.");
#endif

            bool Authenticated = false;

			bool OKToContinue = true;

            while (OKToContinue)
            {
				if (IsConnectionClosed(GoodSocket))
				{
					Log.Info("Connection closed.");

					OKToContinue = false;
					continue;
				}

                try
                {
                    byte[] buffer = new byte[] {};

					while (GoodSocket.Available == 0)
					{
						if (IsConnectionClosed(GoodSocket))
						{
							Log.Info("Connection closed.");

							OKToContinue = false;
							return;
						}
						Thread.Sleep(100);
					}

					Array.Resize(ref buffer, GoodSocket.Available);

					int bytesReceived = GoodSocket.Receive(buffer, GoodSocket.Available, SocketFlags.None);

					RconPacket[] GoodClientPackets = PacketHandler.ConvertPacketArraysToRconPacketArrays(PacketHandler.SplitArray(buffer));

					foreach (RconPacket GoodClientPacket in GoodClientPackets)
					{
						// i hat you c#
						if (GoodClientPacket == null)
						{ continue; }

						(RconPacket GoodServerPacket, RconPacket GoodServerPacket2) = ProcessPacket(GoodClientPacket);

						// for auth_response

						if (GoodServerPacket2 != null)
						{
							GoodServerPacket2.LogPacket();

							GoodSocket.Send(GoodServerPacket2.GetBuffer());
						}

						GoodServerPacket.LogPacket();

						GoodSocket.Send(GoodServerPacket.GetBuffer());
					}
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 995)
                    {
                        Log.Info("Connection was closed by remote client.");
                    }

					Log.Info($"An unknown socket error occurred. ({e.ErrorCode}).");
				}
				catch (ObjectDisposedException)
				{
					Log.Info("Connection was not closed gracefully.");
				}
                catch (Exception e)
                {
                    Log.Error("Something happened within the socket that caused an invalid packet to be sent.");
                    Log.Error("This is usually because someone tried to log in with an invalid client (Like a browser)."	);
                    Log.Error("This is a non-critical error but check you're logging on using an RCon console.");
                    Log.Error("See here for a good one; https://sourceforge.net/projects/rconconsole/.");

					Log.Error($"The exception is as follows:\nType: {e.GetType()}\nMessage: {e.Message}\nSource: {e.Source}\nStack: {e.StackTrace}");

					GoodSocket.Close();
                }
            }
        }
    }
}
