using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YASRL.Shared
{
	internal static class PacketHandler
	{
		public static string PrintByteArrayAsString(byte[] data)
		{
			return Encoding.ASCII.GetString(data, 0, data.Length);
		}

		public static string PrintByteArrayAsHex(byte[] data)
		{
			return BitConverter.ToString(data, 0, data.Length);
		}

		public static byte[][] SplitArray(byte[] packet)
		{
			byte[][] packets = new byte[][] { };

			int fromIndex = 0;

			while (packet.Length > 0)
			{
				int sizeOfCurrentPacket;
				sizeOfCurrentPacket = BitConverter.ToInt32(packet.Skip(fromIndex).Take(4).ToArray(), 0);

				YASRL.Log.Debug($"Size of current packet: {sizeOfCurrentPacket.ToString()}");
				YASRL.Log.Debug($"Current packet in ASCII: {PrintByteArrayAsString(packet.Skip(fromIndex).ToArray())}");
				YASRL.Log.Debug($"Current packet in Hex: {PrintByteArrayAsHex(packet.Skip(fromIndex).ToArray())}");

				Array.Resize(ref packets, packets.Length + 1);

				// plus 4 here because the size field doesn't include the size itself
				packets[packets.Length - 1] = packet.Skip(fromIndex).Take(sizeOfCurrentPacket + 4).ToArray();

				packet = packet.Skip(sizeOfCurrentPacket + 4).ToArray();
			}

			return packets;
		}

		public static RconPacket[] ConvertPacketArraysToRconPacketArrays(byte[][] packets)
		{
			RconPacket[] RconPackets = new RconPacket[packets.Length];

			foreach (byte[] packet in packets)
			{
				RconPackets[RconPackets.Length - 1] = new RconPacket(packet);
			}

			return RconPackets;
		}
	}
}
