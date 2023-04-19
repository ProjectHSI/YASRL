using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YASRL.Shared
{

    internal class RconPacket
    {
        public static readonly Config Config = YASRL.Config;
        private static readonly ILogger Log = YASRL.Log;

        private bool isFrozen = false;
        private int Id;
        private RconPacketType Type;
        private string Body;

        private readonly string overflowString = "The size of the message that the server was going to send is over 4096 bytes. Please check LocalAdmin's output to see the output.";

        public enum RconPacketType
        {
            AUTH = 3,
            EXECCOMMAND = 2,
            AUTH_RESPONSE = 2,
            RESPONSE_VALUE = 0
        }

        public static Dictionary<int, string> RconPacketTypeConversionServer = new Dictionary<int, string>()
        {
            {2, "AUTH_RESPONSE"},
            {0, "RESPONSE_VALUE"}
        };

        public static Dictionary<int, string> RconPacketTypeConversionClient = new Dictionary<int, string>()
        {
            {2, "AUTH_RESPONSE"},
            {0, "RESPONSE_VALUE"}
        };

        public static Exception MalformedPacketException = new Exception("The RCon packet was invalid, this probably isn't due to the application failing but an RCon client not working. Server should force disconnection to the client *immediately*.");

        public RconPacket(int id, RconPacketType type, string body)
        {
            Id = id;
            Type = type;
            Body = body;
        }

        public RconPacket(byte[] packet)
        {
            Log.Debug($"Packet in Hex: {BitConverter.ToString(packet)}");
            Log.Debug($"Length of packet: {packet.Length}");
            byte[] IDBytes = packet.Skip(4).Take(4).ToArray();
            if (!BitConverter.IsLittleEndian) { Array.Reverse(IDBytes); }

            Id = BitConverter.ToInt32(IDBytes, 0);

            Log.Debug($"ID decoded as {Id}");

            byte[] TypeBytes = packet.Skip(8).Take(4).ToArray();
            if (!BitConverter.IsLittleEndian) { Array.Reverse(TypeBytes); }

            int TempType = BitConverter.ToInt32(TypeBytes, 0);

            Log.Debug($"TempType decoded to {TempType}");

            if (TempType == 3)
            {
                Type = RconPacketType.AUTH;
            }
            else if (TempType == 2)
            {
                Type = RconPacketType.EXECCOMMAND;
            }
            else
            {
                throw new Exception("Malformed type *sigh*.");
            }

            Log.Debug($"Type decoded to {Type}");


            Body = Encoding.ASCII.GetString(packet.Skip(12).Take(packet.Skip(12).ToArray().Length - 2).ToArray());

            Log.Debug($"Packet Body in Hex: {BitConverter.ToString(Encoding.ASCII.GetBytes(Body))}");
            Log.Debug($"Packet Body in ASCII: {Body}");
            Log.Debug($"Length of Packet Body: {packet.Length}");

            Freeze();
        }

        public void Freeze()
        {
            if (isFrozen == false)
            {
                isFrozen = true;
            }
            else
            {
                throw new Exception("The Rcon packet was already frozen. (Please report to this plugin's repo!)");
            }
        }

        // 10 bytes is a constant, see https://developer.valvesoftware.com/wiki/Source_RCON_Protocol.
        public int CalculateSize() { return Body.Length + 14; }

        public byte[] GetBuffer()
        {
            byte[] buffer = new byte[CalculateSize()];

            // --- SIZE ENCODING ---

            int Size;

            try
            {
                Size = Convert.ToInt32(CalculateSize());
                if (Size > 4096) { throw new OverflowException("The packet being sent was over 4096 bytes -- Server will send a default string."); }
            }
            catch
            {
                SetBody(overflowString);

                Size = Convert.ToInt32(CalculateSize());
                if (Size > 4096) { throw new OverflowException("Dead X_X"); }
            }

            BitConverter.GetBytes(Size).CopyTo(buffer, 0);

            // -- ID ENCODING ---
            // error handling is for babies (it's already sent as a 32-bit unsigned int so if it's not we're hecked)

            BitConverter.GetBytes(Id).CopyTo(buffer, 4);

            // -- TYPE ENCODING ---
            // error handling is for babies (literally it has to be 3, 2 or 0.)

            BitConverter.GetBytes((int)Type).CopyTo(buffer, 8);

            // -- BODY ENCODING ---

            Encoding.GetEncoding("UTF-8").GetBytes($"{Body}\0\0".ToCharArray()).CopyTo(buffer, 12);

            return buffer;
        }

		public void LogPacket()
		{
			if (Config.LogConfig.EnableTcpPacketLog)
			{
				Log.Debug($"Should password be redacted? {(!Config.LogConfig.DisablePasswordRedaction ? "YES" : "NO")}");

				Log.Debug($"Id: {GetId()}");
				Log.Debug($"Type: {GetPacketType()}");
				if (GetPacketType() == RconPacketType.AUTH && !Config.LogConfig.DisablePasswordRedaction)
				{
					Log.Debug($"Body: [REDACTED]");
				}
				else
				{
					Log.Debug($"Body: {GetBody()}");
				}

				if (GetPacketType() == RconPacketType.AUTH && Config.LogConfig.DisablePasswordRedaction)
				{
					Log.Debug($"Raw Password: {GetBody()}\n{BitConverter.ToString(Encoding.ASCII.GetBytes(GetBody()))}");
					Log.Debug($"Raw Correct Password: {Config.CoreConfig.RconPassword}\n{BitConverter.ToString(Encoding.ASCII.GetBytes(Config.CoreConfig.RconPassword))}");
				}
			}
		}

        // CAUTION: Boilerplate code below this line.

        public void CheckIfFrozen()
        {
            if (isFrozen)
            {
                throw new Exception("The Rcon packet is frozen. Possible confusion between outgoing and incoming packets. (Please report to this plugin's repo!)");
            }
        }

        public void SetType(RconPacketType type)
        {
            CheckIfFrozen();
            Type = type;
        }

        public void SetId(int id)
        {
            CheckIfFrozen();
            Id = id;
        }

        public void SetBody(string body)
        {
            CheckIfFrozen();
            Body = body;
        }

        public RconPacketType GetPacketType() { return Type; }

        public int GetId() { return Id; }

        public string GetBody() { return Body; }
    }
}
