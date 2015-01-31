using System;
using System.IO;

using EOLib;

namespace EndlessClient.Handlers
{
	enum InitReply : byte
	{
		INIT_OUT_OF_DATE = 1,
		INIT_OK = 2,
		INIT_BANNED = 3,
		INIT_FILE_MAP = 4,
		INIT_FILE_EIF = 5,
		INIT_FILE_ENF = 6,
		INIT_FILE_ESF = 7,
		INIT_PLAYERS = 8,
		INIT_MAP_MUTATION = 9,
		INIT_FRIEND_LIST_PLAYERS = 10,
		INIT_FILE_ECF = 11,
		THIS_IS_WRONG = 0
	}

	enum InitBanType : byte
	{
		INIT_BAN_TEMP = 0,
		INIT_BAN_PERM = 2,
		THIS_IS_WRONG = 255
	}

	public enum InitFileType : byte
	{
		Map = 1,
		Item = 2,
		Npc = 3,
		Spell = 4,
		Class = 5
	}

	public struct InitData
	{
		public byte seq_1, seq_2, emulti_e, emulti_d;
		public short clientID;
		public int response;
	}
	
	public static class Init
	{
		private static readonly System.Threading.ManualResetEvent response = new System.Threading.ManualResetEvent(false);

		private static InitReply ServerResponse = InitReply.THIS_IS_WRONG;
		private static InitBanType BanType = InitBanType.THIS_IS_WRONG;
		
		private static byte BanMinsLeft = 255;

		public static bool ExpectingFile { get; private set; }

		public static bool CanProceed { get { return ServerResponse == InitReply.INIT_OK; } }
		public static InitData Data;

		public static bool Initialize()
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

			string HDDserial = Config.GetHDDSerial();

			Packet builder = new Packet(PacketFamily.Init, PacketAction.Init);

			builder.AddThree(Hashes.stupid_hash(new Random().Next(6, 12)));

			builder.AddChar(World.Instance.VersionMajor); //unknown
			builder.AddChar(World.Instance.VersionMinor); //unknown
			builder.AddChar(World.Instance.VersionClient); //client version
			builder.AddChar(112); //unknown
			builder.AddChar((byte)(HDDserial.Length)); //unknown
			builder.AddString(HDDserial);

			if (!client.SendRaw(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseTimeout))
				return false;
			response.Reset();

			return true;
		}

		//sends WELCOME_AGREE to server
		//this does NOT send an INIT family packet. It is much easier
		//	to put the file request in the Init section as it sends an F_INIT, A_INIT
		//	message in response
		public static bool RequestFile(InitFileType file)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

			//send the file request
			Packet builder = new Packet(PacketFamily.Welcome, PacketAction.Agree);
			builder.AddChar((byte)file);
			ExpectingFile = true;

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseFileTimeout))
				return false;
			response.Reset();

			return true;
		}

		//sends WARP_TAKE to server
		//like request file above, does not send INIT family packet (same reasons)
		public static bool WarpGetMap()
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			response.Reset();

			//send the file request
			Packet builder = new Packet(PacketFamily.Warp, PacketAction.Take);
			ExpectingFile = true;

			if (!client.SendPacket(builder))
				return false;

			if (!response.WaitOne(Constants.ResponseFileTimeout))
				return false;
			response.Reset();

			return true;
		}
		
		//this is the handler function for INIT_INIT
		public static void InitResponse(Packet pkt)
		{
			ServerResponse = (InitReply)pkt.GetByte();
			switch (ServerResponse)
			{
				case InitReply.INIT_BANNED:
				{
					//ok...this is SO dumb. Apparently the server sends INIT_BANNED in response to a WARP_TAKE packet. WTF.
					if (ExpectingFile)
					{
						ExpectingFile = false;
						ServerResponse = InitReply.INIT_FILE_MAP;
						goto case InitReply.INIT_FILE_MAP;
					}

					BanType = (InitBanType) pkt.GetByte();
					if (BanType == InitBanType.INIT_BAN_TEMP)
						BanMinsLeft = pkt.GetByte();
				}
					break;
				case InitReply.INIT_OUT_OF_DATE:
				{
					pkt.GetChar();
					pkt.GetChar();
					BanMinsLeft = pkt.GetChar();
				}
					break;
				case InitReply.INIT_OK:
				{
					Data = new InitData
					{
						seq_1 = pkt.GetByte(),
						seq_2 = pkt.GetByte(),
						emulti_d = pkt.GetByte(), //These are switched around from the server: the server's encode function is the client's decode function (and vice versa)
						emulti_e = pkt.GetByte(),
						clientID = pkt.GetShort(),
						response = pkt.GetThree()
					};
				}
					break;
					//file upload is all handled the same way
				case InitReply.INIT_FILE_EIF:
				case InitReply.INIT_FILE_ENF:
				case InitReply.INIT_FILE_ESF:
				case InitReply.INIT_FILE_ECF:
				case InitReply.INIT_FILE_MAP:
				{
					short requestedMap;
					if (!ExpectingFile && Warp.RequestedMap > 0) //condition under which a file was requested for a warp.
						requestedMap = Warp.RequestedMap;
					else
						requestedMap = World.Instance.MainPlayer.ActiveCharacter.CurrentMap;

					ExpectingFile = false;
					string localDir = ServerResponse == InitReply.INIT_FILE_MAP ? "maps" : "pub";

					if (!Directory.Exists(localDir))
						Directory.CreateDirectory(localDir);

					string filename;
					if (ServerResponse == InitReply.INIT_FILE_EIF)
					{
						filename = "dat001.eif";
					}
					else if (ServerResponse == InitReply.INIT_FILE_ENF)
					{
						filename = "dtn001.enf";
					}
					else if (ServerResponse == InitReply.INIT_FILE_ESF)
					{
						filename = "dsl001.esf";
					}
					else if (ServerResponse == InitReply.INIT_FILE_ECF)
					{
						filename = "dat001.ecf";
					}
					else
						filename = string.Format("{0,5:D5}.emf", requestedMap);

					using (FileStream fs = File.Create(Path.Combine(localDir, filename)))
					{
						int dataLen = pkt.Length - 3;
						if (dataLen == 0)
							throw new FileLoadException();
						fs.Write(pkt.GetBytes(dataLen), 0, dataLen);
					}

					//try to load the file that was just downloaded into the world
					//if we are unable to load it, signal an error condition.
					if (ServerResponse == InitReply.INIT_FILE_MAP && !World.Instance.TryLoadMap(requestedMap))
						return;
					if (ServerResponse == InitReply.INIT_FILE_EIF && !World.Instance.TryLoadItems())
						return;
					if (ServerResponse == InitReply.INIT_FILE_ENF && !World.Instance.TryLoadNPCs())
						return;
					if (ServerResponse == InitReply.INIT_FILE_ESF && !World.Instance.TryLoadSpells())
						return;
					if (ServerResponse == InitReply.INIT_FILE_ECF && !World.Instance.TryLoadClasses())
						return;
				}
					break;
			}

			ExpectingFile = false;
			response.Set();
		}

		public static string ResponseMessage(out string caption)
		{
			string msg;

			switch(ServerResponse)
			{
				case InitReply.INIT_BANNED:
					if(BanType == InitBanType.INIT_BAN_PERM)
					{
						msg = "The server dropped the connection, reason: temporary ip ban. " + BanMinsLeft + " minutes.";
					}
					else
					{
						msg = "The server dropped the connection, reason: permanent ip ban.";
					}
					caption = "Connection is blocked";

					break;
				case InitReply.INIT_OUT_OF_DATE:
					msg = "The client you are using is out of date. This server requires version 0.000.0" + BanMinsLeft;
					caption = "Connection refused";
					break;
				default:
					msg = "The game server could not be found. Please try again at a later time";
					caption = "Could not find server";
					break;
			}

			return msg;
		}

		public static void Cleanup()
		{
			response.Dispose();
		}
	}
}
